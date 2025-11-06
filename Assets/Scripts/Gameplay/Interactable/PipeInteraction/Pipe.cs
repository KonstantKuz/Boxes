using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Interactable.PipeInteraction.Abstract;
using Gameplay.Interactable.PipeInteraction.Command;
using Gameplay.Interactable.PipeInteraction.State;
using Infrastructure;
using Infrastructure.InputService.Abstract;
using Infrastructure.Network.Abstract;
using Mirror;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Interactable.PipeInteraction
{
    [RequireComponent(typeof(Rigidbody))]
    public class Pipe : NetworkBehaviour
    {
        private const string Asphalt = "Asphalt";

        [SerializeField]
        private PipeStateHolder stateHolder;

        [SerializeField]
        private new Rigidbody rigidbody;

        [SerializeField]
        private float minMoveForce = 0.3f;

        [SerializeField]
        private float maxMoveForce = 5f;

        [SerializeField]
        private float rotationTorque = 5f;

        [SerializeField]
        private float maxSpeed = 5f;

        [SerializeField]
        private float maxAngularSpeed = 2f;

        [SerializeField]
        private float pipeLength = 2f;

        [SerializeField]
        private float velocityAlignmentStrength = 0.7f;

        [SerializeField]
        private float directionStabilizationForce = 2f;

        [SerializeField]
        private float stabilizationThreshold = 0.1f;

        [SerializeField]
        private float angularDamping = 0.8f;

        [SerializeField]
        private ForceMode forceMode;

        private INetworkService networkService;
        private INetworkManager networkManager;
        private IPipeInteractionMediator mediator;
        private IInputService inputService;
        private IDisposable stateSubscription;

        private float moveForce;
        private RaycastHit[] hits;
        private Dictionary<uint, Vector3> calculatedPositions;
        private Vector3 targetDirection;

        public INetworkStateHolder<PipeSharedState> StateHolder => stateHolder;
        public PipeSharedState State => StateHolder.GetState();

        [Inject]
        private void Construct(
            INetworkService networkService,
            INetworkManager networkManager,
            IPipeInteractionMediator mediator,
            IInputService inputService)
        {
            this.networkService = networkService;
            this.networkManager = networkManager;
            this.mediator = mediator;
            this.inputService = inputService;
        }

        private void Awake()
        {
            hits = new RaycastHit[10];
            calculatedPositions = new Dictionary<uint, Vector3>();

            moveForce = minMoveForce;
            mediator.RegisterPipe(this);
        }

        public override void OnStartClient()
        {
            stateSubscription = StateHolder.Subscribe(OnStateChanged);
        }

        public override void OnStopClient()
        {
            stateSubscription?.Dispose();
        }

        public override void OnStartServer()
        {
            networkService.ObserveToExecute<JoinPipeCommand>(ExecuteJoin);
            networkService.ObserveToExecute<LeavePipeCommand>(ExecuteLeave);
            networkService.ObserveToExecute<UpdatePipeInputCommand>(ExecuteUpdateInput);
        }

        public void ResetState()
        {
            if (State.PlayerCount > 0)
            {
                foreach (uint playerId in State.PlayerInputs.Keys)
                {
                    TryLeave(playerId);
                }
            }
        }

        private void OnStateChanged(PipeSharedState state)
        {
            RecalculatePositions();

            if (State.PlayerCount > 0)
            {
                IEnumerable<string> inputs = state.PlayerInputs.Select(item => $"[{item.Key}={item.Value}]");
                this.Log(LogType.Log, $"Pipe state {string.Join("|", inputs)}");
            }
        }

        private void ExecuteJoin(JoinPipeCommand command)
        {
            if (command.PipeNetId != netId)
            {
                return;
            }

            PipeSharedState state = State;

            if (state.HasPlayer(command.PlayerNetId))
            {
                return;
            }

            state.PlayerInputs[command.PlayerNetId] = Vector2.zero;
            StateHolder.WriteState(new PipeSharedState(state.PlayerInputs));
        }

        private void ExecuteLeave(LeavePipeCommand command)
        {
            if (command.PipeNetId != netId)
            {
                return;
            }

            PipeSharedState state = State;

            if (!state.HasPlayer(command.PlayerNetId))
            {
                return;
            }

            state.PlayerInputs.Remove(command.PlayerNetId);
            StateHolder.WriteState(new PipeSharedState(state.PlayerInputs));
        }

        private void ExecuteUpdateInput(UpdatePipeInputCommand command)
        {
            if (command.PipeNetId != netId)
            {
                return;
            }

            PipeSharedState state = State;

            if (!state.HasPlayer(command.PlayerNetId))
            {
                return;
            }

            state.PlayerInputs[command.PlayerNetId] = command.Input;
            StateHolder.WriteState(new PipeSharedState(state.PlayerInputs));
        }

        public bool TryJoin(uint playerNetId)
        {
            if (State.PlayerCount >= 4 || State.HasPlayer(playerNetId))
            {
                return false;
            }

            networkService.SendCommand(new JoinPipeCommand(playerNetId, netId));
            return true;
        }

        public bool TryLeave(uint playerNetId)
        {
            if (!State.HasPlayer(playerNetId))
            {
                return false;
            }

            networkService.SendCommand(new LeavePipeCommand(playerNetId, netId));
            return true;
        }

        public Vector3 GetPlayerWorldPosition(uint playerNetId)
        {
            if (calculatedPositions.TryGetValue(playerNetId, out Vector3 localPos))
            {
                return transform.TransformPoint(localPos);
            }
            return transform.position;
        }

        private void RecalculatePositions()
        {
            calculatedPositions.Clear();

            int count = State.PlayerCount;
            if (count == 0)
            {
                return;
            }

            uint[] players = State.PlayerInputs.Keys.ToArray();

            if (count == 1)
            {
                calculatedPositions[players[0]] = Vector3.zero;
            }
            else if (count == 2)
            {
                calculatedPositions[players[0]] = new Vector3(-pipeLength / 2f, 0f, 0f);
                calculatedPositions[players[1]] = new Vector3(pipeLength / 2f, 0f, 0f);
            }
            else if (count == 3)
            {
                calculatedPositions[players[0]] = new Vector3(-pipeLength / 2f, 0f, 0f);
                calculatedPositions[players[1]] = new Vector3(pipeLength / 2f, 0f, 0f);
                calculatedPositions[players[2]] = Vector3.zero;
            }
            else if (count == 4)
            {
                calculatedPositions[players[0]] = new Vector3(-pipeLength / 2f, 0f, 0f);
                calculatedPositions[players[1]] = new Vector3(pipeLength / 2f, 0f, 0f);
                calculatedPositions[players[2]] = new Vector3(-pipeLength / 4f, 0f, 0f);
                calculatedPositions[players[3]] = new Vector3(pipeLength / 4f, 0f, 0f);
            }
        }

        private void FixedUpdate()
        {
            if (!isServer || State.PlayerCount == 0)
            {
                return;
            }

            for (int i = 0; i < hits.Length; i++)
            {
                hits[i] = default;
            }

            Physics.RaycastNonAlloc(transform.position, Vector3.down, hits);
            float targetForce =
                hits.Any(hit => hit.collider?.CompareTag(Asphalt) ?? false) ? maxMoveForce : minMoveForce;

            moveForce = Mathf.Lerp(moveForce, targetForce, Time.fixedDeltaTime * 5f);

            Vector2 combinedInput = CalculateCombinedInput();

            Vector3 pipeForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;

            if (Mathf.Abs(combinedInput.x) > stabilizationThreshold)
            {
                targetDirection = pipeForward;
            }

            Vector3 localForward = transform.forward * combinedInput.y;
            Vector3 forceDirection = new Vector3(localForward.x, 0f, localForward.z);
            rigidbody.AddForce(forceDirection * moveForce, forceMode);

            Vector3 torque = Vector3.up * combinedInput.x * rotationTorque;
            rigidbody.AddTorque(torque, forceMode);

            Vector3 velocity = rigidbody.velocity;
            velocity.y = 0f;

            if (velocity.magnitude > 0.1f)
            {
                float velocityDirection = Mathf.Sign(Vector3.Dot(velocity, pipeForward));
                Vector3 targetVelocity = pipeForward * velocity.magnitude * velocityDirection;
                Vector3 alignedVelocity = Vector3.Lerp(velocity, targetVelocity, velocityAlignmentStrength);

                if (alignedVelocity.magnitude > maxSpeed)
                {
                    alignedVelocity = alignedVelocity.normalized * maxSpeed;
                }

                velocity = alignedVelocity;
            }

            rigidbody.velocity = velocity;

            Vector3 angularVelocity = rigidbody.angularVelocity;

            if (Mathf.Abs(combinedInput.x) < stabilizationThreshold && combinedInput.y != 0f && targetDirection != Vector3.zero)
            {
                float angle = Vector3.SignedAngle(pipeForward, targetDirection, Vector3.up);

                if (Mathf.Abs(angle) > 1f)
                {
                    float correctionVelocity = angle * directionStabilizationForce * Time.fixedDeltaTime;
                    angularVelocity.y = Mathf.Lerp(angularVelocity.y, correctionVelocity, 0.5f);
                }
                else
                {
                    angularVelocity.y *= angularDamping;
                }
            }

            if (Mathf.Abs(angularVelocity.y) > maxAngularSpeed)
            {
                angularVelocity.y = Mathf.Sign(angularVelocity.y) * maxAngularSpeed;
            }

            rigidbody.angularVelocity = angularVelocity;
        }

        private Vector2 CalculateCombinedInput()
        {
            if (State.PlayerCount == 0)
            {
                return Vector2.zero;
            }

            float rotation = CalculateRotation();
            float forward = CalculateForward();

            return new Vector2(rotation, forward);
        }

        private float CalculateRotation()
        {
            float totalTorque = 0f;

            if (State.PlayerCount == 1)
            {
                return State.PlayerInputs.First().Value.x;
            }

            if (inputService.DefaultContextActions.Boost.IsPressed())
            {
                return 0;
            }

            foreach (KeyValuePair<uint, Vector2> kvp in State.PlayerInputs)
            {
                uint playerNetId = kvp.Key;
                Vector2 input = kvp.Value;

                if (!calculatedPositions.TryGetValue(playerNetId, out Vector3 localPos))
                {
                    continue;
                }

                float leverArm = localPos.x;
                totalTorque += input.y * leverArm;
            }

            return totalTorque;
        }

        private float CalculateForward()
        {
            if (State.PlayerCount == 0)
            {
                return 0f;
            }

            float[] inputs = State.PlayerInputs.Values.Select(value => value.y).ToArray();

            if (inputService.DefaultContextActions.Boost.IsPressed())
            {
                return inputs.FirstOrDefault(i => Mathf.Abs(i) > 0);
            }

            if (inputs.Any(input => Mathf.Approximately(input, 0f)))
            {
                return 0f;
            }

            float direction = Mathf.Sign(inputs[0]);
            return inputs.All(i => Mathf.Approximately(Mathf.Sign(i), direction)) ? direction : 0f;
        }
    }
}

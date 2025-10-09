using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Interactable.PipeInteraction.Abstract;
using Gameplay.Interactable.PipeInteraction.Command;
using Gameplay.Interactable.PipeInteraction.State;
using Infrastructure;
using Infrastructure.Network.Abstract;
using Mirror;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Interactable.PipeInteraction
{
    [RequireComponent(typeof(Rigidbody))]
    public class Pipe : NetworkBehaviour
    {
        [SerializeField]
        private PipeStateHolder stateHolder;

        [SerializeField]
        private new Rigidbody rigidbody;

        [SerializeField]
        private float moveForce = 10f;

        [SerializeField]
        private float rotationTorque = 5f;

        [SerializeField]
        private float maxSpeed = 5f;

        [SerializeField]
        private float pipeLength = 4f;

        [SerializeField]
        private ForceMode forceMode;

        private INetworkService networkService;
        private IPipeInteractionMediator mediator;
        private IDisposable stateSubscription;

        private readonly Dictionary<uint, Vector3> calculatedPositions = new Dictionary<uint, Vector3>();

        public INetworkStateHolder<PipeSharedState> StateHolder => stateHolder;
        public PipeSharedState State => StateHolder.GetState();

        [Inject]
        private void Construct(INetworkService networkService, IPipeInteractionMediator mediator)
        {
            this.networkService = networkService;
            this.mediator = mediator;
        }

        private void Awake()
        {
            if (rigidbody == null)
            {
                rigidbody = GetComponent<Rigidbody>();
            }
        }

        public override void OnStartClient()
        {
            mediator.RegisterPipe(this);

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

        private void OnStateChanged(PipeSharedState state)
        {
            RecalculatePositions();
            IEnumerable<string> inputs = state.PlayerInputs.Select(item => $"[{item.Key}={item.Value}]");
            this.Log(LogType.Log, $"Pipe state {string.Join("|", inputs)}");
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

            Vector2 combinedInput = CalculateCombinedInput();

            Vector3 localForward = transform.forward * combinedInput.y;
            Vector3 forceDirection = new Vector3(localForward.x, 0f, localForward.z);
            rigidbody.AddForce(forceDirection * moveForce, forceMode);

            Vector3 torque = Vector3.up * combinedInput.x * rotationTorque;
            rigidbody.AddTorque(torque, forceMode);

            Vector3 velocity = rigidbody.velocity;
            velocity.y = 0f;

            if (velocity.magnitude > maxSpeed)
            {
                velocity = velocity.normalized * maxSpeed;
            }

            rigidbody.velocity = velocity;
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

            float totalInput = 0f;

            foreach (Vector2 input in State.PlayerInputs.Values)
            {
                totalInput += input.y;
            }

            return totalInput / State.PlayerCount;
        }

        private void OnDrawGizmosSelected()
        {
            RecalculatePositions();
            Gizmos.color = Color.yellow;

            foreach (Vector3 localPos in calculatedPositions.Values)
            {
                Vector3 worldPos = transform.TransformPoint(localPos);
                Gizmos.DrawWireSphere(worldPos, 0.3f);
            }
        }
    }
}

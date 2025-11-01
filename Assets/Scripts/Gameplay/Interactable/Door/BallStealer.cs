using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.Command;
using Gameplay.Interactable.BallInteraction.Components;
using Infrastructure.Network.Abstract;
using Infrastructure.Network.Components;
using Mirror;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.Interactable.Door
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(BallInteractionInitiator))]
    public class BallStealer : NetworkBehaviour
    {
        private enum State
        {
            Idle,
            ChasingBall,
            Stealing,
            EscapingWithBall,
            ThrowingBall,
            ReturningToDoor
        }

        [SerializeField]
        private Transform escapePoint;

        [SerializeField]
        private float ballCaptureDistance = 1.5f;

        [SerializeField]
        private float ballInteractionDistance = 2f;

        [SerializeField]
        private float reachDistance = 1f;

        private Door door;
        private NavMeshAgent navMeshAgent;
        private NetworkStateHelper networkStateHelper;
        private BallInteractionInitiator ballInteractionInitiator;
        private IBallInteractionMediator ballInteractionMediator;
        private INetworkService networkService;

        private CancellationTokenSource aiTokenSource;

        private State currentState = State.Idle;

        [Inject]
        private void Construct(IBallInteractionMediator ballInteractionMediator, INetworkService networkService)
        {
            this.ballInteractionMediator = ballInteractionMediator;
            this.networkService = networkService;
        }

        public void Activate(Door door)
        {
            this.door = door;
            networkStateHelper.CmdSetActive(true);
            navMeshAgent.transform.position = door.CharacterSpawnPoint.position;
        }

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            networkStateHelper = GetComponent<NetworkStateHelper>();
            ballInteractionInitiator = GetComponent<BallInteractionInitiator>();
        }

        private void OnEnable()
        {
            if (isServer)
            {
                StartAI();
            }
        }

        private void OnDisable()
        {
            if (isServer)
            {
                StopAI();
            }
        }

        private void StartAI()
        {
            if (ballInteractionMediator.Ball == null)
            {
                Debug.LogError("Cannot start AI: Ball not found in mediator!");
                return;
            }

            aiTokenSource?.Cancel();
            aiTokenSource = new CancellationTokenSource();

            RunAIAsync(aiTokenSource.Token).Forget();
        }

        private void StopAI()
        {
            aiTokenSource?.Cancel();
            aiTokenSource = null;
            currentState = State.Idle;

            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.ResetPath();
            }
        }

        private async UniTask RunAIAsync(CancellationToken token)
        {
            currentState = State.ChasingBall;

            while (!token.IsCancellationRequested && isActiveAndEnabled)
            {
                switch (currentState)
                {
                    case State.ChasingBall:
                        await ChaseBallAsync(token);
                        break;

                    case State.Stealing:
                        await StealBallAsync(token);
                        break;

                    case State.EscapingWithBall:
                        await EscapeWithBallAsync(token);
                        break;

                    case State.ThrowingBall:
                        await ThrowBallAsync(token);
                        break;

                    case State.ReturningToDoor:
                        await ReturnToDoorAsync(token);
                        break;
                }

                await UniTask.Yield();
            }
        }

        private async UniTask ChaseBallAsync(CancellationToken token)
        {
            Ball ball = ballInteractionMediator.Ball;
            if (ball == null || token.IsCancellationRequested)
            {
                return;
            }

            navMeshAgent.SetDestination(ball.transform.position);

            while (!token.IsCancellationRequested && currentState == State.ChasingBall)
            {
                float distanceToBall = Vector3.Distance(transform.position, ball.transform.position);

                if (navMeshAgent.remainingDistance > ballCaptureDistance)
                {
                    navMeshAgent.SetDestination(ball.transform.position);
                }

                if (distanceToBall <= ballInteractionDistance)
                {
                    currentState = State.Stealing;
                    break;
                }

                await UniTask.Yield();
            }
        }

        private async UniTask StealBallAsync(CancellationToken token)
        {
            Ball ball = ballInteractionMediator.Ball;
            if (ball == null || token.IsCancellationRequested)
            {
                return;
            }

            float waitTime = 0f;
            float maxWaitTime = 2f;

            while (!token.IsCancellationRequested && waitTime < maxWaitTime)
            {
                float distanceToBall = Vector3.Distance(transform.position, ball.transform.position);

                if (distanceToBall <= ballInteractionDistance)
                {
                    networkService.SendCommand(new HoldCommand(ballInteractionInitiator.netId));

                    await UniTask.Delay(200, cancellationToken: token);

                    if (ball.StateHolder.GetState().HolderNetId == ballInteractionInitiator.netId)
                    {
                        currentState = State.EscapingWithBall;
                        return;
                    }
                }
                else
                {
                    currentState = State.ChasingBall;
                    return;
                }

                waitTime += Time.deltaTime;
                await UniTask.Yield();
            }

            currentState = State.ChasingBall;
        }

        private async UniTask EscapeWithBallAsync(CancellationToken token)
        {
            Ball ball = ballInteractionMediator.Ball;

            if (escapePoint == null || token.IsCancellationRequested)
            {
                currentState = State.ThrowingBall;
                return;
            }

            navMeshAgent.SetDestination(escapePoint.position);

            while (!token.IsCancellationRequested && currentState == State.EscapingWithBall)
            {
                if (ball.StateHolder.GetState().HolderNetId != ballInteractionInitiator.netId)
                {
                    currentState = State.ChasingBall;
                    return;
                }

                float distanceToEscape = Vector3.Distance(transform.position, escapePoint.position);

                if (distanceToEscape <= reachDistance)
                {
                    currentState = State.ThrowingBall;
                    return;
                }

                await UniTask.Yield();
            }
        }

        private async UniTask ThrowBallAsync(CancellationToken token)
        {
            Ball ball = ballInteractionMediator.Ball;
            if (ball == null || token.IsCancellationRequested)
            {
                return;
            }

            if (ball.StateHolder.GetState().HolderNetId == ballInteractionInitiator.netId)
            {
                Vector3 kickDirection = escapePoint != null
                    ? (escapePoint.position - transform.position).normalized
                    : transform.forward;

                networkService.SendCommand(new KickCommand(ballInteractionInitiator.netId, kickDirection));
            }

            await UniTask.Delay(500, cancellationToken: token);

            currentState = State.ReturningToDoor;
        }

        private async UniTask ReturnToDoorAsync(CancellationToken token)
        {
            if (door == null || door.CharacterSpawnPoint == null || token.IsCancellationRequested)
            {
                if (door != null)
                {
                    door.ReleasePatrolRpc();
                }
                gameObject.SetActive(false);
                return;
            }

            navMeshAgent.SetDestination(door.CharacterSpawnPoint.position);

            while (!token.IsCancellationRequested && currentState == State.ReturningToDoor)
            {
                if (navMeshAgent.pathPending)
                {
                    await UniTask.Yield();
                    continue;
                }

                float distanceToDoor = Vector3.Distance(transform.position, door.CharacterSpawnPoint.position);

                bool reachedDestination = distanceToDoor <= reachDistance;
                bool agentStopped = navMeshAgent.hasPath &&
                                   !navMeshAgent.pathPending &&
                                   navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance;

                if (reachedDestination || agentStopped)
                {
                    door.ReleasePatrolRpc();
                    networkStateHelper.CmdSetActive(false);
                    return;
                }

                await UniTask.Yield();
            }

            if (door != null)
            {
                door.ReleasePatrolRpc();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (escapePoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(escapePoint.position, reachDistance);
                Gizmos.DrawLine(transform.position, escapePoint.position);
            }

            if (door != null && door.CharacterSpawnPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(door.CharacterSpawnPoint.position, reachDistance);
            }

            if (ballInteractionMediator?.Ball != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(ballInteractionMediator.Ball.transform.position, ballInteractionDistance);
            }
        }
    }
}

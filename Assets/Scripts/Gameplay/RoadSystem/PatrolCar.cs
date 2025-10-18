using System;
using Cysharp.Threading.Tasks;
using Gameplay.Interactable.PipeInteraction.Abstract;
using Infrastructure;
using Infrastructure.DialogService;
using Infrastructure.DialogService.Abstract;
using Infrastructure.DialogService.Command;
using Infrastructure.Network.Abstract;
using Infrastructure.QuestService.Abstract;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.RoadSystem
{
    public class PatrolCar : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent<bool> OnIsDrivingChanged;

        [Header("Settings")]
        [SerializeField]
        private float radius = 10f;

        [SerializeField]
        private float angle = 45f;

        [SerializeField]
        private LayerMask detectionMask = ~0;

        [Header("References")]
        [SerializeField]
        private GameObject cameraTarget;

        [SerializeField]
        private DialogSequence dialogSequence;

        private IQuestService questService;
        private IDialogService dialogService;
        private INetworkService networkService;
        private IPipeInteractionMediator pipeInteractionMediator;
        private IDisposable disposable;
        private Collider[] hits;
        private PatrolSystem patrolSystem;

        [Inject]
        private void Construct(
            IQuestService questService,
            IDialogService dialogService,
            INetworkService networkService,
            IPipeInteractionMediator pipeInteractionMediator
        )
        {
            this.questService = questService;
            this.dialogService = dialogService;
            this.networkService = networkService;
            this.pipeInteractionMediator = pipeInteractionMediator;
        }

        public void SetSystem(PatrolSystem system)
        {
            patrolSystem = system;
        }

        public void SetIsDriving(bool value)
        {
            OnIsDrivingChanged.Invoke(value);
        }

        private void Awake()
        {
            hits = new Collider[10];
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(GlobalParams.PlayerTag))
            {
                PlayDialogAndRestart();
            }
        }

        private void FixedUpdate()
        {
            for (var i = 0; i < hits.Length; i++)
            {
                hits[i] = null;
            }

            int count = Physics.OverlapSphereNonAlloc(transform.position, radius, hits, detectionMask);
            if (count == 0)
            {
                return;
            }

            Vector3 forward = transform.forward;

            for (int i = 0; i < count; i++)
            {
                Collider hit = hits[i];

                if (hit == null || !hit.CompareTag(GlobalParams.PlayerTag))
                {
                    continue;
                }

                Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
                float currentAngle = Vector3.Angle(forward, dirToTarget);

                if (currentAngle < angle * 0.5f)
                {
                    PlayDialogAndRestart();
                    return;
                }
            }
        }

        private void PlayDialogAndRestart()
        {
            patrolSystem.SetPatrolActive(false);

            disposable?.Dispose();
            cameraTarget.SetActive(true);
            dialogService.StartDialog(0, dialogSequence.Id);
            disposable = networkService.ObserveToReact<StopDialogCommand>(Restart);
        }

        private void Restart(StopDialogCommand _)
        {
            disposable?.Dispose();
            cameraTarget.SetActive(false);
            questService.RestartQuest();

            UniTask.Void(async () =>
            {
                await UniTask.WaitForSeconds(3);
                patrolSystem.SetPatrolActive(true);
            });
        }

        private void OnDisable()
        {
            disposable?.Dispose();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, radius);

            Vector3 forward = transform.forward;
            Quaternion leftRayRotation = Quaternion.Euler(0, -angle / 2, 0);
            Quaternion rightRayRotation = Quaternion.Euler(0, angle / 2, 0);
            Vector3 leftRayDirection = leftRayRotation * forward;
            Vector3 rightRayDirection = rightRayRotation * forward;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + leftRayDirection * radius);
            Gizmos.DrawLine(transform.position, transform.position + rightRayDirection * radius);

#if UNITY_EDITOR
            UnityEditor.Handles.color = new Color(1f, 1f, 0f, 0.15f);
            UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up, leftRayDirection, angle, radius);
#endif
        }
    }
}

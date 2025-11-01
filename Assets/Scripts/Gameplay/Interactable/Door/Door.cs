using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Interactable.BoxesInteraction.Components;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Interactable.Door
{
    public class Door : NetworkBehaviour
    {
        [SerializeField]
        private UnityEvent OnKnock;

        [SerializeField]
        private Transform doorTransform;

        [SerializeField]
        private BoxesStorage boxesStorage;

        [SerializeField]
        private int requiredBoxesToBlock = 3;

        [SerializeField]
        private GameObject patrolCharacter;

        [SerializeField]
        private Transform characterSpawnPoint;

        [SerializeField]
        private float knockDuration = 0.3f;

        [SerializeField]
        private float knockStrength = 0.1f;

        [SerializeField]
        private int knocksBeforeOpen = 3;

        private int currentKnocksCount;
        private bool isOpening;
        private bool isPatrolActive;
        private Vector3 initialPosition;
        private Quaternion initialRotation;

        public bool IsBlocked => boxesStorage != null && boxesStorage.Boxes.Count >= requiredBoxesToBlock;
        public bool IsPatrolActive => isPatrolActive;
        public Transform CharacterSpawnPoint => characterSpawnPoint;
        public int KnocksBeforeOpen => knocksBeforeOpen;

        private void Awake()
        {
            if (doorTransform == null)
            {
                doorTransform = transform;
            }

            initialPosition = doorTransform.localPosition;
            initialRotation = doorTransform.localRotation;
            currentKnocksCount = 0;

            if (patrolCharacter != null)
            {
                patrolCharacter.SetActive(false);
            }
        }

        [ClientRpc]
        public void KnockRpc()
        {
            if (isOpening || isPatrolActive)
            {
                return;
            }

            currentKnocksCount++;

            if (currentKnocksCount > knocksBeforeOpen)
            {
                TryOpenAsync().Forget();
                return;
            }

            doorTransform.DOKill();
            Sequence knockSequence = DOTween.Sequence();
            float stepDuration = knockDuration / 2f;
            Vector3 knockPosition = initialPosition + transform.right * knockStrength;
            knockSequence.AppendCallback(OnKnock.Invoke);
            knockSequence.Append(doorTransform.DOLocalMove(knockPosition, stepDuration));
            knockSequence.Append(doorTransform.DOLocalMove(initialPosition, stepDuration));
            knockSequence.AppendCallback(OnKnock.Invoke);
            knockSequence.Append(doorTransform.DOLocalMove(knockPosition, stepDuration));
            knockSequence.Append(doorTransform.DOLocalMove(initialPosition, stepDuration));
            knockSequence.AppendCallback(OnKnock.Invoke);
            knockSequence.Append(doorTransform.DOLocalMove(knockPosition, stepDuration));
            knockSequence.Append(doorTransform.DOLocalMove(initialPosition, stepDuration));
        }

        private async UniTask TryOpenAsync()
        {
            if (isOpening)
            {
                return;
            }

            isOpening = true;
            currentKnocksCount = 0;

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            if (IsBlocked)
            {
                isOpening = false;
            }
            else
            {
                transform.DOLocalRotateQuaternion(Quaternion.AngleAxis(90, Vector3.up) * initialRotation, 0.5f);
                if (patrolCharacter != null)
                {
                    isPatrolActive = true;
                    patrolCharacter.GetComponent<BallStealer>().Activate(this);
                }

                isOpening = false;
            }
        }

        [ClientRpc]
        public void ReleasePatrolRpc()
        {
            isPatrolActive = false;
            currentKnocksCount = 0;
            transform.DOLocalRotateQuaternion(initialRotation, 0.5f);
        }

        private void OnDestroy()
        {
            doorTransform?.DOKill();
        }
    }
}

using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.State;
using Mirror;
using UnityEngine;

namespace Gameplay.Interactable.BallInteraction.BallReaction
{
    public class DestructibleReactionInitiator : NetworkBehaviour, IBallReactionInitiator
    {
        [SerializeField]
        private int maxHitCount;

        [SyncVar(hook = nameof(OnHitPointsChanged))]
        private int currentHitCount;

        public override void OnStartServer()
        {
            ResetState();
        }

        bool IBallReactionInitiator.TryExecuteReaction(Collision collisionInfo, BallSharedState sharedState)
        {
            if (sharedState.KicksCount >= 3)
            {
                currentHitCount--;
            }

            return true;
        }

        private void ResetState()
        {
            currentHitCount = maxHitCount;
            gameObject.SetActive(true);
        }

        private void OnHitPointsChanged(int oldValue, int newValue)
        {
            transform.DOShakePosition(0.5f);
            if (newValue <= 0)
            {
                gameObject.SetActive(false);
                UniTask.Void(async () =>
                {
                    await UniTask.WaitForSeconds(5);
                    ResetState();
                });
            }
        }
    }
}

using DG.Tweening;
using UnityEngine;

namespace Gameplay.Interactable.BallInteraction.BallReaction
{
    public class TransformShake : MonoBehaviour
    {
        public void Shake()
        {
            transform.DOShakePosition(0.5f);
        }
    }
}

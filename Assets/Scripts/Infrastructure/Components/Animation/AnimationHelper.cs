using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.Components.Animation
{
    public class AnimationHelper : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private List<string> paramNames;

        private int currentParamIndex;

        public void SetCurrentParamIndex(int paramIndex)
        {
            currentParamIndex = paramIndex;
        }

        public void SetCurrentParam(float paramValue)
        {
            animator.SetFloat(paramNames[currentParamIndex], paramValue);
        }
    }
}

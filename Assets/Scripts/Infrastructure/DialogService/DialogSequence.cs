using System;
using UnityEngine;

namespace Infrastructure.DialogService
{
    [Serializable]
    [CreateAssetMenu(fileName = "DialogSequence", menuName = "Infrastructure/Dialog/Sequence")]
    public class DialogSequence : ScriptableObject
    {
        [field:SerializeField]
        public DialogReplica[] Replicas { get; private set; }

        public Guid Id { get; private set; }

        private void OnValidate()
        {
            if (Id == Guid.Empty)
            {
                Id = Guid.NewGuid();
            }
        }
    }
}

using System;
using UnityEngine;

namespace Infrastructure.DialogService
{
    [Serializable]
    [CreateAssetMenu(fileName = "DialogSequence", menuName = "Infrastructure/Dialog/Sequence")]
    public class DialogSequence : ScriptableObject
    {
        [SerializeField]
        private string guid;

        [field:SerializeField]
        public DialogReplica[] Replicas { get; private set; }

        public Guid Id => Guid.Parse(guid);

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(guid))
            {
                guid = Guid.NewGuid().ToString();
            }
        }

        public bool TryGetReplica(byte index, out DialogReplica dialogReplica)
        {
            dialogReplica = null;

            if (index >= Replicas.Length)
            {
                return false;
            }

            dialogReplica = Replicas[index];
            return true;
        }
    }
}

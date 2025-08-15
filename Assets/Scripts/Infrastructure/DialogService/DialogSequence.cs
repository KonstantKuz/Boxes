using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Infrastructure.DialogService
{
    [Serializable]
    [CreateAssetMenu(fileName = "DialogSequence", menuName = "Infrastructure/Dialog/Sequence")]
    public class DialogSequence : ScriptableObject
    {
        [SerializeField]
        private string guid;

        [SerializeField]
        private DialogReplica[] replicas;

        public Guid Id => Guid.Parse(guid);

        public DialogReplica[] Replicas => replicas;

        [Button]
        private void GenerateGuid()
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

using System;
using UnityEngine;
using UnityEngine.Localization;

namespace Infrastructure.DialogService
{
    [Serializable]
    public class DialogReplica
    {
        [field:SerializeField]
        public LocalizedString Title { get; private set; }

        [field:SerializeField]
        public LocalizedString Message { get; private set; }
    }
}

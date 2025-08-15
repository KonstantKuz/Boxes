using System;
using UnityEngine;
using UnityEngine.Localization;

namespace Infrastructure.DialogService
{
    [Serializable]
    public class DialogReplica
    {
        [SerializeField]
        private LocalizedString title;

        [SerializeField]
        private LocalizedString message;

        public LocalizedString Title => title;

        public LocalizedString Message => message;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Reflex.Attributes;
using UnityEngine;

namespace Infrastructure.DialogService
{
    public class DialogService : NetworkBehaviour, IDialogService
    {
        private Dictionary<Guid, DialogSequence> dialogsMap;

        [SyncVar]
        private DialogState currentState;

        public DialogState CurrentState => currentState;

        [Inject]
        private void Construct(DialogServiceInstaller installer)
        {
            dialogsMap = installer.Dialogs.ToDictionary(item => item.Id, item => item);
        }

        [Command(requiresAuthority = false)]
        void IDialogService.CmdStartDialog(Guid dialogId)
        {
            if (!dialogsMap.TryGetValue(dialogId, out DialogSequence dialogSequence))
            {
                Debug.LogError($"Dialog with ID {dialogId} not found.");
                return;
            }
        }

        [Command(requiresAuthority = false)]
        void IDialogService.CmdStopDialog()
        {
            throw new System.NotImplementedException();
        }

        [Command(requiresAuthority = false)]
        void IDialogService.CmdSetPlayerReady(int playerId)
        {
            throw new System.NotImplementedException();
        }
    }
}

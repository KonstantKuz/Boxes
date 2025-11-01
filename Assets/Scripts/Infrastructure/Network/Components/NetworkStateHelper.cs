using System;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Infrastructure.Network.Components
{
    public class NetworkStateHelper : NetworkBehaviour
    {
        [SerializeField]
        private UnityEvent<bool> onIsActiveChanged;

        [SerializeField]
        private bool initialIsActive;

        [SyncVar]
        [SerializeField]
        private Vector3 initialPosition;

        [SyncVar]
        [SerializeField]
        private Quaternion initialRotation;

        [SyncVar(hook = nameof(OnIsActiveChanged))]
        private bool isActive;

        protected override void OnValidate()
        {
            base.OnValidate();

            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        public override void OnStartServer()
        {
            this.Log(LogType.Log, $"{gameObject.name} OnStartServer currentPosition = {transform.position}" +
                                  $" initial is active = {initialIsActive} initialPosition = {initialPosition}");

            isActive = initialIsActive;
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }

        public override void OnStartClient()
        {
            OnIsActiveChanged(false, isActive);

            this.Log(LogType.Log, $"{gameObject.name} OnStartClient isActive = {isActive}");
        }

        [Command(requiresAuthority = false)]
        public void CmdSetActive(bool value)
        {
            isActive = value;

            this.Log(LogType.Log, $"{gameObject.name} CmdSetActive {value}");
        }

        public void ResetState()
        {
            if (!isServer)
            {
                return;
            }

            isActive = initialIsActive;
            transform.position = initialPosition;
            transform.rotation = initialRotation;

            this.Log(LogType.Log, $"{gameObject.name} ResetState");
        }

        private void OnIsActiveChanged(bool oldValue, bool newValue)
        {
            onIsActiveChanged.Invoke(newValue);

            this.Log(LogType.Log, $"{gameObject.name} OnIsActiveChanged {newValue}");
        }

        private void Update()
        {
            this.Log(LogType.Log, $"{gameObject.name} isActive = {isActive} currentPosition = {transform.position}");
        }
    }
}

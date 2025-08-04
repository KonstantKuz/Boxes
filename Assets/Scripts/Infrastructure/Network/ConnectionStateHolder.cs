namespace Infrastructure.Network
{
    public class ConnectionStateHolder : NetworkStateHolderBase
    {
        private void OnEnable()
        {
            NetworkService.RegisterStateHolder<ConnectionState>(this);
        }

        private void OnDisable()
        {
            NetworkService.UnregisterStateHolder<ConnectionState>(this);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            WriteState(ConnectionState.Default);
        }
    }
}

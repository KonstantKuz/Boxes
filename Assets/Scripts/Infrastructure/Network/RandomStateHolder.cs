using Reflex.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Infrastructure.Network
{
    public class RandomStateHolder : NetworkStateHolderBase
    {
        private INetworkService _networkService;

        [Inject]
        private void Construct(INetworkService networkService)
        {
            _networkService = networkService;
        }

        public override void OnStartServer()
        {
            TypeByteMapper.RegisterTypes<RandomState>();
            RandomState initialState = new RandomState { Value = Random.Range(100, 200)};
            byte[] state = _networkService.Serialize(initialState);
            ((INetworkStateHolder)this).WriteState(state);
        }

        public override void OnStartClient()
        {
            _networkService.RegisterStateHolder<RandomState>(this);

            _networkService.ObserveState<RandomState>(OnRandomStateChanged);
        }

        private void OnRandomStateChanged(RandomState state)
        {
            Debug.Log($"Random state changed : {state.Value}");
        }

        public override void OnStopClient()
        {
            _networkService.UnregisterStateHolder<RandomState>(this);
        }
    }
}

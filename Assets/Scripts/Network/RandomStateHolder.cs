using Infrastructure;
using Reflex.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Network
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
            byte type = TypeByteMapper.GetByteFromType(typeof(RandomState));
            _networkService.WriteState(type, state);
        }

        public override void OnStartClient()
        {
            _networkService.RegisterStateHolder<RandomState>(this);

            Debug.Log($"Random state : {_networkService.ReadState<RandomState>().Value}");
        }

        public override void OnStopClient()
        {
            _networkService.UnregisterStateHolder<RandomState>(this);
        }
    }
}

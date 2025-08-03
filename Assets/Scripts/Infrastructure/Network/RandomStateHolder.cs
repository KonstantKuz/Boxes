using UnityEngine;
using Random = UnityEngine.Random;

namespace Infrastructure.Network
{
    public class RandomStateHolder : NetworkStateHolderBase
    {
        public override void OnStartServer()
        {
            RandomState initialState = new RandomState { Value = Random.Range(100, 200)};
            WriteState(initialState);
        }

        public override void OnStartClient()
        {
            NetworkService.RegisterStateHolder<RandomState>(this);

            NetworkService.ObserveState<RandomState>(OnRandomStateChanged);
        }

        private void OnRandomStateChanged(RandomState state)
        {
            Debug.Log($"Random state changed : {state.Value}");
        }

        public override void OnStopClient()
        {
            NetworkService.UnregisterStateHolder<RandomState>(this);
        }
    }
}

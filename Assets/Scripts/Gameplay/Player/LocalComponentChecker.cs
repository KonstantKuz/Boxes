using System.Collections.Generic;
using System.Linq;
using Infrastructure.Network.Abstract;
using Infrastructure.Network.State;
using Mirror;
using R3;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Player
{
    public class LocalComponentChecker : NetworkBehaviour
    {
        [SerializeField]
        private SkinnedMeshRenderer renderer;

        [SerializeField]
        private List<Color> colors;

        [SerializeField]
        private List<Component> destroyComponents;

        [SerializeField]
        private List<GameObject> destroyGameObjects;

        private INetworkManager networkManager;

        [Inject]
        private void Construct(INetworkManager networkManager)
        {
            this.networkManager = networkManager;
        }

        private void Awake()
        {
            networkManager.ConnectionState.Subscribe(UpdateColor);
        }

        private void UpdateColor(ConnectionState state)
        {
            Dictionary<uint, int> indexById = state.Players
                .OrderBy(id => id)
                .Select((id, index) => (id, index))
                .ToDictionary(tuple => tuple.id, tuple => tuple.index);

            if (indexById.TryGetValue(netId, out int i) && colors.Count >= i)
            {
                renderer.materials[0].color = colors[i];
            }
        }

        public override void OnStartClient()
        {
            if (!isLocalPlayer)
            {
                foreach (Component localComponent in destroyComponents)
                {
                    Destroy(localComponent);
                }

                foreach (GameObject localComponent in destroyGameObjects)
                {
                    Destroy(localComponent);
                }
            }
        }
    }
}

using System;
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
        private List<GameObject> skins;

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

        private IDisposable connectionStateSubscription;

        private void Start()
        {
            InitializeSkinsNetworkAnimators();

            connectionStateSubscription = networkManager.ConnectionState.Subscribe(UpdateColor);

            if (netId != 0)
            {
                UpdateColor(networkManager.ConnectionState.CurrentValue);
            }
        }

        private void OnDestroy()
        {
            connectionStateSubscription?.Dispose();
        }

        private void InitializeSkinsNetworkAnimators()
        {
            for (int i = 0; i < skins.Count; i++)
            {
                NetworkAnimator networkAnimator = skins[i].GetComponent<NetworkAnimator>();
                if (networkAnimator != null)
                {
                    networkAnimator.enabled = skins[i].activeSelf;
                }
            }
        }

        private void UpdateColor(ConnectionState state)
        {
            Dictionary<uint, int> indexById = state.Players
                .OrderBy(id => id)
                .Select((id, index) => (id, index))
                .ToDictionary(tuple => tuple.id, tuple => tuple.index);

            if (indexById.TryGetValue(netId, out int colorId) && colors.Count >= colorId)
            {
                renderer.materials[0].color = colors[colorId];
            }

            if (indexById.TryGetValue(netId, out int skinId) && skins.Count >= skinId)
            {
                for (int i = 0; i < skins.Count; i++)
                {
                    bool isActiveSkin = i == skinId;

                    NetworkAnimator networkAnimator = skins[i].GetComponent<NetworkAnimator>();
                    if (networkAnimator != null)
                    {
                        networkAnimator.enabled = isActiveSkin;
                    }

                    skins[i].SetActive(isActiveSkin);
                }
            }
        }

        public override void OnStartClient()
        {
            if (!isOwned)
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

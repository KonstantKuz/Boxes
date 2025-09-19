using System.Collections.Generic;
using System.Linq;
using Infrastructure.Bootstrap;
using Infrastructure.Components;
using Infrastructure.Network.Abstract;
using Infrastructure.Network.State;
using Mirror;
using R3;
using Reflex.Attributes;
using Reflex.Extensions;
using Reflex.Injectors;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infrastructure.Network
{
    public class CustomNetworkManager : NetworkManager, IPostBuildInjectable, INetworkFactory, INetworkManager
    {
        private INetworkService networkService;
        private INetworkStateHolder<ConnectionState> connectionStateHolder;

        private ReactiveCommand<Unit>  localSpawnStream;
        private int spawnedObjectsCount;
        private int previousSpawnedObjectsCount;

        ReactiveCommand<Unit> INetworkFactory.LocalSpawnStream => localSpawnStream;
        public GameObject LocalPlayer => Spawned.GetValueOrDefault(NetworkClient.localPlayer?.netId ?? 0);

        public Dictionary<uint, GameObject> Spawned =>
            NetworkClient.spawned.ToDictionary(item => item.Key, item => item.Value.gameObject);

        bool INetworkManager.IsServer => NetworkServer.active;

        [Inject]
        private void Construct(INetworkService networkService, INetworkStateHolder<ConnectionState> connectionStateHolder)
        {
            this.networkService = networkService;
            this.connectionStateHolder = connectionStateHolder;

            localSpawnStream = new ReactiveCommand<Unit>();
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            Transform startPoint = GetStartPosition();
            Vector3 startPosition = startPoint?.position ?? Vector3.zero;
            Quaternion startRotation = startPoint?.rotation ?? Quaternion.identity;
            GameObject player = Spawn(playerPrefab, startPosition, startRotation);
            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, player);

            uint netId = player.GetComponent<NetworkIdentity>().netId;

            ConnectionState connectionState = connectionStateHolder.GetState();
            connectionState.Players?.Add(netId);
            connectionStateHolder.WriteState(connectionState);
        }

        protected override void RegisterClientMessages()
        {
            base.RegisterClientMessages();

            NetworkClient.UnregisterPrefab(playerPrefab);
            NetworkClient.RegisterPrefab(playerPrefab, SpawnHandler, UnspawnHandler);
        }

        private GameObject SpawnHandler(SpawnMessage msg)
        {
            if (!NetworkClient.GetPrefab(msg.assetId, out GameObject prefab))
            {
                return null;
            }

            return Spawn(prefab, msg.position, msg.rotation);
        }

        private void UnspawnHandler(GameObject spawned)
        {
            Destroy(spawned);
        }

        private GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            bool wasPrefabActive = prefab.activeSelf;

            if (wasPrefabActive)
            {
                prefab.gameObject.SetActive(false);
            }

            GameObject spawned = Instantiate(prefab, position, rotation);

            if (wasPrefabActive)
            {
                prefab.gameObject.SetActive(true);
            }

            if (spawned.TryGetComponent(out GameObjectScope gameObjectContext))
            {
                AttributeInjector.Inject(gameObjectContext, SceneManager.GetActiveScene().GetSceneContainer());
            }

            spawned.gameObject.SetActive(true);

            return spawned;
        }

        public override void Update()
        {
            base.Update();

            spawnedObjectsCount = NetworkClient.spawned.Count;

            if (previousSpawnedObjectsCount != spawnedObjectsCount)
            {
                localSpawnStream.Execute(Unit.Default);
                previousSpawnedObjectsCount = spawnedObjectsCount;
            }
        }

        // public override void OnClientSceneChanged()
        // {
        //     base.OnClientSceneChanged();
        //
        //     Cursor.visible = false;
        //     Cursor.lockState = CursorLockMode.Locked;
        // }
        //
        // public override void OnServerSceneChanged(string sceneName)
        // {
        //     base.OnServerSceneChanged(sceneName);
        //
        //     Cursor.visible = false;
        //     Cursor.lockState = CursorLockMode.Locked;
        //
        //     SpawnPlayers();
        // }
        //
        // private void SpawnPlayers()
        // {
        //     if (!NetworkServer.active)
        //     {
        //         return;
        //     }
        //
        //     NetworkClient.Ready();
        //
        //     foreach (NetworkConnectionToClient connection in NetworkServer.connections.Values)
        //     {
        //         if (connection != null && !connection.identity)
        //         {
        //             SpawnPlayerForConnection(connection);
        //         }
        //     }
        // }
        //
        // private void SpawnPlayerForConnection(NetworkConnectionToClient conn)
        // {
        //     GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        //     if (player.TryGetComponent(out GameObjectContext gameObjectContext))
        //     {
        //         AttributeInjector.Inject(gameObjectContext, SceneManager.GetActiveScene().GetSceneContainer());
        //     }
        //     NetworkServer.AddPlayerForConnection(conn, player);
        // }
        //
        // public void ChangeScene()
        // {
        //     if (NetworkServer.active)
        //     {
        //         ServerChangeScene("GameScene");
        //     }
        // }
    }
}

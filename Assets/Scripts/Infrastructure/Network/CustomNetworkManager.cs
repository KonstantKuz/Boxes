using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Bootstrap;
using Infrastructure.Components;
using Infrastructure.InputService.Abstract;
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
    public partial class CustomNetworkManager : NetworkManager, IPostBuildInjectable, INetworkFactory, INetworkManager, IInitializable, IDisposable
    {
        private INetworkStateHolder<ConnectionState> connectionStateHolder;
        private IInputService inputService;
        private ReactiveProperty<ConnectionState> stateReactive;
        private IDisposable stateSubscription;

        private ReactiveCommand<Unit>  localSpawnStream;
        private int spawnedObjectsCount;
        private int previousSpawnedObjectsCount;
        private Dictionary<uint, NetworkIdentity> players;

        ReactiveCommand<Unit> INetworkFactory.LocalSpawnStream => localSpawnStream;
        NetworkIdentity INetworkFactory.LocalPlayer => NetworkClient.localPlayer;
        Dictionary<uint, NetworkIdentity> INetworkFactory.Players => players;
        Dictionary<uint, NetworkIdentity> INetworkFactory.Spawned => NetworkClient.spawned;

        bool INetworkManager.IsServer => NetworkServer.active;
        bool INetworkManager.IsClientReady => NetworkClient.ready;
        ReadOnlyReactiveProperty<ConnectionState> INetworkManager.ConnectionState => stateReactive;

        [Inject]
        private void Construct(INetworkStateHolder<ConnectionState> connectionStateHolder, IInputService inputService)
        {
            this.connectionStateHolder = connectionStateHolder;
            this.inputService = inputService;

            localSpawnStream = new ReactiveCommand<Unit>();
            stateReactive = new ReactiveProperty<ConnectionState>(ConnectionState.Default);
            players = new Dictionary<uint, NetworkIdentity>();
        }

        void IInitializable.Initialize()
        {
            stateSubscription = connectionStateHolder.Subscribe(state => stateReactive.Value = state);
        }

        void IDisposable.Dispose()
        {
            stateReactive?.Dispose();
            localSpawnStream?.Dispose();
            stateSubscription?.Dispose();
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            Transform startPoint = GetStartPosition();
            Vector3 startPosition = startPoint?.position ?? Vector3.zero;
            Quaternion startRotation = startPoint?.rotation ?? Quaternion.identity;
            GameObject player = Spawn(playerPrefab, startPosition, startRotation);
            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, player);

            players.TryAdd(conn.identity.netId, conn.identity);

            ConnectionState connectionState = connectionStateHolder.GetState();
            connectionState.Players?.Add(conn.identity.netId);
            connectionStateHolder.WriteState(connectionState);

            Debug.Log($"OnServerAddPlayer: Registered player netId={conn.identity.netId}, Players={string.Join(",", connectionState.Players ?? new HashSet<uint>())}");
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

            spawned.gameObject.SetActive(wasPrefabActive);

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

        public void SpawnLocalPlayer()
        {
            if (!NetworkServer.active)
            {
                Debug.LogError("SpawnLocalPlayer: NetworkServer is not active");
                return;
            }

            Transform startPoint = GetStartPosition();
            Vector3 startPosition = startPoint?.position ?? Vector3.zero;
            Quaternion startRotation = startPoint?.rotation ?? Quaternion.identity;

            GameObject player = Spawn(playerPrefab, startPosition, startRotation);
            NetworkIdentity identity = player.GetComponent<NetworkIdentity>();

            NetworkServer.Spawn(player, NetworkServer.localConnection);

            uint netId = identity.netId;
            player.name = $"{playerPrefab.name} [Local Player {netId}]";

            players.TryAdd(netId, identity);

            ConnectionState connectionState = connectionStateHolder.GetState();
            connectionState.Players?.Add(netId);
            connectionStateHolder.WriteState(connectionState);

            Debug.Log($"SpawnLocalPlayer: Registered player netId={netId}, Players={string.Join(",", connectionState.Players ?? new HashSet<uint>())}");
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

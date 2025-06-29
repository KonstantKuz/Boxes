using Infrastructure;
using Mirror;
using Reflex.Extensions;
using Reflex.Injectors;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class CustomNetworkManager : NetworkManager
    {
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            GameObject player = Spawn(playerPrefab, Vector3.zero, Quaternion.identity);
            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, player);
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
            GameObject spawned = Instantiate(prefab, position, rotation);

            if (spawned.TryGetComponent(out GameObjectContext gameObjectContext))
            {
                AttributeInjector.Inject(gameObjectContext, SceneManager.GetActiveScene().GetSceneContainer());
            }

            return spawned;
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

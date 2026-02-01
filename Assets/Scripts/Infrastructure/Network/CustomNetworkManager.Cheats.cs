#if DEBUG
using Infrastructure.Cheats;
using UnityEngine;

namespace Infrastructure.Network
{
    public partial class CustomNetworkManager : ICheatsProvider
    {
        private const int MaxLocalPlayers = 4;

        bool ICheatsProvider.IsOpen { get; set; }

        string ICheatsProvider.GetLabel() => "Local Multiplayer";

        void ICheatsProvider.RenderCheats()
        {
            State.ConnectionState state = connectionStateHolder.GetState();
            int currentPlayers = state.Players?.Count ?? 0;

            GUILayout.Label($"Local players: {currentPlayers}/{MaxLocalPlayers}");

            if (currentPlayers < MaxLocalPlayers && GUILayout.Button("Spawn Local Player"))
            {
                SpawnLocalPlayer();
            }
        }
    }
}
#endif

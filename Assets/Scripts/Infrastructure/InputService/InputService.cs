using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Bootstrap;
using Infrastructure.InputService.Abstract;
using Infrastructure.Network.Abstract;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Infrastructure.InputService
{
    [Serializable]
    public class InputService : IInputService, IInitializable, IDisposable
    {
        private GameInput globalInput;
        private Dictionary<uint, GameInput> localInputs = new();
        private Dictionary<uint, InputUser> localInputUsers = new();
        private Dictionary<uint, InputDevice> reservedDevices = new();
        private HashSet<uint> localPlayerIds = new();
        private INetworkManager networkManager;

        [Inject]
        private void Construct(INetworkManager networkManager)
        {
            Debug.Log($"InputService.Construct: networkManager={networkManager != null}");
            this.networkManager = networkManager;
        }

        void IInitializable.Initialize()
        {
            Debug.Log($"InputService.Initialize: networkManager={networkManager != null}");
            globalInput = new GameInput();
            globalInput.Enable();
            Debug.Log($"InputService.Initialize: globalInput created and enabled");
        }

        GameInputActions IInputService.GetInput(uint playerId)
        {
            if (globalInput == null || networkManager == null)
            {
                Debug.LogWarning($"InputService.GetInput({playerId}): globalInput or networkManager is null");
                return null;
            }

            localPlayerIds.Add(playerId);

            Debug.Log($"InputService.GetInput({playerId}): localPlayerIds.Count={localPlayerIds.Count}, localInputs.Count={localInputs.Count}");

            if (!localInputs.ContainsKey(playerId))
            {
                Debug.Log($"InputService.GetInput({playerId}): Creating local input for player {playerId}");
                CreateLocalInput(playerId);
            }

            bool hasLocalInput = localInputs.TryGetValue(playerId, out GameInput localInput);
            Debug.Log($"InputService.GetInput({playerId}): hasLocalInput={hasLocalInput}, returning {(hasLocalInput ? "localInput" : "globalInput")}");

            return hasLocalInput
                ? new GameInputActions(localInput)
                : new GameInputActions(globalInput);
        }

        private void CreateLocalInput(uint playerId)
        {
            if (localInputs.ContainsKey(playerId))
            {
                Debug.LogWarning($"CreateLocalCoopPlayerInput: Player {playerId} already has local input");
                return;
            }

            InputDevice[] devices = GetDevices(playerId);
            if (devices == null || devices.Length == 0)
            {
                Debug.LogError($"CreateLocalCoopPlayerInput: No available devices for player {playerId}");
                return;
            }

            Debug.Log($"CreateLocalCoopPlayerInput: Creating input for player {playerId} with devices: {string.Join(", ", devices.Select(d => d.name))}");

            GameInput playerInput = new GameInput();

            InputUser inputUser = InputUser.PerformPairingWithDevice(devices[0]);
            Debug.Log($"CreateLocalCoopPlayerInput: Created InputUser {inputUser.id} paired with {devices[0].name}");

            for (int i = 1; i < devices.Length; i++)
            {
                InputUser.PerformPairingWithDevice(devices[i], user: inputUser);
                Debug.Log($"CreateLocalCoopPlayerInput: Paired {devices[i].name} to InputUser {inputUser.id}");
            }

            inputUser.AssociateActionsWithUser(playerInput);
            Debug.Log($"CreateLocalCoopPlayerInput: Associated actions with InputUser {inputUser.id}");

            playerInput.Enable();

            localInputs[playerId] = playerInput;
            localInputUsers[playerId] = inputUser;
            reservedDevices[playerId] = devices[0];

            Debug.Log($"CreateLocalCoopPlayerInput: Completed for player {playerId}, InputUser {inputUser.id}");
        }

        private InputDevice[] GetDevices(uint playerId)
        {
            List<uint> sortedLocalPlayers = localPlayerIds.OrderBy(id => id).ToList();

            Debug.Log($"GetDevicesForPlayer({playerId}): sortedLocalPlayers=[{string.Join(",", sortedLocalPlayers)}]");

            if (sortedLocalPlayers.Count == 0)
            {
                Debug.LogError($"GetDevicesForPlayer({playerId}): sortedLocalPlayers is empty");
                return null;
            }

            int playerIndex = sortedLocalPlayers.IndexOf(playerId);
            Debug.Log($"GetDevicesForPlayer({playerId}): playerIndex={playerIndex}");

            if (playerIndex < 0)
            {
                Debug.LogError($"GetDevicesForPlayer({playerId}): playerId not found in sortedLocalPlayers");
                return null;
            }

            if (playerIndex == 0 && Keyboard.current != null && Mouse.current != null)
            {
                Debug.Log($"GetDevicesForPlayer({playerId}): Assigning Keyboard+Mouse (playerIndex=0)");
                return new InputDevice[] { Keyboard.current, Mouse.current };
            }

            int gamepadIndex = playerIndex - 1;
            Debug.Log($"GetDevicesForPlayer({playerId}): gamepadIndex={gamepadIndex}, Gamepad.all.Count={Gamepad.all.Count}");

            if (gamepadIndex >= 0 && gamepadIndex < Gamepad.all.Count)
            {
                Debug.Log($"GetDevicesForPlayer({playerId}): Assigning Gamepad[{gamepadIndex}] = {Gamepad.all[gamepadIndex].name}");
                return new InputDevice[] { Gamepad.all[gamepadIndex] };
            }

            Debug.LogError($"GetDevicesForPlayer({playerId}): No device available (gamepadIndex={gamepadIndex} out of range)");
            return null;
        }

        void IInputService.SwitchToDefaultContext()
        {
            globalInput.DialogContext.Disable();
            globalInput.DefaultContext.Enable();

            foreach (GameInput playerInput in localInputs.Values)
            {
                playerInput.DialogContext.Disable();
                playerInput.DefaultContext.Enable();
            }
        }

        void IInputService.SwitchToDialogContext()
        {
            globalInput.DefaultContext.Disable();
            globalInput.DialogContext.Enable();

            foreach (GameInput playerInput in localInputs.Values)
            {
                playerInput.DefaultContext.Disable();
                playerInput.DialogContext.Enable();
            }
        }

        void IInputService.DisableAllInputs()
        {
            globalInput.DefaultContext.Disable();
            globalInput.DialogContext.Disable();

            foreach (GameInput playerInput in localInputs.Values)
            {
                playerInput.DefaultContext.Disable();
                playerInput.DialogContext.Disable();
            }
        }

        void IInputService.EnableAllInputs()
        {
            globalInput.DefaultContext.Enable();
            globalInput.DialogContext.Enable();

            foreach (GameInput playerInput in localInputs.Values)
            {
                playerInput.DefaultContext.Enable();
                playerInput.DialogContext.Enable();
            }
        }

        public void Dispose()
        {
            globalInput?.Dispose();

            foreach (KeyValuePair<uint, InputUser> kvp in localInputUsers)
            {
                kvp.Value.UnpairDevicesAndRemoveUser();
            }

            foreach (GameInput playerInput in localInputs.Values)
            {
                playerInput?.Dispose();
            }

            localInputs.Clear();
            localInputUsers.Clear();
            reservedDevices.Clear();
            localPlayerIds.Clear();
        }
    }
}

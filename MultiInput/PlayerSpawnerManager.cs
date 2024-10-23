using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

// Combination of input manager and spawn manager
public class PlayerSpawnerManager : GenericSpawnManager<PlayerSpawnerManager.PlayerTypeEnum>
{
    public enum PlayerTypeEnum { PlayerType1 }

    [SerializeField] InputActionAsset ActionAsset;
    [SerializeField] PlayerTypeEnum PlayerType;
    [SerializeField] Transform DefaultSpawnLocation;
    [SerializeField] int PlayerLimitCount = 3;
    [SerializeField] bool AllowNewJoin = true;
    [SerializeField] bool AllowKeyboard;

    Dictionary<InputDevice, MultiInputSystem> deviceRegistry = new();
    Dictionary<MultiInputSystem, IControlBinder> playerRegistry = new();

    event Action<bool> OnAllowNewPlayers;
    public void SubOnAllowNewPlayers(Action<bool> sub) => OnAllowNewPlayers += sub;
    public void UnsubOnAllowNewPlayers(Action<bool> unsub) => OnAllowNewPlayers -= unsub;
    public void OnAllowNewPlayersEvent(bool cast) => OnAllowNewPlayers?.Invoke(cast); 

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        SceneManager.sceneLoaded += OnSceneLoaded;

        SubOnAllowNewPlayers(OnAllowNewJoinChange);
        KeepSingleton(true);

        CheckForExistingDevices();
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
        SceneManager.sceneLoaded -= OnSceneLoaded;

        UnsubOnAllowNewPlayers(OnAllowNewJoinChange);
    }

    void OnAllowNewJoinChange(bool change) => AllowNewJoin = change;

    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added)
        {
            Debug.Log($"Device added: {device.displayName}");
            TryRegisterDevice(device);
        }
        else if (change == InputDeviceChange.Removed)
        {
            Debug.Log($"Device removed: {device.displayName}");
            UnregisterDevice(device);
        }
    }

    void CheckForExistingDevices()
    {
        foreach (var device in InputSystem.devices)
        {
            TryRegisterDevice(device);
        }
    }

    void TryRegisterDevice(InputDevice device)
    {
        if (AllowNewJoin)
        {
            if (device is Gamepad || (device is Keyboard && AllowKeyboard))
            {
                if (!deviceRegistry.ContainsKey(device))
                {
                    if (playerRegistry.Count < PlayerLimitCount)
                    {
                        GameObject playerObj = SpawnObject(PlayerType, DefaultSpawnLocation.position, Quaternion.identity);
                        if (playerObj != null)
                        {
                            IControlBinder controlBinder = playerObj.GetComponent<IControlBinder>();

                            if (controlBinder != null)
                                RegisterPlayer(controlBinder, device);
                            else
                                Debug.LogError("Spawned object does not have a component that implements IControlBinder.");
                        }
                    }
                    else
                        Debug.Log("Player limit reached. Cannot register new player.");
                }
            }
        }
    }

    void RegisterPlayer(IControlBinder playerBinder, InputDevice device)
    {
        ControlSchemeEnum controlScheme = UtilityMethods.GetDeviceType(device);
        MultiInputSystem registry = new MultiInputSystem(device, ActionAsset, controlScheme);

        deviceRegistry.Add(device, registry);
        playerRegistry.Add(registry, playerBinder);

        registry.BindObject(playerBinder);
    }

    void UnregisterDevice(InputDevice device)
    {
        if (deviceRegistry.TryGetValue(device, out MultiInputSystem registry))
        {
            if (playerRegistry.TryGetValue(registry, out IControlBinder playerBinder))
            {
                registry.UnbindObject();

                MonoBehaviour binderBehaviour = playerBinder as MonoBehaviour;
                if (binderBehaviour != null)
                {
                    DespawnObject(binderBehaviour.gameObject);
                    playerRegistry.Remove(registry);
                    deviceRegistry.Remove(device);

                    Debug.Log($"Unregistered player {playerBinder.GetType().Name} and removed device {device.displayName}");
                }
                else
                    Debug.LogError($"{playerBinder.GetType().Name} is not a MonoBehaviour. Cannot despawn object.");
            }
        }
    }

    void UnregisterAll()
    {
        foreach (var registry in playerRegistry.Keys)
            registry.UnbindObject();

        foreach (var playerBinder in playerRegistry.Values)
        {
            MonoBehaviour binderBehaviour = playerBinder as MonoBehaviour;
            if (binderBehaviour != null)
                DespawnObject(binderBehaviour.gameObject);
        }

        playerRegistry.Clear();
        deviceRegistry.Clear();

        Debug.Log("All players have been unregistered.");
    }

    public void ResetGame()
    {
        UnregisterAll();
        CheckForExistingDevices();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        ReinstantiatePlayers();
    }

    void ReinstantiatePlayers()
    {
        List<MultiInputSystem> registries = new List<MultiInputSystem>(playerRegistry.Keys);
        playerRegistry.Clear();

        foreach (var registry in registries)
        {
            GameObject playerObj = SpawnObject(PlayerTypeEnum.PlayerType1, Vector3.zero, Quaternion.identity);
            if (playerObj != null)
            {
                IControlBinder controlBinder = playerObj.GetComponent<IControlBinder>();
                if (controlBinder != null)
                {
                    registry.BindObject(controlBinder);
                    playerRegistry.Add(registry, controlBinder);

                    Debug.Log($"Reinstantiated player {controlBinder.GetType().Name} and bound to device {registry.Device.displayName}");
                }
                else
                    Debug.LogError("Spawned object does not have a component that implements IControlBinder.");
            }
            else
                Debug.LogError("Failed to spawn player object.");
        }
    }
}

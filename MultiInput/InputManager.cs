using System.Collections.Generic;

#if ENABLE_INPUT_SYSTEM

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

// For objects that already existed
// Rest of code look for "PlayerAController"
public class InputManager : GenericSingleton<InputManager>
{
    [SerializeField] InputActionAsset ActionAsset;
    [SerializeField] int PlayerLimitCount = 3;
    [SerializeField] bool AllowNewJoin = true;
    [SerializeField] bool AllowKeyboard;

    public Dictionary<InputDevice, MultiInputSystem> DeviceRegistry = new();
    public Dictionary<MultiInputSystem, IControlBinder> PlayerRegistry = new();

    public bool GetAllowKeyboard() => AllowKeyboard;

    public bool RegisterPlayer(IControlBinder playerObject, InputDevice device)
    {
        if (!AllowNewJoin)
        {
            Debug.LogWarning("New players are not allowed to join at this time.");
            return false;
        }

        if (PlayerRegistry.Count > PlayerLimitCount)
        {
            Debug.LogWarning("Player limit reached. Cannot register new player.");
            return false;
        }

        if (DeviceRegistry.ContainsKey(device))
        {
            Debug.LogWarning($"{device.displayName} is already registered.");
            return false;
        }

        ControlSchemeEnum controlScheme = UtilityMethods.GetDeviceType(device);
        MultiInputSystem registry = new MultiInputSystem(device, ActionAsset, controlScheme);

        DeviceRegistry.Add(device, registry);
        PlayerRegistry.Add(registry, playerObject);

        registry.BindObject(playerObject);
        Debug.Log($"Current player count: {PlayerRegistry.Count}");
        return true;
    }

    public void UnregisterPlayer(IControlBinder playerObject)
    {
        MultiInputSystem registry = null;

        foreach (var entry in PlayerRegistry)
        {
            if (entry.Value == playerObject)
            {
                registry = entry.Key;
                break;
            }
        }

        if (registry != null)
        {
            registry.UnbindObject();
            PlayerRegistry.Remove(registry);
            DeviceRegistry.Remove(registry.Device);
            Debug.Log($"Player with device {registry.Device} has been unregistered.");
        }
        else
            Debug.LogWarning("Attempted to unregister a player that is not registered.");
    }

    public void UnregisterAll()
    {
        foreach (var entry in PlayerRegistry)
            entry.Key.UnbindObject();

        PlayerRegistry.Clear();
        DeviceRegistry.Clear();

        Debug.Log("All players have been unregistered.");
    }

}

#endif
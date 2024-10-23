using UnityEngine;

#if ENABLE_INPUT_SYSTEM

using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem;
using System;

public class MultiInputSystem
{
    public MultiInputSystem(InputDevice device, InputActionAsset asset, ControlSchemeEnum controltype)
    {
        User = InputUser.PerformPairingWithDevice(device);
        Device = device;
        Asset = UnityEngine.Object.Instantiate(asset);
        _controlScheme = controltype.GetControlType();
    }

    public InputUser User;
    public InputDevice Device;
    public InputActionAsset Asset;

    string _controlScheme;
    InputActionMap ActionMap;
    IControlBinder _binder;

    public event Action<InputDevice, InputDeviceChange> OnBindObject;
    public event Action<InputDevice, InputDeviceChange> OnUnbindObject;

    public void EnableInput() => ActionMap.Enable();
    public void DisableInput() => ActionMap.Disable();

    public void BindObject<T>(T bindobject)
        where T : IControlBinder
    {
        if (_binder != null)
            UnbindObject();

        _binder = bindobject;
        _binder.Registry = this;

        string actionmapname = UtilityMethods.InterfaceToStringName(bindobject.InputInterface, "Actions", string.Empty);
        Debug.Log($"Finding action map name of {actionmapname}");
        ActionMap = Asset.FindActionMap(actionmapname);

        if (ActionMap == null)
        {
            Debug.LogError($"InputActionMap '{actionmapname}' not found in the InputActionAsset.");
            return;
        }

        User.AssociateActionsWithUser(ActionMap);
        User.ActivateControlScheme(_controlScheme);

        UtilityMethods.BindPlayerAction(_binder, ActionMap);
        EnableInput();

        OnBindObject?.Invoke(Device, InputDeviceChange.Added);
        _binder.OnBind();
    }

    public void UnbindObject()
    {
        _binder.Registry = null;
        _binder = null;

        foreach(var action in ActionMap)
            action.Reset();

        OnUnbindObject?.Invoke(Device, InputDeviceChange.Removed);
        DisableInput();
    }
}

public interface IControlBinder 
{
    public Type InputInterface { get; }
    MultiInputSystem Registry { get; set; }
    void OnBind();
}

#endif
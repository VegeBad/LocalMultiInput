How to use?

1. On Unity, click on your preferred Input Action Asset and tick "Generate C# Class".
2. This should auto generate a new script of the said Input Action.
3. If you open the generated C# class, and scroll to the very bottom, you'll see an interface
4. On your player object script, insert and implement the interface of the input action class. Usually goes with name "Name of input action asset".I"Name of action map"Action
5. Still your player object script, Insert and implement the interface "IControlBinder"
6. For Registry you can leave it as it is, same as OnBind(). OnBind() will only be called when the binding is successful, you can insert anything you want.
7. IMPORTANT: For InputInterface put {public Type InputInterface => typeof("The interface mentioned in No.4");}
8. Set your player object as a prefab and on inspector create a gameobject with the script "PlayerSpawnManager", drag this prefab to Serialized/Element 0/Prefab
9. Cause the spawn manager is universal throughout my other projects if don't inherit from it go ahead and set up your own spawning code
10. The InputManager is for player object that already exist in the scene but lacking OnDeviceChange() and TryRegisterDevice() found in PlayerSpawnManager, which those functions have to put on the player object script with a bit of modifications

# Lighting & Post Processing

## Lighting
To light your scene, you must add a single Directional Light to the `GameContent` scene.

The light *must* be set to Realtime mode, but the rest of the settings are up to you to customize.

If you want to use the default directional light so that it aligns with the skybox, you can use the `Directional Light` prefab in the `Mapify/Prefabs/Misc` folder.
Once you've added it to the scene, ensure it's position is at 0, 0, 0 and leave the rotation as set by the prefab.


## Post Processing

### Importing The Package
To add custom post-processing effects to your scene, you'll need to import the `Post Processing` package from the package manager.
You can open the package manager from `Window > Package Manager`, then select `Unity Registry` in the top left, and search for `Post Processing`.
Once you find it, simply click "Install" at the bottom right.

### Setting Up The Global Volume
In the `GameContent` scene, create a new object called `[GlobalPostProcessing]`.
On this object, add the `Post-process Volume` component, and tick the `Is Global` checkbox.

To a profile, click the "New" button to the far right of the `Profile` field.
In the Overrides section, you can now add the effects you want.

Note that Ambient Occlusion, Bloom, and Auto Exposure are set by the game and cannot be customized.

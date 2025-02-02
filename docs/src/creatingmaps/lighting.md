# Lighting & Post Processing

## Lighting
Lighting your scene is now controlled by Derail Valley's weather system.
For an approximate representation of how your scene will look in-game, you can add the `Directional Light` prefab in the `Mapify/Prefabs/Misc` folder to your `GameContent` scene.

**Note:** When Mapify builds your map, ^^all^^ Directional Lights will be ^^disabled^^.


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

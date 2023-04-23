# Terrain

Derail Valley makes use of terrain streaming to dynamically load in terrain that's around the player.
Due to this, you have to follow some rules when making terrain.

- Terrain can *only* go in the terrain scene.
- All terrains must have unique TerrainData.
- All terrains must use the same material.
- All terrains must be the same size.
- All terrains must be a square.
- All terrains must have the same Pixel Error*.
- All terrains must have the same Basemap Distance*.
- All terrains must have the same Draw Instanced state*.

&#42; If each piece of terrain has a different setting, only the setting from the first one in the hierarchy will be used.

## Creating Terrain

The easiest way to create terrain is using the [Terrain Tools Package][terrain-tools-package].
It allows you to easily create huge terrains, automatically split up into smaller chunks, load heightmaps, and adds new tools for modifying terrain.

For more information on creating terrains and using Terrain Tools, check out [this][brackeys-terrain-tutorial] great video from Brackeys.

## Terrain Size

In the base game, each terrain chunk is 250m x 250m.
You should try to keep each chunk of your terrain around the same size.

## Terrain Shape & Position

Due to the way Derail Valley handles maps, your map ***must*** be a perfect square, originating from 0, 0 expanding positively on the X and Z axis.

## Streaming Settings

You can customize how terrain is streamed in around the player in the "Terrain Streaming" section of the MapInfo asset.

`Terrain Loading Ring Size` is the amount of terrain assets to load around the player.
The value of this highly depends on the size of your terrain assets.
The smaller each terrain piece, the larger this should be.

[terrain-tools-package]: https://docs.unity3d.com/Packages/com.unity.terrain-tools@4.0/manual/index.html
[brackeys-terrain-tutorial]: https://youtu.be/MWQv2Bagwgk

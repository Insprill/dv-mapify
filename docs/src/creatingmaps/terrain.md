# Terrain

## Terrain Tools

The easiest way to create terrain is using the [Terrain Tools Package][terrain-tools-package].
It allows you to easily create huge terrains, automatically split them into smaller chunks, load heightmaps, and it adds new tools for modifying terrain.
Throughout this page, the assumption is made that you are using Terrain Tools.

For more information on creating terrains and using Terrain Tools, check out this great video from Brackeys.

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/MWQv2Bagwgk" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>


## Creating Terrain

After installing Terrain Tools (see `0:45` of the video above), you can create your terrain.

Open the Terrain scene, and set it as the active scene if you have multiple open.
You can do this by right-clicking the scene in the hierarchy and clicking "Set Active Scene."
If you can't click it, then it's already the active scene.

Now, open the Terrain Toolbox window by going to `Window > Terrain > Terrain Toolbox`.

The first three settings control the terrain's total width, length, and height.
This will be the total size of your map.
The width and length ***must*** be set to the same value!
Note that you can always change these sizes later.

The fourth setting, Start Position, ***must*** have an X, Y, and Z of `0`.

The fifth & sixth settings control how many tiles there are along each axis.
These ***must*** be set to the same value!
At the bottom of the window, there is a label called "Tile Size".
This shows you how large each tile is.
On the default Derail Valley map, each tile is 250x250.
Unless a different size is specifically required, you should set the tile count such that each tile is 250m x 250m.
![Terrain Toolbox][terrain-toolbox]


## Streaming Settings

You can customize how the terrain is streamed in around the player in the "Terrain Streaming" section of the MapInfo asset.

`Terrain Loading Ring Size` is the number of terrain assets to load around the player.
The value of this highly depends on the size of your terrain assets.
The smaller each terrain piece, the larger this should be.
The default should be fine if you're using the recommended size of 250m x 250m.


## Non-Terrain Tools Setup

If you don't use Terrain Tools to create your terrain, you must follow some rules to ensure it gets loaded correctly.

- Terrain can *only* go in the terrain scene.
- All terrains must have unique TerrainData.
- All terrains must use the same material.
- All terrains must be the same size.
- All terrains must be square.
- All terrains must have the same Pixel Error*.
- All terrains must have the same Basemap Distance*.
- All terrains must have the same Draw Instanced state*.

&#42; If each piece of terrain has a different setting, only the setting from the one closest to 0, 0 will be used.


[terrain-tools-package]: https://docs.unity3d.com/Packages/com.unity.terrain-tools@4.0/manual/index.html
[terrain-toolbox]: ../assets/terrain-toolbox.png

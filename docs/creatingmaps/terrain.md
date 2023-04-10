# Terrain

Derail Valley makes use of terrain streaming to dynamically load in terrain that's around the player.
Due to this, you have to follow some rules when making terrain.

- Terrain can only go in the terrain scene.
- All terrains must use the same material.
- All terrains must be the same size.
- All terrains must be a square.
- All terrains must have the same Pixel Error.
- All terrains must have the same Basemap Distance.
- All terrains must have the same Draw Instanced state.

For the last three, if each piece of terrain has a different setting, only the setting from the first one will be used.

## Terrain Size

In the base game, each terrain chunk is 250m x 250m.
You should try to keep each chunk of your terrain around the same size.

## Terrain Shape & Position

Due to the way Derail Valley handles maps, your map ***must*** be a perfect square, with the bottom left corner at 0, 0.

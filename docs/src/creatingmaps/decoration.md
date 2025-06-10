# Map Decorations

Map decorations are anything that aren't essential to the map functioning.
This includes structures, props, etc.

All of these should go in the `Streaming` scene.
This will allow the game to dynamically stream them in around the player.

## Streaming Configuration

You can customize how these assets get streamed in around the player in the "World Streaming" section of the MapInfo asset.

`Chunk Size` is the size, in meters, of each chunk.
To avoid issues when splitting up the scene, this should be set fairly high.
The value is restricted to be between 128 and 2048.
Vanilla sets it set to 1024, and I'd recommend no lower than 256.

`World Loading Ring Size` is the amount of chunks around the player that are loaded in.
Vanilla sets it to two, but this highly depends on the Chunk Size, and how open your map is.

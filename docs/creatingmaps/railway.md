# Railway

Creating trackage may seem a little complicated at first, but you'll get used to it pretty quick.

## The Railway Parent

To begin, you must create an object called `[railway]` in the Railway scene.
This must be the only root object in that scene, and all tracks will go under it.

## Creating Track

All prefabs mentioned are located in the `Mapify/Prefabs` folder.

### Basic Track
The easiest way to create track is to use the `Track` prefab.
Simply grab it and drag it into the scene under the `[railway]` object.

To avoid issues, it's best to unpack the Track prefab. This does not apply to other prefabs.
You can unpack it by right-clicking on it in the scene hierarchy, and clicking "Unpack Prefab Completely".

Each track is made up of a series of points with handles that control how the track will be created between them.
There is currently no way to visualize the actual track in the editor.

On each piece of track you'll see a `Track` component with a few fields.
Don't worry about those for now, we'll get to them in the [Stations & Job Generation](stations.md) section.

### Switches
Switches can be placed using the `Switch Left` and `Switch Right` prefabs.

By default, the switch stand will be placed on the through side of the switch.
You can change this with the "Stand Side" option on the `Switch` component.

Switches *must* have a track attached to all ends.

You *cannot* modify the curves of switches!
Derail Valley switches are a single static asset, and therefor you can only use the angle of switch they give us.

### Turntables
Turntables can be placed using the `Turntable` prefab.

The control stand *cannot* be moved or rotated independently.
To change it's position, rotate the pit.

### Buffer Stops
At the ends of your track, you can place down buffer stops to prevent trains from rolling off.

Like regular track and switches, you add it to the scene the same way using the `Buffer Stop` prefab, and snap it to the end of a track.

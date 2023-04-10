# Railway

Creating trackage may seem a little complicated at first, but you'll get used to it pretty quick.

## The Railway Parent

To begin, you must create an object called `[railway]` in the Railway scene.
This must be the only root object in that scene, and all tracks will go under it.

## Creating Track

### Basic Track
The easiest way to create track is to use the "Track" prefab.
You can find it at `Mapify/Prefabs/Track` in the project view.
Simply grab it and drag it into the scene under the `[railway]` object.

Each track is made up of a series of points with handles that control how the track will be created between them.
There is currently no way to visualize the actual track in the editor.

On each piece of track you'll see a `Track` component with a few fields.
Don't worry about those for now, we'll get to them in the [Stations & Job Generation](stations.md) section.

### Switches
Switches can be placed into the scene the same way regular track is, but using the `Switch Left` and `Switch Right` prefabs in the same location.

Switches *must* have a track attached to all ends.

You *cannot* modify the curves of switches!
Derail Valley switches are a single static asset, and therefor you can only use the angle of switch they give us.

### Buffer Stops
At the ends of your track, you can place down buffer stops to prevent trains from rolling off.

Like regular track and switches, you add it to the scene the same way using the `Buffer Stop` prefab, and snap it to the end of a track.

# Service Stations

Services stations can be created using the `Service Station` prefab in the `Mapify/Prefabs/Service Station` folder.
They must be located in the `GameContent` scene.


## Configuration

Service stations can be configured to support whatever resource types you want with the `Resources` list.

You may *not* have more than one of the same resource type.


## Snapping To Track

To get the marker to snap to the track, you can add a new point to the track and set the `Handle Type` to `None`.
This will allow for a point that only defines position without affecting the curve.

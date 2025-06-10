# Stores

Stores can be created using the `Store` prefab in the `Mapify/Prefabs/Store` folder, or you can assemble one manually.

## Using The Vanilla Store

If you just want to place down a store to use, drag & drop the aforementioned prefab into the GameContent scene.

On the object you'll see an array called "Item Types".
These are all the items that can be purchased at the store.

## Creating Custom Stores

If you have your own store building you'd like to use, you can create your own store without reusing the vanilla assets.

To begin, add the `Store` component to your object, select the `Item Module` prefab
from `Mapify/Prefabs/Store/Components` as the Visual Prefab, and tick the "Is Custom" box.

Then, add a `Cash Register` prefab as a child of that object, and add it to the Store component.

Under the cash register, create two empty objects.
These will be used as the reference point for the item modules, and the location where bought items will spawn.
Add them to the Store component.

Now you can configure what items can be bough at the store, with a visual of where the item modules will be placed.

# Area Blockers

If you have an area of your map that should only be accessible if the player has a certain license, you can do that with an Area Blocker.

To create an Area Blocker, create a new GameObject in the `GameContent` scene.
On this object, add at least one collider, and size it to cover the area appropriately.

Then, add an `AreaBlocker` component to the object.
On that component, you can choose what license the player needs in order to remove the barrier.

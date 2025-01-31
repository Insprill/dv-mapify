# Area Blockers

If you have an area on your map that should only be accessible if the player has a particular license, you can do that with an Area Blocker.

To create an Area Blocker, create a new GameObject in the `GameContent` scene.
Add at least one collider on this object, and size it to cover the area appropriately.

Then, add an `Area Blocker` component to the object.
On that component, you can choose what license the player needs to remove the barrier.

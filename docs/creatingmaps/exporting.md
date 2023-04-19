# Exporting

Once it's time to export your map, you can do so by going to `Mapify > Export Map`.

Before you can export your map, you'll need to validate it.
This is to ensure that everything is setup correctly and it'll load and be playable in-game.

To validate your map, simply click the "Validate" button at the top of the menu.
If anything is wrong, you'll get a pop-up telling you what it is, with a button to jump to the problematic object.

If you want to validate your map while working on it, you can do so by going to `Mapify > Validate Map`.

## For Testing

If you're exporting your map to test, click `Export Map (Debug)`.

Exporting in debug mode exports directly into a folder, and doesn't compress the Asset Bundles.
This means it'll build and load much faster, but can be an order of magnitude larger.

## For Release

If you're exporting your map to release to others, click `Export Map (Release)`.

This will create a `.zip` with the map inside so you don't have to worry about packaging it yourself.
It will also compress the Asset Bundles to reduce the overall file size of the map.

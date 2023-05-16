# Using Maps

## Installing Maps

### Vortex

The easiest way to install maps is to use [Vortex][vortex] with the [Derail Valley Support Extension][vortex-dv].

To install a map from Nexus Mods using Vortex, click `Mod Manager Download` instead of `Manual Download` on the `Files` tab of the mod.
This will open Vortex, and prompt you to install the map.

### Manual

To install maps manually, you should load the game once with Mapify installed to create the necessary folders.

In the `Mods` folder (`../steamapps/common/Derail Valley/Mods`), open `Mapify`, then `Maps`.
This is where you'll unzip any maps you want to install.

Once unzipped, the `mapInfo.json` file should be located at `Mapify/Maps/map name/mapInfo.json`.


## Changing Maps

Mapify allows you to install multiple maps and select which one is used.
By default, the default map will be loaded.

To select the map you want to play on, open the Unity Mod Manager settings (`CTRL+F10`),
open Mapify's settings and choose the desired map from the list.

![Map Selection Menu][map-selector]

When loading the default map, Mapify is effectively disabled.


## The Savegame

Mapify will automatically create a separate savegame for each map.

You can find them with the default savegame, at `steamapps/common/Derail Valley/DerailValley_Data/SaveGameData`.
The savegame name for custom maps will end in the map name.


[vortex]: https://www.nexusmods.com/about/vortex/
[vortex-dv]: https://www.nexusmods.com/site/mods/527
[map-selector]: ../assets/map-selector.png

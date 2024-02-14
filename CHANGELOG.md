# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Runtime:
#### Added
- Support for build 98 ([#64] by [@Tostiman]).
 
#### Fixed
- Job booklets spawning under the station ([#45] by [@Tostiman]).
- Stations showing missing translation text instead of the station name ([#48] by [@Tostiman]).
- The world map getting modified when loading the default map ([#61] by [@Tostiman]).

### Map Creation Package:
#### Added
- All-new track laying tools ([#25] by [@Wiz]).
- The S060 to the vanilla rolling stock list.
- An option to show station names instead of IDs on the world map ([#32] by [@Tostiman]).
- An option to use an existing texture for the world map instead of procedurally generating one ([#34] by [@Tostiman]).

#### Fixed
- Maps not exporting if they're over 4GB ([#30] by [@Tostiman]).
- Dumpsters and water towers being rotated incorrectly ([#33] by [@Tostiman]).
- Station colors being transparent by default ([#49] by [@Tostiman]).
- Overwriting a release zip not working ([#63] by [@Tostiman]).

#### Changed
- Clarified the error message when warehouse machine validation failed ([#36] by [@Tostiman]).


## [0.4.1] - 2023-07-23
### Runtime:
#### Fixed
- The main menu not loading if no maps are installed.


## [0.4.0] - 2023-07-22
### Runtime:
#### Fixed
- World maps not updating when changing map.
- The streamer scene disappearing when going above Y 10.
- An error if a station's minimum job length was more than the player's licenses allow.
#### Changed
- Switched back to Unity Mod Manager.

### Map Creation Package:
#### Added
- A button to generate track names.
- The ability to set the default state of switches.
- The ability to toggle loading gauge visualization for multiple tracks simultaneously.
- Validation for terrain shape and Y position.
- Validation for the installed build support.

#### Fixed
- Station not initializing properly if their last warehouse machine was removed.
- Track names getting overridden when exporting.
- Validation failing without a directional light (no longer required).

#### Changed
- The default water height to -1 to reduce collisions with flat terrain.
- Existing files in the export directory are now moved to the trash instead of being deleted.
- Improved readability of the validation menu.
- Refactored how locomotive spawners work. Refer to the documentation for more information.
- Warehouse machines now must be assigned to Station's manually.

#### Removed
- The ManualShunterBooklet and ManualSteamBooklet store item types.


## [0.3.0] - 2023-07-02
### Runtime:
#### Added
- Support for Simulator.

#### Fixed
- Issues if there is a piece of track shorter than 0.75m.
- Regular tracks not connecting to each other.

#### Changed
- Switched to BepInEx.

### Map Creation Package:
#### Added
- Support for custom buffer stops.
- The new coaling and water towers.
- A validation and startup warning if an incorrect Unity version is being used.

#### Fixed
- An error during validation if a Terrain object was missing its TerrainData.
- Tracks not snapping if the camera was too far from the Track root.
- The default debug export path on Linux.

#### Changed
- Bumped the Unity version to 2019.4.40f1.
- GameObject's with Camera or Directional Lights are now excluded from the build.


## [0.2.0] - 2023-04-27
### Runtime:
#### Added
- Track LODs. You shouldn't see tracks popping in anymore when traveling through the world.

#### Fixed
- Vegetation not being visible on terrain.
- An error when initializing streamers.
- Tracks on the map not appearing smooth on smaller maps.

### Map Creation Package:
#### Added
- The ability to create fully custom turntables.
- A toggle to visualize the loading gauge of trackage.
- An option to toggle whether ballast and sleepers are generated for tracks.

#### Fixed
- The disconnected switch validation not working.
- Switches snapping to turntables and other switches.
- Occasional terrain-related issues when exporting.
- Procedural map backgrounds being completely white.
- The "polygon is self-intersecting" warning when importing the creation package.


## [0.1.1] - 2023-04-23
### Runtime:
#### Fixed
- The mod trying to load a non-existent map if it was deleted while selected.
- The mod's home page and repository URLs not being set.

#### Changed
- Specified minimum UMM version as 0.24.6.

### Map Creation Package:
#### Added
- Validation for terrain height.

#### Fixed
- Spawn position validation always failing until a build was run.
- An error when generating the map background if the terrain or water levels were 0.


## [0.1.0] - 2023-04-22
- Initial release.


<!-- Users -->
[@Tostiman]: https://github.com/t0stiman
[@Wiz]: https://github.com/WhistleWiz

<!-- Pull Requests -->
[#25]: https://github.com/Insprill/dv-mapify/pull/25
[#30]: https://github.com/Insprill/dv-mapify/pull/30
[#32]: https://github.com/Insprill/dv-mapify/pull/32
[#33]: https://github.com/Insprill/dv-mapify/pull/33
[#34]: https://github.com/Insprill/dv-mapify/pull/34
[#36]: https://github.com/Insprill/dv-mapify/pull/36
[#45]: https://github.com/Insprill/dv-mapify/pull/45
[#48]: https://github.com/Insprill/dv-mapify/pull/48
[#49]: https://github.com/Insprill/dv-mapify/pull/49
[#61]: https://github.com/Insprill/dv-mapify/pull/61
[#63]: https://github.com/Insprill/dv-mapify/pull/63
[#64]: https://github.com/Insprill/dv-mapify/pull/64

<!-- Diffs -->
[Unreleased]: https://github.com/Insprill/dv-mapify/compare/v0.4.1...HEAD
[0.4.1]: https://github.com/Insprill/dv-mapify/compare/v0.4.0...v0.4.1
[0.4.0]: https://github.com/Insprill/dv-mapify/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/Insprill/dv-mapify/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/Insprill/dv-mapify/compare/v0.1.1...v0.2.0
[0.1.1]: https://github.com/Insprill/dv-mapify/compare/v0.1.0...v0.1.1
[0.1.0]: https://github.com/Insprill/dv-mapify/releases/tag/v0.1.0

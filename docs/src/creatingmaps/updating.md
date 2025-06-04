# Updating

When it comes to updating Mapify, you can simply [download the latest release from GitHub][github-releases],
then go to `Mapify > Package > Import` and select the file you downloaded.

There may be breaking changes between Mapify versions.
Because of this, make sure to check the [Breaking Changes](#breaking-changes) section before updating!

## Breaking Changes

All breaking changes between Mapify versions will be listed here.
Each change is relative to the last version in the list.
If you're upgrading through multiple versions, read the list bottom-up, starting from the version after the one you're running.

### 0.4.0
- All `Locomotive Spawner` components must be replaced with `Vanilla Locomotive Spawner`s, and all values must be reassigned.
- The `ManualShunterBooklet` and `ManualSteamBooklet` store item types have been removed.
- `Warehouse Machine`s must now be manually assigned to their respective `Station`s.

### 0.3.0
- The Unity version has been bumped to 2019.4.40f1.
- All Store Item Types must be reassigned.
- All Track ages must be reassigned.
- All service station resources must be reassigned.
- Diesel service station resources are now required to be last in the list.

[github-releases]: https://github.com/Insprill/dv-mapify/releases

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![Apache 2.0 License][license-shield]][license-url]




<!-- PROJECT LOGO -->
<div align="center">
  <h1>Mapify</h1>
  <p>
    An experimental <a href="https://store.steampowered.com/app/588030">Derail Valley</a> mod for loading custom maps
    <br />
    <br />
    <a href="https://github.com/Insprill/dv-mapify/issues">Report Bug</a>
    ·
    <a href="https://github.com/Insprill/dv-mapify/issues">Request Feature</a>
  </p>
</div>




<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li><a href="#about-the-project">About The Project</a></li>
    <li><a href="#creating-maps">Creating Maps</a></li>
    <li><a href="#building">Building</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
  </ol>
</details>




<!-- ABOUT THE PROJECT -->

## About The Project

Mapify (new name soon™) is a Derail Valley mod that allows people to create and load custom maps.  
They said it couldn't be done, so I did it :)




<!-- CREATING MAPS -->

## Creating Maps

### Project Setup
1. Create a new 3D project with Unity **2019.4.22f1**.
2. Import *only the `Editor` folder* of the [Bezier Curve Editor](https://assetstore.unity.com/packages/tools/bezier-curve-editor-11278) asset from the Unity Asset Store.
3. Copy `../DerailValley_Data/Managed/BezierCurves.dll` from your Derail Valley installation directory into the `Scripts` folder.
4. Import the [Post Processing](https://docs.unity3d.com/Packages/com.unity.postprocessing@3.2/manual/index.html) package from the Package Manager.
5. Build or download the latest version of Mapify.
6. Create a folder called `Scripts` and drag `MapifyEditor.dll` into it. If building, you can find it in the `build` folder.
7. Create a folder called `Scenes`, and create three scenes inside it. `Terrain`, `Railway`, and `GameContent`. No scenes should have cameras in them.
8. Add your scenes to an AssetBundle named `scenes`.
9. Add all other assets to an AssetBundle named `assets`.


### Creating a Map
1. Create a `MapInfo` scriptable object. The name doesn't matter, but there must be exactly one in the project.
2. In your `Terrain` scene, create an object called `[distant terrain]`. If you have a terrain backdrop, put in there. If you don't, just leave it empty.
3. In your `Terrain` scene, create an object called `[GlobalPostProcessing]`. Add a `PostProcessingVolume` component to it, and enable `Is Global`.
4. Create a new post processing profile that has Ambient Occlusion, Bloom, and Auto Exposure.
5. In your `Railway` scene, create an object called `[railway]`. This is where your track splines will go.
6. To create splines, go to `Mapify > Create Track`. You can then adjust the track with the attached BezierCurve. To add switches, use the buttons on the Track component. Any track ends within one meter of each other will be linked together when exporting.


### Exporting
To export your map, go to `Mapify > Export Map`.


### Loading the map
To load the map, create a folder called `Map` in the mods install directory. Drag your `scenes` and `assets` files into there.
If you have an existing savegame on the default map, remove it before launching the game.




<!-- BUILDING -->

## Building

To build Mapify, you'll need to create a new [`Directory.Build.targets`](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-your-build?view=vs-2022) file to specify your reference paths. 
There should be two of these files, one in the `Mapify` folder and one in the `MapifyEditor` folder.
You can use the examples below as templates depending on your platform.

<details>
<summary>Windows</summary>

Here's an example file for Windows you can use as a template.
Replace the provided paths with the paths to your Derail Valley installation directory.
Make sure to include the semicolons between each of the paths, but not after the last one!
Note that shortcuts like `%ProgramFiles%` *cannot* be used.
```xml
<Project>
    <PropertyGroup>
        <ReferencePath>
            C:\Program Files (x86)\Steam\steamapps\common\Derail Valley\DerailValley_Data\Managed\;
            C:\Program Files (x86)\Steam\steamapps\common\Derail Valley\DerailValley_Data\Managed\UnityModManager\;
            C:\Program Files\Unity\Hub\Editor\2019.4.22f1\Editor\Data\Managed
        </ReferencePath>
        <AssemblySearchPaths>$(AssemblySearchPaths);$(ReferencePath);</AssemblySearchPaths>
    </PropertyGroup>
</Project>
```
</details>

<details>
<summary>Linux</summary>

Here's an example file for Linux you can use as a template.
Replace the provided paths with the paths to your Derail Valley installation directory.
Make sure to include the semicolons between each of the paths, but not after the last one!
```xml
<Project>
    <PropertyGroup>
        <ReferencePath>
            /home/username/.local/share/Steam/steamapps/common/Derail Valley/DerailValley_Data/Managed/;
            /home/username/.local/share/Steam/steamapps/common/Derail Valley/DerailValley_Data/Managed/UnityModManager/;
            /home/username/.local/share/UnityHub/Editor/2019.4.22f1/Editor/Data/Managed/
        </ReferencePath>
        <AssemblySearchPaths>$(AssemblySearchPaths);$(ReferencePath);</AssemblySearchPaths>
    </PropertyGroup>
</Project>
```
</details>

To test your changes, `Mapify.dll` and `MapifyEditor.dll` will need to be copied into the mod's install directory (e.g. `...Derail Valley/Mods/Mapify`) along with the `info.json`.
The .dll can be found in `../bin/Debug` or `../bin/Release` depending on the selected build configuration.
If you're on Linux, you can find them both in the `build` folder.
The info.json can be found in the root of this repository.




<!-- CONTRIBUTING -->

## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create.  
Any contributions you make are **greatly appreciated**!  
If you're new to contributing to open-source projects, you can follow [this][contributing-quickstart-url] guide.




<!-- LICENSE -->

## License

Distributed under the Apache 2.0 license.  
See [LICENSE][license-url] for more information.




<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->

[contributors-shield]: https://img.shields.io/github/contributors/Insprill/dv-mapify.svg?style=for-the-badge
[contributors-url]: https://github.com/Insprill/dv-mapify/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/Insprill/dv-mapify.svg?style=for-the-badge
[forks-url]: https://github.com/Insprill/dv-mapify/network/members
[stars-shield]: https://img.shields.io/github/stars/Insprill/dv-mapify.svg?style=for-the-badge
[stars-url]: https://github.com/Insprill/dv-mapify/stargazers
[issues-shield]: https://img.shields.io/github/issues/Insprill/dv-mapify.svg?style=for-the-badge
[issues-url]: https://github.com/Insprill/dv-mapify/issues
[license-shield]: https://img.shields.io/github/license/Insprill/dv-mapify.svg?style=for-the-badge
[license-url]: https://github.com/Insprill/dv-mapify/blob/master/LICENSE
[contributing-quickstart-url]: https://docs.github.com/en/get-started/quickstart/contributing-to-projects

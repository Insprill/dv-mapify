# Building The Project

To build Mapify, you'll need to create a new [`Directory.Build.targets`](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-your-build?view=vs-2022) file to specify your reference paths.
There should be two of these files, one in the `Mapify` folder and one in the `MapifyEditor` folder.
You can use the examples below as templates depending on your operating system.

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
The DLL's can be found in the `build` folder, and the `info.json` at the root of the repository.

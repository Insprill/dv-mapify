# Building The Project

## Project Setup

### Reference Directories

To ensure MSBuild and your IDE can find Derail Valley / Unity classes,
you'll need to create a [`Directory.Build.targets`][directory-build-targets-docs] file to specify your reference paths.
This file should be created in the root of the project, next to the `LICENSE` file.
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
            C:\Program Files\Unity\Hub\Editor\2019.4.40f1\Editor\Data\Managed\
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
            /home/username/.local/share/UnityHub/Editor/2019.4.40f1/Editor/Data/Managed/
        </ReferencePath>
        <AssemblySearchPaths>$(AssemblySearchPaths);$(ReferencePath);</AssemblySearchPaths>
    </PropertyGroup>
</Project>
```
</details>

### Environment Variables

When building Mapify, it will try to update the mod in your Derail Valley installation directory so you don't have to manually copy it.
To do this, it needs to know where your game is installed.
Open the file called `.env` in the root of the project, next to the `LICENSE` file.
Inside, you'll find all the environment variables that need to be set, along with comments explaining what they do.


[directory-build-targets-docs]: https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory?view=vs-2022#directorybuildprops-and-directorybuildtargets

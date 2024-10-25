# Building The Project

## Project Setup

## Dependencies

To build Mapify, you'll need to have some dependencies installed.

- [.NET 7][dotnet-download]
- [PowerShell][powershell-download]

If you're running windows, ensure you have Powershell **7** installed.
If you're on Linux, you should be able to download it from your package manager.


### Reference Directories

To ensure MSBuild and your IDE can find Derail Valley / Unity classes, and to avoid having to copy DLLs around,
you'll need to create a [`Directory.Build.targets`][directory-build-targets-docs] file to specify your reference paths.
This file should be created in the root of the project, next to the `LICENSE` file.

You can use the examples below as templates depending on your operating system.

- `DvInstallDir` is the directory where Derail Valley is installed (Where `DerailValley.exe` is located).
- `UnityInstallDir` is the directory where Unity is installed (Where `Unity.exe` or `Unity` is located).

<details>
<summary>Windows</summary>

Here's an example file for Windows you can use as a template.
Replace the provided paths with the paths to your Derail Valley installation directory.
Make sure to include the semicolons between each of the paths, but not after the last one!
Note that shortcuts like `%ProgramFiles%` *cannot* be used.
```xml
<Project>
    <PropertyGroup>
        <DvInstallDir>C:\Program Files (x86)\Steam\steamapps\common\Derail Valley</DvInstallDir>
        <UnityInstallDir>C:\Program Files\Unity\Hub\Editor\2019.4.40f1\Editor</UnityInstallDir>
        <ReferencePath>
            $(DvInstallDir)\DerailValley_Data\Managed\;
            $(DvInstallDir)\DerailValley_Data\Managed\UnityModManager\;
            $(UnityInstallDir)\Data\Managed\
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
        <DvInstallDir>/home/username/.local/share/Steam/steamapps/common/Derail Valley</DvInstallDir>
        <UnityInstallDir>/home/username/.local/share/UnityHub/Editor/2019.4.40f1/Editor</UnityInstallDir>
        <ReferencePath>
            $(DvInstallDir)/DerailValley_Data/Managed/;
            $(DvInstallDir)/DerailValley_Data/Managed/UnityModManager/;
            $(UnityInstallDir)/Data/Managed/
        </ReferencePath>
        <AssemblySearchPaths>$(AssemblySearchPaths);$(ReferencePath);</AssemblySearchPaths>
    </PropertyGroup>
</Project>
```
</details>


## Packaging

To package a build for distribution, you can run the `package.ps1` PowerShell script in the root of the project.
If no parameters are supplied, it will create a `.zip` file ready for distribution in the `dist` directory.

- **Linux:** `pwsh ./package.ps1`
- **Windows:** `powershell -executionpolicy bypass .\package.ps1`


[dotnet-download]: https://dotnet.microsoft.com/en-us/download
[powershell-download]: https://github.com/PowerShell/PowerShell#get-powershell
[directory-build-targets-docs]: https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory?view=vs-2022#directorybuildprops-and-directorybuildtargets

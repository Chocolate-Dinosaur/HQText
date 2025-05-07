# HQ-Text Build Instructions

## Development requirements
- IDE to edit C++ and C# code
- cmake

### Windows

#### Build Pango Libraries

Install [vcpkg](https://vcpkg.io/)

Clone into your desired directory:

```bash
git clone https://github.com/Microsoft/vcpkg.git
```

Run bootstrap:

```bash
.\vcpkg\bootstrap-vcpkg.bat
```

Run the command  to install Pango Library (this can take some time):
```bash
vcpkg install pango --triplet x64-windows-static
```

But actually we have a patch to fix a bug in cairo (static builds don't initialize mutexes in win32 backend mode) so we need to specify our port fix:

```bash
vcpkg install --overlay-ports=c:/andrew/proj/HQ-Text/_Native/ports/ --triplet x64-windows-static pango
```

_Note: We last tested this with Pango package version 1.50.14_
_Note: See https://docs.gtk.org/Pango for Pango docs._

#### Build HQ-Text Plugin

Install Visual Studio 2019 or 2022 with C++ modules

Install [CMake](https://cmake.org/download/)

Generate the Visual Studio project files by running these commands in the terminal:
```bash
cd <repo>/Native/src
cmake configure . -Dvcpkgpath=PUT PATH TO VCPKG HERE e.g c:/vcpkg
```

Open the generated `libHQText.sln` in Visual Studio and build the HQText project to generate the `libHQText.dll` plugin file.

### Deploying the plugin

Once the plugin has been built, you need to copy it to the Unity folder to use it in your project.  To update the plugin in Unity, copy the newly created plugin file to the Unity package from the build folder (either the `Debug` or `Release` on Windows folder depending on which build configuration was used).  For the changes to take effect Unity needs to be restarted.  The copy may also fail if Unity has already been running using an existing plugin, as Unity locks the plugin file - in which case Unity needs to be closed before running the copy command.

On Mac:
```bash
cp libHQText.dylib ../../Assets/ChocDino/HQText/Runtime/Plugins/Mac
```

On Windows:
```bash
copy HQText.dll ../../Assets/ChocDino/HQText/Runtime/Plugins/Windows
```

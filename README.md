# Debug Draw API for Unity

```DbgDraw``` is an API that provides the ability to render various 2D and 3D shapes for visual debugging purposes. It's similar to Unity's [Gizmos](https://docs.unity3d.com/ScriptReference/Gizmos.html) and [Handles](https://docs.unity3d.com/ScriptReference/Handles.html) API's.

Unity provides a rather limited set of debug draw functions in the Debug class, such as [Debug.DrawLine](https://docs.unity3d.com/ScriptReference/Debug.DrawLine.html) for example.

Unlike Unity's Gizmo, which requires rendering code to be located in a [OnDrawGizmos](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnDrawGizmos.html) method, you can issue DbgDraw calls from anywhere in your code. Rather than issuing these render calls immediately, DbgDraw collects those calls and defers the actual rendering to a later time. This allows to call DbgDraw from any method, be it Start(), Update() etc.

DbgDraw provides functionality to render the following shapes:
* Arc
* Cube
* Disc
* Line, Lines and PolyLine
* Matrix
* Plane
* Pyramid
* Quad
* Ray
* Sphere
* Tube

Most of these shapes can be rendered solid and in wireframe.


# Example

```csharp
using Oddworm.Framework;

// Draw a wireframe cube for a single frame
DbgDraw.WireCube(position, rotation, scale, color); 

// Draw a solid cube for ten seconds
DbgDraw.Cube(position, rotation, scale, color, 10);
```
Please import the "DbgDraw Examples" file, which part of this package, for more examples.


# Limitations

* DbgDraw works with Unity 2020.3 and later versions.
* DbgDraw works in the Unity editor and [development mode](https://docs.unity3d.com/Manual/BuildSettings.html).
* DbgDraw works in play mode only.
* DbgDraw has been created for visual debugging purposes, not as a fast general purpose shape rendering API.


# Installation

Open in Unity Window > Package Manager, choose "Add package from git URL" and insert one of the Package URL's you can find below.

## Package URL's

I recommend to right-click the URL below and choose "Copy Link" rather than selecting the text and copying it, because sometimes it copies a space at the end and the Unity Package Manager can't handle it and spits out an error when you try to add the package.

Please see the ```CHANGELOG.md``` file to see what's changed in each version.

| Version  |     Link      |
|----------|:-------------:|
| 1.2.0 | https://github.com/pschraut/UnityDbgDraw.git#1.2.0 |
| 1.1.0 | https://github.com/pschraut/UnityDbgDraw.git#1.1.0 |
| 1.0.0 | https://github.com/pschraut/UnityDbgDraw.git#1.0.0 |



# FAQ

### It's not working in the editor

DbgDraw works in the editor only, if you tick the ```Development Build``` checkbox in the Build Settings window (File > Build Settings).

### It's not working in a build

DbgDraw works in a build only, if you tick the ```Development Build``` checkbox in the Build Settings window (File > Build Settings).

### Remove DbgDraw calls from release builds

If you untick the ```Development Build``` checkbox in the Build Settings window (File > Build Settings), calls to DbgDraw are being removed by the compiler.

### "Hidden/DbgDraw-Shaded" in Always Included Shaders

The package automatically adds the ```Hidden/DbgDraw-Shaded``` shader to the 'Always Included Shaders' list in the Player settings when you create a build. This is required to make the DbgDraw package work in a build. It's a very simple shader that does add a trivial amount of size to the build.
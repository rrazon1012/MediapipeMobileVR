This is the major project for COMP 8037 BTECH. This project aims to make use of mediapipe to replace controllers for Mobile VR.
To showcase the functionality of mediapipe, a recreation of the mobile game fruit ninja will be made with VR functionality. 

# Uses MediaPipe Unity Plugin by Homuler at https://github.com/homuler/MediaPipeUnityPlugin

This is a Unity (2020.3.23f1) [Native Plugin](https://docs.unity3d.com/Manual/NativePlugins.html) to use [MediaPipe](https://github.com/google/mediapipe) (0.8.9).

The goal of this project is to port the MediaPipe API (C++) _one by one_ to C# so that it can be called from Unity.\
This approach may sacrifice performance when you need to call multiple APIs in a loop, but it gives you the flexibility to use MediaPipe instead.

With this plugin, you can

- Write MediaPipe code in C#.
- Run MediaPipe's official solution on Unity.
- Run your custom `Calculator` and `CalculatorGraph` on Unity.
  - :warning: Depending on the type of input/output, you may need to write C++ code.

## :smile_cat: Hello World!

# C# Wpf Project Template with unmanaged C++ OpenGL

Demo Projects that uses C# WPF for the UI and unmanaged C++ OpenGL on a seperate thread. (Visual Studio 2017 Solution).
Note: Only the **x64** configuration is correctly configured!

## CppOpenGL

C++ DLL with the following functions (interface.cpp):
* **initialize()** will be called after the OpenGL context creation. This functions uses glad to load the OpenGL function pointers (OpenGL Version 4.6). Visit https://glad.dav1d.de/ to generate glad for other OpenGL Versions.
* **resize(width, height)** resizes the OpenGL viewport.
* **render()** is the main render function (draws a green screen).

## WpfUnmanagedOpenGL

Wpf Project that hosts a OpenGLHost (HwndHandler) within a WPF border element.
The OpenGLHost has the following functions:
* **BuildWindowCore(HWND)** creates the win32 window handle for the OpenGL context and starts the render thread.
* **Render()** is the render loop of the programm. This function initializes OpenGL, calls the resize() function if the client area has changed and calls the render() method from the C++ DLL as long as the window is open.

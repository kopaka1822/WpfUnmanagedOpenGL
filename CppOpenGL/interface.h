#pragma once
#include <cstdint>
#include <basetsd.h>

extern "C"
__declspec(dllexport)
INT_PTR
__cdecl
create_context(INT_PTR hWnd, int width, int height);

extern "C"
__declspec(dllexport)
void
__cdecl
init_gl();

extern "C"
__declspec(dllexport)
bool
__cdecl
render();
#pragma once
#include <string>

extern "C"
__declspec(dllexport)
bool
__cdecl
initialize();

extern "C"
__declspec(dllexport)
bool
__cdecl
render();

extern "C"
__declspec(dllexport)
const char*
__cdecl
get_error();

void set_error(std::string error);
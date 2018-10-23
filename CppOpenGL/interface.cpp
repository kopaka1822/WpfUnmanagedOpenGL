#include "interface.h"
#include <Windows.h>
#include <minwinbase.h>
#include <glad/glad.h>
#include <iostream>

static HWND s_windowHandle = nullptr;
static HDC s_deviceContext = nullptr;

DWORD WINAPI ha(void *d)
{
	HWND hWnd = (HWND)d;
	
	// opengl setup
	s_deviceContext = GetDC(hWnd);
	PIXELFORMATDESCRIPTOR pfd;
	ZeroMemory(&pfd, sizeof(pfd));
	pfd.nSize = sizeof(pfd);
	pfd.nVersion = 1;
	pfd.dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER;
	pfd.iPixelType = PFD_TYPE_RGBA;
	pfd.cColorBits = 24;
	pfd.cDepthBits = 16;
	pfd.iLayerType = PFD_MAIN_PLANE;
	const auto iFormat = ChoosePixelFormat(s_deviceContext, &pfd);
	if(!SetPixelFormat(s_deviceContext, iFormat, &pfd))
	{
		std::cerr << "SetPixelFormat failed\n";
		return 5;
	}

	// opengl rendering context
	const auto hRC = wglCreateContext(s_deviceContext);
	if(!hRC)
	{
		std::cerr << "wglCreateContext failed\n";
		return 1;
	}

	if(!wglMakeCurrent(s_deviceContext, hRC))
	{
		std::cerr << "wglMakeCurrent failed\n";
		return 2;
	}
	
	// vsynch?
	using PFNWGLSWAPINTERVALEXTPROC = BOOL(WINAPI * ) (int interval);
	const auto wglSwapIntervalExt = reinterpret_cast<PFNWGLSWAPINTERVALEXTPROC>(wglGetProcAddress("wglSwapIntervalEXT"));
	if(wglSwapIntervalExt)
	{
		// enable vsynch
		wglSwapIntervalExt(1);
	}

	if (!gladLoadGL())
	{
		std::cerr << "gladLoadGL failed\n";
		return 3;
	}

	glClearColor(1.0f, 0.0f, 0.0f, 0.0f);
	glClear(GL_COLOR_BUFFER_BIT);

	if(!SwapBuffers(s_deviceContext))
	{
		std::cerr << "SwapBuffers failed\n";
		return 4;
	}

	while(render());
	return 0;
}

INT_PTR create_context(INT_PTR hWnd, int width, int height)
{
	// http://www.rohitab.com/discuss/topic/38722-wpf-and-opengl/
	// https://docs.microsoft.com/de-de/dotnet/framework/wpf/advanced/walkthrough-hosting-a-win32-control-in-wpf)
	s_windowHandle = CreateWindowEx(
		0, // style
		"static",
		"",
		WS_CHILD | WS_VISIBLE,
		0, // x
		0, // y
		width, // width
		height, // height
		reinterpret_cast<HWND>(hWnd),
		nullptr, // menu
		nullptr, // hinstance
		nullptr // lparam
	);

	// initialize everything
	HANDLE hThread = CreateThread(nullptr, 0, &ha, s_windowHandle, 0, nullptr);
	//init_gl(); // does not work

	return reinterpret_cast<INT_PTR>(s_windowHandle);
}

void init_gl()
{
	ha(s_windowHandle);
}

bool render()
{
	static float t = 1.0f;
	glViewport(0, 0, 100, 100);
	glClearColor(0.0f, t, 0.0f, 1.0f);
	t = 1.0f - t;
	glClear(GL_COLOR_BUFFER_BIT);

	if(!SwapBuffers(s_deviceContext))
	{
		// window closed
		std::cerr << "SwapBuffers failed\n";
		return false;
	}
	return true;
}
#include "interface.h"
#include <glad/glad.h>
#include <string>

static std::string s_lastError;

const char* get_error(int& length)
{
	length = static_cast<int>(s_lastError.length());
	return s_lastError.data();
}

void set_error(std::string error)
{
	s_lastError = move(error);
}

bool initialize()
{
	if (!gladLoadGL())
	{
		set_error("gladLoadGL failed");
		return false;
	}
	return true;
}

bool render()
{
	static float t = 0.0f;
	glClearColor(0.0f, t, 0.0f, 1.0f);
	t += 0.01f;
	if (t > 1.0f) t -= 1.0f;
	glClear(GL_COLOR_BUFFER_BIT);

	return true;
}

bool resize(int width, int height)
{
	// glViewport should not be called with width or height zero
	if (width == 0 || height == 0) return true;

	glViewport(0, 0, width, height);
	return true;
}

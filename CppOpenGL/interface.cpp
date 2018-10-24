#include "interface.h"
#include <glad/glad.h>

static std::string s_lastError;

const char* get_error()
{
	return s_lastError.c_str();
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
	static float t = 1.0f;
	glViewport(0, 0, 100, 100);
	glClearColor(0.0f, t, 0.0f, 1.0f);
	t = 1.0f - t;
	glClear(GL_COLOR_BUFFER_BIT);

	return true;
}
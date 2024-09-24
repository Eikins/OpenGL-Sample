// Core.glsl

layout (binding = 0, std140) uniform PerFrameConstants
{
	mat4 engine_Matrix_VP;
	vec3 engine_LightDir;
	float engine_Time;
};

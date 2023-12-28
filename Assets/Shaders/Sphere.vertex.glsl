#version 460 core //Using version GLSL version 4.6
layout (location = 0) in vec3 InPosition;
layout (location = 1) in vec3 InNormal;
layout (location = 2) in vec2 InTexCoord0;
        
out VertexData {
    vec2 TexCoord0;
} vs_out;

#include "Assets/Shaders/Library/Core.glsl"
#include "Assets/Shaders/Library/SpaceTransforms.glsl"

uniform mat4 _LocalToWorld;

void main()
{
    vs_out.TexCoord0 = InTexCoord0;

    vec3 worldPosition = (_LocalToWorld * vec4(InPosition, 1.0)).xyz;
    gl_Position = TransformWorldToClip(worldPosition); 
}
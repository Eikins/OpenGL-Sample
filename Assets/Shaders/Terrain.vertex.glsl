#version 460 core

#include "Assets/Shaders/Library/Core.glsl"
#include "Assets/Shaders/Library/SpaceTransforms.glsl"

layout (location = 0) in vec3 InPosition;
layout (location = 1) in vec3 InNormal;
layout (location = 2) in vec2 InTexCoord0;
        
out VertexData {
    vec3 Normal;
    vec3 WorldPosition;
} vs_out;

uniform mat4 _LocalToWorld;

uniform sampler2D _HeightMap;

vec3 GetNormal(vec2 uv)
{
    float uvOffset = 0.0002; 
    float dx = texture(_HeightMap, uv + vec2(uvOffset, 0)).r - texture(_HeightMap, uv - vec2(uvOffset, 0)).r;
    float dz = texture(_HeightMap, uv + vec2(0, uvOffset)).r - texture(_HeightMap, uv - vec2(0, uvOffset)).r;

   
    vec3 ddx = normalize(vec3(uvOffset * 200, dx, 0));
    vec3 ddz = normalize(vec3(0, dz, uvOffset * 200));

    return cross(ddz, ddx);
}

void main()
{
    vec3 worldPosition = (_LocalToWorld * vec4(InPosition, 1.0)).xyz;

    float yOffset = texture(_HeightMap, InTexCoord0).r * 30;
    vs_out.Normal = GetNormal(InTexCoord0);

    worldPosition.y += yOffset - 5;

    vs_out.WorldPosition = worldPosition;

    gl_Position = TransformWorldToClip(worldPosition); 
}
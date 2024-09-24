#version 460 core

#include "Assets/Shaders/Library/Core.glsl"

out vec4 FragColor;

uniform vec4 _Color;
uniform sampler2D _BaseColor;

in VertexData {
    vec3 Normal;
    vec3 WorldPosition;
} vs_out;

void main()
{
    vec2 uv = vs_out.WorldPosition.xz;
    float lightIntensity = 0.5 + 0.5 * dot(engine_LightDir, vs_out.Normal);
    FragColor = texture(_BaseColor, uv) * lightIntensity;
}
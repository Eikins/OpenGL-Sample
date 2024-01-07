#version 460 core // Using GLSL version 4.6.

#include "Assets/Shaders/Library/Core.glsl"

out vec4 FragColor;

uniform vec4 _Color;
uniform sampler2D _BaseColor;

in VertexData {
    vec3 Normal;
    vec2 TexCoord0;
} vs_out;

void main()
{
    float lightIntensity = 0.5 + 0.5 * dot(engine_LightDir, vs_out.Normal);
    FragColor = _Color * texture(_BaseColor, vs_out.TexCoord0) * lightIntensity;
}
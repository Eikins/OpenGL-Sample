#version 460 core
out vec4 FragColor;

uniform vec4 _Color;
uniform sampler2D _BaseColor;

in VertexData {
    vec2 TexCoord0;
} vs_out;

void main()
{
    FragColor = _Color * texture(_BaseColor, vs_out.TexCoord0);
}
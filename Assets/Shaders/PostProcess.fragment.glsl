#version 460 core

out vec4 FragColor;

uniform sampler2D _SourceColor;
uniform vec4 _SourceSize;

uniform float _Brightness;

uniform float _TritoneEnabled;
uniform vec4 _ShadowsColor;
uniform vec4 _MidtonesColor;
uniform vec4 _HighlightsColor;

uniform float _ChromaticEnabled;
uniform float _ChromaticIntensity;

void main()
{
    vec2 uv = gl_FragCoord.xy / _SourceSize.xy;
    vec3 color = texture(_SourceColor, uv).rgb;

    color = color * _Brightness;

    if (_ChromaticEnabled == 1.0)
    {
        vec2 shiftR = (uv - 0.5) * (1 + _ChromaticIntensity * 0.05) + 0.5;
        vec2 shiftB = (uv - 0.5) * (1 - _ChromaticIntensity * 0.05) + 0.5;

        color.r = texture(_SourceColor, shiftR).r * _Brightness;
        color.b = texture(_SourceColor, shiftB).b * _Brightness;
    }

    if (_TritoneEnabled == 1.0)
    {
        float luminance = dot(vec3(0.299, 0.587, 0.114), color);
        if (luminance < 0.5)
        {
            color = mix(_ShadowsColor, _MidtonesColor, luminance * 2.0).rgb;
        }
        else
        {
            color = mix(_MidtonesColor, _HighlightsColor, (luminance - 0.5) * 2.0).rgb;
        }
    }

    FragColor = vec4(color, 1);
}
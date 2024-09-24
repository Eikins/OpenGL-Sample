using GLSample.Sources.Core;
using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace GLSample.Rendering
{
    public class PostProcessPass : GLRenderPass
    {
        private static readonly GLShaderUniformId s_SourceColor = GLShaderUniformId.FromName("_SourceColor");
        private static readonly GLShaderUniformId s_SourceSize = GLShaderUniformId.FromName("_SourceSize");

        private static readonly GLShaderUniformId s_Brightness = GLShaderUniformId.FromName("_Brightness");
        private static readonly GLShaderUniformId s_TritoneEnabled = GLShaderUniformId.FromName("_TritoneEnabled");
        private static readonly GLShaderUniformId s_ShadowsColor = GLShaderUniformId.FromName("_ShadowsColor");
        private static readonly GLShaderUniformId s_MidtonesColor = GLShaderUniformId.FromName("_MidtonesColor");
        private static readonly GLShaderUniformId s_HighlightsColor = GLShaderUniformId.FromName("_HighlightsColor");

        private static readonly GLShaderUniformId s_ChromaticEnabled = GLShaderUniformId.FromName("_ChromaticEnabled");
        private static readonly GLShaderUniformId s_ChromaticIntensity = GLShaderUniformId.FromName("_ChromaticIntensity");

        private GLMesh _fullscreenTriangle;
        private GLShader _shader;
        private GLTexture _source;
        private GLTexture _destination;
        private PostProcessing _postProcessing;
        
        public PostProcessPass(GL gl, GLTexture src, GLTexture dst, GLShader postProcessShader, PostProcessing postProcessing) : base(gl) 
        {
            _source = src;
            _destination = dst;
            _shader = postProcessShader;
            _postProcessing = postProcessing;
        }

        public override void ConfigureTargets()
        {
            colorAttachments = new List<ColorAttachment>()
            {
                new ColorAttachment()
                {
                    target = _destination,
                    mipLevel = 0
                }
            };

            // No depth for this pass.
            depthAttachment = null;

            _fullscreenTriangle = new GLMesh(
                gl,
                new GLMesh.Vertex[]
                {
                    new () { position = new Vector3(-1f, -1f, 0 ) },
                    new () { position = new Vector3(-1f, 3f, 0 ) },
                    new () { position = new Vector3(3f, -1f, 0 ) },
                }, 
                new uint[] { 0, 1, 2 }
            );
        }

        public override void Render()
        {
            gl.Viewport(0, 0, _destination.Descriptor.width, _destination.Descriptor.height);

            _shader.Use();
            _shader.SetTexture(s_SourceColor, _source, 0);
            _shader.SetVector(s_SourceSize, new Vector4(_source.Descriptor.width, _source.Descriptor.height, 0, 0));

            _shader.SetFloat(s_Brightness, _postProcessing.Brightness);
            _shader.SetFloat(s_TritoneEnabled, _postProcessing.TritoneEnabled ? 1.0f : 0.0f);
            _shader.SetVector(s_ShadowsColor, _postProcessing.ShadowsColor);
            _shader.SetVector(s_MidtonesColor, _postProcessing.MidtonesColor);
            _shader.SetVector(s_HighlightsColor, _postProcessing.HighlightsColor);

            _shader.SetFloat(s_ChromaticEnabled, _postProcessing.ChromaticAberrationEnabled ? 1.0f : 0.0f);
            _shader.SetFloat(s_ChromaticIntensity, _postProcessing.ChromaticAberrationIntensity);

            _fullscreenTriangle.Draw();
        }
    }
}

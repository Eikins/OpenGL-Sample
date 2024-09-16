using GLSample.Core;
using Silk.NET.OpenGL;
using System;
using System.Numerics;

namespace GLSample.Rendering
{
    public class GraphicObject
    {
        private static readonly GLShaderUniformId s_LocalToWorldId = GLShaderUniformId.FromName("_LocalToWorld");

        public bool IsTransparent { get; set; }
        public bool CastShadows { get; set; }
        public int QueueOrder { get; set; }

        public GLMesh Mesh { get; set; }
        public GLShader Shader { get; set; }
        public Transform Transform { get; set; }

        public void Draw(DrawingSettings settings)
        {
            var shader = settings.overrideShader != null ? settings.overrideShader : Shader;

            if (shader != null && Mesh != null)
            {
                shader.Use();
                shader.SetMatrix(s_LocalToWorldId, Transform.LocalToWorldMatrix);
                Mesh.Draw();
            }
        }
    }
}

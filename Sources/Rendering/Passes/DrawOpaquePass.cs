using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.Numerics;

namespace GLSample.Rendering
{
    public class DrawOpaquePass : GLRenderPass
    {
        private Renderer _renderer;

        public DrawOpaquePass(Renderer renderer) : base(renderer.GL) 
        {
            _renderer = renderer;
        }

        public override void ConfigureTargets()
        {
            colorAttachments = new List<ColorAttachment>()
            {
                new ColorAttachment()
                {
                    target = _renderer.CameraColorBuffer,
                }
            };

            depthAttachment = new DepthAttachment()
            {
                target = _renderer.CameraDepthBuffer
            };
        }

        public override void Render()
        {
            // Clear Depth and Color
            var clearValue = new Vector4(0, 0, 0, 0);
            gl.ClearColor(clearValue.X, clearValue.Y, clearValue.Z, clearValue.W);
            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Setup Depth State
            gl.DepthMask(true);
            gl.DepthFunc(DepthFunction.Lequal);
            gl.Enable(EnableCap.DepthTest);

            _renderer.OpaqueList.Draw(new DrawingSettings());
        }
    }
}

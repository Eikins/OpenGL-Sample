using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.Numerics;

namespace GLSample.Rendering
{
    public struct ColorAttachment
    {
        public GLTexture target;
        public int mipLevel;
    }

    public struct DepthAttachment
    {
        public GLTexture target;
        public bool clearBeforePass;
    }

    public abstract class GLRenderPass
    {
        protected List<ColorAttachment> colorAttachments;
        protected DepthAttachment? depthAttachment;
        protected GL gl;

        private uint _framebufferHandle;

        public GLRenderPass(GL gl) { this.gl = gl; }

        public abstract void ConfigureTargets();
        public abstract void Render();

        public void ExecutePass()
        {
            // bind the fbo and draw.
            gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _framebufferHandle);
            Render();
        }

        public void Initialize()
        {
            ConfigureTargets();

            // Create corresponding framebuffer object
            _framebufferHandle = gl.CreateFramebuffer();
            var activeColorBuffers = new GLEnum[colorAttachments.Count];
            
            // Attach the textures
            for (int i = 0; i < colorAttachments.Count; i++)
            {
                var slot = (GLEnum)(int)GLEnum.ColorAttachment0 + i;
                activeColorBuffers[i] = slot;

                var attachment = colorAttachments[i];
                if (attachment.target != null)
                {
                    gl.NamedFramebufferTexture(_framebufferHandle, slot, attachment.target.Handle, attachment.mipLevel);
                }
            }

            // Declare the draw buffers. In this case, we take the color attachment order.
            gl.NamedFramebufferDrawBuffers(_framebufferHandle, activeColorBuffers);

            if (depthAttachment != null)
            {
                gl.NamedFramebufferTexture(
                    _framebufferHandle,
                    FramebufferAttachment.DepthAttachment,
                    depthAttachment.Value.target.Handle,
                    0
                );
            }

            // Check the framebuffer status.
            GLEnum status = gl.CheckNamedFramebufferStatus(_framebufferHandle, FramebufferTarget.DrawFramebuffer);
            if (status != GLEnum.FramebufferComplete)
            {
                throw new System.Exception($"Invalid framebuffer for pass : {this}");
            }
        }
    }
}

using GLSample.Core;
using GLSample.Sources.Rendering;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace GLSample.Rendering
{
    public class Renderer : IDisposable
    {
        public DrawList OpaqueList { get; }  = new DrawList("Opaque Objects");
        public DrawList TransparentList { get; } = new DrawList("Transparent Objects");
        public DrawList ShadowCasterList { get; } = new DrawList("Shadow Casters");

        public List<GraphicObject> SceneObjects { get; } = new List<GraphicObject>(64);
        public GL GL => _gl;

        private GL _gl;
        private ImGuiController _imGuiController;
        private GLPerFrameUniformBuffer _perFrameUniformBuffer;

        public GLTexture CameraColorBuffer { get; private set; }
        public GLTexture CameraDepthBuffer { get; private set; }

        private DrawOpaquePass _drawOpaquePass;
        private MysteryPass _mysteryPass;

        private uint _screenWidth, _screenHeight;

        public Renderer(GL gl, ImGuiController imGuiController) 
        {
            _gl = gl;
            _imGuiController = imGuiController;
        }

        public void Initialize(uint width, uint height)
        {
            _screenWidth = width;
            _screenHeight = height;

#if DEBUG
            SetupDebugCallback();
#endif

            _perFrameUniformBuffer = new GLPerFrameUniformBuffer(_gl);

            CameraColorBuffer = new GLTexture(_gl, new GLTextureDescriptor(width, height, SizedInternalFormat.Rgba8));
            CameraDepthBuffer = new GLTexture(_gl, new GLTextureDescriptor(width, height, SizedInternalFormat.DepthComponent24));

            _drawOpaquePass = new DrawOpaquePass(this);
            _mysteryPass = new MysteryPass(GL);

            _drawOpaquePass.Initialize();
            _mysteryPass.Initialize();
        }

        public void Dispose()
        {
            _perFrameUniformBuffer.Dispose();
        }

        public void RenderScene(Camera camera)
        {
            UpdateDrawLists();
            SetupPerFrameConstants(camera);

            _gl.Viewport(0, 0, _screenWidth, _screenHeight);

            _drawOpaquePass.ExecutePass();
            _mysteryPass.ExecutePass();

            _imGuiController.Render();

            // Finally, present to the screen.
            _gl.BlitNamedFramebuffer(_drawOpaquePass.FramebufferHandle, 0, 
                0, 0, (int) _screenWidth, (int) _screenHeight, 
                0, 0, (int) _screenWidth, (int) _screenHeight, 
                ClearBufferMask.ColorBufferBit,
                BlitFramebufferFilter.Nearest);
        }

        private void SetupPerFrameConstants(Camera camera)
        {
            Matrix4x4.Invert(camera.Transform.LocalToWorldMatrix, out var viewMatrix);
            var projectionMatrix = camera.ProjectionMatrix;

            var perFrameConstants = new GLPerFrameUniformBuffer.Constants()
            {
                viewProjectionMatrix = viewMatrix * projectionMatrix,
                lightDirection = Vector3.Normalize(new Vector3(1, 1, 1))
            };

            _perFrameUniformBuffer.UpdateConstants(perFrameConstants);
        }

        public void UpdateDrawLists()
        {
            OpaqueList.Clear();
            TransparentList.Clear();
            ShadowCasterList.Clear();

            foreach (var obj in SceneObjects)
            {
                if (obj.IsTransparent)
                {
                    TransparentList.Add(obj);
                }
                else
                {
                    OpaqueList.Add(obj);
                }

                if (obj.CastShadows)
                {
                    ShadowCasterList.Add(obj);
                }
            }
        }

#if DEBUG
        private void SetupDebugCallback()
        {
            unsafe
            {
                DebugProc callback = (source, type, id, severity, length, message, userParam) =>
                {
                    if ((int)severity == (int)DebugSeverity.DebugSeverityNotification)
                        return;

                    var messageStr = Marshal.PtrToStringAnsi(message, length);
                    Console.WriteLine($"{source}:{type}[{severity}]({id}) {messageStr}");
                };
                _gl.DebugMessageCallback(callback, null);
                _gl.DebugMessageControl(DebugSource.DontCare, DebugType.DontCare, DebugSeverity.DontCare, 0, null, true);
            }
        }
#endif
    }
}

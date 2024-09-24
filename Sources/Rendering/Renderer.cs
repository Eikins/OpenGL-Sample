using GLSample.AssetLoaders;
using GLSample.Core;
using GLSample.Sources.Core;
using GLSample.Sources.Rendering;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using System;
using System.Collections.Generic;
using System.IO;
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
        private GLPerFrameUniformBuffer _perFrameUniformBuffer;

        public GLTexture CameraColorBuffer { get; private set; }
        public GLTexture CameraDepthBuffer { get; private set; }
        public GLTexture PostProcessColorBuffer { get; private set; }

        private DrawOpaquePass _drawOpaquePass;
        private PostProcessPass _postProcessPass;
        private ImGuiPass _imGuiPass;
        private uint _screenWidth, _screenHeight;

        public Renderer(GL gl) 
        {
            _gl = gl;
        }

        public void Initialize(uint width, uint height, ImGuiController imGuiController = null, PostProcessing postProcessing = null)
        {
            _screenWidth = width;
            _screenHeight = height;

#if DEBUG
            SetupDebugCallback();
#endif

            _perFrameUniformBuffer = new GLPerFrameUniformBuffer(_gl);

            CameraColorBuffer = new GLTexture(_gl, new GLTextureDescriptor(width, height, SizedInternalFormat.Rgb8));
            CameraDepthBuffer = new GLTexture(_gl, new GLTextureDescriptor(width, height, SizedInternalFormat.DepthComponent24));
            PostProcessColorBuffer = new GLTexture(_gl, new GLTextureDescriptor(width, height, SizedInternalFormat.Rgb8));

            CameraColorBuffer.SetWrapMode(WrapMode.Clamp);

            var postProcessShader = new GLShader(_gl,
                ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/PostProcess.vertex.glsl")),
                ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/PostProcess.fragment.glsl")));

            _drawOpaquePass = new DrawOpaquePass(this);
            _postProcessPass = new PostProcessPass(_gl, CameraColorBuffer, PostProcessColorBuffer, postProcessShader, postProcessing);
            _imGuiPass = new ImGuiPass(this, imGuiController);

            _drawOpaquePass.Initialize();
            _imGuiPass.Initialize();
            _postProcessPass.Initialize();
        }

        public void Dispose()
        {
            _perFrameUniformBuffer.Dispose();
        }

        public void RenderScene(Camera camera)
        {
            UpdateDrawLists();
            SetupPerFrameConstants(camera);

            _drawOpaquePass.ExecutePass();
            _postProcessPass.ExecutePass();
            _imGuiPass.ExecutePass();

            // Finally, present to the screen.
            _gl.BlitNamedFramebuffer(_postProcessPass.FramebufferHandle, 0, 
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
                lightDirection = Vector3.Normalize(new Vector3(1, 1, 1)),
                time = Time.Value
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

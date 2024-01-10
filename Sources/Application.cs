﻿using GLSample.AssetLoaders;
using GLSample.Core;
using GLSample.Rendering;
using GLSample.Sources.Rendering;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace GLSample
{
    class Application
    {
        private const int kDefaultWidth = 800;
        private const int kDefaultHeight = 600;

        private static void Main(string[] args)
        {
            var app = new Application();
            app.Start();
        }

        // Context Objects
        private IWindow _window;
        private Camera _camera;
        private GL _gl;
        private IInputContext _inputContext;
        private ImGuiController _imGuiController;

        private Transform _sphereTransform;
        private GLMesh[] _sphereMeshes;
        private GLShader _shader;
        private GLTexture _sphereBaseColorTexture;
        private GLPerFrameUniformBuffer _perFrameUniformBuffer;
        private Vector3 _sphereColor = Vector3.One;

        private TestRenderPass _testRenderPass;

        public void Start()
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(kDefaultWidth, kDefaultHeight);
            options.Title = "OpenGL with Silk.NET";

            var flags = ContextFlags.ForwardCompatible;
#if DEBUG
            flags |= ContextFlags.Debug;
#endif

            options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, flags, new APIVersion(4, 6));
            options.PreferredDepthBufferBits = 24;

            _window = Window.Create(options);
            _window.Load += OnLoad;
            _window.Render += OnRender;
            _window.Update += OnUpdate;
            _window.Closing += OnClose;
            _window.FramebufferResize += OnResize;

            _window.Run();
            _window.Dispose();
        }

        private void OnResize(Vector2D<int> newSize)
        {
            _camera.AspectRatio = (float)newSize.X / newSize.Y;
            _gl.Viewport(0, 0, (uint) newSize.X, (uint) newSize.Y);
        }

        private unsafe void OnLoad()
        {
            _gl = _window.CreateOpenGL();
            _inputContext = _window.CreateInput();
            _imGuiController = new ImGuiController(_gl, _window, _inputContext);

            for (int i = 0; i < _inputContext.Keyboards.Count; i++)
            {
                _inputContext.Keyboards[i].KeyDown += KeyDown;
            }

#if DEBUG
            SetupDebugCallback();
#endif

            CreateScene();

            _testRenderPass = new TestRenderPass(_gl);
            _testRenderPass.Initialize();
        }

        private void CreateScene()
        {
            _perFrameUniformBuffer = new GLPerFrameUniformBuffer(_gl);

            _camera = new Camera();
            _camera.AspectRatio = (float)kDefaultWidth / kDefaultHeight;
            _camera.Transform.Position = new Vector3(0, 0, 10);
            _camera.Transform.EulerAngles = new Vector3(0, 0, 0);

            // All spheres share the same transform.
            _sphereTransform = new Transform();
            _sphereTransform.EulerAngles = new Vector3(-90.0f, 0.0f, 0.0f);
            _sphereTransform.Scale = new Vector3(0.4f, 0.4f, 0.4f);

            _sphereBaseColorTexture = TextureLoader.LoadRGBA8Texture2DFromFile(_gl, "Assets/Textures/Spheres_BaseColor.png");

            var vertexSource = ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/Sphere.vertex.glsl"));
            var fragmentSource = ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/Sphere.fragment.glsl"));
            _shader = new GLShader(_gl, vertexSource, fragmentSource);
            _sphereMeshes = AssimpLoader.LoadMeshes(_gl, "Assets/MetalRoughSpheres.gltf");
        }

        private void OnUpdate(double deltaTime) { }

        private void OnRender(double deltaTime)
        {
            _imGuiController.Update((float) deltaTime);

            // ImGui Example : change sphere color.
            ImGui.ColorEdit3("Color", ref _sphereColor);

            unsafe
            {
                SetupPerFrameConstants();

                _gl.Viewport(0, 0, kDefaultWidth, kDefaultHeight);
                _gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
                _gl.ClearColor(0, 0, 0, 0);
                _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                // Setup Depth State
                _gl.DepthMask(true);
                _gl.DepthFunc(DepthFunction.Lequal);
                _gl.Enable(EnableCap.DepthTest);

                _shader.Use();
                _shader.SetVector("_Color", new Vector4(_sphereColor, 1.0f));
                _shader.SetTexture("_BaseColor", _sphereBaseColorTexture, 0);
                _shader.SetMatrix("_LocalToWorld", _sphereTransform.LocalToWorldMatrix);

                foreach (var mesh in _sphereMeshes)
                {
                    mesh.Draw();
                }

                _imGuiController.Render();

                _testRenderPass.ExecutePass();
            }
        }

        private void SetupPerFrameConstants()
        {
            Matrix4x4.Invert(_camera.Transform.LocalToWorldMatrix, out var viewMatrix);
            var projectionMatrix = _camera.ProjectionMatrix;

            var perFrameConstants = new GLPerFrameUniformBuffer.Constants()
            {
                viewProjectionMatrix = viewMatrix * projectionMatrix,
                lightDirection = Vector3.Normalize(new Vector3(1, 1, 1))
            };

            _perFrameUniformBuffer.UpdateConstants(perFrameConstants);
        }

        private void OnClose()
        {
            _sphereBaseColorTexture.Dispose();
            foreach (var mesh in _sphereMeshes) { mesh.Dispose(); }
            _perFrameUniformBuffer.Dispose();
            _imGuiController.Dispose();
            _inputContext.Dispose();
            _gl.Dispose();
        }

        private void KeyDown(IKeyboard arg1, Key arg2, int arg3)
        {
            if (arg2 == Key.Escape)
            {
                _window.Close();
            }
        }

#if DEBUG
        private void SetupDebugCallback()
        {
            unsafe
            {
                DebugProc callback = (source, type, id, severity, length, message, userParam) =>
                {
                    if ((int) severity == (int) DebugSeverity.DebugSeverityNotification)
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
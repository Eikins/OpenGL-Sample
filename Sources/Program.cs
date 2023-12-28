using GLSample.AssetLoaders;
using GLSample.Core;
using GLSample.Rendering;
using GLSample.Sources.Rendering;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace GLSample
{
    class Program
    {
        private const int kDefaultWidth = 800;
        private const int kDefaultHeight = 600;

        private static IWindow s_Window;
        private static Camera s_Camera;
        private static GL s_GL;

        private static Transform s_SphereTransform;
        private static GLShader s_Shader;
        private static GLMesh[] s_SphereMeshes;
        private static GLTexture s_SphereBaseColorTexture;
        private static GLPerFrameUniformBuffer s_PerFrameUniformBuffer;
        private static GLPerFrameUniformBuffer.Constants s_PerFrameConstants;

        private static void Main(string[] args)
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(kDefaultWidth, kDefaultHeight);
            options.Title = "OpenGL with Silk.NET";
            options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug, new APIVersion(4, 6));
            options.PreferredDepthBufferBits = 24;

            s_Window = Window.Create(options);

            s_Window.Load += OnLoad;
            s_Window.Render += OnRender;
            s_Window.Update += OnUpdate;
            s_Window.Closing += OnClose;
            s_Window.Resize += OnResize;

            s_Window.Run();
            s_Window.Dispose();
        }

        private static void OnResize(Vector2D<int> newSize)
        {
            s_Camera.AspectRatio = (float)newSize.X / newSize.Y;
            s_GL.Viewport(0, 0, (uint) newSize.X, (uint) newSize.Y);
        }

        private static unsafe void OnLoad()
        {
            IInputContext input = s_Window.CreateInput();
            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += KeyDown;
            }

            s_GL = s_Window.CreateOpenGL();

            unsafe
            {
                DebugProc callback = (source, type, id, severity, length, message, userParam) =>
                {
                    var sourceStr = (DebugSource)source;
                    var typeStr = (DebugType)type;
                    var severityStr = (DebugSeverity)severity;
                    var messageStr = Marshal.PtrToStringAnsi(message, length);
                    Console.Write($"{source}:{type}[{severity}]({id}) {messageStr}");
                };
                s_GL.DebugMessageCallback(callback, null);
                s_GL.DebugMessageControl(DebugSource.DontCare, DebugType.DontCare, DebugSeverity.DontCare, 0, null, true);
            }

            s_Camera = new Camera();
            s_Camera.AspectRatio = (float) kDefaultWidth / kDefaultHeight;
            s_Camera.Transform.Position = new Vector3(0, 0, 10);
            s_Camera.Transform.EulerAngles = new Vector3(0, 0, 0);

            s_PerFrameUniformBuffer = new GLPerFrameUniformBuffer(s_GL);
            s_PerFrameConstants = new GLPerFrameUniformBuffer.Constants();

            // All spheres share the same transform.
            s_SphereTransform = new Transform();
            s_SphereTransform.EulerAngles = new Vector3(-90.0f, 0.0f, 0.0f);
            s_SphereTransform.Scale = new Vector3(0.4f, 0.4f, 0.4f);

            s_SphereBaseColorTexture = TextureLoader.LoadRGBA8Texture2DFromFile(s_GL, "Assets/Textures/Spheres_BaseColor.png");

            var vertexSource = ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/Sphere.vertex.glsl"));
            var fragmentSource = ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/Sphere.fragment.glsl"));
            s_Shader = new GLShader(s_GL, vertexSource, fragmentSource);
            s_SphereMeshes = AssimpLoader.LoadMeshes(s_GL, "Assets/MetalRoughSpheres.gltf");
        }

        private static void OnRender(double deltaTime)
        {
            unsafe
            {
                SetupPerFrameConstants();

                s_GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                // Setup Depth State
                s_GL.DepthMask(true);
                s_GL.DepthFunc(DepthFunction.Lequal);
                s_GL.Enable(EnableCap.DepthTest);

                s_Shader.Use();
                s_Shader.SetVector("_Color", Vector4.One);
                s_Shader.SetTexture("_BaseColor", s_SphereBaseColorTexture, 0);
                s_Shader.SetMatrix("_LocalToWorld", s_SphereTransform.LocalToWorldMatrix);

                foreach (var mesh in s_SphereMeshes)
                {
                    mesh.Draw();
                }
            }
        }

        private static void SetupPerFrameConstants()
        {
            Matrix4x4.Invert(s_Camera.Transform.LocalToWorldMatrix, out var viewMatrix);
            var projectionMatrix = s_Camera.ProjectionMatrix;

            s_PerFrameConstants.viewProjectionMatrix = viewMatrix * projectionMatrix;
            s_PerFrameUniformBuffer.UpdateConstants(s_PerFrameConstants);
        }

        private static void OnUpdate(double deltaTime) { }

        private static void OnClose()
        {
            s_SphereBaseColorTexture.Dispose();
            foreach (var mesh in s_SphereMeshes) { mesh.Dispose(); }
            s_PerFrameUniformBuffer.Dispose();
        }

        private static void KeyDown(IKeyboard arg1, Key arg2, int arg3)
        {
            if (arg2 == Key.Escape)
            {
                s_Window.Close();
            }
        }
    }
}
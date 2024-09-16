using GLSample.AssetLoaders;
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
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace GLSample
{
    class Application
    {
        private const int kDefaultWidth = 1280;
        private const int kDefaultHeight = 720;

        private static void Main(string[] args)
        {
            var app = new Application();
            app.Start();
        }

        // Context Objects
        private IWindow _window;
        private Camera _camera;
        private IInputContext _inputContext;
        private GL _gl;
        private ImGuiController _imGuiController;
        private Renderer _renderer;

        private GraphicObject[] _sphereObjects;
        private GLTexture _sphereBaseColorTexture;
        private Vector3 _sphereColor = Vector3.One;

        public void Start()
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(kDefaultWidth, kDefaultHeight);
            options.Title = "OpenGL with Silk.NET";
            options.WindowBorder = WindowBorder.Fixed;

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
            _renderer = new Renderer(_gl);
            _renderer.Initialize(kDefaultWidth, kDefaultHeight, _imGuiController);

            for (int i = 0; i < _inputContext.Keyboards.Count; i++)
            {
                _inputContext.Keyboards[i].KeyDown += KeyDown;
            }

            CreateScene();
        }

        private void CreateScene()
        {
            _camera = new Camera();
            _camera.AspectRatio = (float)kDefaultWidth / kDefaultHeight;
            _camera.Transform.Position = new Vector3(0, 0, 10);
            _camera.Transform.EulerAngles = new Vector3(0, 0, 0);

            // All spheres share the same transform.
            var sphereTransform = new Transform();
            sphereTransform.EulerAngles = new Vector3(-90.0f, 0.0f, 0.0f);
            sphereTransform.Scale = new Vector3(0.4f, 0.4f, 0.4f);

            _sphereBaseColorTexture = TextureLoader.LoadRGBA8Texture2DFromFile(_gl, "Assets/Textures/Spheres_BaseColor.png");

            var vertexSource = ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/Sphere.vertex.glsl"));
            var fragmentSource = ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/Sphere.fragment.glsl"));
            var sphereShader = new GLShader(_gl, vertexSource, fragmentSource);
            var sphereMeshes = AssimpLoader.LoadMeshes(_gl, "Assets/MetalRoughSpheres.gltf");

            _sphereObjects = sphereMeshes.Select(mesh => new GraphicObject
            {
                IsTransparent = false,
                CastShadows = false,
                QueueOrder = 0,
                Mesh = mesh,
                Shader = sphereShader,
                Transform = sphereTransform
            }).ToArray();

            _renderer.SceneObjects.AddRange(_sphereObjects);
        }

        private void OnUpdate(double deltaTime) { }

        private void OnRender(double deltaTime)
        {
            _imGuiController.Update((float) deltaTime);

            // ImGui Example : change sphere color.
            // All sphere share the same shader.
            ImGui.ColorEdit3("Color", ref _sphereColor);
            _sphereObjects[0].Shader.SetVector("_Color", new Vector4(_sphereColor, 1.0f));
            _sphereObjects[0].Shader.SetTexture("_BaseColor", _sphereBaseColorTexture, 0);

            _renderer.RenderScene(_camera);
        }

        private void OnClose()
        {
            _sphereBaseColorTexture.Dispose();
            _sphereObjects[0].Shader.Dispose();
            foreach (var sphereObject in _sphereObjects) 
            {
                sphereObject.Mesh.Dispose();
            }
            _renderer.Dispose();
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
    }
}
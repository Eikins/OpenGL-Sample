using GLSample.AssetLoaders;
using GLSample.Core;
using GLSample.Rendering;
using GLSample.Sources.Core;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Numerics;

namespace GLSample
{
    class Application
    {
        private const int kDefaultWidth = 1920;
        private const int kDefaultHeight = 1040;

        private static void Main(string[] args)
        {
            var app = new Application();
            app.Start();
        }

        // Context Objects
        private IWindow _window;
        private Camera _camera;
        private CameraController _cameraController;
        private IInputContext _inputContext;
        private GL _gl;
        private ImGuiController _imGuiController;
        private Renderer _renderer;

        // Scene objects
        private GraphicObject _sealObject;
        private GraphicObject _terrainObject;
        private GLTexture _terrainHeightmap;
        private GLTexture _prototypeTexture;
        private GLTexture _sealAlbedoTexture;
        private PostProcessing _postProcessing = new();

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
            _renderer.Initialize(kDefaultWidth, kDefaultHeight, _imGuiController, _postProcessing);

            foreach (var keyboard in _inputContext.Keyboards)
            {
                keyboard.KeyDown += KeyDown;
                keyboard.KeyDown += (keyboard, key, state) => _cameraController.OnKeyDown(key);
                keyboard.KeyUp += (keyboard, key, state) => _cameraController.OnKeyUp(key);
            }

            foreach (var mice in _inputContext.Mice)
            {
                mice.MouseMove += (mouse, delta) => _cameraController.OnMouseMove(delta);
            }

            CreateScene();
        }

        private void CreateScene()
        {
            _camera = new Camera();
            _camera.AspectRatio = (float)kDefaultWidth / kDefaultHeight;
            _camera.Transform.Position = new Vector3(0, 0.3f, 2);
            _camera.Transform.EulerAngles = new Vector3(0, 0, 0);

            _cameraController = new CameraController(_camera);

            // All spheres share the same transform.
            var sphereTransform = new Transform();
            sphereTransform.EulerAngles = new Vector3(-90.0f, 0.0f, 0.0f);
            sphereTransform.Scale = new Vector3(0.4f, 0.4f, 0.4f);

            _terrainHeightmap = TextureLoader.LoadHeightTexture(_gl, "Assets/Textures/heightmap.jpeg");
            _prototypeTexture = TextureLoader.LoadRGBA8Texture2DFromFile(_gl, "Assets/Textures/tex_proto.png");
            _sealAlbedoTexture = TextureLoader.LoadRGBA8Texture2DFromFile(_gl, "Assets/Textures/seal_albedo.jpg");

            var vertexSourceInstanced = ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/StandardInstanced.vertex.glsl"));
            var vertexSource = ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/Standard.vertex.glsl"));
            var fragmentSource = ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/Standard.fragment.glsl"));
            var sealShader = new GLShader(_gl, vertexSourceInstanced, fragmentSource);

            var planeMesh = AssimpLoader.LoadMeshes(_gl, "Assets/Models/plane.fbx")[0];


            _prototypeTexture.SetWrapMode(WrapMode.Repeat);

            var terrainVertexSource = ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/Terrain.vertex.glsl"));
            var terrainFragmentSource = ShaderPreProcessor.ProcessShaderSource(File.ReadAllText("Assets/Shaders/Terrain.fragment.glsl"));
            var terrainShader = new GLShader(_gl, terrainVertexSource, terrainFragmentSource);
            _terrainObject = new GraphicObject
            {
                IsTransparent = false,
                CastShadows = false,
                QueueOrder = 0,
                Mesh = planeMesh,
                Shader = terrainShader,
                Transform = sphereTransform
            };

            _sealObject = new GraphicObject
            {
                CastShadows = true,
                IsTransparent = false,
                QueueOrder = 10,
                Transform = new Transform(),
                Mesh = AssimpLoader.LoadMeshes(_gl, "Assets/Models/seal.obj", true)[0],
                Shader = sealShader
            };

            var random = new Random();
            var heightmap = Image.Load<L8>("Assets/Textures/heightmap.jpeg");
            var instances = new Vector4[5000];
            for (int i = 0; i < instances.Length; i++)
            {
                var x = random.NextSingle() * 500;
                var z = random.NextSingle() * 500;
                var y = (heightmap[(int) (x / 500 * 1920), (int) (x / 500 * 1920)].PackedValue / 255f) * 30f + 5f;

                x -= 250;
                z -= 250;

                instances[i] = new Vector4(x, y, z, random.NextSingle() * MathF.Tau);
            }

            heightmap.Dispose();

            _sealObject.SetInstances(_gl, instances);

            _renderer.SceneObjects.Add(_terrainObject);
            _renderer.SceneObjects.Add(_sealObject);
        }

        private void OnUpdate(double deltaTime) 
        {
            Time.Value += (float) deltaTime;
            _cameraController.Update((float) deltaTime);

            _sealObject.Transform.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)deltaTime * 2 * MathF.PI);
        }

        private void OnRender(double deltaTime)
        {
            _imGuiController.Update((float) deltaTime);

            ImGui.Text($"Framerate: {1000.0f / ImGui.GetIO().Framerate:0.#} ms/frame ({ImGui.GetIO().Framerate:0.} FPS)");

            _postProcessing.RenderImGui();

            _terrainObject.Shader.SetVector("_Color", Vector4.One);
            _terrainObject.Shader.SetTexture("_HeightMap", _terrainHeightmap, 0);
            _terrainObject.Shader.SetTexture("_BaseColor", _prototypeTexture, 1);

            _sealObject.Shader.SetVector("_Color", Vector4.One);
            _sealObject.Shader.SetTexture("_BaseColor", _sealAlbedoTexture, 2);

            _renderer.RenderScene(_camera);
        }

        private void OnClose()
        {
            _terrainHeightmap.Dispose();
            _terrainObject.Shader.Dispose();
            _terrainObject.Mesh.Dispose();
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
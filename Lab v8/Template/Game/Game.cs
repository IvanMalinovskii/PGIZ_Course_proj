using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Resources;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using Template.Properties; // For work with resources of project Assembly.Properties. Then we can access to all inside Resorces.resx.
using Template.Graphics;
using System.Drawing;

namespace Template
{
    class Game : IDisposable
    {      
        // TODO: HUD to separate class.
        public struct HUDResources
        {
            public int textFPSTextFormatIndex;
            public int textFPSBrushIndex;
            public int armorIconIndex;
        }

        /// <summary>Main form of application.</summary>
        private RenderForm _renderForm;

        /// <summary>Flag if render form resized by user.</summary>
        //private bool _renderFormUserResized;

        /// <summary>DirectX 3D graphics objects.</summary>
        private DirectX3DGraphics _directX3DGraphics;

        /// <summary>Renderer.</summary>
        private Renderer _renderer;

        /// <summary>DirectX 2D graphic object.</summary>
        private DirectX2DGraphics _directX2DGraphics;

        private SamplerStates _samplerStates;
        private Textures _textures;
        private Materials _materials;
        private Illumination _illumination;
        private MeshObject _floor;
        private MeshObject _cube;
        private MeshObject _cube2;
        /// <summary>List of objects with meshes.</summary>
        private MeshObjects _meshObjects;

        /// <summary>HUD resources.</summary>
        private HUDResources _HUDResources;

        /// <summary>Flag for display help.</summary>
        private bool _displayHelp;
        private string _helpString;

        /// <summary>Character.</summary>
        private Character _character;

        /// <summary>Camera object.</summary>
        private Camera _camera;

        /// <summary>Projection matrix.</summary>
        private Matrix _projectionMatrix;

        /// <summary>View matrix.</summary>
        private Matrix _viewMatrix;

        /// <summary>Camera angular ratation step for moving mouse by 1 pixel.</summary>
        private float _angularCameraRotationStep;

        /// <summary>Input controller.</summary>
        private InputController _inputController;

        /// <summary>Time helper object for current time and delta time measurements.</summary>
        private TimeHelper _timeHelper;

        private Random _random;

        /// <summary>First run flag for create DirectX buffers before render in first time.</summary>
        private bool _firstRun = true;

        /// <summary>Init HUD resources.</summary>
        /// <remarks>Create text format, text brush and armor icon.</remarks>
        private void InitHUDResources()
        {
            _HUDResources.textFPSTextFormatIndex = _directX2DGraphics.NewTextFormat("Input", SharpDX.DirectWrite.FontWeight.Normal,
                SharpDX.DirectWrite.FontStyle.Normal, SharpDX.DirectWrite.FontStretch.Normal, 12,
                SharpDX.DirectWrite.TextAlignment.Leading, SharpDX.DirectWrite.ParagraphAlignment.Near);
            _HUDResources.textFPSBrushIndex = _directX2DGraphics.NewSolidColorBrush(new SharpDX.Mathematics.Interop.RawColor4(1.0f, 1.0f, 0.0f, 1.0f));
            _HUDResources.armorIconIndex = _directX2DGraphics.LoadBitmapFromFile("Resources\\armor.bmp");  // Don't use before Resizing. Bitmaps loaded, but not created.
        }
        private List<MeshObject> meshObjects;
        /// <summary>
        /// Constructor. Initialize all objects.
        /// </summary>
        public Game()
        {
            //_gameState = GameState.BeforeStart;
            _helpString = Resources.HelpString;
            meshObjects = new List<MeshObject>();
            // Initialization order:
            // 1. Render form.
            _renderForm = new RenderForm("SharpDX");
            _renderForm.UserResized += RenderFormResizedCallback;
            _renderForm.Activated += RenderFormActivatedCallback;
            _renderForm.Deactivate += RenderFormDeactivateCallback;
            // 2. DirectX 3D.
            _directX3DGraphics = new DirectX3DGraphics(_renderForm);
            // 3. Renderer.
            _renderer = new Renderer(_directX3DGraphics);
            _renderer.CreateConstantBuffers();
            // 4. DirectX 2D.
            _directX2DGraphics = new DirectX2DGraphics(_directX3DGraphics);
            // 5. Load materials
            Loader loader = new Loader(_directX3DGraphics, _directX2DGraphics, _renderer, _directX2DGraphics.ImagingFactory);
            _samplerStates = new SamplerStates(_directX3DGraphics);
            _textures = new Textures();
            _textures.Add(loader.LoadTextureFromFile("Resources\\floor.png", false, _samplerStates.Colored));
            _textures.Add(loader.LoadTextureFromFile("Resources\\white.bmp", false, _samplerStates.Colored));
            _renderer.SetWhiteTexture(_textures["white.bmp"]);
            _textures.Add(loader.LoadTextureFromFile("Resources\\stone_wall.png", false, _samplerStates.Textured));
            _textures.Add(loader.LoadTextureFromFile("Resources\\delorean.png", false, _samplerStates.Textured));
            _materials = loader.LoadMaterials("Resources\\materials.txt", _textures);
            // 6. Load meshes.
            _meshObjects = new MeshObjects();
            _floor = loader.LoadMeshObject("Resources\\floor.txt", _materials);
            _meshObjects.Add(_floor);
            _cube = loader.LoadMeshObject("Resources\\cube.txt", _materials);
            //_cube.MoveBy(0.0f, 2.0f, 0.0f);
            _meshObjects.Add(_cube);
            //_cube2 = loader.LoadMeshFromObject("Resources\\box.obj", _materials[2]);
            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("-----------------------------------------------");
            //_meshObjects.Add(_cube2);
            List<MeshObject> meshes = loader.LoadMeshesFromObject("Resources\\delorian.obj", _materials[3]);
            foreach (var obj in meshes)
            {
                meshObjects.Add(obj);
            }
            // 6. Load HUD resources into DirectX 2D object.
            InitHUDResources();

            
            _illumination = new Illumination(Vector4.Zero, new Vector4(1.0f, 1.0f, 0.9f, 1.0f), new LightSource[]
            {
                new LightSource(LightSource.LightType.DirectionalLight,
                    new Vector4(0.0f, 20.0f, 0.0f, 1.0f),   // Position
                    new Vector4(1.0f, -1.0f, 0.0f, 1.0f),   // Direction
                    new Vector4(1.0f, 1.0f, 1.0f, 1.0f),    // Color
                    0.0f,                                   // Spot angle
                    1.0f,                                   // Const atten
                    0.0f,                                   // Linear atten
                    0.0f,                                   // Quadratic atten
                    1),
                new LightSource(LightSource.LightType.SpotLight,
                    new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                    new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    PositionalObject.HALF_PI / 4.0f,
                    1.0f,
                    0.05f,
                    0.01f,
                    1),
                new LightSource(LightSource.LightType.PointLight,
                    new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    Vector4.Zero,
                    new Vector4(-4.0f, 1.0f, 0.0f, 1.0f),
                    1.0f,
                    1.0f,
                    0.05f,
                    0.005f,
                    0),
                new LightSource(),
                new LightSource(),
                new LightSource(),
                new LightSource(),
                new LightSource()
            });

            // Character and camera. X0Z - ground, 0Y - to up.
            _character = new Character(new Vector4(0.0f, 0.0f, 0.0f, 1.0f), 10.0f, _inputController); //********
            _camera = new Camera(new Vector4(-10.0f, 10.0f, 0.0f, 1.0f));
            _character.AttachMeshObject(_cube);
            _character.AttachLightSource(_illumination[1]);
            //_camera.AttachToObject(_character);

            // Input controller and time helper.
            _inputController = new InputController(_renderForm);
            _timeHelper = new TimeHelper();
            _random = new Random();
            
            Vector4 initialPosition = new Vector4(20.5f, 1f, 20.5f, 1);
            Vector4 position = initialPosition;
            Bitmap bmp = new Bitmap("Resources\\map.bmp");
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    position = new Vector4(initialPosition.X - x * 2, 1f, initialPosition.Z - y * 2, 1f);
                    System.Drawing.Color color = bmp.GetPixel(x, y);
                    Console.WriteLine(color);
                    if (color.R == 0 && color.G == 0 && color.B == 0)
                    {
                        MeshObject mesh = loader.LoadMeshObject("Resources\\wall.txt", _materials);
                        mesh.Position = position;
                        meshObjects.Add(mesh);
                    }
                    Console.WriteLine($"x={x} y={y}");
                    Console.WriteLine(position);           
                }
            }
            loader = null;
        }

        /// <summary>Render form activated callback. Hide cursor.</summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="args">Arguments.</param>
        public void RenderFormActivatedCallback(object sender, EventArgs args)
        {
            Cursor.Hide();
        }

        /// <summary>Render form deactivate event callback. Show cursor.</summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="args">Arguments.</param>
        public void RenderFormDeactivateCallback(object sender, EventArgs args)
        {
            Cursor.Show();
        }

        /// <summary>Render form user resized callback. Perform resizing of DirectX 3D object and renew camera rotation step and projection matrix.</summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="args">Arguments.</param>
        public void RenderFormResizedCallback(object sender, EventArgs args)
        {
            _directX3DGraphics.Resize();
            _camera.Aspect = _renderForm.ClientSize.Width / (float)_renderForm.ClientSize.Height;
            _angularCameraRotationStep = _camera.FOVY / _renderForm.ClientSize.Height;
            _projectionMatrix = _camera.GetProjectionMatrix();
        }

        /// <summary>Callback for RenderLoop.Run. Handle input and render scene.</summary>
        public void RenderLoopCallback()
        {
            if (_firstRun)
            {
                RenderFormResizedCallback(this, new EventArgs());
                _firstRun = false;
            }

            _timeHelper.Update();
            _inputController.UpdateKeyboardState();
            _inputController.UpdateMouseState();

            if (_inputController.KeyboardUpdated)
            {
                if (_inputController[SharpDX.DirectInput.Key.W]) _character.MoveByDirection(_timeHelper.DeltaT * _character.Speed, "forward");
                if (_inputController[SharpDX.DirectInput.Key.S]) _character.MoveByDirection(_timeHelper.DeltaT * _character.Speed, "back");
                if (_inputController[SharpDX.DirectInput.Key.D]) _character.MoveByDirection(_timeHelper.DeltaT * _character.Speed, "right");
                if (_inputController[SharpDX.DirectInput.Key.A]) _character.MoveByDirection(_timeHelper.DeltaT * _character.Speed, "left");
                if (_inputController.Esc) _renderForm.Close();                               // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // Toggle help by F1.
                if (_inputController.Func[0]) _displayHelp = !_displayHelp;
                // Switch solid and wireframe modes by F2, F3.
                if (_inputController.Func[1]) _directX3DGraphics.RenderMode = DirectX3DGraphics.RenderModes.Solid;
                if (_inputController.Func[2]) _directX3DGraphics.RenderMode = DirectX3DGraphics.RenderModes.Wireframe;
                // Toggle fullscreen mode by F4, F5.
                if (_inputController.Func[3]) _directX3DGraphics.IsFullScreen = false;
                if (_inputController.Func[4]) _directX3DGraphics.IsFullScreen = true;
                if (_inputController[SharpDX.DirectInput.Key.Down]) _camera.PitchBy(0.01f);
                if (_inputController[SharpDX.DirectInput.Key.Up]) _camera.PitchBy(-0.01f);
                if (_inputController[SharpDX.DirectInput.Key.Left]) _camera.YawBy(0.01f);
                if (_inputController[SharpDX.DirectInput.Key.Right]) _camera.YawBy(-0.01f);
                if (_inputController[SharpDX.DirectInput.Key.H]) _camera.MoveRightBy(-0.5f);
                if (_inputController[SharpDX.DirectInput.Key.K]) _camera.MoveRightBy(+0.5f);
                if (_inputController[SharpDX.DirectInput.Key.J]) _camera.MoveForwardBy(-0.5f);
                if (_inputController[SharpDX.DirectInput.Key.U]) _camera.MoveForwardBy(0.5f);
                if (_inputController[SharpDX.DirectInput.Key.Y]) _camera.MoveUpBy(-0.5f);
                if (_inputController[SharpDX.DirectInput.Key.I]) _camera.MoveUpBy(0.5f);
            }

            _viewMatrix = _camera.GetViewMatrix();

            _renderer.BeginRender();

            _illumination.EyePosition = _camera.Position;
            LightSource light1 = _illumination[1];
            Vector4 pos = _character.Position;
            pos.Y += 10;
            light1.Position = pos;
            _illumination[1] = light1;
            _renderer.UpdateIlluminationProperties(_illumination);

            _renderer.SetPerObjectConstants(_timeHelper.Time, 0);//1);
            float angle = _timeHelper.Time * 2.0f * (float)Math.PI * 0.25f; // Frequency = 0.25 Hz
            _cube.Pitch = angle;

            float time = _timeHelper.Time;
            _renderer.SetPerObjectConstants(time, 0); //1);
            Matrix worldMatrix = _cube.GetWorldMatrix();
            //_renderer.UpdatePerObjectConstantBuffer(0, worldMatrix, _viewMatrix, _projectionMatrix);
            _cube.Render(_viewMatrix, _projectionMatrix);
            _renderer.SetPerObjectConstants(time, 0);
            worldMatrix = _floor.GetWorldMatrix();
            //_renderer.UpdatePerObjectConstantBuffer(0, worldMatrix, _viewMatrix, _projectionMatrix);
            _floor.Render(_viewMatrix, _projectionMatrix);


            //worldMatrix = _cube2.GetWorldMatrix();
            //_renderer.UpdatePerObjectConstantBuffer(0, worldMatrix, _viewMatrix, _projectionMatrix);
            //_cube2.Render();

            foreach (var mesh in meshObjects)
            {
                _renderer.SetPerObjectConstants(_timeHelper.Time, 0);//1);
                float angle1 = _timeHelper.Time * 2.0f * (float)Math.PI * 0.25f; // Frequency = 0.25 Hz
                mesh.Yaw = angle1;
                worldMatrix = mesh.GetWorldMatrix();
                //_renderer.UpdatePerObjectConstantBuffer(0, worldMatrix, _viewMatrix, _projectionMatrix);
                mesh.Render(_viewMatrix, _projectionMatrix);
            }

            RenderHUD();

            _renderer.EndRender();
        }

        /// <summary>Render HUD.</summary>
        private void RenderHUD()
        {
            string text = $"FPS: {_timeHelper.FPS,3:d2}\ntime: {_timeHelper.Time:f1}\n" +
                                $"MX: {_inputController.MouseRelativePositionX,3:d2} MY: {_inputController.MouseRelativePositionY,3:d2} MZ: {_inputController.MouseRelativePositionZ,4:d3}\n" +
                                $"LB: {(_inputController.MouseButtons[0] ? 1 : 0)} MB: {(_inputController.MouseButtons[2] ? 1 : 0)} RB: {(_inputController.MouseButtons[1] ? 1 : 0)}\n" +
                                $"Pos: {_character.Position.X,6:f1}, {_character.Position.Y,6:f1}, {_character.Position.Z,6:f1}\n" +
                                $"Camera_pos: {_camera.description.pos}\n" +
                                $"Camera_target: {_camera.description.target}\n" +
                                $"Camera_ip: {_camera.description.up}";
            if (_displayHelp) text += "\n\n" + _helpString;
            float armorWidthInDIP = _directX2DGraphics.Bitmaps[_HUDResources.armorIconIndex].Size.Width;
            float armorHeightInDIP = _directX2DGraphics.Bitmaps[_HUDResources.armorIconIndex].Size.Height;
            Matrix3x2 armorTransformMatrix = Matrix3x2.Translation(new Vector2(_directX2DGraphics.RenderTargetClientRectangle.Right - armorWidthInDIP - 1, 1));
            _directX2DGraphics.BeginDraw();
            _directX2DGraphics.DrawText(text, _HUDResources.textFPSTextFormatIndex,
                _directX2DGraphics.RenderTargetClientRectangle, _HUDResources.textFPSBrushIndex);
            _directX2DGraphics.DrawBitmap(_HUDResources.armorIconIndex, armorTransformMatrix, 1.0f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
            _directX2DGraphics.EndDraw();
        }

        /// <summary>Rum main render loop.</summary>
        public void Run()
        {
            RenderLoop.Run(_renderForm, RenderLoopCallback);
        }

        /// <summary>Realise all resources</summary>
        public void Dispose()
        {
            // MeshObjects disposing
            _textures.Dispose();
            _samplerStates.Dispose();
            _inputController.Dispose();
            _directX2DGraphics.Dispose();
            _renderer.Dispose();
            _directX3DGraphics.Dispose();
            _renderForm.Dispose();
        }
    }
}

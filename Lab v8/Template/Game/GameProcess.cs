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
using Template.Game.Services;
using Template.Sound;
using Template.Game.gameObjects.newServices;
using Template.Game.gameObjects.newObjects;

namespace Template
{
    class GameProcess : IDisposable
    {
        public struct HUDResources
        {
            public int textFPSTextFormatIndex;
            public int textFPSBrushIndex;
            public int armorIconIndex;
        }
        private CameraService cameraService;
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
        /// <summary>List of objects with meshes.</summary>
        private MeshObjects _meshObjects;

        /// <summary>HUD resources.</summary>
        private HUDResources _HUDResources;

        /// <summary>Flag for display help.</summary>
        private bool _displayHelp;
        private string _helpString;


        /// <summary>Camera object.</summary>
        //private Camera _camera;

        /// <summary>Projection matrix.</summary>
        private Matrix _projectionMatrix;

        /// <summary>View matrix.</summary>
        private Matrix _viewMatrix;

        /// <summary>Input controller.</summary>
        private InputController _inputController;

        /// <summary>Time helper object for current time and delta time measurements.</summary>
        private TimeHelper _timeHelper;

        /// <summary>First run flag for create DirectX buffers before render in first time.</summary>
        private bool _firstRun = true;
        private MainCharacterService mainCharacterService;
        private MapService mapService;
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
        /// <summary>
        /// Constructor. Initialize all objects.
        /// </summary>
        public GameProcess()
        {
            _helpString = Resources.HelpString;
            _meshObjects = new MeshObjects();
            _renderForm = new RenderForm("SharpDX");
            _renderForm.ClientSize = new Size(1500, 800);

            _renderForm.UserResized += RenderFormResizedCallback;
            _renderForm.Activated += RenderFormActivatedCallback;
            _renderForm.Deactivate += RenderFormDeactivateCallback;
            _directX3DGraphics = new DirectX3DGraphics(_renderForm);
            _renderer = new Renderer(_directX3DGraphics);
            _renderer.CreateConstantBuffers();
            _directX2DGraphics = new DirectX2DGraphics(_directX3DGraphics);
            Loader loader = new Loader(_directX3DGraphics, _directX2DGraphics, _renderer, _directX2DGraphics.ImagingFactory);
            _samplerStates = new SamplerStates(_directX3DGraphics);

            
            InitializeTextures(loader);
            _renderer.SetWhiteTexture(_textures["white.bmp"]);
            loader.StubTexture = _textures["white.bmp"];
            _materials = loader.LoadMaterials("Resources\\materials.txt", _textures);
            List<MeshObject> meshes = loader.LoadMeshesFromObject("Resources\\PlagueDoctor.obj", _materials[3]);

            //_meshObjects.AddRange(loader.LoadMeshesFromObject("Resources\\FloorObj.obj", _materials[2]));
            Vector3 initial = new Vector3(105f, 0, -105f);
            Vector4 pos = new Vector4(0, 0, 0, 0);
            //for (int i = 0; i < 15; i ++)
            //{
            //    for (int j = 0; j < 15; j++)
            //    {
            //        pos = new Vector4(initial.X - j * 15, 0, initial.Z + i * 15, 0f);
            //        List<MeshObject> meshObjects = loader.LoadMeshesFromObject("Resources\\FloorTile.obj", _materials[2]);
            //        meshObjects.ForEach(e => e.Position = pos);
            //        _meshObjects.AddRange(meshObjects);
            //    }
            //}
            InitHUDResources();
            InitializeLight();
            
            _inputController = new InputController(_renderForm);

            //Archer archer = new Archer(new Vector4(0, 0, 0, 0));
            //archer.AddMeshObjects(meshes);
            mainCharacterService = new MainCharacterService("some", _inputController, loader);
            mainCharacterService.AddMeshObjects(meshes);
            mapService = new MapService("some", loader, _materials[2]);
            _timeHelper = new TimeHelper();
            cameraService = new CameraService(new Camera(new Vector4(-116.0f, 84.0f, 0.0f, 1.0f)), _inputController);
            
            Vector4 initialPosition = new Vector4(20.5f, 1f, 20.5f, 1);
            Vector4 position = initialPosition;

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
            _projectionMatrix = cameraService.SetAfterResize(_renderForm.ClientSize.Width, _renderForm.ClientSize.Height);
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
            //_inputController.UpdateKeyboardState();
            _inputController.UpdateMouseState();

            UpdateKeyBoard();
            _viewMatrix = cameraService.GetViewMatrix();

            _renderer.BeginRender();

            _renderer.UpdateIlluminationProperties(_illumination);

            _renderer.SetPerObjectConstants(_timeHelper.Time, 0);//1);
            float angle = _timeHelper.Time * 2.0f * (float)Math.PI * 0.25f; // Frequency = 0.25 Hz
            //_cube.Pitch = angle;

            float time = _timeHelper.Time;
            _renderer.SetPerObjectConstants(time, 0); //1);
            //_cube.Render(_viewMatrix, _projectionMatrix);
            _renderer.SetPerObjectConstants(time, 0);
            //_floor.Render(_viewMatrix, _projectionMatrix);

            foreach (var mesh in _meshObjects)
            {
                _renderer.SetPerObjectConstants(_timeHelper.Time, 0);//1);
                float angle1 = _timeHelper.Time * 2.0f * (float)Math.PI * 0.25f; // Frequency = 0.25 Hz
                //mesh.Yaw = angle1;
                mesh.Render(_viewMatrix, _projectionMatrix);
            }
            mainCharacterService.Render(_viewMatrix, _projectionMatrix);
            mapService.Render(_viewMatrix, _projectionMatrix);
            RenderHUD();

            _renderer.EndRender();
        }

        public void InitializeLight()
        {
            _illumination = new Illumination(Vector4.Zero, new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new LightSource[]
            {
                new LightSource(LightSource.LightType.DirectionalLight,
                    new Vector4(0.0f, 20.0f, 0.0f, 1.0f),   // Position
                    new Vector4(0.0f, -100.0f, 0.0f, 1.0f),   // Direction
                    new Vector4(1.0f, 1.0f, 1.0f, 1.0f),    // Color
                    0.0f,                                   // Spot angle
                    1.0f,                                   // Const atten
                    1.0f,                                   // Linear atten
                    1.0f,                                   // Quadratic atten
                    1),
                new LightSource(LightSource.LightType.SpotLight,
                    new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                    new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    PositionalObject.HALF_PI / 4.0f,
                    1.0f,
                    0.05f,
                    0.01f,
                    0),
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
        }
        public void InitializeTextures(Loader loader)
        {
            _textures = new Textures();
            _textures.Add(loader.LoadTextureFromFile("Resources\\floor.png", false, _samplerStates.Colored));
            _textures.Add(loader.LoadTextureFromFile("Resources\\white.bmp", false, _samplerStates.Colored));
            _textures.Add(loader.LoadTextureFromFile("Resources\\stone_wall.png", false, _samplerStates.Textured));
            _textures.Add(loader.LoadTextureFromFile("Resources\\delorean.png", false, _samplerStates.Textured));

        }
        private void RenderHUD()
        {
            string text = $"FPS: {_timeHelper.FPS,3:d2}\ntime: {_timeHelper.Time:f1}\n" +
                                $"MX: {_inputController.MouseRelativePositionX,3:d2} MY: {_inputController.MouseRelativePositionY,3:d2} MZ: {_inputController.MouseRelativePositionZ,4:d3}\n" +
                                $"LB: {(_inputController.MouseButtons[0] ? 1 : 0)} MB: {(_inputController.MouseButtons[2] ? 1 : 0)} RB: {(_inputController.MouseButtons[1] ? 1 : 0)}\n" +
                                //$"Pos: {_character.Position.X,6:f1}, {_character.Position.Y,6:f1}, {_character.Position.Z,6:f1}\n" +
                                cameraService.GetDebugString();
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
        private void UpdateKeyBoard()
        {
            _inputController.UpdateKeyboardState();
            if (_inputController.Esc) _renderForm.Close();                               // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            if (_inputController.Func[0]) _displayHelp = !_displayHelp;
            // Switch solid and wireframe modes by F2, F3.
            if (_inputController.Func[1]) _directX3DGraphics.RenderMode = DirectX3DGraphics.RenderModes.Solid;
            if (_inputController.Func[2]) _directX3DGraphics.RenderMode = DirectX3DGraphics.RenderModes.Wireframe;
            // Toggle fullscreen mode by F4, F5.
            if (_inputController.Func[3]) _directX3DGraphics.IsFullScreen = false;
            if (_inputController.Func[4]) _directX3DGraphics.IsFullScreen = true;
            mainCharacterService.Update();
            cameraService.Update();
        }

        public void Run()
        {
            RenderLoop.Run(_renderForm, RenderLoopCallback);
        }

        public void Dispose()
        {
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

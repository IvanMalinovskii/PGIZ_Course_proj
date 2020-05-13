using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Windows;
using Template.Graphics;
using System.Drawing;
using Template.Game.Services;
using Template.Sound;
using Template.Game.gameObjects.newServices;
using Template.Game.gameObjects.newObjects;
using Template.Game.GameObjects.Services;
using SharpDX.Mathematics.Interop;

namespace Template
{
    class GameProcess : IDisposable
    {
        public struct HUDResources
        {
            public int restartFormatIndex;
            public int formatIndex;
            public int brushIndex;
            public int heartIconIndex;
            public int arrowIconIndex;
        }
        private CameraService cameraService;
        private RenderForm renderForm;
        private DirectX3DGraphics directX3DGraphics;
        private Renderer renderer;

        private DirectX2DGraphics directX2DGraphics;

        private SamplerStates samplerStates;
        private Textures textures;
        private Materials materials;
        private Illumination illumination;

        private HUDResources hudResources;

        private Matrix projectionMatrix;

        private Matrix viewMatrix;

        private InputController inputController;

        private TimeHelper timeHelper;

        private bool firstRun = true;
        private MainCharacterService mainCharacterService;
        private LevelService levelService;
        private SharpAudioDevice audioDevice;

        private void InitHUDResources()
        {
            hudResources.formatIndex = directX2DGraphics.NewTextFormat("Input", SharpDX.DirectWrite.FontWeight.Normal,
                SharpDX.DirectWrite.FontStyle.Normal, SharpDX.DirectWrite.FontStretch.Normal, 80,
                SharpDX.DirectWrite.TextAlignment.Trailing, SharpDX.DirectWrite.ParagraphAlignment.Far);
            hudResources.restartFormatIndex = directX2DGraphics.NewTextFormat("Input", SharpDX.DirectWrite.FontWeight.Normal,
                SharpDX.DirectWrite.FontStyle.Normal, SharpDX.DirectWrite.FontStretch.Normal, 100,
                SharpDX.DirectWrite.TextAlignment.Center, SharpDX.DirectWrite.ParagraphAlignment.Center);
            hudResources.brushIndex = directX2DGraphics.NewSolidColorBrush(new RawColor4(1.0f, 1.0f, 1.0f, 1.0f));
            hudResources.heartIconIndex = directX2DGraphics.LoadBitmapFromFile("Resources\\heart.png");
            hudResources.arrowIconIndex = directX2DGraphics.LoadBitmapFromFile("Resources\\arrow.png");
        }
        /// <summary>
        /// Constructor. Initialize all objects.
        /// </summary>
        public GameProcess()
        {
            audioDevice = new SharpAudioDevice();
            renderForm = new RenderForm("SharpDX");
            renderForm.ClientSize = new Size(1500, 800);

            renderForm.UserResized += RenderFormResizedCallback;
            renderForm.Activated += RenderFormActivatedCallback;
            renderForm.Deactivate += RenderFormDeactivateCallback;
            directX3DGraphics = new DirectX3DGraphics(renderForm);
            renderer = new Renderer(directX3DGraphics);
            renderer.CreateConstantBuffers();
            directX2DGraphics = new DirectX2DGraphics(directX3DGraphics);
            Loader loader = new Loader(directX3DGraphics, directX2DGraphics, renderer, directX2DGraphics.ImagingFactory);
            samplerStates = new SamplerStates(directX3DGraphics);
            timeHelper = new TimeHelper();

            InitializeTextures(loader);
            renderer.SetWhiteTexture(textures["white.bmp"]);
            loader.StubTexture = textures["white.bmp"];
            materials = loader.LoadMaterials("Resources\\materials.txt", textures);

            Vector3 initial = new Vector3(105f, 0, -105f);
            Vector4 pos = new Vector4(0, 0, 0, 0);

            InitHUDResources();
            InitializeLight();
            
            inputController = new InputController(renderForm);

            mainCharacterService = new MainCharacterService("some", inputController, loader, materials[3], audioDevice);
            Queue<string> files = new Queue<string>();
            files.Enqueue("Resources\\Description\\map1.txt");
            files.Enqueue("Resources\\Description\\map1.txt");
            levelService = new LevelService(files, mainCharacterService, loader, materials[2], inputController, audioDevice);

            cameraService = new CameraService(new Camera(new Vector4(-119.0f, 144.0f, 129.0f, 1.0f)), inputController);
            
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
            directX3DGraphics.Resize();
            projectionMatrix = cameraService.SetAfterResize(renderForm.ClientSize.Width, renderForm.ClientSize.Height);
        }

        /// <summary>Callback for RenderLoop.Run. Handle input and render scene.</summary>
        public void RenderLoopCallback()
        {
            if (firstRun)
            {
                RenderFormResizedCallback(this, new EventArgs());
                firstRun = false;
            }

            timeHelper.Update();
            //_inputController.UpdateKeyboardState();
            inputController.UpdateMouseState();

            UpdateKeyBoard();
            viewMatrix = cameraService.GetViewMatrix();

            renderer.BeginRender();

            renderer.UpdateIlluminationProperties(illumination);

            renderer.SetPerObjectConstants(timeHelper.Time, 0);
            levelService.Render(viewMatrix, projectionMatrix);
            RenderHUD();

            renderer.EndRender();
        }

        public void InitializeLight()
        {
            illumination = new Illumination(Vector4.Zero, new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new LightSource[]
            {
                new LightSource(LightSource.LightType.DirectionalLight,
                    new Vector4(-40.0f, 10.0f, 0.0f, 1.0f),   // Position
                    new Vector4(10.0f, -20.0f, 0.0f, 1.0f),   // Direction
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
            textures = new Textures();
            textures.Add(loader.LoadTextureFromFile("Resources\\floor.png", false, samplerStates.Colored));
            textures.Add(loader.LoadTextureFromFile("Resources\\white.bmp", false, samplerStates.Colored));
            textures.Add(loader.LoadTextureFromFile("Resources\\stone_wall.png", false, samplerStates.Textured));
            textures.Add(loader.LoadTextureFromFile("Resources\\delorean.png", false, samplerStates.Textured));

        }
        private void RenderHUD()
        {
            float armorHeightInDIP = directX2DGraphics.Bitmaps[hudResources.heartIconIndex].Size.Height;
            directX2DGraphics.BeginDraw();
            directX2DGraphics.DrawText("LEVEL: " + (levelService.Level + 1).ToString(), hudResources.formatIndex, directX2DGraphics.RenderTargetClientRectangle, hudResources.brushIndex);
            if (!mainCharacterService.Character.IsAlive)
                directX2DGraphics.DrawText("PRESS R TO RESTART", hudResources.restartFormatIndex, directX2DGraphics.RenderTargetClientRectangle, hudResources.brushIndex);
            if (levelService.IsDone)
                directX2DGraphics.DrawText("YOU WIN\nPRESS R TO RESTART", hudResources.restartFormatIndex, directX2DGraphics.RenderTargetClientRectangle, hudResources.brushIndex);
            DrawIcon(0, mainCharacterService.Character.Health, hudResources.heartIconIndex);
            int arrows = ((Archer)mainCharacterService.Character).ArrowAmount;
            int maxCount = 10;
            int layers = arrows < maxCount ? 1 : (arrows / maxCount) + 1;
            for (int i = 0; i < layers; i++)
            {
                int count = (arrows - i * maxCount) < maxCount ? (arrows - i * maxCount) : maxCount;
                DrawIcon(armorHeightInDIP + i * directX2DGraphics.Bitmaps[hudResources.arrowIconIndex].Size.Height, count, hudResources.arrowIconIndex);
            }
            directX2DGraphics.EndDraw();
        }

        private void DrawIcon(float prevHeight, int count, int bmpIndex)
        {
            if (count > 8)
                prevHeight *= count / 8;
            float armorWidthInDIP = directX2DGraphics.Bitmaps[bmpIndex].Size.Width;
            for (int i = 1; i <= count; i++)
            {
                Matrix3x2 armorTransformMatrix = Matrix3x2.Translation(new Vector2(directX2DGraphics.RenderTargetClientRectangle.Right - i * armorWidthInDIP - 1, 1 + prevHeight));
                directX2DGraphics.DrawBitmap(bmpIndex, armorTransformMatrix, 1.0f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
            }
        }

        private void UpdateKeyBoard()
        {
            inputController.UpdateKeyboardState();
            if (inputController.Esc) renderForm.Close();                     

            // Switch solid and wireframe modes by F2, F3.
            if (inputController.Func[1]) directX3DGraphics.RenderMode = DirectX3DGraphics.RenderModes.Solid;
            if (inputController.Func[2]) directX3DGraphics.RenderMode = DirectX3DGraphics.RenderModes.Wireframe;
            // Toggle fullscreen mode by F4, F5.
            if (inputController.Func[3]) directX3DGraphics.IsFullScreen = false;
            if (inputController.Func[4]) directX3DGraphics.IsFullScreen = true;
            //mainCharacterService.Update();
            levelService.Update();
            cameraService.Update();
        }

        public void Run()
        {
            RenderLoop.Run(renderForm, RenderLoopCallback);
        }

        public void Dispose()
        {
            mainCharacterService.Dispose();
            levelService.Dispose();
            audioDevice.Dispose();
            textures.Dispose();
            samplerStates.Dispose();
            inputController.Dispose();
            directX2DGraphics.Dispose();
            renderer.Dispose();
            directX3DGraphics.Dispose();
            renderForm.Dispose();
        }
    }
}

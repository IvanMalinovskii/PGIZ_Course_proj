using SharpDX;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.interfaces;
using Template.Game.gameObjects.newObjects;
using Template.Graphics;

namespace Template.Game.gameObjects.newServices
{
    class MainCharacterService : ICharacterService
    {
        private AnimationService animationService;
        private bool isAnimation;
        private TimeHelper timeHelper;
        private Archer archer;
        private MeshObject pointer;
        private Material optionalPointerColor;
        private Material originalPointerColor;
        private InputController controller;
        private Queue<string> animationQueue;

        public Character Character { get => archer; set => archer = value as Archer; }
        private Map map;
        public Map Map { get => map; set
            {
                map = value;
                archer.Offset = map.CellSize;
            } }

        public MainCharacterService(string configFile, InputController controller, Loader loader, Material stub, TimeHelper timeHelper)
        {
            animationQueue = new Queue<string>();
            isAnimation = false;
            this.timeHelper = timeHelper;
            optionalPointerColor = stub;
            archer = new Archer(new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
            archer.IsActive = true;
            //archer.Offset = Map.CellSize;
            archer.AddMeshObjects(loader.LoadMeshesFromObject("Resources\\PlagueDoctor.obj", stub));
            pointer = loader.LoadMeshFromObject("Resources\\Pointer.obj", stub);
            originalPointerColor = pointer.Material;
            archer.AddMeshObject(pointer);
            this.controller = controller;
            animationService = new AnimationService(archer, new Sound.SharpAudioDevice());
        }

        public void AddMeshObjects(List<MeshObject> meshObjects)
        {
            archer.AddMeshObjects(meshObjects);
        }

        public void Update()
        {
            if (isAnimation)
            {
                foreach (var el in animationQueue)
                    Console.WriteLine($"el: {el}");
                animationService.Animate(animationQueue.Peek());
                return;
            }
            //if (!archer.IsActive) return;
            if (controller[Key.W]) archer.Direction = new Vector3(1, 0, 0);
            if (controller[Key.S]) archer.Direction = new Vector3(-1, 0, 0);
            if (controller[Key.A]) archer.Direction = new Vector3(0, 0, 1);
            if (controller[Key.D]) archer.Direction = new Vector3(0, 0, -1);

            pointer.Position = archer.GetNewPosition(15);
            pointer.Yaw = archer.GetNewYawRotation();

            if (Map[pointer.Position].Value.Unit != Unit.Empty && Map[pointer.Position].Value.Unit != Unit.Archer)
                pointer.Material = optionalPointerColor;
            else pointer.Material = originalPointerColor;

            if (archer.Direction != new Vector3(0, 0, 0))
            {
                archer.IsActive = false;
                if (controller[Key.G])
                {
                    isAnimation = true;
                    animationQueue.Enqueue("rotation");
                    animationQueue.Enqueue("slide");
                    animationService.SetUpParameters("rotation", (s, e) => { animationQueue.Dequeue(); Console.WriteLine("first"); });
                    animationService.SetUpParameters("slide", (s, e) => { animationQueue.Dequeue(); isAnimation = false; archer.Direction = Vector3.Zero; Console.WriteLine("second"); });
                }
                else if (controller[Key.F]) archer["Pointer"].Yaw += 0.001f;
                else archer.IsActive = true;
            }
        }

        public void Render(Matrix viewMatrix, Matrix projectionMatrix)
        {
            foreach(var meshObject in archer.MeshObjects)
            {
                if (meshObject == pointer && (archer.Direction == Vector3.Zero || isAnimation)) continue;
                meshObject.Render(viewMatrix, projectionMatrix);
            }
        }

        public override string ToString()
        {
            return $"Pointer's Yaw: {pointer.Yaw}";
        }
    }
}

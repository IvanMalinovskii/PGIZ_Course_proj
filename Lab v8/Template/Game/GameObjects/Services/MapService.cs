using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Template.Game.gameObjects.interfaces;
using Template.Game.gameObjects.newObjects;
using Template.Game.GameObjects.Objects;
using Template.Game.GameObjects.Objects.PickUps;
using Template.Game.GameObjects.Services;
using Template.Graphics;
using Template.Sound;
using static Template.Game.Animation;
using static Template.Game.gameObjects.newObjects.Map;

namespace Template.Game.gameObjects.newServices
{
    class MapService : IDisposable
    {
        private Vector4 doorPos;
        public bool IsAnimation { get; set; }
        public Character Scene { get; set; }
        public bool OnTheDoor { get; set; }
        private Map map;
        private ICharacterService characterService;
        private List<PickUp> pickUps;
        private List<StaticObject> staticObjects;
        private List<ICharacterService> services;
        private InputController controller;
        private Queue<ICharacterService> servicesQueue;
        private AnimationService sceneAnimation;
        public MapService(ICharacterService characterService, string configFile, Loader loader, Material stub, InputController controller, SharpAudioDevice device)
        {
            OnTheDoor = false;
            this.controller = controller;
            services = new List<ICharacterService>();
            staticObjects = new List<StaticObject>();
            pickUps = new List<PickUp>();
            

            List<string> descriptors = LoadDescriptors(configFile);
            SetMap(descriptors, loader, stub);
            SetCharacter(descriptors, characterService);
            SetEnemies(descriptors, loader, stub, device);
            SetPickUps(descriptors, loader, stub);
            SetStatics(descriptors, loader, stub);

            //SetScene();
            //sceneAnimation = new AnimationService(Scene, device);
            servicesQueue = new Queue<ICharacterService>();
            UpdateQueue();
        }

        public void SetAnimation(AnimationHandler handler, List<object> parameters)
        {
            sceneAnimation.SetUpParameters("slide", handler, parameters);
            //IsAnimation = true;
        }

        private void SetScene()
        {
            Scene = new Character(Vector4.Zero);
            Scene.MeshObjects.AddRange(map.MeshObjects.ToList());
            Scene.MeshObjects.AddRange(characterService.Character.MeshObjects.ToList());
            services.ForEach(s => Scene.MeshObjects.AddRange(s.Character.MeshObjects.ToList()));
            pickUps.ForEach(p => Scene.MeshObjects.AddRange(p.MeshObjects.ToList()));
            staticObjects.ForEach(st => Scene.MeshObjects.AddRange(st.MeshObjects.ToList()));
        }

        private void SetEnemies(List<string> descriptors, Loader loader, Material stub, SharpAudioDevice device)
        {
            descriptors.FindAll(d => d.Split(' ')[0] == "enemy")
                .ForEach(d => SetEnemy(d, loader, stub, device));
        }

        private void SetCharacter(List<string> descriptors, ICharacterService service)
        {
            string[] characterParams = descriptors.Find(s => s.Split(' ')[0] == "character").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            characterService = service;
            Point targetPoint = new Point(int.Parse(characterParams[1]), int.Parse(characterParams[2]));
            Cell cell = map[targetPoint];
            cell.Unit = Unit.Archer;
            cell.UnitObject = characterService.Character;
            map[targetPoint] = cell;
            characterService.Character.SetDefault();
            characterService.Character.Position = cell.Position;
            characterService.Map = map;
        }

        public void Update()
        {
            if (characterService.Character.Position == doorPos)
                OnTheDoor = true;
            if (IsAnimation)
            {
                sceneAnimation.Animate("scene");
                return;
            }
            if (services.Count == 0)
                map.IsClear = true;
            ICharacterService service = servicesQueue.Peek();
            if (!service.Character.IsActive || !service.Character.IsAlive)
            {
                servicesQueue.Dequeue();
                if (servicesQueue.Count != 0)
                    servicesQueue.Peek().Character.IsActive = true;
                else
                    UpdateQueue();
            }
            else
            {
                service.Update();
            }
            for (int i = 0; i < pickUps.Count; i++)
                if (!pickUps[i].IsExist)
                    pickUps.Remove(pickUps[i]);
            for (int i = 0; i < staticObjects.Count; i++)
                if (!staticObjects[i].IsAlive)
                {
                    Cell cell = map[staticObjects[i].Position].Value;
                    cell.Unit = Unit.Empty;
                    cell.UnitObject = null;
                    map[staticObjects[i].Position] = cell;
                    staticObjects.Remove(staticObjects[i]);
                }
            for (int i = 0; i < services.Count; i++)
            {
                if (!services[i].Character.IsAlive)
                {
                    Cell cell = map[services[i].Character.Position].Value;
                    cell.Unit = Unit.Empty;
                    cell.UnitObject = null;
                    map[services[i].Character.Position] = cell;
                    services.Remove(services[i]);
                }
            }
        }

        private void SetMap(List<string> descriptors, Loader loader, Material stub)
        {
            string[] mapParams = descriptors.Find(s => s.Split(' ')[0] == "map").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            map = new Map(Vector4.Zero, new Point(int.Parse(mapParams[1]), int.Parse(mapParams[2])), float.Parse(mapParams[3]));
            CreateField(mapParams[4], loader, stub);
        }

        private void UpdateQueue()
        {
            servicesQueue.Enqueue(characterService);
            services.ForEach(s => servicesQueue.Enqueue(s));
            servicesQueue.Peek().SetActive();
        }

        public void Render(Matrix viewMatrix, Matrix projectionMatrix)
        {
            foreach(var meshObject in map.MeshObjects)
            {
                meshObject.Render(viewMatrix, projectionMatrix);
            }
            pickUps.ForEach(p => { p.Render(viewMatrix, projectionMatrix); });
            staticObjects.ForEach(s => { s.Render(viewMatrix, projectionMatrix); });
            services.ForEach(s => { s.Render(viewMatrix, projectionMatrix); });
            if (characterService.Character.IsAlive)
                characterService.Render(viewMatrix, projectionMatrix);
        }

        private void CreateField(string fileName, Loader loader, Material stub)
        {
            float initialX = map.CellSize * (map.Size.X - 1) * 0.5f;
            float initialZ = map.CellSize * (map.Size.Y - 1) * 0.5f;

            for(int y = 0; y < map.Size.Y; y++)
            {
                for(int x = 0; x < map.Size.X; x++)
                {
                    Vector4 position = new Vector4(initialX - x * map.CellSize, 0, initialZ - y * map.CellSize, 0);
                    List<MeshObject> meshObjects = loader.LoadMeshesFromObject(fileName, stub);
                    meshObjects.ForEach(mesh => mesh.Position = position);
                    map.MeshObjects.AddRange(meshObjects);
                    map[new Point(x, y)] = new Map.Cell
                    {
                        Position = position,
                        Unit = Unit.Empty,
                        UnitObject = null
                    };
                }
            }
            for (int i = 0; i < map.Size.Y; i++)
                map[new Point(map.Size.X, i)] = new Cell
                {
                    Position = Vector4.One,
                    Unit = Unit.None,
                    UnitObject = null
                };
            doorPos = new Vector4(0, 0, -(map.CellSize * (map.Size.X - 1) / 2 + map.CellSize), 0);
            map[new Point(map.Size.X, 0)] = new Cell
            {
                Position = doorPos,
                Unit = Unit.Door,
                UnitObject = null
            };
            List<MeshObject> doorMeshes = loader.LoadMeshesFromObject(fileName, stub);
            doorMeshes.ForEach(mesh => mesh.Position = doorPos);
            map.MeshObjects.AddRange(doorMeshes);
        }

        private void SetPickUps(List<string> descriptors, Loader loader, Material stub)
        {
            descriptors.FindAll(d => d.Split(' ')[0] == "pick_up")
                .ForEach(d => SetPickUp(d, loader, stub));
        }

        private void SetPickUp(string description, Loader loader, Material stub)
        {
            string[] pickUpParams = description.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Point targetPoint = new Point(int.Parse(pickUpParams[2]), int.Parse(pickUpParams[2]));
            Cell cell = map[targetPoint];
            PickUp pickUp;
            switch (pickUpParams[1])
            {
                case "armor": pickUp = new MoreArmorItem(cell.Position); break;
                case "arrow": pickUp = new MoreArrowsItem(cell.Position); break;
                case "turn": 
                    pickUp = new MovementAbilityItem(cell.Position); break;
                default: pickUp = null; break;
            }
            pickUp.AddMeshObjects(loader.LoadMeshesFromObject(pickUpParams[4], stub));
            pickUp["Stand"].IsMoveable = false;
            cell.Unit = Unit.Item;
            cell.UnitObject = pickUp;
            pickUps.Add(pickUp);
            map[targetPoint] = cell;
        }

        private void SetStatics(List<string> descriptions, Loader loader, Material stub)
        {
            descriptions.FindAll(d => d.Split(' ')[0] == "static")
                .ForEach(d => SetStatic(d, loader, stub));
        }

        private void SetStatic(string description, Loader loader, Material stub)
        {
            string[] staticParams = description.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Point targetPoint = new Point(int.Parse(staticParams[1]), int.Parse(staticParams[2]));
            Cell cell = map[targetPoint];
            StaticObject staticObject = new StaticObject(cell.Position);

            staticObject.AddMeshObjects(loader.LoadMeshesFromObject(staticParams[3], stub));
            cell.Unit = Unit.Static;
            cell.UnitObject = staticObject;
            staticObjects.Add(staticObject);
            map[targetPoint] = cell;
        }

        private void SetEnemy(string description, Loader loader, Material stub, SharpAudioDevice device)
        {
            string[] enemyParams = description.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Point targetPoint = new Point(int.Parse(enemyParams[2]), int.Parse(enemyParams[3]));
            Cell cell = map[targetPoint];
            ICharacterService service;
            
            switch (enemyParams[1])
            {
                case "mile": service = new MileEnemyService(characterService.Character, enemyParams[4], loader, stub, controller, device); break;
                default: service = null; break;
            }
            service.Character.IsActive = false;
            service.Map = map;
            service.Character.Position = cell.Position;
            cell.Unit = Unit.Enemy;
            cell.UnitObject = service.Character;
            map[targetPoint] = cell;
            services.Add(service);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            services.ForEach(s => stringBuilder.Append(s.ToString()));
            return stringBuilder.ToString();
        }

        public void Dispose()
        {
            services.ForEach(s => s.Dispose());
        }


        private List<string> LoadDescriptors(string file)
        {
            List<string> descriptors = new List<string>();
            using(StreamReader reader = new StreamReader(file))
            {
                while(!reader.EndOfStream)
                {
                    descriptors.Add(reader.ReadLine());
                }
            }
            return descriptors;
        }
    }
}

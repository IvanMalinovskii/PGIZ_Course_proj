using SharpDX;
using System;
using System.Collections.Generic;
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
using static Template.Game.gameObjects.newObjects.Map;

namespace Template.Game.gameObjects.newServices
{
    class MapService
    {
        private Map map;
        private ICharacterService characterService;
        private List<PickUp> pickUps;
        private List<StaticObject> staticObjects;
        private List<ICharacterService> services;
        private InputController controller;
        public MapService(ICharacterService characterService, string configFile, Loader loader, Material stub, InputController controller)
        {
            this.controller = controller;
            services = new List<ICharacterService>();
            staticObjects = new List<StaticObject>();
            pickUps = new List<PickUp>();
            this.characterService = characterService;
            map = new Map(Vector4.Zero, new Point(9, 9), 15);
            CreateField("Resources\\FloorTile.obj", loader, stub);
            this.characterService.Map = map;
            Cell characterCell = map[characterService.Character.Position].Value;
            characterCell = new Cell
            {
                Position = characterCell.Position,
                Unit = Unit.Archer,
                UnitObject = characterService.Character
            };
            map[characterService.Character.Position] = characterCell;
            SetPickUps(loader, stub);
            SetStatics(loader, stub);
            SetEnemy("mile", new Point(0, 0), loader, stub);
        }

        public void Update()
        {
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
                else services[i].Update();
            }
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
        }

        private void SetPickUps(Loader loader, Material stub)
        {
            SetPickUp("turn", new Point(4, 5), loader.LoadMeshesFromObject("Resources\\Moka.obj", stub));
            SetPickUp("turn", new Point(4, 6), loader.LoadMeshesFromObject("Resources\\Shield.obj", stub));
            SetPickUp("turn", new Point(4, 7), loader.LoadMeshesFromObject("Resources\\Quiver.obj", stub));
        }

        private void SetPickUp(string type, Point point, List<MeshObject> meshObjects)
        {
            Cell cell = map[point];
            PickUp pickUp;
            switch (type)
            {
                case "armor": pickUp = new MoreArmorItem(cell.Position); break;
                case "arrow": pickUp = new MoreArrowsItem(cell.Position); break;
                case "turn": pickUp = new MovementAbilityItem(cell.Position); break;
                default: pickUp = null; break;
            }
            pickUp.AddMeshObjects(meshObjects);
            pickUp["Stand"].IsMoveable = false;
            cell.Unit = Unit.Item;
            cell.UnitObject = pickUp;
            pickUps.Add(pickUp);
            map[point] = cell;
        }

        private void SetStatics(Loader loader, Material stub)
        {
            SetStatic("barrel", new Point(4, 3), loader.LoadMeshesFromObject("Resources\\Barrel.obj", stub));
        }

        private void SetStatic(string type, Point point, List<MeshObject> meshObjects)
        {
            Cell cell = map[point];
            StaticObject staticObject;
            switch (type)
            {
                case "barrel": staticObject = new StaticObject(cell.Position); break;
                default: staticObject = null; break;
            }
            staticObject.AddMeshObjects(meshObjects);
            cell.Unit = Unit.Static;
            cell.UnitObject = staticObject;
            staticObjects.Add(staticObject);
            map[point] = cell;
        }

        private void SetEnemy(string type, Point point, Loader loader, Material stub)
        {
            Cell cell = map[point];
            ICharacterService service;
            switch (type)
            {
                case "mile": service = new MileEnemyService(characterService.Character, "some", loader, stub, controller); break;
                default: service = null; break;
            }
            service.Map = map;
            service.Character.Position = cell.Position;
            cell.Unit = Unit.Empty;
            cell.UnitObject = service.Character;
            services.Add(service);
        }

        public override string ToString()
        {
            return services[0].ToString();
        }
    }
}

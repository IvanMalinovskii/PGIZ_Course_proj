using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Template.Game.gameObjects.newObjects;
using Template.Game.GameObjects.Objects;
using Template.Game.GameObjects.Objects.PickUps;
using Template.Graphics;
using static Template.Game.gameObjects.newObjects.Map;

namespace Template.Game.gameObjects.newServices
{
    class MapService
    {
        private Map map;
        private ICharacterService characterService;
        private List<PickUp> pickUps;
        public MapService(ICharacterService characterService, string configFile, Loader loader, Material stub)
        {
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
        }

        public void Update()
        {
            for (int i = 0; i < pickUps.Count; i++)
                if (!pickUps[i].IsExist)
                    pickUps.Remove(pickUps[i]);
        }

        public void Render(Matrix viewMatrix, Matrix projectionMatrix)
        {
            foreach(var meshObject in map.MeshObjects)
            {
                meshObject.Render(viewMatrix, projectionMatrix);
            }
            pickUps.ForEach(p => { p.Yaw += 0.03f; p.Render(viewMatrix, projectionMatrix); });
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
            cell.Unit = Unit.Item;
            cell.UnitObject = pickUp;
            pickUps.Add(pickUp);
            map[point] = cell;
        }
    }
}

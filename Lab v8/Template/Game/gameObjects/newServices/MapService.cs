using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.newObjects;
using Template.Graphics;

namespace Template.Game.gameObjects.newServices
{
    class MapService
    {
        private Map map;
        public MapService(string configFile, Loader loader, Material stub)
        {
            map = new Map(Vector4.Zero, new Point(11, 11), 15);
            CreateField("Resources\\FloorTile.obj", loader, stub);
        }

        public void Update()
        {

        }

        public void Render(Matrix viewMatrix, Matrix projectionMatrix)
        {
            foreach(var meshObject in map.MeshObjects)
            {
                meshObject.Render(viewMatrix, projectionMatrix);
            }
        }

        private void CreateField(string fileName, Loader loader, Material stub)
        {
            float initialX = map.CellSize * (map.Size.X - 1) * 0.5f;
            float initialZ = -map.CellSize * (map.Size.Y - 1) * 0.5f;

            for(int y = 0; y < map.Size.Y; y++)
            {
                for(int x = 0; x < map.Size.X; x++)
                {
                    Vector4 position = new Vector4(initialX - x * map.CellSize, 0, initialZ + y * map.CellSize, 0);
                    List<MeshObject> meshObjects = loader.LoadMeshesFromObject(fileName, stub);
                    meshObjects.ForEach(mesh => mesh.Position = position);
                    map.MeshObjects.AddRange(meshObjects);
                }
            }
        }
    }
}

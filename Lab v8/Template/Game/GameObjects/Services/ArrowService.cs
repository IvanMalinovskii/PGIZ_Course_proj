using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.interfaces;
using Template.Game.gameObjects.newObjects;
using Template.Game.gameObjects.newServices;
using Template.Game.GameObjects.Objects;
using Template.Graphics;

namespace Template.Game.GameObjects.Services
{
    public class ArrowService : ICharacterService
    {
        private Arrow arrow;
        private Map map;
        public Map Map
        {
            get => map; set
            {
                map = value;
                arrow.Offset = map.CellSize;
            }
        }
        public Character Character { get => arrow; set => arrow = value as Arrow; }

        public ArrowService(string configFile, Loader loader, Material stub)
        {
            arrow = new Arrow(Vector4.Zero);
            arrow.AddMeshObject(loader.LoadMeshFromObject("Resources\\Bolt.obj", stub));
        }

        public void Render(Matrix viewMatrix, Matrix projectionMatrix)
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }
    }
}

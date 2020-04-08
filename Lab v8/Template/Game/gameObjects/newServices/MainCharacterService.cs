using SharpDX;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.interfaces;
using Template.Game.gameObjects.newObjects;

namespace Template.Game.gameObjects.newServices
{
    class MainCharacterService : ICharacterService
    {
        private Archer archer;
        private InputController controller;
        public Character Character { get => archer; set => archer = value as Archer; }

        public MainCharacterService(string configFile, InputController controller, Loader loader)
        {
            archer = new Archer(new SharpDX.Vector4(0.0f, 0.0f, 0.0f, 0.0f));
            archer.IsActive = true;
            this.controller = controller;
        }

        public void AddMeshObjects(List<MeshObject> meshObjects)
        {
            archer.AddMeshObjects(meshObjects);
        }

        public void Update()
        {
            if (!archer.IsActive) return;
            if (controller[Key.W]) archer.Direction = new Vector3(1, 0, 0);
            if (controller[Key.S]) archer.Direction = new Vector3(-1, 0, 0);
            if (controller[Key.A]) archer.Direction = new Vector3(0, 0, 1);
            if (controller[Key.D]) archer.Direction = new Vector3(0, 0, -1);

            archer["Stand"].Position = archer.GetNewPosition(15);
            if (archer.Direction != new Vector3(0, 0, 0))
            {
                archer.IsActive = false;
                if (controller[Key.G]) { archer.MoveByDirection(15); archer.Direction = new Vector3(0, 0, 0); }
                else if (controller[Key.F]) archer.Shoot();
                else archer.IsActive = true;
            }
        }

        public void Render(Matrix viewMatrix, Matrix projectionMatrix)
        {
            foreach(var meshObject in archer.MeshObjects)
            {
                meshObject.Render(viewMatrix, projectionMatrix);
            }
        }
    }
}

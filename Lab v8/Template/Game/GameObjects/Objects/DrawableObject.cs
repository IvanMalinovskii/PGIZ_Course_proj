using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Graphics;

namespace Template.Game.gameObjects.newObjects
{
    public abstract class DrawableObject : PositionalObject
    {
        public override Vector4 Position { 
            get => base.Position; 
            set 
            {
                base.Position = value;
                foreach(var meshObject in MeshObjects)
                {
                    meshObject.Position = value;
                }
            }
        }

        public override float Yaw 
        {
            get => base.Yaw; 
            set
            {
                base.Yaw = value;
                foreach (var meshObject in MeshObjects)
                {
                    meshObject.Yaw = value;
                }
            }
        }

        public MeshObjects MeshObjects { get; protected set; }
        public MeshObject this[string name] { get => MeshObjects[name]; }
        public DrawableObject(Vector4 initialPosition) : base(initialPosition)
        {
            MeshObjects = new MeshObjects();
        }

        public void AddMeshObject(MeshObject meshObject)
        {
            meshObject.Position = position;
            MeshObjects.Add(meshObject);
        }

        public void AddMeshObjects(List<MeshObject> meshObjects)
        {
            meshObjects.ForEach(mesh => mesh.Position = position);
            MeshObjects.AddRange(meshObjects);
        }

        public void ChangePosition(Vector4 newPosition)
        {
            position = newPosition;
            foreach (var meshObject in MeshObjects)
            {
                meshObject.Position = newPosition;
            }
        }
        public void Render(Matrix view, Matrix projection)
        {
            foreach (var mesh in MeshObjects)
                mesh.Render(view, projection);
        }
    }
}

using SharpDX;
using System.Collections.Generic;
using Template.Graphics;

namespace Template.Game.gameObjects.newObjects
{
    public class DrawableObject : PositionalObject
    {
        public override Vector4 Position { 
            get => base.Position; 
            set 
            {
                base.Position = value;
                foreach(var meshObject in MeshObjects)
                {
                    if (!meshObject.IsMoveable) continue;
                    meshObject.Position = value;
                }
            }
        }

        public void SetRawPosition(Vector4 position)
        {
            base.Position = position;
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
        public virtual void Render(Matrix view, Matrix projection)
        {
            foreach (var mesh in MeshObjects)
                mesh.Render(view, projection);
        }
    }
}

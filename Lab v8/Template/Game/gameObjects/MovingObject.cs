using SharpDX;
using Template.Graphics;
using SharpDX.Mathematics;
using System.Collections.Generic;
using System;

namespace Template
{
    /// <summary>
    /// Character object.
    /// </summary>
    class MovingObject : PositionalObject
    {
        public string Direction { get; private set; }
        //public BoundingBox collider;
        public float Speed { get; set; }
        public MeshObject meshCollider;
        public MeshObjects MeshObjects { get; private set; }
        public LightSource LightSource { get; private set; }
        public InputController InputController { get; set; }
        public MovingObject(Vector4 initialPosition, float speed, InputController inputController) :
            base(initialPosition)
        {
            Speed = speed;
            InputController = inputController;
            MeshObjects = new MeshObjects();
            Direction = "none";
        }
    
        public void InitializeCollider()
        {
            if (MeshObjects == null || MeshObjects.Count == 0) return;
            //collider = new BoundingBox(MeshObject.GetMin(), MeshObject.GetMax());

            foreach(var mesh in MeshObjects)
            {
                if (mesh.IsCollider)
                {
                    meshCollider = mesh;
                    return;
                }
            }
        }

        public void AttachMeshObject(MeshObject meshObject)
        {
            MeshObjects.Add(meshObject);
            SetMeshPosition();
        }

        public void AttachLightSource(LightSource lightSource)
        {
            LightSource = lightSource;
            LightSource.Position = position;
        }

        public override void MoveByDirection(float speed, string direction)
        {
            position = GetNewPosition(speed, direction);
            SetMeshPosition();
            if (LightSource != null)
            {
                LightSource.Position = position;
            }
            meshCollider.collider = new BoundingBox(meshCollider.GetMin(), meshCollider.GetMax());
        }

        private Vector4 GetNewPosition(float speed, string direction)
        {
            Vector4 newPosition = position;
            switch (direction)
            {
                case "forward": newPosition.X += speed; break;
                case "back": newPosition.X -= speed; break;
                case "left": newPosition.Z += speed; break;
                case "right": newPosition.Z -= speed; break;
            }
            return newPosition;
        }

        public void MoveByDirection(float speed, string direction, MeshObjects otherColliders)
        {
            //Console.WriteLine(collider);
            //MoveByDirection(speed, direction);
            //collider = new BoundingBox(MeshObject.GetMin(), MeshObject.GetMax());
            BoundingBox newCollider = meshCollider.GetNewCollider(GetNewPosition(speed, direction));
            foreach(MeshObject otherCollider in otherColliders)
            {
                if (meshCollider.Name == otherCollider.Name) continue;
                if (newCollider.Contains(otherCollider.collider) != ContainmentType.Disjoint)
                {
                    //Console.WriteLine(collider.Contains(otherCollider.collider));
                    while(meshCollider.collider.Contains(otherCollider.collider) == ContainmentType.Disjoint)
                    MoveByDirection(0.1f, direction);
                    //collider = new BoundingBox(MeshObject.GetMin(), MeshObject.GetMax());
                    MoveByDirection(0.1f, GetReverse(direction));
                    return;
                }
            }
            MoveByDirection(speed, direction);
        }

        public string GetReverse(string direction)
        {
            switch(direction)
            {
                case "forward": return "back";
                case "back": return "forward";
                case "left": return "right";
                case "right": return "left";
                default: throw new Exception("woopsy");
            }
        }

        private void SetMeshPosition()
        {
            foreach (var mesh in MeshObjects)
            {
                mesh.Position = position;
                mesh.MoveTo(mesh.Position.X, mesh.Position.Y + 1, mesh.Position.Z);
            }
        }
    }
}

using SharpDX;
using Template.Graphics;

namespace Template
{
    /// <summary>
    /// Character object.
    /// </summary>
    class Character : PositionalObject
    {

        public float Speed { get; set; }

        public MeshObject MeshObject { get; private set; }
        public LightSource LightSource { get; private set; }
        public InputController InputController { get; set; }
        public Character(Vector4 initialPosition, float speed, InputController inputController) :
            base(initialPosition)
        {
            Speed = speed;
            InputController = inputController;
        }
    
        public void AttachMeshObject(MeshObject meshObject)
        {
            MeshObject = meshObject;
            SetMeshPosition();
        }

        public void AttachLightSource(LightSource lightSource)
        {
            LightSource = lightSource;
            LightSource.Position = _position;
        }

        public override void MoveByDirection(float speed, string direction)
        {
            switch (direction)
            {
                case "forward": _position.X += speed; break;
                case "back": _position.X -= speed; break;
                case "left": _position.Z += speed; break;
                case "right": _position.Z -= speed; break;
            }
            SetMeshPosition();
            LightSource.Position = _position;
        }

        private void SetMeshPosition()
        {
            MeshObject.Position = _position;
            MeshObject.MoveTo(MeshObject.Position.X, MeshObject.Position.Y + 1, MeshObject.Position.Z);
        }
    }
}

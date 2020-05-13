using SharpDX;
using Template.Game.gameObjects.newObjects;

namespace Template.Game.GameObjects.Objects
{
    public abstract class PickUp : DrawableObject
    {
        private static readonly int UP = 2;
        private static readonly float DELTA = 0.05f;
        private static readonly float MAX = 2;
        private Vector4 initialPosition;
        private bool isUp;
        public bool IsExist { get; protected set; }
        public int Up { get; set; }
        public PickUp(Vector4 initialPosition) : base(initialPosition)
        {
            isUp = true;
            this.initialPosition = initialPosition;
            Up = UP;
            IsExist = true;
        }
        public override void Render(Matrix view, Matrix projection)
        {
            float max = initialPosition.Y + MAX;
            float min = initialPosition.Y - MAX;
            if (position.Y >= max) isUp = false;
            else if (position.Y <= min) isUp = true;
            float delta = (isUp) ? DELTA : -DELTA;
            Yaw += 0.03f;
            Vector4 pos = Position;
            pos.Y += delta;
            Position = pos;
            base.Render(view, projection);
        }
        public abstract void ChangeStates(Archer character);

        public void Destroy()
        {
            IsExist = false;
        }
    }
}

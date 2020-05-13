using SharpDX;
using Template.Game.gameObjects.interfaces;

namespace Template.Game.GameObjects.Objects
{
    public class Arrow : Character
    {
        private static readonly int DAMAGE = 1;
        public int Damage { get; set; }
        public Arrow(Vector4 initialPosition) : base(initialPosition)
        {
            Damage = DAMAGE;
        }
    }
}

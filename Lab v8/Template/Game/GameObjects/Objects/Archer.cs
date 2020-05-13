using SharpDX;
using Template.Game.gameObjects.interfaces;

namespace Template.Game.gameObjects.newObjects
{
    public class Archer : Character
    {
        private static readonly int ARROW_AMOUNT = 10;
        public int ArrowAmount { get; set; }
        public Archer(Vector4 initialPosition) : base(initialPosition)
        {
            ArrowAmount = ARROW_AMOUNT;
        }

        // TODO: create an arrow class
        public bool Shoot()
        {
            ArrowAmount = (ArrowAmount - 1 < 0) ? 0 : ArrowAmount - 1;
            return ArrowAmount != 0;
        }

        internal override void SetDefault()
        {
            base.SetDefault();
            ArrowAmount = ARROW_AMOUNT;
        }
    }
}

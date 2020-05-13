using SharpDX;
using Template.Game.gameObjects.newObjects;

namespace Template.Game.GameObjects.Objects.PickUps
{
    public class MovementAbilityItem : PickUp
    {
        public MovementAbilityItem(Vector4 initialPosition) : base(initialPosition)
        {

        }

        public override void ChangeStates(Archer character)
        {
            character.TurnCount = Up;
            IsExist = false;
        }
    }
}

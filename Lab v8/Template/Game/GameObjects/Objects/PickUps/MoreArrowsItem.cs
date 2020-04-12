using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.newObjects;

namespace Template.Game.GameObjects.Objects.PickUps
{
    public class MoreArrowsItem : PickUp
    {
        public MoreArrowsItem(Vector4 initialPosition) : base(initialPosition)
        {

        }

        public override void ChangeStates(Archer character)
        {
            character.ArrowAmount += Up;
            foreach (var meshObject in MeshObjects)
                meshObject.IsVisible = false;
            IsExist = false;
        }
    }
}

using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.interfaces;
using Template.Game.gameObjects.newObjects;

namespace Template.Game.GameObjects.Objects
{
    public abstract class PickUp : DrawableObject
    {
        private static readonly int UP = 2;
        public bool IsExist { get; protected set; }
        public int Up { get; set; }
        public PickUp(Vector4 initialPosition) : base(initialPosition)
        {
            Up = UP;
            IsExist = true;
        }

        public abstract void ChangeStates(Archer character);
    }
}

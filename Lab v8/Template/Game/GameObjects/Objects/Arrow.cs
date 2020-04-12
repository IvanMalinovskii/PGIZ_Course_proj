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

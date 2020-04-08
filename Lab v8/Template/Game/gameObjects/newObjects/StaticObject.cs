using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Graphics;

namespace Template.Game.gameObjects.newObjects
{
    public class StaticObject : DrawableObject
    {
        private static readonly int ARMOR = 2;
        public int Armor { get; set; }
        public bool IsDestroyable { get; set; }
        public bool IsAlive { get; set; }
        public StaticObject(Vector4 initialPosition) : base(initialPosition)
        {
            Armor = ARMOR;
            IsDestroyable = true;
        }

        public void GetDamage(int damage)
        {
            Armor = (Armor - damage < 0) ? 0 : Armor - damage;
            if (Armor == 0) IsAlive = false;
        }
    }
}

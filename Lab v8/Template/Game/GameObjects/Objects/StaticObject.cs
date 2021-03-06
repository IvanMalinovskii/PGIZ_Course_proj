﻿using SharpDX;

namespace Template.Game.gameObjects.newObjects
{
    public class StaticObject : DrawableObject
    {
        private static readonly int ARMOR = 2;
        public int Armor { get; set; }
        public bool IsAlive { get; set; }
        public StaticObject(Vector4 initialPosition) : base(initialPosition)
        {
            IsAlive = true;
            Armor = ARMOR;
        }

        public void GetDamage(int damage)
        {
            Armor = (Armor - damage < 0) ? 0 : Armor - damage;
            if (Armor == 0) IsAlive = false;
        }
    }
}

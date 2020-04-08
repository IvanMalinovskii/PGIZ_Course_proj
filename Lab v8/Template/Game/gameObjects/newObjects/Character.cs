using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.newObjects;
using Template.Graphics;

namespace Template.Game.gameObjects.interfaces
{
    public abstract class Character : DrawableObject 
    {
        protected static readonly int MOVEMENT_ABILITY = 1;
        protected static readonly int HEALTH = 6;
        protected static readonly int DAMAGE = 1;
        public bool IsActive { get; set; }
        public bool IsAlive { get; set; }
        public int Health { get; set; }
        public int MovementAbility { get; set; }
        public int Damage { get; set; }
        public Vector3 Direction { get; set; }
        public Character(Vector4 initialPosition) : base(initialPosition) 
        {
            Direction = new Vector3(0.0f, 0.0f, 0.0f);
            MovementAbility = MOVEMENT_ABILITY;
            Health = HEALTH;
            Damage = DAMAGE;
            MeshObjects = new MeshObjects();
            IsAlive = true;
            IsActive = false;
        }

        public void MoveByDirection()
        {
            MoveByDirection(1);
        }

        public void MoveByDirection(float offset)
        {
            position = GetNewPosition(offset);
            foreach (var meshObject in MeshObjects)
            {
                meshObject.Position = position;
            }
        }

        public Vector4 GetNewPosition(float offset)
        {
            Vector4 newPosition = position;
            newPosition.X += MovementAbility * offset * Direction.X;
            newPosition.Z += MovementAbility * offset * Direction.Z;
            return newPosition;
        } 

        public void GetDamage(int damage)
        {
            Health = (Health - damage < 0) ? 0 : Health - damage;
            if (Health == 0) IsAlive = false;
        }

        public void SetNullDirection()
        {
            Direction = new Vector3(0, 0, 0);
        }
    }
}

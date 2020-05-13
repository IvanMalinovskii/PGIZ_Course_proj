using SharpDX;
using Template.Game.gameObjects.newObjects;
using Template.Graphics;

namespace Template.Game.gameObjects.interfaces
{
    public class Character : DrawableObject 
    {
        protected static readonly int MOVEMENT_ABILITY = 1;
        protected static readonly int TURN_COUNT = 1;
        public static readonly int HEALTH = 6;
        //protected static readonly int DAMAGE = 1;
        public float Offset { get; set; }
        public bool IsActive { get; set; }
        public bool IsAlive { get; set; }
        public int Health { get; set; }
        public int MovementAbility { get; set; }
        public int TurnCount { get; set; }
        //public int Damage { get; set; }
        public Vector3 Direction { get; set; }
        public Character(Vector4 initialPosition) : base(initialPosition) 
        {
            Direction = new Vector3(0.0f, 0.0f, 0.0f);
            MovementAbility = MOVEMENT_ABILITY;
            TurnCount = TURN_COUNT;
            Health = HEALTH;
            MeshObjects = new MeshObjects();
            IsAlive = true;
            IsActive = false;
        }

        public void MoveByDirection()
        {
            MoveByDirection(1);
        }

        public virtual void MoveByDirection(float offset)
        {
            Position = GetNewPosition(offset);
        }

        internal virtual void SetDefault()
        {
            TurnCount = TURN_COUNT;    
        }

        public virtual Vector4 GetNewPosition()
        {
            return GetNewPosition(Offset);
        }

        public Vector4 GetNewPosition(float offset)
        {
            Vector4 newPosition = position;
            newPosition.X += MovementAbility * offset * Direction.X;
            newPosition.Z += MovementAbility * offset * Direction.Z;
            return newPosition;
        } 

        public float GetNewYawRotation()
        {
            if (Direction == new Vector3(-1, 0, 0)) return 0.0f;
            else if (Direction == new Vector3(1, 0, 0)) return PI;
            else if (Direction == new Vector3(0, 0, -1)) return PI + HALF_PI;
            else if (Direction == new Vector3(0, 0, 1)) return HALF_PI;
            return 0.0f;
        }

        public virtual void GetDamage(int damage)
        {
            Health = (Health - damage < 0) ? 0 : Health - damage;
            if (Health == 0)
            {
                IsAlive = false;
            }
        }

        public void SetNullDirection()
        {
            Direction = new Vector3(0, 0, 0);
        }

        public void ChangeYaw(float yaw)
        {
            Yaw = yaw;
            //foreach(var meshObject in MeshObjects)
            //{
            //    meshObject.Yaw = yaw;
            //}
        }
    }
}

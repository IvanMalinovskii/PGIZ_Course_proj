using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.interfaces;

namespace Template.Game.GameObjects.Objects
{
    public class MileEnemy : Character
    {
        private Random random;
        private char[] axises;
        public int Damage { get; set; }
        public Vector4 Target { get; set; }
        public MileEnemy(Vector4 initialPosition) : base(initialPosition)
        {
            Damage = 2;
            Health = 3;
            random = new Random();
            axises = new char[] { 'x', 'z' };
            Target = Vector4.Zero;
        }

        public override Vector4 GetNewPosition()
        {
            float horizontalDistance = position.Z - Target.Z;
            float verticalDistance = position.X - Target.X;
            
            SetDirection(horizontalDistance, verticalDistance);

            Console.WriteLine($"hor: {horizontalDistance} vert: {verticalDistance}");
            Console.WriteLine($"direction: {Direction}");

            return base.GetNewPosition();
        }

        private void SetDirection(float horizontalDistance, float verticalDistance)
        {
            float z = (horizontalDistance < 0) ? 1 : -1;
            float x = (verticalDistance < 0) ? 1 : -1; ;
            Vector3 direction = Vector3.Zero;
            if (verticalDistance != 0)
                direction.X = x;
            else
                direction.Z = z;
            //if (Math.Abs(horizontalDistance) < Math.Abs(verticalDistance))
            //    direction.X = z;
            //else if (Math.Abs(horizontalDistance) > Math.Abs(verticalDistance))
            //    direction.Z = x;
            //else
            //{
            //    char axis = axises[random.Next(0, axises.Length - 1)];
            //    if (axis == 'x')
            //        direction.X = x;
            //    else
            //        direction.Z = z;

            //}
            Direction = direction;
        }

        public Vector4 GetNewDirectionPosition(int x, int z)
        {
            Direction = new Vector3(x, 0, z);
            return base.GetNewPosition();
        }
    }
}

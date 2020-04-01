using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Template
{
    public abstract class PositionalObject
    {
        public static readonly float PI = (float)Math.PI;
        public static readonly float TWO_PI = (float)Math.PI * 2.0f;
        public static readonly float HALF_PI = (float)Math.PI / 2.0f;

        protected Vector4 _position;
        public Vector4 Position { get => _position; set => _position = value; }

        public PositionalObject(Vector4 initialPosition)
        {
            _position = initialPosition;
        }

        public virtual void MoveTo(float x, float y, float z)
        {
            _position.X = x;
            _position.Y = y;
            _position.Z = z;
        }

        public Matrix GetWorldMatrix()
        {
            return Matrix.Translation((Vector3)_position);
        }
    }
}

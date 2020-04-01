﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Template
{
    class Camera : PositionalObject
    {
        /// <summary>Field Of View angle in vertical plane.</summary>
        private float _fovY = HALF_PI / 2.0f;
        /// <summary>Field Of View angle in vertical plane.</summary>
        /// <value>Field Of View angle in vertical plane.</value>
        public float FOVY { get => _fovY; set => _fovY = value; }
        public struct Descr
        {
            public Vector3 pos;
            public Vector3 target;
            public Vector3 up;
        }
        public Descr description = new Descr();
        public float targetDelta = 2f;
        public float upDelta = 0f;
        /// <summary>Aspect ratio.</summary>
        private float _aspect;
        /// <summary>Aspect ratio.</summary>
        /// <value>Aspect ratio.</value>
        public float Aspect { get => _aspect; set => _aspect = value; }

        /// <summary>Game object, to what attached camera.</summary>
        private PositionalObject _objectToAttached = null;

        /// <summary>
        /// Constructor. Set fields initial values.
        /// </summary>
        /// <param name="initialPosition">Initial position of camera in world.</param>
        /// <param name="yaw">Initial angle of rotation around 0Y axis (x - to left, y - to up, z - to back), rad.</param>
        /// <param name="pitch">Initial angle of rotation around 0X axis (x - to left, y - to up, z - to back), rad.</param>
        /// <param name="roll">Initial rotation around 0Z axis (x - to left, y - to up, z - to back), rad.</param>
        public Camera(Vector4 initialPosition, float aspect = 1.0f) :
            base (initialPosition)
        {
            _aspect = aspect;
        }

        /// <summary>Attach camera to game object. When camera attached to some object, position and rotation copies from object.
        /// Values of position and rotation of camera ignored.</summary>
        /// <param name="game3DObject">Game object to attach.</param>
        public void AttachToObject(PositionalObject game3DObject)
        {
            _objectToAttached = game3DObject;
        }

        /// <summary>Dettach camera from game object. After this you need to set camera position and rotation by yourself.</summary>
        public void DettachFromObject()
        {
            _objectToAttached = null;
        }

        /// <summary>Get projection matrix.</summary>
        /// <returns>Projection matrix.</returns>
        public Matrix GetProjectionMatrix()
        {
            return Matrix.PerspectiveFovLH(_fovY, _aspect, 0.1f, 100.0f);
            //return Matrix.OrthoOffCenterLH(-15, 15, -15, 15, 0.1f, 100);
        }

        /// <summary>Get view matrix.</summary>
        /// <returns>View matrix.</returns>
        public Matrix GetViewMatrix()
        {
            if (_objectToAttached != null)
            {
                _position = _objectToAttached.Position;
                _position.Y += 15;
                _position.X -= 25;
            }
            Vector3 viewUp = new Vector3(1, 0, 0);
            Vector3 target = (Vector3)_position;
            target.Y -= 1;
            target.X += 1.5f;
            description.pos = (Vector3)_position;
            description.target = target;
            description.up = viewUp;
            
            return Matrix.LookAtLH((Vector3)_position, target, viewUp);
        }


    }
}
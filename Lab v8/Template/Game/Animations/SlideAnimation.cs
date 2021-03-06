﻿using SharpDX;
using Template.Game.gameObjects.newObjects;

namespace Template.Game.Animations
{
    public class SlideAnimation : Animation
    {
        public SlideAnimation(DrawableObject targetObject) : base(targetObject)
        {
            Parameters.Add("targetPosition", null);
            Parameters.Add("offset", null);
        }

        public override void Animate()
        {
            if ((Vector4)Parameters["targetPosition"] == null && (Vector4)Parameters["offset"] == null)
                return;
            if (TargetObject.Position == (Vector4)Parameters["targetPosition"])
            {
                EndAnimation("slide");
                ClearHandlers();
                return;
            }
            TargetObject.Position += (Vector4)Parameters["offset"];
            //Console.WriteLine($"Pos: {TargetObject.Position}, Target: {(Vector4)Parameters["targetPosition"]}");
        }

        public void AnimateComplex()
        {
            if ((Vector4)Parameters["targetPosition"] == null && (Vector4)Parameters["offset"] == null)
                return;
            if (TargetObject.Position == (Vector4)Parameters["targetPosition"])
            {
                EndAnimation("slide");
                ClearHandlers();
                return;
            }
            TargetObject.SetRawPosition(TargetObject.Position + (Vector4)Parameters["offset"]);
            for (int i = 0; i < TargetObject.MeshObjects.Count; i++)
                TargetObject.MeshObjects[i].Position += (Vector4)Parameters["offset"];
        }
    }
}

﻿using SharpDX;
using System.Collections.Generic;
using Template.Game.Animations;
using Template.Game.gameObjects.interfaces;
using Template.Game.gameObjects.newObjects;
using Template.Sound;
using static Template.Game.Animation;

namespace Template.Game.gameObjects.newServices
{
    public class AnimationService
    {
        private Dictionary<string, SharpAudioVoice> voices;
        private Animation slideAnimation;
        private Animation backSlideAnimation;
        private Animation rotationAnimation;
        private Character targetObject;

        public AnimationService(DrawableObject targetObject, SharpAudioDevice audioDevice)
        {
            this.targetObject = (Character)targetObject;
            slideAnimation = new SlideAnimation(targetObject);
            backSlideAnimation = new SlideAnimation(targetObject);
            rotationAnimation = new RotationAnimation(targetObject, "yawRotation");
        }

        public void SetUpParameters(string type, AnimationHandler animationEndedHandler)
        {
            switch(type)
            {
                case "slide":
                    slideAnimation.Parameters["targetPosition"] = targetObject.GetNewPosition();
                    slideAnimation.Parameters["offset"] = (Vector4)targetObject.Direction * 1.0f;
                    slideAnimation.AnimationEnded += animationEndedHandler;
                    break;
                case "rotation":
                    rotationAnimation.Parameters["initialRotation"] = targetObject.Yaw;
                    rotationAnimation.Parameters["targetRotation"] = targetObject.GetNewYawRotation();
                    rotationAnimation.Parameters["offset"] = 0.06f;
                    rotationAnimation.AnimationEnded += animationEndedHandler;
                    break;
            }
        }

        public void SetUpParameters(string type, AnimationHandler animationEndedHandler, List<object> parameters)
        {
            switch (type)
            {
                case "slide":
                    slideAnimation.Parameters["targetPosition"] = parameters[0];
                    slideAnimation.Parameters["offset"] = (Vector4)parameters[1];
                    slideAnimation.AnimationEnded += animationEndedHandler;
                    break;
                case "rotation":
                    rotationAnimation.Parameters["initialRotation"] = parameters[0];
                    rotationAnimation.Parameters["targetRotation"] = parameters[1];
                    rotationAnimation.Parameters["offset"] = 0.03f;
                    rotationAnimation.AnimationEnded += animationEndedHandler;
                    break;
                case "back_slide":
                    backSlideAnimation.Parameters["targetPosition"] = parameters[0];
                    backSlideAnimation.Parameters["offset"] = (Vector4)parameters[1];
                    backSlideAnimation.AnimationEnded += animationEndedHandler;
                    break;
            }
        }

        public void Animate(string type)
        {
            switch(type)
            {
                case "slide":
                    SlideAnimation();
                    break;
                case "rotation":
                    RotationAnimation();
                    break;
                case "back_slide":
                    BackSlideAnimation();
                    break;
                case "scene":
                    ((SlideAnimation)slideAnimation).AnimateComplex();
                    break;
            }
        }
        public void SlideAnimation()
        {
            slideAnimation.Animate();
        }

        public void RotationAnimation()
        {
            rotationAnimation.Animate();
        }

        public void BackSlideAnimation()
        {
            backSlideAnimation.Animate();
        }
    }
}

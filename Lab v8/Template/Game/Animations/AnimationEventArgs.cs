using Template.Game.gameObjects.newObjects;

namespace Template.Game.Animations
{
    public class AnimationEventArgs
    {
        public DrawableObject AnimationObject { get; set; }
        public string AnimationType { get; set; }

        public AnimationEventArgs(DrawableObject animationObject, string animationType)
        {
            AnimationObject = animationObject;
            AnimationType = animationType;
        }
    }
}

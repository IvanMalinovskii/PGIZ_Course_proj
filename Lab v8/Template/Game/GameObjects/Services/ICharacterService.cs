using SharpDX;
using System;
using Template.Game.gameObjects.interfaces;
using Template.Game.gameObjects.newObjects;

namespace Template.Game.gameObjects.newServices
{
    public interface ICharacterService : IDisposable
    {
        Map Map { get; set; }
        Character Character { get; set; }
        void Update();
        void Render(Matrix viewMatrix, Matrix projectionMatrix);
        void SetActive();
    }
}

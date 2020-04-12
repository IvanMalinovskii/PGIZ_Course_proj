using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.interfaces;
using Template.Game.gameObjects.newObjects;

namespace Template.Game.gameObjects.newServices
{
    interface ICharacterService
    {
        Map Map { get; set; }
        Character Character { get; set; }
        void Update();
        void Render(Matrix viewMatrix, Matrix projectionMatrix);
    }
}

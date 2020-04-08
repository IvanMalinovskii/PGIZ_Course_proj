using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.interfaces;

namespace Template.Game.gameObjects.newServices
{
    interface ICharacterService
    {
        Character Character { get; set; }
        void Update();
        void Render(Matrix viewMatrix, Matrix projectionMatrix);
    }
}

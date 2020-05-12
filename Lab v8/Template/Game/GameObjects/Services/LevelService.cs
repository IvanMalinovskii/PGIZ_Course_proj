using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using Template.Game.gameObjects.interfaces;
using Template.Game.gameObjects.newServices;
using Template.Graphics;
using Template.Sound;

namespace Template.Game.GameObjects.Services
{
    public class LevelService : IDisposable
    {
        private LinkedList<MapService> mapServices;
        private ICharacterService characterService;
        private Loader loader;
        private Material stub;
        private InputController controller;
        private SharpAudioDevice device;
        private Queue<string> configFiles;
        public bool IsDone { get; private set; }
        public int Level { get; private set; }
        public LevelService(Queue<string> configFiles, ICharacterService characterService, Loader loader, Material stub, InputController controller, SharpAudioDevice device)
        {
            this.configFiles = configFiles;
            this.characterService = characterService;
            this.loader = loader;
            this.stub = stub;
            this.controller = controller;
            this.device = device;
            mapServices = new LinkedList<MapService>();
            mapServices.AddLast(new MapService(characterService, configFiles.Peek(), loader, stub, controller, device));
        }

        public void Update()
        {
            if (mapServices.Count != 0 && !IsDone)
            {
                MapService service = mapServices.First();
                if (service.OnTheDoor)
                {
                    service.OnTheDoor = false;
                    configFiles.Dequeue();
                    if (configFiles.Count == 0)
                    {
                        IsDone = true;
                        return;
                    }
                    MapService secondService = new MapService(characterService, configFiles.Peek(), loader, stub, controller, device);
                    mapServices.AddLast(secondService);

                    mapServices.RemoveFirst();
                    service.Dispose();
                    Level++;
                }
                if (characterService.Character.IsAlive)
                    mapServices.First().Update();
                else if (!characterService.Character.IsAlive && controller[SharpDX.DirectInput.Key.R])
                {
                    characterService.Character.IsAlive = true;
                    characterService.Character.SetDefault();
                    characterService.Character.Health = Character.HEALTH;
                    mapServices.RemoveFirst();
                    mapServices.AddLast(new MapService(characterService, configFiles.Peek(), loader, stub, controller, device));
                }
            }
        }

        public void Render(Matrix view, Matrix projection)
        {
                foreach (var mapService in mapServices)
                    mapService.Render(view, projection);
        }

        public void Dispose()
        {
            while(mapServices.Count != 0)
            {
                MapService service = mapServices.First();
                mapServices.RemoveFirst();
                service.Dispose();
            }
        }
    }
}

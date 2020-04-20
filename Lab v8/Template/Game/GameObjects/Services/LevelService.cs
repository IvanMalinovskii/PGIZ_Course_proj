using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public LevelService(Queue<string> configFiles, ICharacterService characterService, Loader loader, Material stub, InputController controller, SharpAudioDevice device)
        {
            this.configFiles = configFiles;
            this.characterService = characterService;
            this.loader = loader;
            this.stub = stub;
            this.controller = controller;
            this.device = device;
            mapServices = new LinkedList<MapService>();
            mapServices.AddLast(new MapService(characterService, configFiles.Dequeue(), loader, stub, controller, device));
        }

        public void Update()
        {
            if (mapServices.Count != 0)
            {
                MapService service = mapServices.First();
                if (service.OnTheDoor)
                {
                    service.OnTheDoor = false;
                    MapService secondService = new MapService(characterService, configFiles.Dequeue(), loader, stub, controller, device);
                    //secondService.Scene.Position = new Vector4(0, 0, 100, 0);
                    mapServices.AddLast(secondService);

                    mapServices.RemoveFirst();
                    service.Dispose();

                    //service.SetAnimation((s, a) =>
                    //{
                    //    service.IsAnimation = false;
                    //    mapServices.RemoveFirst();
                    //    service.Dispose();
                    //}, new List<object> { new Vector4(0, 0, -100, 0), new Vector4(0, 0, 1, 0)});

                    //secondService.SetAnimation((s, a) =>
                    //{
                    //    secondService.IsAnimation = false;
                    //}, new List<object> { Vector4.Zero, new Vector4(0, 0, -1, 0) });

                    //service.IsAnimation = true;
                    //secondService.IsAnimation = true;
                }
                foreach (var mapService in mapServices)
                    mapService.Update();
            }
        }

        public void Render(Matrix view, Matrix projection)
        {
            //if (mapServices.Count != 0)
            //    mapServices.First().Render(view, projection);
            //else if (mapServices.Count == 2)
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

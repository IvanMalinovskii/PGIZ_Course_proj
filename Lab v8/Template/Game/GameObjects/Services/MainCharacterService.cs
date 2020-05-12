using SharpDX;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using Template.Game.gameObjects.interfaces;
using Template.Game.gameObjects.newObjects;
using Template.Game.GameObjects.Objects;
using Template.Graphics;
using Template.Sound;
using static Template.Game.gameObjects.newObjects.Map;

namespace Template.Game.gameObjects.newServices
{
    public class MainCharacterService : ICharacterService
    {
        private Dictionary<string, SharpAudioVoice> voices;
        private AnimationService archerAnimationService;
        private AnimationService ArrowAnimationService;
        private bool isAnimation;
        private Archer archer;
        private MeshObject pointer;
        private Arrow arrow;
        private Material optionalPointerColor;
        private Material originalPointerColor;
        private InputController controller;
        private Queue<string> animationQueue;
        private int turns;
        public Character Character { get => archer; set => archer = value as Archer; }
        private Map map;
        public Map Map { get => map; set
            {
                map = value;
                archer.Offset = map.CellSize;
                arrow.Offset = map.CellSize;
            } }

        public MainCharacterService(string configFile, InputController controller, Loader loader, Material stub, SharpAudioDevice device)
        {
            voices = new Dictionary<string, SharpAudioVoice>();
            SetVoices(device);
            animationQueue = new Queue<string>();
            isAnimation = false;
            optionalPointerColor = stub;
            archer = new Archer(new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
            archer.IsActive = true;
            archer.AddMeshObjects(loader.LoadMeshesFromObject("Resources\\Assassin.obj", stub));
            arrow = new Arrow(archer.Position);
            arrow.AddMeshObject(archer["Bolt"]);
            arrow.MeshObjects[0].IsVisible = false;
            pointer = loader.LoadMeshFromObject("Resources\\Pointer.obj", stub);
            originalPointerColor = pointer.Material;
            archer.AddMeshObject(pointer);
            this.controller = controller;
            archerAnimationService = new AnimationService(archer, device);
            ArrowAnimationService = new AnimationService(arrow, device);
            turns = archer.TurnCount;
        }

        public void AddMeshObjects(List<MeshObject> meshObjects)
        {
            archer.AddMeshObjects(meshObjects);
        }

        public void Update()
        {
            if (controller[Key.U]) { archer.IsActive = true; turns = archer.TurnCount; }
            if (isAnimation)
            {
                Animate();
                return;
            }
            if (!archer.IsActive) return;
            SetUpDirection();
            SetUpPointer();
            if ((controller[Key.W]
                || controller[Key.A]
                || controller[Key.S]
                || controller[Key.D]) && pointer.IsVisible)
            {
                voices["select"].Stop();
                voices["select"].Play();
            }
                DoAction();
        }
        private void Animate()
        {
            string[] actions = animationQueue.Peek().Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (actions[0].Equals("archer"))
            {
                archerAnimationService.Animate(actions[1]);
                voices["slide"].Play();
            }
            else if (actions[0].Equals("arrow"))
            {
                voices["slide"].Stop();
                ArrowAnimationService.Animate(actions[1]);
            }
        }
        public void Render(Matrix viewMatrix, Matrix projectionMatrix)
        {
            foreach(var meshObject in archer.MeshObjects)
            {
                if (meshObject == pointer && (archer.Direction == Vector3.Zero || isAnimation)) continue;
                meshObject.Render(viewMatrix, projectionMatrix);
            }
            arrow.MeshObjects[0].Render(viewMatrix, projectionMatrix);
        }
        public void SetActive()
        {
            archer.IsActive = true;
            turns = archer.TurnCount;
        }
        public override string ToString()
        {
            return $"ARMOR: {archer.Health}\n" +
                $"ARROWS: {archer.ArrowAmount}\n" +
                $"TURNS: {archer.TurnCount}\n";
        }

        private void SetVoices(SharpAudioDevice device)
        {
            voices.Add("select", new SharpAudioVoice(device, "Resources\\Audio\\select.wav"));
            voices.Add("pickUp", new SharpAudioVoice(device, "Resources\\Audio\\pickUp.wav"));
            voices.Add("shoot", new SharpAudioVoice(device, "Resources\\Audio\\shoot.wav"));
            voices.Add("arrowDestroy", new SharpAudioVoice(device, "Resources\\Audio\\arrowDestroy.wav"));
            voices.Add("boxDestroy", new SharpAudioVoice(device, "Resources\\Audio\\boxDestroy.wav"));
            voices.Add("enemyHit", new SharpAudioVoice(device, "Resources\\Audio\\enemyHit.wav"));
            voices.Add("slide", new SharpAudioVoice(device, "Resources\\Audio\\slide.wav"));
        }

        private void SetUpDirection()
        {
            if (controller[Key.W]) archer.Direction = new Vector3(1, 0, 0);
            if (controller[Key.S]) archer.Direction = new Vector3(-1, 0, 0);
            if (controller[Key.A]) archer.Direction = new Vector3(0, 0, 1);
            if (controller[Key.D]) archer.Direction = new Vector3(0, 0, -1);
        }

        private void SetUpPointer()
        {
            pointer.Position = archer.GetNewPosition();
            pointer.Yaw = archer.GetNewYawRotation();
            pointer.IsVisible = true;

            if (!Map[pointer.Position].HasValue || archer.Direction == Vector3.Zero || isAnimation) pointer.IsVisible = false;
            else if ((Map[pointer.Position].Value.Unit == Unit.Door && map.IsClear))
                pointer.Material = originalPointerColor;
            else if ((Map[pointer.Position].Value.Unit == Unit.Door && !map.IsClear))
                pointer.Material = optionalPointerColor;
            else if (Map[pointer.Position].Value.Unit != Unit.Empty
                && Map[pointer.Position].Value.Unit != Unit.Archer
                && Map[pointer.Position].Value.Unit != Unit.Item)
                pointer.Material = optionalPointerColor;
            else pointer.Material = originalPointerColor;
        }

        private void DoAction()
        {
            if (archer.Direction == Vector3.Zero) return;
            if (controller[Key.G]) { if (!CanMove()) return; Move(); }
            else if (controller[Key.F]) Shoot();
            
        }
        private void Move()
        {            
            Map.CheckIn(archer.Position, Unit.Empty, null);
            Vector4 newPos = archer.GetNewPosition();
            isAnimation = true;
            animationQueue.Enqueue("archer_rotation");
            animationQueue.Enqueue("archer_slide");
            archerAnimationService.SetUpParameters("rotation", (s, e) => { animationQueue.Dequeue(); Console.WriteLine("first"); voices["slide"].Stop(); });
            archerAnimationService.SetUpParameters("slide", (s, e) => {
                if (Map[newPos].Value.Unit == Unit.Item)
                {
                    ((PickUp)Map[newPos].Value.UnitObject).ChangeStates(archer);
                    voices["pickUp"].Play();
                }
                if (Map[newPos].Value.Unit == Unit.Door)
                {
                    Cell cell = Map[newPos].Value;
                    cell.Unit = Unit.Archer;
                    Map[newPos] = cell;
                }
                Map.CheckIn(newPos, Unit.Archer, Character);
                animationQueue.Dequeue();
                isAnimation = false;
                arrow.Position = archer.Position;
                archer.Direction = Vector3.Zero;
                turns--;
                turns = (turns <= 0) ? 0 : turns;
                archer.IsActive = (turns == 0) ? false : true;
            });
            
        }
        private void Shoot()
        {
            if (!archer.Shoot()) return;
            
            isAnimation = true;
            arrow.Position = archer.Position;
            arrow.Yaw = archer.GetNewYawRotation();
            arrow.Direction = archer.Direction;
            Vector4 initialPosition = arrow.Position;
            while(true)
            {
                arrow.Position = arrow.GetNewPosition();
                if (!Map[arrow.Position].HasValue || (Map[arrow.Position].Value.Unit != Unit.Empty && Map[arrow.Position].Value.Unit != Unit.Item))
                    break;
            }
            Vector4 targetPosition = arrow.Position;
            animationQueue.Enqueue("archer_rotation");
            animationQueue.Enqueue("arrow_slide");
            archerAnimationService.SetUpParameters("rotation", (s, e) => { animationQueue.Dequeue(); arrow.MeshObjects[0].IsVisible = true; voices["slide"].Stop(); voices["shoot"].Play(); });
            ArrowAnimationService.SetUpParameters("slide", (s, e) => {
                if (Map[targetPosition].HasValue && Map[targetPosition].Value.Unit == Unit.Static)
                {
                    ((StaticObject)Map[targetPosition].Value.UnitObject).GetDamage(arrow.Damage);
                    voices["boxDestroy"].Play();
                }
                else if (Map[targetPosition].HasValue && Map[targetPosition].Value.Unit == Unit.Enemy)
                {
                    ((Character)Map[targetPosition].Value.UnitObject).GetDamage(arrow.Damage);
                    voices["enemyHit"].Play();
                }
                else
                    voices["arrowDestroy"].Play();
                animationQueue.Dequeue(); 
                arrow.MeshObjects[0].IsVisible = false; 
                arrow.Position = archer.Position; 
                isAnimation = false; 
                archer.Direction = Vector3.Zero;
                turns--;
                turns = (turns <= 0) ? 0 : turns;
                archer.IsActive = (turns == 0) ? false : true;
            },
                new List<object> { arrow.Position, (Vector4)arrow.Direction * 3.0f});
            arrow.Position = initialPosition;
        }

        private bool CanMove()
        {          
            Vector4 newPosition = archer.GetNewPosition();
            if (!Map[newPosition].HasValue
                || (Map[newPosition].Value.Unit != Unit.Empty
                && Map[newPosition].Value.Unit != Unit.Item
                && Map[newPosition].Value.Unit != Unit.Door))
                return false;
            if (Map[newPosition].Value.Unit == Unit.Door && !Map.IsClear)
                return false;
            return true;
        }

        public void Dispose()
        {
            for (int i = 0; i < voices.Values.Count; i++)
                voices.Values.ElementAt(i).Dispose();
            archer.MeshObjects.Dispose();
            arrow.MeshObjects.Dispose();
        }
    }
}

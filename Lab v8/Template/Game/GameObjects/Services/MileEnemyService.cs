using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.interfaces;
using Template.Game.gameObjects.newObjects;
using Template.Game.gameObjects.newServices;
using Template.Game.GameObjects.Objects;
using Template.Graphics;
using Template.Sound;

namespace Template.Game.GameObjects.Services
{
    class MileEnemyService : ICharacterService
    {
        private Dictionary<string, SharpAudioVoice> voices;
        private AnimationService enemyAnimationService;
        private InputController controller;
        private bool isAnimation;
        private MileEnemy enemy;
        private Character target;
        private Queue<string> animationQueue;
        private int turns;
        private List<Point> directions;

        private Map map;
        public Map Map
        {
            get => map; set
            {
                map = value;
                enemy.Offset = map.CellSize;
            }
        }
        public Character Character { get => enemy; set => enemy = value as MileEnemy; }

        public MileEnemyService(Character target, string meshFile, Loader loader, Material stub, InputController controller, SharpAudioDevice device)
        {
            voices = new Dictionary<string, SharpAudioVoice>();
            SetVoices(device);
            directions = new List<Point>
            {
                new Point(1,0),
                new Point(-1, 0),
                new Point(0,1),
                new Point(0,-1)
            };
            this.controller = controller;
            this.target = target;
            animationQueue = new Queue<string>();
            isAnimation = false;
            enemy = new MileEnemy(Vector4.Zero);
            enemy.AddMeshObjects(loader.LoadMeshesFromObject(meshFile, stub));
            turns = enemy.TurnCount;
            enemyAnimationService = new AnimationService(enemy, device);
            enemy.IsActive = true;
        }

        private void SetVoices(SharpAudioDevice device)
        {
            voices.Add("slide", new SharpAudioVoice(device, "Resources\\Audio\\slide.wav"));
            voices.Add("mileAttack", new SharpAudioVoice(device, "Resources\\Audio\\mileAttack.wav"));
            voices.Add("boxDestroy", new SharpAudioVoice(device, "Resources\\Audio\\boxDestroy.wav"));
            voices.Add("characterHit", new SharpAudioVoice(device, "Resources\\Audio\\characterHit.wav"));
        }

        public void Render(Matrix viewMatrix, Matrix projectionMatrix)
        {
            enemy.Render(viewMatrix, projectionMatrix);
        }

        public void Update()
        {
            if (controller[SharpDX.DirectInput.Key.H]) enemy.IsActive = true;
            if (isAnimation)
            {
                enemyAnimationService.Animate(animationQueue.Peek());
                return;
            }
            if (!enemy.IsActive) return;
            DoAction();
        }

        private void DoAction()
        {
            enemy.Target = target.Position;
            Vector4 newPosition = enemy.GetNewPosition();
            if (!map[newPosition].HasValue || map[newPosition].Value.Unit == Unit.Enemy)
            {
                directions.ForEach(dir =>
                {
                    if (!map[newPosition].HasValue || map[newPosition].Value.Unit == Unit.Enemy)
                        newPosition = enemy.GetNewDirectionPosition(dir.X, dir.Y);
                });
                    
            }
            Console.WriteLine($"UNIT: {map[newPosition].Value.Unit}");
            if (map[newPosition].Value.Unit == Unit.Empty || map[newPosition].Value.Unit == Unit.Item)
                Walk(newPosition);
            else if (map[newPosition].Value.Unit == Unit.Static || map[newPosition].Value.Unit == Unit.Archer)
                Attack(map[newPosition].Value.UnitObject);
            
        }
        public void SetActive()
        {
            enemy.IsActive = true;
            turns = enemy.TurnCount;
        }
        private void Walk(Vector4 newPos)
        {
            Map.CheckIn(enemy.Position, Unit.Empty, null);
            isAnimation = true;
            animationQueue.Enqueue("rotation");
            animationQueue.Enqueue("slide");
            voices["slide"].Play();
            enemyAnimationService.SetUpParameters("rotation", (s, e) => { animationQueue.Dequeue(); voices["slide"].Stop(); voices["slide"].Play(); });
            enemyAnimationService.SetUpParameters("slide", (s, e) => {
                if (Map[newPos].Value.Unit == Unit.Item)
                {
                    ((PickUp)Map[newPos].Value.UnitObject).Destroy();
                    voices["boxDestroy"].Play();
                }
                Map.CheckIn(newPos, Unit.Enemy, Character);
                animationQueue.Dequeue();
                isAnimation = false;
                turns--;
                turns = (turns <= 0) ? 0 : turns;
                enemy.IsActive = (turns == 0) ? false : true;
            }, new List<object> { newPos, (Vector4)enemy.Direction });
        }

        private void Attack(DrawableObject target)
        {
            isAnimation = true;
            Vector4 initialPos = enemy.Position;
            //Console.WriteLine($"POS: {initialPos}, ENEMY_POS: {target.Position}");
            //Console.WriteLine($"DIR: {enemy.Direction}, -DIR: {-enemy.Direction}");
            animationQueue.Enqueue("rotation");
            animationQueue.Enqueue("slide");
            animationQueue.Enqueue("back_slide");
            
            enemyAnimationService.SetUpParameters("rotation", (s, e) =>
            {
                animationQueue.Dequeue();
                voices["mileAttack"].Play();
            });
            enemyAnimationService.SetUpParameters("slide", (s, e) =>
            {

                animationQueue.Dequeue();
            }, new List<object> { target.Position, (Vector4)enemy.Direction * 1.0f });
            enemyAnimationService.SetUpParameters("back_slide", (s, e) =>
            {
                voices["mileAttack"].Stop();
                animationQueue.Dequeue();
                isAnimation = false;
                if (target is StaticObject)
                {
                    ((StaticObject)target).GetDamage(enemy.Damage);
                    voices["boxDestroy"].Play();
                }
                else if (target is Character)
                {
                    ((Character)target).GetDamage(enemy.Damage);
                    voices["characterHit"].Play();
                }
                turns--;
                turns = (turns <= 0) ? 0 : turns;
                enemy.IsActive = (turns == 0) ? false : true;
            }, new List<object> { initialPos, (Vector4)enemy.Direction * -1.0f });
        }

        public override string ToString()
        {
            return $"Pso: {enemy.Position} \nTarget: {enemy.Target}";
        }

        public void Dispose()
        {
            for (int i = 0; i < voices.Values.Count; i++)
                voices.Values.ElementAt(i).Dispose();
            enemy.MeshObjects.Dispose();
        }
    }
}

using SharpDX;
using System.Collections;
using System.Collections.Generic;

namespace Template.Game.gameObjects.newObjects
{
    public class Map : DrawableObject, IEnumerable<Map.Cell>
    {
        public struct Cell
        {
            public Vector4 Position;
            public Unit Unit;
            public DrawableObject UnitObject;
        }
        public bool IsClear { get; set; }
        public float CellSize { get; set; }

        private Cell[,] map;
        public Cell this[Point point]
        {
            get => map[point.X, point.Y];
            set => map[point.X, point.Y] = value;
        }
        public Cell? this[Vector4 position]
        {
            get
            {
                foreach (var cell in map)
                {
                    if (cell.Position == position) return cell;
                }
                return null;
            }
            set
            {
                for (int i = 0; i < Size.X; i++)
                    for (int j = 0; j < Size.Y; j++)
                        if (map[i, j].Position == position)
                            map[i, j] = (Cell)value;
            }
        }
        public Point Size { get; private set; }
        public Map(Vector4 initialPosition, Point size, float cellSize) : base(initialPosition)
        {
            map = new Cell[size.X + 1, size.Y];
            CellSize = cellSize;
            Size = size;
        }

        public void CheckIn(Point point, Unit unit)
        {
            map[point.X, point.Y].Unit = unit; 
        }

        public void CheckIn(Vector4 position, Unit unit, DrawableObject unitObject)
        {
            this[position] = new Cell
            {
                Position = this[position].Value.Position,
                Unit = unit,
                UnitObject = unitObject
            };
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            foreach(var cell in map)
            {
                yield return cell;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Graphics;

namespace Template.Game.gameObjects.newObjects
{
    public class Map : DrawableObject
    {
        public struct Cell
        {
            public Vector4 Position;
            public Unit Unit;
        }
        public float CellSize { get; set; }

        private Cell[,] map;
        public Cell this[Point point]
        {
            get => map[point.X, point.Y];
        }
        public Point Size { get; private set; }
        public Map(Vector4 initialPosition, Point size, float cellSize) : base(initialPosition)
        {
            map = new Cell[size.X, size.Y];
            CellSize = cellSize;
            Size = size;
        }

        public void CheckIn(Point point, Unit unit)
        {
            map[point.X, point.Y].Unit = unit; 
        }
    }
}

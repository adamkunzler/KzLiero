using Raylib_cs;
using System.Numerics;

namespace Kz.Liero
{
    public class Player
    {        
        /// <summary>
        /// Position in the world
        /// </summary>
        public Vector2 Position { get; set; }

        public static int Size => 3;

        public static int HalfSize => Size / 2;
        
        public Color Color { get; set; }

        public Player()
        {
            Position = new Vector2();            
        }

        public void Move(Vector2 amount, int worldWidth, int worldHeight)
        {
            var x = Position.X + amount.X;
            var y = Position.Y + amount.Y;

            // constrain player position to world boundaries
            if (x < HalfSize) x = HalfSize;
            if (x >= worldWidth - HalfSize - 1) x = worldWidth - HalfSize - 1;
            if (y < HalfSize) y = HalfSize;
            if (y >= worldHeight - HalfSize - 1) y = worldHeight - HalfSize - 1;

            Position = new Vector2(x, y);
        }

        public void Update()
        {
        }

        public void Render(Vector2 worldPosition)
        {
            var x = Position.X - worldPosition.X;
            var y = Position.Y - worldPosition.Y;

            Raylib.DrawCircle((int)x, (int)y, Size, Color);
        }        
    }
}
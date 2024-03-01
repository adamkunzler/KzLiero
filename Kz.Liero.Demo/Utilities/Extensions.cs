using Kz.Engine.DataStructures;

namespace Kz.Liero.Demo.Utilities
{
    public static class Extensions
    {
        public static bool Contains(this Raylib_cs.Rectangle rect, Vector2f point)
        {
            var horiz = point.X > rect.X && point.X < rect.X + rect.Width;
            var vert = point.Y > rect.Y && point.Y < rect.Y + rect.Height;
            return horiz && vert;
        }
    }
}
using Raylib_cs;

namespace Kz.Liero
{
    public enum DirtType
    {
        Dirt,
        Rock,
    }

    public struct Dirt
    {
        public bool IsActive { get; set; }
        
        public Color Color { get; set; }
        
        public DirtType Type { get; set;}
    }
}
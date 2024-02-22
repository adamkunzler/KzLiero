using System.Numerics;

namespace Kz.Liero
{
    public struct WindowSettings
    {
        public int WindowWidth { get; init; }

        public int HalfWindowWidth { get; init; }

        public int WindowHeight { get; init; }

        public int HalfWindowHeight { get; init; }

        public int Scale { get; init; }

        public int ScreenWidth { get; init; }

        public int HalfScreenWidth { get; init; }

        public int ScreenHeight { get; init; }

        public int HalfScreenHeight { get; init; }

        public WindowSettings(int windowWidth, int windowHeight, int scale)
        {            
            WindowWidth = windowWidth;
            WindowHeight = windowHeight;            
            HalfWindowWidth = WindowWidth / 2;
            HalfWindowHeight = WindowHeight / 2;
            Scale = scale;

            ScreenWidth = WindowWidth / scale;
            ScreenHeight = WindowHeight / scale;
            HalfScreenWidth = ScreenWidth / 2;
            HalfScreenHeight = ScreenHeight / 2;
        }        
    }
}
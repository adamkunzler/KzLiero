using Kz.Liero.Utilities;
using Raylib_cs;
using System.Numerics;

namespace Kz.Liero
{
    public class Background
    {
        private readonly Color[] _backgroundColors = [
            new Color(109, 56, 0, 255),
            new Color(109, 56, 0, 255), // dup
            new Color(97, 48, 0, 255),
            new Color(97, 48, 0, 255), // dup
            new Color(93, 44, 0, 255),
            new Color(93, 44, 0, 255), // dup
            new Color(85, 40, 0, 255)
        ];

        public static Color DefaultColor => new Color(85, 40, 0, 255);

        private RenderTexture2D _backgroundTexture;

        private readonly int _worldWidth;
        private readonly int _worldHeight;

        public Background(int worldWidth, int worldHeight)
        {
            _worldWidth = worldWidth;
            _worldHeight = worldHeight;

            InitBackgroundTexture();
        }

        /// <summary>
        /// Render a chunk of the arena
        /// </summary>
        /// <param name="position">top left in the world</param>
        /// <param name="size">width/height</param>
        public void Render(Vector2 position, Vector2 size)
        {
            RaylibHelper.RenderTexture(_backgroundTexture,
                            (int)position.X, (int)position.Y, (int)size.X, -(int)size.Y,
                            0, 0, (int)size.X, (int)size.Y,
                            Color.White);
        }

        public void Cleanup()
        {
            Raylib.UnloadRenderTexture(_backgroundTexture);
        }

        private void InitBackgroundTexture()
        {
            _backgroundTexture = Raylib.LoadRenderTexture(_worldWidth, _worldHeight);

            var noise = Utils.GenerateNoiseMap(_worldWidth, _worldHeight);

            Raylib.BeginTextureMode(_backgroundTexture);
            Raylib.ClearBackground(Color.Black);

            for (var y = 0; y < _worldHeight; y++)
            {
                for (var x = 0; x < _worldWidth; x++)
                {
                    var colorIndex = noise[x + y * _worldWidth];
                    Raylib.DrawPixel(x, y, _backgroundColors[colorIndex]);
                }
            }

            Raylib.EndTextureMode();
        }
    }
}
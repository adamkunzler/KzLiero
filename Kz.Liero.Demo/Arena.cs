using Kz.Liero.Utilities;
using Raylib_cs;
using System.Numerics;

namespace Kz.Liero
{
    /// <summary>
    /// Represents the foreground dirt and rocks
    /// </summary>
    public class Arena
    {
        #region Colors

        private readonly Color[] _shadowColors = [
            new Color(60, 28, 0, 255),
            new Color(65, 28, 0, 255),
            new Color(69, 32, 0, 255),
            new Color(73, 36, 0, 255)
        ];

        private readonly Color[] _dirtColors = [
            new Color(154, 85, 20, 255),
            new Color(146, 81, 16, 255),
            new Color(146, 81, 16, 255), // dup
            new Color(130, 69, 8, 255),
            new Color(130, 69, 8, 255), // dup
            new Color(121, 65, 8, 255),
            new Color(121, 65, 8, 255), // dup
        ];

        #endregion Colors

        #region ctor

        private readonly Random _random = new Random();
        private WindowSettings _settings;

        private Dirt[] _dirt = [];
        private readonly int _worldWidth;
        private readonly int _worldHeight;

        public Arena(WindowSettings settings, int worldWidth, int worldHeight)
        {
            _settings = settings;
            _worldWidth = worldWidth;
            _worldHeight = worldHeight;
        }

        #endregion ctor

        #region Public

        public void Init()
        {
            InitDirt();
        }

        public void Update()
        {
        }

        /// <summary>
        /// Render a chunk of the arena
        /// </summary>
        /// <param name="position">top left in the world</param>
        /// <param name="size">width/height</param>
        public void Render(Vector2 position, Vector2 size)
        {
            // check bounds
            if (!IsInBounds((int)position.X, (int)position.Y)) return;
            if (!IsInBounds((int)position.X + (int)size.X, (int)position.Y + (int)size.Y)) return;
            
            // render dirt
            for (var y = 0; y < (int)size.Y; y++)
            {
                for (var x = 0; x < (int)size.X; x++)
                {
                    var worldX = (int)position.X + x;
                    var worldY = (int)position.Y + y;

                    var index = worldX + worldY * _worldWidth;
                    if (!_dirt[index].IsActive)
                    {
                        Raylib.DrawPixel(x, y, new Color(0,0,0, 0)); // draw a transparent pixel
                        continue;
                    }

                    Raylib.DrawPixel(x, y, _dirt[index].Color);
                }
            }
        }

        public void Cleanup()
        {
        }

        public Dirt? DirtAt(int x, int y)
        {
            if (!IsInBounds(x, y)) return null;

            return _dirt[x + y * _worldWidth];
        }

        public void RemoveDirt(int px, int py, int radius)
        {
            var radiusSquared = radius * radius;

            // iterate over each pixel in the circle's bounding box
            for (var y = py - radius; y <= py + radius; y++)
            {
                for (var x = px - radius; x <= px + radius; x++)
                {
                    if (!IsInBounds(x, y)) continue;

                    // check if x and y are in circle => (x - x0)^2 + (y - y0)^2 <= radius^2
                    var val = ((x - px) * (x - px)) + ((y - py) * (y - py));
                    if (val < radiusSquared)
                    {
                        _dirt[x + y * _worldWidth].IsActive = false;
                    }
                }
            }
        }

        /// <summary>
        /// Check if coordinate is in world bounds
        /// </summary>
        public bool IsInBounds(int x, int y)
        {
            var isInBounds = x >= 0 && x < _worldWidth && y >= 0 && y < _worldHeight;
            return isInBounds;
        }

        #endregion Public

        #region Private Methods

        private void InitDirt()
        {
            var noise = Utils.GenerateNoiseMap(_worldWidth, _worldHeight);

            _dirt = new Dirt[_worldWidth * _worldHeight];
            for (var i = 0; i < _worldWidth * _worldHeight; i++)
            {
                _dirt[i] = new Dirt
                {
                    IsActive = true,
                    Color = _dirtColors[noise[i]],
                };
            }
        }

        #endregion Private Methods
    }
}
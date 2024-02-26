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

        #region Sprites

        private const int NUM_ROCK_IMAGES = 3;
        private const int MAX_ROCKS = 25;
        private Image[] _rocks = new Image[NUM_ROCK_IMAGES];

        private const int NUM_DIRTBALL_IMAGES = 3;
        private const int MAX_DIRTBALLS = 50;
        private Image[] _dirtballs = new Image[NUM_DIRTBALL_IMAGES];

        private const int NUM_BONE_IMAGES = 3;
        private const int MAX_BONES = 25;
        private Image[] _bones = new Image[NUM_BONE_IMAGES];

        #endregion Sprites

        #region ctor

        private readonly Random _random = new Random();
        private WindowSettings _settings;

        private Dirt[] _dirt = [];
        private readonly int _worldWidth;
        private readonly int _worldHeight;

        public Arena() { }

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
            InitRocks();
            InitDirtBalls();
            InitBones();
            InitDirt();
        }

        public void Update()
        {
        }

        /// <summary>
        /// Render a chunk of the arena
        /// </summary>        
        public void Render(Rectangle viewPortDimension)
        {
            // check bounds
            if (!IsInBounds((int)viewPortDimension.X, (int)viewPortDimension.Y)) return;
            if (!IsInBounds(
                (int)viewPortDimension.X + (int)viewPortDimension.Width, 
                (int)viewPortDimension.Y + (int)viewPortDimension.Height)) return;
            
            // render dirt
            for (var y = 0; y < (int)viewPortDimension.Height; y++)
            {
                for (var x = 0; x < (int)viewPortDimension.Width; x++)
                {
                    var worldX = (int)viewPortDimension.X + x;
                    var worldY = (int)viewPortDimension.Y + y;

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
            for(var i = 0; i < NUM_ROCK_IMAGES; i++)
            {
                Raylib.UnloadImage(_rocks[i]);
            }
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
                        var index = x + y * _worldWidth;
                        if (_dirt[index].Type != DirtType.Dirt) continue; // only remove DirtType.Dirt
                        _dirt[index].IsActive = false;
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
            //
            // create the dirt
            //
            var noise = Utils.GenerateNoiseMap(_worldWidth, _worldHeight);
            
            _dirt = new Dirt[_worldWidth * _worldHeight];

            for (var i = 0; i < _worldWidth * _worldHeight; i++)
            {
                _dirt[i] = new Dirt
                {
                    IsActive = true,
                    Color = _dirtColors[noise[i]],
                    Type = DirtType.Dirt,
                };
            }

            //
            // create the rocks, dirt balls, and bones
            //
            InitDirtObjects(MAX_ROCKS, _rocks, DirtType.Rock);            
            InitDirtObjects(MAX_DIRTBALLS, _dirtballs, DirtType.Dirt);
            InitDirtObjects(MAX_BONES, _bones, DirtType.Dirt);
        }

        private void InitDirtObjects(int maxObjects, Image[] images, DirtType dirtType)
        {
            var numObjects = _random.Next(maxObjects);
            for (var i = 0; i < numObjects; i++)
            {
                var objIndex = _random.Next(images.Length);
                var image = images[objIndex];

                var originX = _random.Next(_worldWidth);
                var originY = _random.Next(_worldHeight);

                var halfImageWidth = image.Width / 2;
                var halfImageHeight = image.Height / 2;

                for (var y = 0; y < image.Height; y++)
                {
                    for (var x = 0; x < image.Width; x++)
                    {
                        var color = Raylib.GetImageColor(image, x, y);
                        if (color.R == 0 && color.G == 0 && color.B == 0 && color.A == 0) continue;

                        var wx = x + originX - halfImageWidth;
                        var wy = y + originY - halfImageHeight;

                        if (!IsInBounds(wx, wy)) continue;

                        var index = wx + wy * _worldWidth;
                        _dirt[index] = new Dirt
                        {
                            IsActive = true,
                            Color = color,
                            Type = dirtType,
                        };
                    }
                }
            }
        }

        private void InitRocks()
        {                        
            for(var i = 1; i <= NUM_ROCK_IMAGES; i++)
            {
                var filename = $"Resources\\Rock00{i}.png";
                _rocks[i - 1] = Raylib.LoadImage(filename);
            }
        }

        private void InitDirtBalls()
        {
            for (var i = 1; i <= NUM_DIRTBALL_IMAGES; i++)
            {
                var filename = $"Resources\\DirtBall00{i}.png";
                _dirtballs[i - 1] = Raylib.LoadImage(filename);
            }
        }

        private void InitBones()
        {
            for (var i = 1; i <= NUM_BONE_IMAGES; i++)
            {
                var filename = $"Resources\\Bones00{i}.png";
                _bones[i - 1] = Raylib.LoadImage(filename);
            }
        }

        #endregion Private Methods
    }
}
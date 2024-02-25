using Raylib_cs;
using System.Numerics;

namespace Kz.POC
{   
    public struct SpriteConfig
    {
        /// <summary>
        /// Filename of the spritesheet
        /// </summary>
        public string Filename;

        /// <summary>
        /// Filename of the frag shader
        /// </summary>
        public string FragShaderFilename;

        /// <summary>
        /// How many frames of animation there are (columns)
        /// </summary>
        public int MaxFrames;

        /// <summary>
        /// How many different animations there are (rows)
        /// </summary>
        public int MaxAnimations;

        /// <summary>
        /// How fast to play the animation
        /// </summary>
        public float FrameSpeed;

        /// <summary>
        /// The default frame to display when not animating
        /// </summary>
        public int DefaultFrameIndex;

        /// <summary>
        /// The width of a single image in the spritesheet
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of a single image in the spritesheet
        /// </summary>
        public int Height;        

        /// <summary>
        /// Color to tint the image (currently tied to the shader)
        /// </summary>
        public Color Tint;
    }

    /// <summary>
    /// TODO - what if no shader? or have a default shader?
    /// </summary>
    public class Sprite
    {
        private int _spriteIndex;
        private int _frameIndex;
        private float _frameSpeed;
        private float _frameTime = 0.0f;
        private int _frameDir = 1;
        private int _defaultFrameIndex;
        private int _maxFrames;  
        private int _maxAnimations;

        private Texture2D _sprites;

        private Shader _shader;
        private int _shaderShadeLocation;
        private float[] _shaderShade = [];

        private int _width;
        private int _height;
                
        public Sprite(SpriteConfig config)
        {
            _maxFrames = config.MaxFrames;
            _maxAnimations = config.MaxAnimations;
            _frameSpeed = config.FrameSpeed;
            _defaultFrameIndex = config.DefaultFrameIndex;
            _width = config.Width;
            _height = config.Height;
            
            _sprites = Raylib.LoadTexture(config.Filename);
            _shader = Raylib.LoadShader("", config.FragShaderFilename);

            _shaderShadeLocation = Raylib.GetShaderLocation(_shader, "shade");            
            _shaderShade = [config.Tint.R / 255.0f, config.Tint.G / 255.0f, config.Tint.B / 255.0f, config.Tint.A / 255.0f];

            //var temp = Raylib.LoadShaderFromMemory("", "");
        }

        public void Update()
        {
            _frameTime += Raylib.GetFrameTime();
            if (_frameTime > _frameSpeed)
            {
                _frameTime = 0.0f;
                _frameIndex += _frameDir;
                if (_frameIndex >= _maxFrames)
                {
                    _frameIndex = _maxFrames - 2;
                    _frameDir = -_frameDir;
                }
                else if (_frameIndex < 0)
                {
                    _frameIndex = 1;
                    _frameDir = -_frameDir;
                }
            }
        }

        public void Render(int x, int y, int sizeX, int sizeY, bool flipX, bool flipY)
        {            
            Raylib.SetShaderValue(_shader, _shaderShadeLocation, _shaderShade, ShaderUniformDataType.Vec4);
            
            Raylib.BeginShaderMode(_shader);
            var spriteX = _frameIndex * _width;
            var spriteY = _spriteIndex * _height;
            var source = new Rectangle(spriteX, spriteY, flipX ? -_width : _width, flipY ? -_height : _height);
            var dest = new Rectangle(x, y, sizeX, sizeY );
            var origin = new Vector2(sizeX, sizeY);
            Raylib.DrawTexturePro(_sprites, source, dest, origin, 0.0f, Color.White);
            Raylib.EndShaderMode();
        }

        public void Cleanup()
        {
            Raylib.UnloadTexture(_sprites);
            Raylib.UnloadShader(_shader);
        }

        public void SetDefaultState()
        {
            _frameIndex = _defaultFrameIndex;
            _frameTime = 0.0f;
            _frameDir = 1;
        }

        public void SetSpriteAnimationIndex(int index)
        {
            if (index < 0 || index >= _maxAnimations) return;
            _spriteIndex = index;
        }

        public List<(int X, int Y)> GetSpriteEdges(int frameIndex, int animationIndex)
        {          
            // get the part of the spritesheet we care about
            var wholeImage = Raylib.LoadImageFromTexture(_sprites);
            var section = new Rectangle(frameIndex * _width, animationIndex * _height, _width, _height);
            var sectionImage = Raylib.ImageFromImage(wholeImage, section);

            // load the color data
            Color[] pixels = [];            
            unsafe
            {
                var pixelsPointer = Raylib.LoadImageColors(sectionImage);                
                pixels = new Color[_width * _height];

                for (int i = 0; i < _width * _height; i++)
                {
                    pixels[i] = pixelsPointer[i]; // Copy each element
                }

                Raylib.UnloadImageColors(pixelsPointer);
            }
                        
            // Define what we consider as 'transparent'
            var transparent = new Color(0, 0, 0, 0);  // Assuming fully transparent

            // find the edges
            var edgePixels = new List<(int, int)>();
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    // Get the current pixel
                    var currentPixel = pixels[y * _width + x];

                    // Skip transparent pixels
                    if (currentPixel.A == transparent.A) continue;

                    // Check neighboring pixels
                    var isEdge = false;
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            // Skip the center pixel
                            if (dx == 0 && dy == 0) continue;

                            var nx = x + dx;
                            var ny = y + dy;

                            // Check boundaries
                            if (nx >= 0 && nx < _width && ny >= 0 && ny < _height)
                            {
                                var neighborPixel = pixels[ny * _width + nx];
                                if (neighborPixel.A == transparent.A)
                                {
                                    isEdge = true;
                                    break;
                                }
                            }
                        }
                        if (isEdge) break;  // No need to check other neighbors if already found an edge
                    }

                    if (isEdge)
                    {
                        edgePixels.Add((x, y));
                    }
                }
            }

            // cleanup
            Raylib.UnloadImage(wholeImage);
            Raylib.UnloadImage(sectionImage);
            
            return edgePixels;
        }
    }
}

using Raylib_cs;
using System.Numerics;


namespace Kz.POC
{   
    public struct SpriteConfig
    {
        public string Filename;
        public string FragShaderFilename;
        public int MaxFrames;
        public int MaxAnimations;
        public float FrameSpeed;
        public int DefaultFrameIndex;
        public int Width;
        public int Height;
        //public float Scale;
        public Color Tint;
    }

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
        //private float _scale;
        
        public Sprite(SpriteConfig config)
        {
            _maxFrames = config.MaxFrames;
            _maxAnimations = config.MaxAnimations;
            _frameSpeed = config.FrameSpeed;
            _defaultFrameIndex = config.DefaultFrameIndex;
            _width = config.Width;
            _height = config.Height;
            //_scale = config.Scale;

            _sprites = Raylib.LoadTexture(config.Filename);
            _shader = Raylib.LoadShader("", config.FragShaderFilename);

            _shaderShadeLocation = Raylib.GetShaderLocation(_shader, "shade");            
            _shaderShade = [config.Tint.R / 255.0f, config.Tint.G / 255.0f, config.Tint.B / 255.0f, config.Tint.A / 255.0f];
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

            //Raylib.BeginShaderMode(_shader);
            //var spriteX = _frameIndex * 20;
            //var spriteY = _spriteIndex * 20;
            //var source = new Rectangle(spriteX, spriteY, flipX ? -_width : _width,  flipY ? -_height : _height);
            //var dest = new Rectangle(x, y, sizeX * _scale, sizeY * _scale);
            //var origin = new Vector2(sizeX + _scale / 2.0f, sizeY + _scale / 2.0f);
            //Raylib.DrawTexturePro(_sprites, source, dest, origin, 0.0f, Color.White);
            //Raylib.EndShaderMode();

            Raylib.BeginShaderMode(_shader);
            var spriteX = _frameIndex * _width;
            var spriteY = _spriteIndex * _height;
            var source = new Rectangle(spriteX, spriteY, flipX ? -_width : _width, flipY ? -_height : _height);
            var dest = new Rectangle(x, y, sizeX, sizeY );
            var origin = new Vector2(sizeX, sizeY);
            Raylib.DrawTexturePro(_sprites, source, dest, origin, 0.0f, Color.White);
            Raylib.EndShaderMode();
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
    }
}

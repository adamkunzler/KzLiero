using Raylib_cs;
using System.Numerics;

namespace Kz.Liero
{    
    public class ViewPort
    {
        public Vector2 ScreenPosition { get; set; }
        public Vector2 Size { get; init; }
        public Vector2 HalfSize => new Vector2(Size.X / 2, Size.Y / 2);

        private World _world;
        private Background _background;


        private RenderTexture2D _target;
        public RenderTexture2D Target => _target;

        private Shader _transparentBlackShader;
        private Shader _greyscaleShader;


        public ViewPort(World world, Vector2 screenPosition, Vector2 size)
        {
            _world = world;

            _background = new Background(_world.WorldWidth, _world.WorldHeight);

            ScreenPosition = screenPosition;
            Size = size;
            
            _target = Raylib.LoadRenderTexture((int)Size.X, (int)Size.Y);

            _transparentBlackShader = Raylib.LoadShader("", "Shaders/TransparentBlack.frag");
            _greyscaleShader = Raylib.LoadShader("", "Shaders/Greyscale.frag");
        }

        public Rectangle GetViewPortDimension(Vector2 targetCenter)
        {
            // get top/left - either 0 or the targetCenter minus halfSize
            var left = Math.Max(0, targetCenter.X - HalfSize.X);
            var top = Math.Max(0, targetCenter.Y - HalfSize.Y);

            // check if past right/bottom world boundaries
            var right = left + Size.X;
            if (right > _world.WorldWidth - 1)
            {
                left = _world.WorldWidth - Size.X - 1;
            }

            var bottom = top + Size.Y;
            if (bottom > _world.WorldHeight - 1)
            {
                top = _world.WorldHeight - Size.Y - 1;
            }

            return new Rectangle(new Vector2(left, top), Size);
        }

        public void Render(World world, Vector2 targetCenter)
        {
            //
            // calculate boundaries of the viewport in the world
            //
            var viewPortDimension = GetViewPortDimension(targetCenter);
            
            //
            // render the world to it's own texture
            //
            world.Render(viewPortDimension);

            //
            // render to target texture
            //
            Raylib.BeginTextureMode(_target);
            Raylib.ClearBackground(Color.Black);
            
            // draw background
            _background.Render(viewPortDimension);

            // render foreground as a shadow
            Raylib.BeginShaderMode(_greyscaleShader);                        
            RaylibHelper.RenderTexture(world.Target,
                0, 0, (int)Size.X, (int)Size.Y,
                -2, 2, (int)Size.X - 2, (int)Size.Y + 2,
                Color.White);
            Raylib.EndShaderMode();

            // render the world with transparency
            Raylib.BeginShaderMode(_transparentBlackShader);
            RaylibHelper.RenderTexture(world.Target, (int)Size.X, (int)Size.Y);
            Raylib.EndShaderMode();

            Raylib.EndTextureMode();
        }
        
        public void Cleanup()
        {
            Raylib.UnloadRenderTexture(_target);
            _background.Cleanup();
            Raylib.UnloadShader(_transparentBlackShader);
            Raylib.UnloadShader(_greyscaleShader);
        }
    }
}
using Raylib_cs;
using System.Numerics;

/*

BACKGROUND
rgb(93, 44, 0)
rgb(97, 48, 0)
rgb(109, 56, 0)
rgb(85, 40, 0)

BACKGROUND SHADOW
rgb(60, 28, 0)
rgb(65, 28, 0)
rgb(69, 32, 0)
rgb(73, 36, 0)

DIRT
rgb(146, 81, 16)
rgb(154, 85, 20)
rgb(130, 69, 8)
rgb(121, 65, 8)

ROCK
rgb(85, 85, 85)
rgb(101, 101, 101)
rgb(125, 125, 125)
rgb(134, 134, 134)
rgb(150, 150, 150)
rgb(158, 158, 158)

DIRT BALL
rgb(109, 77, 44)
rgb(125, 85, 48)
rgb(142, 97, 56)
rgb(158, 109, 65)
rgb(174, 121, 73)

INDICATOR
rgb(251, 60, 60)

DINOSAUR BONE
rgb(134, 93, 40)
rgb(162, 134, 73)
rgb(190, 178, 105)
rgb(219, 223, 138)

 */

namespace Kz.Liero
{
    public class Game : IGame
    {
        #region ctor

        private WindowSettings _settings;
        private RenderTexture2D _target;
        private World _world;
        private ViewPort _view1;
        private ViewPort _view2;
        private Minimap _minimap;

        private const float VIEWPORT_HEIGHT_PERCENT = 0.75f;

        public RenderTexture2D Texture => _target;

        public Game(WindowSettings settings, int worldWidth, int worldHeight)
        {
            _settings = settings;            
            _world = new World(settings, worldWidth, worldHeight);

            var vpWidth = settings.ScreenWidth / 2;
            var vpHeight = (int)(settings.ScreenHeight * VIEWPORT_HEIGHT_PERCENT);

            _view1 = new ViewPort(
                _world,
                new Vector2(0, 0), // screen position
                new Vector2(vpWidth - 1, vpHeight) // width/height
            );

            _view2 = new ViewPort(
                _world,
                new Vector2(vpWidth + 1, 0), // screen position
                new Vector2(vpWidth - 1, vpHeight) // width/height
            );

            var mmHeight = (settings.ScreenHeight - vpHeight) - 10;
            var mmWidth = mmHeight * (worldWidth / worldHeight);
            _minimap = new Minimap(mmWidth, mmHeight);

            Init();
        }

        #endregion ctor

        #region Public Methods - IGame

        private void Init()
        {
            _target = Raylib.LoadRenderTexture(_settings.ScreenWidth, _settings.ScreenHeight);
            _world.Init();
        }

        public void Update()
        {
            _world.Update();
        }

        public void Render()
        {
            _view1.Render(_world, _world.Player1Position);
            _view2.Render(_world, _world.Player2Position);
            _minimap.Render(_world);

            // Final Render to Target Texture
            Raylib.BeginTextureMode(_target);
            Raylib.ClearBackground(Color.Black);

            RenderViewPortToTexture(_view1);
            RenderViewPortToTexture(_view2);

            // center minimap beneath the viewports
            var mmX = (_settings.ScreenWidth / 2.0f) - (_minimap.Width / 2.0f);
            var mmY = _settings.ScreenHeight * VIEWPORT_HEIGHT_PERCENT + 5;
            RaylibHelper.RenderTexture(_minimap.Target,
                0, 0, _minimap.Width, _minimap.Height,
                (int)mmX, (int)mmY, _minimap.Width, _minimap.Height,
                Color.White);

            Raylib.EndTextureMode();
        }

        public void ProcessInputs()
        {
            _world.ProcessInputs();
        }

        public void End()
        {
            Raylib.UnloadRenderTexture(_target);
            _world.Cleanup();
            _view1.Cleanup();
        }

        #endregion Public Methods - IGame

        #region Private Methods

        public void RenderViewPortToTexture(ViewPort view)
        {
            var src = new Rectangle(0, 0, view.Target.Texture.Width, -view.Target.Texture.Height);
            var dest = new Rectangle(view.ScreenPosition.X, view.ScreenPosition.Y, view.Size.X, view.Size.Y);

            Raylib.DrawTexturePro(
                view.Target.Texture,
                src,
                dest,
                new System.Numerics.Vector2(0.0f, 0.0f),
                0,
                Color.White);
        }

        #endregion
    }
}
using Raylib_cs;
using System.Numerics;

namespace Kz.Liero
{
    /// <summary>
    /// Represents the world. The background, dirt, and any other objects that make up the world
    /// </summary>
    public class World
    {
        #region ctor

        private WindowSettings _settings;
        private Random _random = new Random();

        private readonly int _worldWidth;
        public int WorldWidth => _worldWidth;

        private readonly int _worldHeight;
        public int WorldHeight => _worldHeight;

        private RenderTexture2D? _target = null;
        public RenderTexture2D Target => _target!.Value;

        private Arena _arena = new();
        private Player _player1 = new(0,0, Color.RayWhite);
        private Player _player2 = new(0, 0, Color.RayWhite);

        private static float _playerSpeed = 1.75f;
                        
        public Vector2 Player1Position => _player1.Position;
        public Vector2 Player2Position => _player2.Position;
        public Color Player1Color => _player1.Color;
        public Color Player2Color => _player2.Color;

        private const int DIG_SIZE = 7;
        private const int DIG_DISTANCE = 3;

        public World(WindowSettings settings, int worldWidth, int worldHeight)
        {
            _settings = settings;
            _worldWidth = worldWidth;
            _worldHeight = worldHeight;
        }

        #endregion ctor

        #region Public Methods

        public void Init()
        {
            _arena = new Arena(_settings, _worldWidth, _worldHeight);
            _arena.Init();

            _player1 = new Player(
                _random.Next(10, _worldWidth - 10), 
                _random.Next(10, _worldWidth - 10), 
                Color.Green);            
            _arena.RemoveDirt((int)_player1.Position.X, (int)_player1.Position.Y, DIG_SIZE);

            _player2 = new Player(
                _random.Next(10, _worldWidth - 10), 
                _random.Next(10, _worldWidth - 10), 
                Color.Purple);            
            _arena.RemoveDirt((int)_player2.Position.X, (int)_player2.Position.Y, DIG_SIZE);
        }
        
        public void Update(Rectangle viewPortDimension1, Rectangle viewPortDimension2)
        {            
            _arena.Update();
            _player1.Update(_worldWidth, _worldHeight, viewPortDimension1, _arena.DirtAt);
            _player2.Update(_worldWidth, _worldHeight, viewPortDimension2, _arena.DirtAt);
        }

        /// <summary>
        /// Render a chunk of the world defined by an AABB
        /// </summary>        
        public void Render(Rectangle viewPortDimension)
        {            
            if (!_target.HasValue)
            {
                _target = Raylib.LoadRenderTexture(
                    (int)viewPortDimension.Width, 
                    (int)viewPortDimension.Height);
            }

            Raylib.BeginTextureMode(_target.Value);
            Raylib.ClearBackground(Color.Black);

            _arena.Render(viewPortDimension);

            // render entities (weapons, particles, etc)
            // TODO
            
            // render players (if in bounds)            
            RenderPlayers(viewPortDimension);

            Raylib.EndTextureMode();
        }

        public void ProcessInputs(Rectangle viewPortDimension1, Rectangle viewPortDimension2)
        {
            if (Raylib.IsMouseButtonDown(MouseButton.Left))
            {
                var mouseX = Raylib.GetMousePosition().X / _settings.Scale;
                var mouseY = Raylib.GetMousePosition().Y / _settings.Scale;
            }

            #region Player 1 Controls
            
            if (Raylib.IsKeyDown(KeyboardKey.A))
            {
                _player1.MoveLeft(viewPortDimension1.Position, _arena.DirtAt);
            }
            else if (Raylib.IsKeyDown(KeyboardKey.D))
            {
                _player1.MoveRight(viewPortDimension1.Position, _arena.DirtAt);
            }

            if (Raylib.IsKeyUp(KeyboardKey.A) && !Raylib.IsKeyDown(KeyboardKey.D)) { _player1.State = WormState.Still; }
            if (Raylib.IsKeyUp(KeyboardKey.D) && !Raylib.IsKeyDown(KeyboardKey.A)) { _player1.State = WormState.Still; }

            if (Raylib.IsKeyDown(KeyboardKey.W))
            {
                _player1.Aim(-1);
            }
            else if (Raylib.IsKeyDown(KeyboardKey.S))
            {
                _player1.Aim(1);
            }

            #endregion Player 1 Controls

            #region Player 2 Controls
            
            if (Raylib.IsKeyDown(KeyboardKey.Left))
            {
                _player2.MoveLeft(viewPortDimension2.Position, _arena.DirtAt);
            }
            else if (Raylib.IsKeyDown(KeyboardKey.Right))
            {
                _player2.MoveRight(viewPortDimension2.Position, _arena.DirtAt);                
            }

            if (Raylib.IsKeyPressed(KeyboardKey.RightControl))
            {
                var dir = _player2.GetAimAngleVector(viewPortDimension2.Position);
                Dig(_player2.Position, new Vector2(dir.X, dir.Y));
            }

            if (Raylib.IsKeyUp(KeyboardKey.Left) && !Raylib.IsKeyDown(KeyboardKey.Right)) { _player2.State = WormState.Still; }
            if (Raylib.IsKeyUp(KeyboardKey.Right) && !Raylib.IsKeyDown(KeyboardKey.Left)) { _player2.State = WormState.Still; }

            if (Raylib.IsKeyDown(KeyboardKey.Up))
            {                
                _player2.Aim(-1);
            }
            else if (Raylib.IsKeyDown(KeyboardKey.Down))
            {
                _player2.Aim(1);
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                _player2.Jump();
            }

            #endregion Player 2 Controls
        }

        public void Cleanup()
        {
            _arena.Cleanup();
        }

        public Dirt? DirtAt(int x, int y)
        {
            var dirt =_arena.DirtAt(x, y);
            return dirt;
        }

        #endregion Public Methods

        /// <summary>
        /// Render players relative to the viewport
        /// </summary>
        /// <param name="viewPortDimension"></param>
        private void RenderPlayers(Rectangle viewPortDimension)
            {
            var maxBounds = new Vector2
            (
                viewPortDimension.X + viewPortDimension.Width,
                viewPortDimension.Y + viewPortDimension.Height
            );

            if (InBounds(viewPortDimension.Position, maxBounds, _player1.Position))
            {
                _player1.Render(viewPortDimension.Position);
            }

            if (InBounds(viewPortDimension.Position, maxBounds, _player2.Position))
            {
                _player2.Render(viewPortDimension.Position);
            }
        }

        private bool InBounds(Vector2 minBounds, Vector2 maxBounds, Vector2 position)
        {
            return position.X >= minBounds.X && position.X <= maxBounds.X &&
                   position.Y >= minBounds.Y && position.Y <= maxBounds.Y;
        }

        private void Dig(Vector2 position, Vector2 dir)
        {
            var px = position.X + dir.X * DIG_DISTANCE;
            var py = position.Y + dir.Y * DIG_DISTANCE;
            _arena.RemoveDirt((int)px, (int)py, DIG_SIZE);
        }
    }
}
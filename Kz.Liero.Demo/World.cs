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

        private Arena _arena;
        private Player _player1;
        private Player _player2;

        private static float _playerSpeed = 1.75f;
                        
        public Vector2 Player1Position => _player1.Position;
        public Vector2 Player2Position => _player2.Position;
        public Color Player1Color => _player1.Color;
        public Color Player2Color => _player2.Color;



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

            _player1 = new Player();
            _player1.Position = new Vector2(_random.Next(10, _worldWidth - 10), _random.Next(10, _worldWidth - 10));
            _player1.Color = Color.DarkGreen;
            _arena.RemoveDirt((int)_player1.Position.X, (int)_player1.Position.Y, Player.Size * 3);

            _player2 = new Player();
            _player2.Position = new Vector2(_random.Next(10, _worldWidth - 10), _random.Next(10, _worldWidth - 10));
            _player2.Color = Color.DarkBlue;
            _arena.RemoveDirt((int)_player2.Position.X, (int)_player2.Position.Y, Player.Size * 3);
        }

        public void Update()
        {
            _arena.Update();
            _player1.Update();
            _player2.Update();
        }

        /// <summary>
        /// Render a chunk of the world defined by an AABB
        /// </summary>
        /// <param name="position">top left in the world</param>
        /// <param name="size">width/height</param>
        public void Render(Vector2 position, Vector2 size)
        {
            if(!_target.HasValue)
            {
                _target = Raylib.LoadRenderTexture((int)size.X, (int)size.Y);
            }

            Raylib.BeginTextureMode(_target.Value);
            Raylib.ClearBackground(Color.Black);

            _arena.Render(position, size);

            // render entities (weapons, particles, etc)
            // TODO
            
            // render players (if in bounds)            
            RenderPlayers(position, size);

            Raylib.EndTextureMode();
        }

        public void ProcessInputs()
        {
            if (Raylib.IsMouseButtonDown(MouseButton.Left))
            {
                var mouseX = Raylib.GetMousePosition().X / _settings.Scale;
                var mouseY = Raylib.GetMousePosition().Y / _settings.Scale;
            }

            #region Player 1 Controls
            
            if (Raylib.IsKeyDown(KeyboardKey.A))
            {
                _player1.Move(new Vector2(-_playerSpeed, 0), WorldWidth, WorldHeight);
            }
            else if (Raylib.IsKeyDown(KeyboardKey.D))
            {
                _player1.Move(new Vector2(_playerSpeed, 0), WorldWidth, WorldHeight);
            }

            if (Raylib.IsKeyDown(KeyboardKey.W))
            {
                _player1.Move(new Vector2(0, -_playerSpeed), WorldWidth, WorldHeight);
            }
            else if (Raylib.IsKeyDown(KeyboardKey.S))
            {
                _player1.Move(new Vector2(0, _playerSpeed), WorldWidth, WorldHeight);
            }

            #endregion Player 1 Controls

            #region Player 2 Controls

            if (Raylib.IsKeyDown(KeyboardKey.Left))
            {
                _player2.Move(new Vector2(-_playerSpeed, 0), WorldWidth, WorldHeight);

                if (Raylib.IsKeyPressed(KeyboardKey.RightControl))
                {
                    Dig(_player2.Position, new Vector2(-1, 0)); // TODO get vector in dir of worm aim angle
                }
            }
            else if (Raylib.IsKeyDown(KeyboardKey.Right))
            {
                _player2.Move(new Vector2(_playerSpeed, 0), WorldWidth, WorldHeight);

                if (Raylib.IsKeyPressed(KeyboardKey.RightControl))
                {
                    Dig(_player2.Position, new Vector2(1, 0)); // TODO get vector in dir of worm aim angle
                }
            }

            if (Raylib.IsKeyDown(KeyboardKey.Up))
            {
                _player2.Move(new Vector2(0, -_playerSpeed), WorldWidth, WorldHeight);
            }
            else if (Raylib.IsKeyDown(KeyboardKey.Down))
            {
                _player2.Move(new Vector2(0, _playerSpeed), WorldWidth, WorldHeight);
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

        private void RenderPlayers(Vector2 position, Vector2 size)
        {
            var maxBounds = new Vector2(position.X + size.X, position.Y + size.Y);
            if (InBounds(position, maxBounds, _player1.Position))
            {
                _player1.Render(position);
            }
            if (InBounds(position, maxBounds, _player2.Position))
            {
                _player2.Render(position);
            }
        }

        private bool InBounds(Vector2 minBounds, Vector2 maxBounds, Vector2 position)
        {
            return position.X >= minBounds.X && position.X <= maxBounds.X &&
                   position.Y >= minBounds.Y && position.Y <= maxBounds.Y;
        }

        private void Dig(Vector2 position, Vector2 dir)
        {
            var px = position.X + dir.X;
            var py = position.Y + dir.Y;
            _arena.RemoveDirt((int)px, (int)py, (int)(Player.Size * 1.5f));
        }
    }
}
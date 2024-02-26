using Kz.Engine.Trigonometry;
using Kz.Liero.Utilities;
using Raylib_cs;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;

namespace Kz.Liero
{
    public enum WormDir
    {
        Right,
        Left,
    }

    public enum WormState
    {
        Still,
        Moving,
    }

    public class Player
    {
        #region ctor

        public float X { get; set; }
        public float Y { get; set; }
        public Vector2 Position => new(X, Y);

        private Color _color;
        public Color Color => _color;

        // dimension of the sprite
        public int Size  => 20;
        private Sprite _sprite;

        public float AimAngle { get; set; }
        private float _aimSpeed => TrigUtil.DegreesToRadians(1.0f);

        public WormDir Direction { get; set; }
        public WormState State { get; set; }
        private bool _isJumping = false;

        private float _velocityX = 1.5f;                
        private float _velocityY = 0.0f;
        private float _gravity = 0.40f;
        private float _jumpVelocity = -10.5f;

        
        
        public Player(float x, float y, Color color)
        {
            X = x;
            Y = y;
            _color = color;

            State = WormState.Still;
            var config = new SpriteConfig
            {
                Filename = "Resources\\Worm.png",
                FragShaderFilename = "Shaders\\Worm.frag",
                MaxFrames = 3,
                MaxAnimations = 7,
                FrameSpeed = 0.15f,
                DefaultFrameIndex = 1,
                Width = Size,
                Height = Size,
                Tint = _color,
            };
            _sprite = new Sprite(config);            
        }

        #endregion ctor

        public void Update(Rectangle viewPortDimension)
        {
            // constrain player position to world boundaries
            // TODO

            //Y += _velocityY;
            //if (Y > 512)
            //{
            //    Y = 512;
            //    _isJumping = false;
            //}

            //_velocityY += _gravity;
            //if (_velocityY > 100) _velocityY = 100; // ?? Better way to do this...keep it from continuing to climb

            //
            // Calculate FrameIndex
            //
            if (State == WormState.Still)
            {
                _sprite.SetDefaultState();
            }
            else
            {
                _sprite.Update();
            }

            //
            // Calculate SpriteIndex
            //
            var spriteIndex = 0;
            if (Direction == WormDir.Right)
            {
                // two ranges: 3pi/2 to 2pi and 0 to pi/4
                // 3pi/2 to 2pi is indexes 2-6
                // 0 to pi/4 is indexes 0 - 2
                if (AimAngle >= TrigConsts.THREE_PI_OVER_TWO && AimAngle < TrigConsts.TWO_PI)
                {
                    spriteIndex = (int)Kz.Engine.General.Utils.RangeMap(AimAngle, TrigConsts.THREE_PI_OVER_TWO, TrigConsts.TWO_PI, 6, 2);
                }
                else if (AimAngle >= 0 && AimAngle <= TrigConsts.PI_OVER_FOUR)
                {
                    spriteIndex = (int)Kz.Engine.General.Utils.RangeMap(AimAngle, 0, TrigConsts.PI_OVER_FOUR, 2, 0);
                }
            }
            else if (Direction == WormDir.Left)
            {
                spriteIndex = (int)Kz.Engine.General.Utils.RangeMap(AimAngle, TrigConsts.THREE_PI_OVER_FOUR, TrigConsts.THREE_PI_OVER_TWO, 0, 6);
            }

            spriteIndex = Math.Clamp(spriteIndex, 0, 6);
            _sprite.SetSpriteAnimationIndex(spriteIndex);
        }

        public void Render(Vector2 worldPosition)
        {
            var x = X - worldPosition.X;
            var y = Y - worldPosition.Y;
            _sprite.Render((int)x, (int)y, 10, 10, Direction == WormDir.Left, false);

            // crosshairs            
            var xx = x + MathF.Cos(AimAngle) * 25.0f;
            var yy = y + MathF.Sin(AimAngle) * 25.0f;
            Raylib.DrawRectangleLines((int)xx, (int)yy, 3, 3, Color.Red);

            if (true)
            {
                //// bounding box
                //var aabb = GetBoundingBox(worldPosition);
                //Raylib.DrawRectangleLines(
                //    (int)aabb.X, (int)aabb.Y,
                //    (int)aabb.Width, (int)aabb.Height,
                //    Color.Purple);

                //// smaller bounding box
                //var smallAABB = GetCollisionBoundingBox(worldPosition);
                //Raylib.DrawRectangleLines(
                //    (int)smallAABB.X, (int)smallAABB.Y,
                //    (int)smallAABB.Width, (int)smallAABB.Height,
                //    Color.Blue);
                
                var pixels = GetCollisionPixels(worldPosition);
                foreach (var p in pixels)
                {
                    Raylib.DrawPixel(p.X, p.Y, Color.Red);                    
                }

                foreach(var p in _recentCollisions)
                {
                    Raylib.DrawPixel(p.X, p.Y, Color.Blue);
                }
            }
        }

        public Kz.Engine.DataStructures.Vector2f GetAimAngleVector(Vector2 worldPosition)
        {
            var x =  MathF.Cos(AimAngle);
            var y = MathF.Sin(AimAngle);
            var temp = new Kz.Engine.DataStructures.Vector2f(x, y);
            return temp;
        }

        public void MoveRight(Vector2 worldPosition, Func<int, int, Dirt?> dirtAt)
        {
            State = WormState.Moving;

            X += _velocityX;
            
            var isCollision = IsCollision(worldPosition, dirtAt);
            if (isCollision)
            {
                X -= _velocityX;
            }

            if (Direction == WormDir.Left)
            {
                AimAngle = TrigUtil.MirrorAngle(AimAngle);
            }

            Direction = WormDir.Right;
        }

        public void MoveLeft(Vector2 worldPosition, Func<int, int, Dirt?> dirtAt)
        {
            State = WormState.Moving;

            var isCollision1 = IsCollision(worldPosition, dirtAt);

            X -= _velocityX;

            var isCollision = IsCollision(worldPosition, dirtAt);
            if (isCollision)
            {
                X += _velocityX;
            }

            if (Direction == WormDir.Right)
            {
                AimAngle = TrigUtil.MirrorAngle(AimAngle);
            }

            Direction = WormDir.Left;
        }        

        public void Aim(int dir)
        {
            if (Direction == WormDir.Right)
            {
                AimAngle += _aimSpeed * dir;
                AimAngle = TrigUtil.NormalizeAngle(AimAngle);

                // constrain angle between 3pi/2 and pi/4
                var isInRange = (AimAngle >= TrigConsts.THREE_PI_OVER_TWO || AimAngle <= TrigConsts.PI_OVER_FOUR);
                if (!isInRange)
                {
                    // clamp to range
                    var diff = TrigConsts.THREE_PI_OVER_TWO - AimAngle;
                    if (AimAngle < TrigConsts.THREE_PI_OVER_TWO && (diff < 0.25f))
                    {
                        AimAngle = TrigConsts.THREE_PI_OVER_TWO;
                    }
                    else if (AimAngle > TrigConsts.PI_OVER_FOUR)
                    {
                        AimAngle = TrigConsts.PI_OVER_FOUR;
                    }
                }
            }
            else if (Direction == WormDir.Left)
            {
                AimAngle -= _aimSpeed * dir;

                // constrain angle between 3pi/2 and 3pi/4
                if (AimAngle > TrigConsts.THREE_PI_OVER_TWO) { AimAngle = TrigConsts.THREE_PI_OVER_TWO; }
                if (AimAngle < TrigConsts.THREE_PI_OVER_FOUR) { AimAngle = TrigConsts.THREE_PI_OVER_FOUR; }
            }
        }

        public void Jump()
        {
            if (_isJumping) return;

            _velocityY = _jumpVelocity;
            _isJumping = true;
        }

        public void Cleanup()
        {
            _sprite.Cleanup();
        }

        #region Bounding Boxes / Collision Detection
        private List<(int X, int Y)> _recentCollisions = new List<(int X, int Y)>();
        public bool IsCollision(Vector2 worldPosition, Func<int, int, Dirt?> dirtAt)
        {
            _recentCollisions.Clear();

            var pixels = GetCollisionPixels(worldPosition);
            for (var i = 0; i < pixels.Count; i++)
            {
                var dirt = dirtAt((int)worldPosition.X + pixels[i].X, (int)worldPosition.Y + pixels[i].Y);
                if (dirt == null) continue;
                if (dirt.Value.IsActive)
                {
                    _recentCollisions.Add((pixels[i].X, pixels[i].Y));
                    //return true;
                }
            }

            if (_recentCollisions.Count > 0) return true;

            return false;
        }

        private Rectangle GetBoundingBox(Vector2 worldPosition)
        {
            var Size = 10;
            var x = X - (Size / 2.0f) - worldPosition.X;
            var y = Y - (Size / 2.0f) - worldPosition.Y;
            var width = Size;
            var height = Size;

            return new Rectangle(x, y, width, height);
        }

        private Rectangle GetCollisionBoundingBox(Vector2 worldPosition)
        {
            var Size = 10;
            var aabb = GetBoundingBox(worldPosition);            
            aabb.Width *= 0.5f;
            aabb.Height *= 0.75f;
            aabb.X += (0.25f * Size);
            aabb.Y += (0.25f * Size);

            return aabb;
        }

        /// <summary>
        /// Get a list of pixels from the borders of the collision bounding box
        /// </summary>        
        private List<(int X, int Y)> GetCollisionPixels(Vector2 worldPosition)
        {
            var aabb = GetCollisionBoundingBox(worldPosition);
            var pixelsPerSide = 1;
            var pixels = new List<(int X, int Y)>();

            // get top and bottom points
            var stepX = (int)(aabb.Width / pixelsPerSide);
            for(var x = 0; x < aabb.Width; x += stepX)
            {
                pixels.Add(((int)(aabb.X + x), (int)aabb.Y)); // top
                pixels.Add(((int)(aabb.X + x), (int)(aabb.Y + aabb.Height - 1))); // bottom
            }

            // get left and right points
            var stepY = (int)(aabb.Height / pixelsPerSide);
            for (var y = 0; y < aabb.Height; y += stepY)
            {
                pixels.Add(((int)aabb.X, (int)(aabb.Y + y))); // left
                pixels.Add(((int)(aabb.X + aabb.Width - 1), (int)(aabb.Y + y))); // right
            }

            return pixels;
        }

        #endregion Bounding Boxes / Collision Detection
    }
}
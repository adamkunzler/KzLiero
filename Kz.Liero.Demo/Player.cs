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

        private float _velocityX = 0.75f;                
        private float _velocityY = 0.0f;
        private float _gravity = 0.10f;
        private float _jumpVelocity = -2.5f;
        private float _walkingYAdjustment = 1.0f;

        private GrapplingHook _hook;
        private float _grapplingHookMaxLength = 100.0f;
        private float _grapplingHookGravity = 0.75f;


        public Player(float x, float y, Color color)
        {
            X = x;
            Y = y;
            _color = color;

            _hook = new GrapplingHook(_grapplingHookMaxLength, _grapplingHookGravity);

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

        public void Update(float worldWidth, float worldHeight, Rectangle viewPortDimension, Func<int, int, Dirt?> dirtAt)
        {
            //
            // update grappling gook
            //
            _hook.Update(X, Y, worldWidth, worldHeight);
            var springForce = _hook.GetSpringForce(new Engine.DataStructures.Vector2f(X, Y));
            _velocityX += springForce.X;
            _velocityY += springForce.Y;

            // constrain player position to world boundaries
            #region Constrain to World

            var halfSize = Size / 2.0f;
            if (X < halfSize) X = halfSize;
            else if (X > worldWidth - halfSize) X = worldWidth - halfSize;

            if (Y < halfSize) Y = halfSize;
            else if (Y > worldHeight - halfSize) Y = worldHeight - halfSize;

            #endregion Constrain to World

            //
            // update worm position/velocity
            //            
            _velocityY += _gravity;            
            Y += _velocityY;

            //
            // check top/bottom collisions
            //
            #region Top/Bottom collisions
            var isTopCollision = IsCollisionTop(viewPortDimension.Position, dirtAt);
            var isBottomCollision = IsCollisionBottom(viewPortDimension.Position, dirtAt);
                        
            if (isTopCollision.IsCollision && !isBottomCollision)
            {               
                _velocityY = 0;
            }
            
            if (isBottomCollision)
            {                
                Y -= _velocityY;
                _velocityY = 0;
                _isJumping = false;
                
            }

            // prevent jumping when falling
            if(_velocityY > 0)
            {
                _isJumping = true;
            }

            #endregion Top/Bottom collisions

            //
            // Calculate FrameIndex
            //
            #region Calculate FrameIndex
            if (State == WormState.Still)
            {
                _sprite.SetDefaultState();
            }
            else
            {
                _sprite.Update();
            }
            #endregion Calculate FrameIndex

            //
            // Calculate SpriteIndex based on aimAngle
            //
            #region Calculate SpriteIndex based on AimAngle
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
            #endregion Calculate SpriteIndex based on AimAngle

            //
            // make sure grappling hook start and player location are synced
            //
            if(_hook.IsHooked)
            {
                _hook.SetStart(new Engine.DataStructures.Vector2f(X, Y));
            }
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

            _hook.Render(worldPosition, Color.Red);

            // render bounding boxes/collision pixels
            if (false)
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

                var pixels = new List<(int X, int Y)>();                
                pixels.AddRange(GetSideCollisionPixels(worldPosition));
                foreach (var p in pixels)
                {
                    Raylib.DrawPixel(p.X, p.Y, Color.Red);                    
                }

                var pixels2 = new List<(int X, int Y)>();
                pixels2.AddRange(GetTopCollisionPixels(worldPosition));
                pixels2.AddRange(GetBottomCollisionPixels(worldPosition));
                foreach (var p in pixels2)
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
                        
            var isCollision = IsCollisionSides(worldPosition, dirtAt);
            if (isCollision)
            {
                // try to move up a little and check for collision again
                Y -= _walkingYAdjustment;

                isCollision = IsCollisionSides(worldPosition, dirtAt);
                if (isCollision)
                {
                    Y += _walkingYAdjustment;
                    X -= _velocityX;
                }                                
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
            
            X -= _velocityX;
            
            var isCollision = IsCollisionSides(worldPosition, dirtAt);
            if (isCollision)
            {
                // try to move up a little and check for collision again
                Y -= _walkingYAdjustment;

                isCollision = IsCollisionSides(worldPosition, dirtAt);
                if (isCollision)
                {
                    Y += _walkingYAdjustment;
                    X += _velocityX;
                }
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

        public void JumpOrHook()
        {
            if (_isJumping)
            {
                // Grappling Hook
                _hook.Fire(X, Y, AimAngle);
            }
            else
            {
                // JUMP
                _hook.Disable();

                if (_isJumping) return;

                _velocityY = _jumpVelocity;
                _isJumping = true;
            }
        }

        public void Cleanup()
        {
            _sprite.Cleanup();
        }

        #region Bounding Boxes / Collision Detection
        
        /// <summary>
        /// Check if there was a collision at the top of the player
        /// </summary>        
        public (bool IsCollision, float Y) IsCollisionTop(Vector2 worldPosition, Func<int, int, Dirt?> dirtAt)
        {
            var pixels = GetTopCollisionPixels(worldPosition);
            for (var i = 0; i < pixels.Count; i++)
            {
                var dirt = dirtAt((int)worldPosition.X + pixels[i].X, (int)worldPosition.Y + pixels[i].Y);
                if (dirt == null) continue;
                if (dirt.Value.IsActive)
                {                    
                    return (true, pixels[i].Y);
                }
            }

            return (false, 0.0f);
        }

        /// <summary>
        /// Check if there was a collision at either side of the player
        /// </summary>        
        public bool IsCollisionSides(Vector2 worldPosition, Func<int, int, Dirt?> dirtAt)
        {
            var pixels = GetSideCollisionPixels(worldPosition);
            for (var i = 0; i < pixels.Count; i++)
            {
                var dirt = dirtAt((int)worldPosition.X + pixels[i].X, (int)worldPosition.Y + pixels[i].Y);
                if (dirt == null) continue;
                if (dirt.Value.IsActive)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if there was a collission at the bottom of the player
        /// </summary>        
        public bool IsCollisionBottom(Vector2 worldPosition, Func<int, int, Dirt?> dirtAt)
        {            
            var pixels = GetBottomCollisionPixels(worldPosition);
            for (var i = 0; i < pixels.Count; i++)
            {
                var dirt = dirtAt((int)worldPosition.X + pixels[i].X, (int)worldPosition.Y + pixels[i].Y);
                if (dirt == null) continue;
                if (dirt.Value.IsActive)
                {                    
                    return true;
                }
            }

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
        /// Get a list of pixels from the left/right borders of the collision bounding box
        /// </summary>        
        private List<(int X, int Y)> GetSideCollisionPixels(Vector2 worldPosition)
        {
            var aabb = GetCollisionBoundingBox(worldPosition);
            var pixelsPerSide = 3;
            var pixels = new List<(int X, int Y)>();
            
            // get left and right points (skipping any top and bottom pixels)
            var stepY = (int)(aabb.Height / pixelsPerSide);
            for (var y = stepY; y < aabb.Height; y += stepY)
            {
                pixels.Add(((int)aabb.X, (int)(aabb.Y + y))); // left
                pixels.Add(((int)(aabb.X + aabb.Width - 1), (int)(aabb.Y + y))); // right
            }

            return pixels;
        }

        /// <summary>
        /// Get a list of pixels from the top border of the collision bounding box
        /// </summary>        
        private List<(int X, int Y)> GetTopCollisionPixels(Vector2 worldPosition)
        {
            var aabb = GetCollisionBoundingBox(worldPosition);
            var pixelsPerSide = 3;
            var pixels = new List<(int X, int Y)>();

            // get top and bottom points
            var stepX = (int)(aabb.Width / pixelsPerSide);
            for (var x = 0; x < aabb.Width; x += stepX)
            {
                pixels.Add(((int)(aabb.X + x), (int)aabb.Y)); // top                
            }

            return pixels;
        }

        /// <summary>
        /// Get a list of pixels from the bottom border of the collision bounding box
        /// </summary>        
        private List<(int X, int Y)> GetBottomCollisionPixels(Vector2 worldPosition)
        {
            var aabb = GetCollisionBoundingBox(worldPosition);
            var pixelsPerSide = 3;
            var pixels = new List<(int X, int Y)>();
            
            var stepX = (int)(aabb.Width / pixelsPerSide);
            for (var x = 0; x < aabb.Width; x += stepX)
            {
                pixels.Add(((int)(aabb.X + x), (int)(aabb.Y + aabb.Height - 1))); // bottom
            }

            return pixels;
        }

        #endregion Bounding Boxes / Collision Detection
    }
}
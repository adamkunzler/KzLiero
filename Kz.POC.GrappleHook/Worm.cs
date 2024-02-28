using Kz.Engine.DataStructures;
using Raylib_cs;

namespace Kz.POC.GrappleHook
{
    public class Worm
    {
        public Vector2f Location { get; set; } = Vector2f.Zero;

        public Vector2f Velocity { get; set; } = Vector2f.Zero;        
        
        public float Radius { get; set; }

        /// <summary>
        /// How fast the worm moves laterally
        /// </summary>
        private float _horizontalSpeed = 5.0f;

        private Vector2f _gravity = new(0.0f, 3.5f);

        private Vector2f _jumpVelocity = new(0.0f, -25.0f);

        private bool _isJumping = false;

        private GrapplingHook _hook;

        public Worm(float x, float y, float radius)
        {
            Location.X = x;
            Location.Y = y;
            Radius = radius;

            _hook = new GrapplingHook();
        }

        public void Update(float worldWidth, float worldHeight)
        {
            _hook.Update(Location.X, Location.Y, worldWidth, worldHeight);
            
            ApplyForce(_gravity);
            var springForce = _hook.GetSpringForce(Location);
            ApplyForce(springForce);
                        
            Location += Velocity;

            // dampen velocity
            Velocity *= 0.99f;
            
            // constrain to world boundaries
            if (Location.X < Radius)
            {
                Location.X = Radius;                
                Velocity.X = 0.0f;
            }
            if (Location.Y < Radius)
            {
                Location.Y = Radius;                
                Velocity.Y = 0.0f;
            }
            if (Location.X > worldWidth - Radius)
            {
                Location.X = worldWidth - Radius;                
                Velocity.X = 0.0f;
            }
            if (Location.Y > worldHeight - Radius)
            {
                Location.Y = worldHeight - Radius;                
                Velocity.Y = 0.0f;
                _isJumping = false;
            }

            if(_hook.IsHooked)
                _hook.SetStart(Location);
        }

        public void Render()
        {
            Raylib.DrawCircle((int)Location.X, (int)Location.Y, Radius, Color.Purple);

            var crosshairCoords = GetCrosshairCoords();
            Raylib.DrawCircle((int)crosshairCoords.X, (int)crosshairCoords.Y, 5, Color.Orange);

            _hook.Render();

            Raylib.DrawText($"Velocity: {Velocity}", 10, 10, 20, Color.RayWhite);
            Raylib.DrawText($"Location: {Location}", 10, 35, 20, Color.RayWhite);
        }

        public void ProcessInputs(float worldWidth, float worldHeight)
        {
            if (Raylib.IsKeyDown(KeyboardKey.A))
            {
                Move(-_horizontalSpeed, 0, worldWidth, worldHeight);
            }
            else if (Raylib.IsKeyDown(KeyboardKey.D))
            {
                Move(_horizontalSpeed, 0, worldWidth, worldHeight);
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                if (_isJumping)
                {
                    _hook.Fire(Location.X, Location.Y, GetAimAngle());
                }
                else
                {
                    Jump();
                }                
            }

            //if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            //{
            //    _hook.Fire(Location.X, Location.Y, GetAimAngle());
            //}

            //if (Raylib.IsMouseButtonPressed(MouseButton.Right))
            //{
            //    _hook.Disable();
            //}
        }

        public void Cleanup()
        {
            _hook.Cleanup();
        }

        #region Private Methods

        private void ApplyForce(Vector2f force)
        {
            Velocity += force;            
        }

        private void Jump()
        {
            _hook.Disable();

            if (_isJumping) return;

            Velocity += _jumpVelocity;
            _isJumping = true;
        }

        private float GetAimAngle()
        {
            var dx = Raylib.GetMousePosition().X - Location.X;
            var dy = Raylib.GetMousePosition().Y - Location.Y;
            var theta = MathF.Atan2(dy, dx);
            return theta;
        }

        private (float X, float Y) GetCrosshairCoords()
        {
            var theta = GetAimAngle();
            var chX = Location.X + MathF.Cos(theta) * (Radius * 5);
            var chY = Location.Y + MathF.Sin(theta) * (Radius * 5);
            return (chX, chY);
        }

        private void Move(float x, float y, float worldWidth, float worldHeight)
        {
            Location.X += x;
            Location.Y += y;


        }

        #endregion Private Methods
    }
}
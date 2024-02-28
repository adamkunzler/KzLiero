using Kz.Engine.DataStructures;
using Raylib_cs;

namespace Kz.POC.GrappleHook
{
    public class GrapplingHook
    {
        private Vector2f _fireVelocity = new();
        private Vector2f _start = new();
        private Vector2f _end = new();
        private float _initAngle = 0.0f;
        private bool _isActive = false;

        private float _gravity = 0.75f;

        private bool _isHooked = false;

        public GrapplingHook()
        {
        }

        public void Update(float startX, float startY, float worldWidth, float worldHeight)
        {
            _start.X = startX;
            _start.Y = startY;

            if (!_isActive || _isHooked) return;
            
            _fireVelocity.X -= 0.05f;
            _fireVelocity.Y -= _gravity;

            _end.X += MathF.Cos(_initAngle) * _fireVelocity.X;
            _end.Y += MathF.Sin(_initAngle) * _fireVelocity.Y;

            if (_end.X > worldWidth)
            {
                _end.X = worldWidth;
                _isHooked = true;
            }
            if (_end.X < 0)
            {
                _end.X = 0;
                _isHooked = true;
            }
            if (_end.Y > worldHeight)
            {
                _end.Y = worldHeight;
                _isHooked = true;
            }
            if (_end.Y < 0)
            {
                _end.Y = 0;
                _isHooked = true;
            }
        }

        public void Render()
        {
            if (!_isActive) return;

            Raylib.DrawLine((int)_start.X, (int)_start.Y, (int)_end.X, (int)_end.Y, Color.Brown);
        }

        public void Cleanup()
        {
        }

        /// <summary>
        /// Fire the grappling hook
        /// </summary>
        /// <param name="x">X Origin</param>
        /// <param name="y">Y Origin</param>
        /// <param name="theta">Angle to fire hook</param>
        public void Fire(float x, float y, float theta)
        {
            _start = new Vector2f(x, y);
            _end = new Vector2f(x, y);
            _initAngle = theta;
            _isActive = true;
            _isHooked = false;
            _fireVelocity = new Vector2f(25.0f, 25.0f);
        }

        public void Disable()
        {
            _isActive = false;
            _isHooked = false;
        }
    }
}
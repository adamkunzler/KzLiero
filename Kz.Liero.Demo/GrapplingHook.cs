using Kz.Engine.DataStructures;
using Raylib_cs;
using System.Numerics;

namespace Kz.Liero
{
    /*
        TODO 
        [] bug where worm gets transported to edge
        [] grappling hook needs to expire if it doesn't attach to anything after n-seconds
    */
    public class GrapplingHook
    {
        private Vector2f _fireVelocity = new();

        /// <summary>
        /// The "bob" of the spring (the part that is NOT anchored)
        /// </summary>
        private Vector2f _start = new();
        public Vector2f Start => _start;

        /// <summary>
        /// The "anchor" of the spring. The Hook.
        /// </summary>
        private Vector2f _end = new();

        private float _initAngle = 0.0f;

        /// <summary>
        /// if true, the grappling hook has been fired
        /// </summary>
        private bool _isActive = false;

        /// <summary>
        /// the grappling hooks own gravity value
        /// </summary>
        private float _gravity = 0.75f;


        private float _maxLength = 900.0f;

        /// <summary>
        /// if true, the grappling hook has attached to something
        /// </summary>
        private bool _isHooked = false;
        public bool IsHooked => _isHooked;

        /// <summary>
        /// Used to calculate a spring force when the grappling hook is attached to something
        /// </summary>
        private SimpleSpring _spring;

        /// <summary>
        /// Hack method to set the start of the grappling hook to a specific value
        /// </summary>
        /// <param name="start"></param>
        public void SetStart(Vector2f start) => _start = start;

        /// <summary>
        /// Initialize the grappling hook
        /// </summary>
        public GrapplingHook(float maxLength, float gravity)
        {
            _spring = new SimpleSpring(7.5f, 0.009f);
            _maxLength = maxLength;
            _gravity = gravity;
        }

        /// <summary>
        /// Syncs the start of the grappling hook and updates the hooks 
        /// position if the hook is active and not attached to anything
        /// </summary>        
        public void Update(float startX, float startY, float worldWidth, float worldHeight, Func<int, int, Dirt?> dirtAt)
        {
            _start.X = startX;
            _start.Y = startY;

            // no need to update anything if the grappling hook is attached to something or not active
            if (!_isActive || _isHooked) return;

            // update grappling hook position
            _fireVelocity.X -= 0.05f;
            _fireVelocity.Y -= _gravity;

            _end.X += MathF.Cos(_initAngle) * _fireVelocity.X;
            _end.Y += MathF.Sin(_initAngle) * _fireVelocity.Y;

            // constrain grapple length
            var length = (_end - _start).Magnitude();
            if (length > _maxLength)
            {
                var dir = (_end - _start).Normal();
                _end = _start + (dir * _maxLength);
            }

            // check for dirt collision
            var dirt = dirtAt((int)_end.X, (int)_end.Y);
            if (dirt.HasValue && dirt.Value.IsActive)
            {                
                _isHooked = true;
            }

            // check for collision on world bounds and set the hook
            if (_end.X > worldWidth)
            {
                _end.X = worldWidth - 1;
                _isHooked = true;
            }
            if (_end.X < 0)
            {
                _end.X = 1;
                _isHooked = true;
            }
            if (_end.Y > worldHeight)
            {
                _end.Y = worldHeight - 1;
                _isHooked = true;
            }
            if (_end.Y < 0)
            {
                _end.Y = 1;
                _isHooked = true;
            }
        }

        /// <summary>
        /// Calculate the spring force of the grappling hook if it's hooked to something
        /// </summary>        
        public Vector2f GetSpringForce(Vector2f location)
        {
            if (!_isHooked) return Vector2f.Zero;

            _spring.Anchor = _end;
            var force = _spring.GetForce(location);
            return force;
        }

        /// <summary>
        /// Renders the grappling hook if it's active
        /// </summary>        
        public void Render(Vector2 worldPosition, Color color)
        {
            if (!_isActive) return;
            
            Raylib.DrawLine(
                (int)(_start.X - worldPosition.X), (int)(_start.Y - worldPosition.Y), 
                (int)(_end.X - worldPosition.X), (int)(_end.Y - worldPosition.Y), color);
        }
        
        /// <summary>
        /// Fire the grappling hook from a specified location and angle.
        /// 
        /// Initializes the properties of the grappling hook with fresh values
        /// </summary>        
        public void Fire(float x, float y, float theta)
        {
            _start = new Vector2f(x, y);
            _end = new Vector2f(x, y);
            _initAngle = theta;
            _isActive = true;
            _isHooked = false;
            _fireVelocity = new Vector2f(35.0f, 35.0f);
        }

        /// <summary>
        /// Disables the grappling hook, e.g. disabling it and unhooking it
        /// </summary>
        public void Disable()
        {
            _isActive = false;
            _isHooked = false;
        }
    }
}
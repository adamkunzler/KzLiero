using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kz.POC.GrappleHook
{
    public class Worm
    {
        public float X { get; set; }
        public float Y { get; set; }

        public float Radius { get; set; }

        private float _speed = 5.0f;
        private float _gravity = 0.55f;
        private float _velocityY = 0.0f;
        private float _jumpVelocity = 10.0f;
        private bool _isJumping = false;
        private GrapplingHook _hook;

        public Worm(float x, float y, float radius)
        {
            X = x;
            Y = y;
            Radius = radius;

            _hook = new GrapplingHook();
        }

        public void Update(float worldWidth, float worldHeight)
        {
            _hook.Update(X, Y, worldWidth, worldHeight);

            _velocityY += _gravity;
            if (_velocityY > 100.0f) _velocityY = 100.0f;

            Move(0, _velocityY, worldWidth, worldHeight);
        }

        public void Render()
        {
            Raylib.DrawCircle((int)X, (int)Y, Radius, Color.Purple);

            var crosshairCoords = GetCrosshairCoords();
            Raylib.DrawCircle((int)crosshairCoords.X, (int)crosshairCoords.Y, 5, Color.Orange);

            _hook.Render();
        }

        public void ProcessInputs(float worldWidth, float worldHeight)
        {
            if (Raylib.IsKeyDown(KeyboardKey.A))
            {
                Move(-_speed, 0, worldWidth, worldHeight);
            }
            else if (Raylib.IsKeyDown(KeyboardKey.D))
            {
                Move(_speed, 0, worldWidth, worldHeight);
            }

            

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                Jump();
            }

            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                _hook.Fire(X, Y, GetAimAngle());
            }

            if (Raylib.IsMouseButtonPressed(MouseButton.Right))
            {
                _hook.Disable();
            }
        }

        public void Cleanup()
        {
            _hook.Cleanup();
        }

        #region Movement

        private void Jump()
        {
            if (_isJumping) return;

            _velocityY = -_jumpVelocity;            
            _isJumping = true;
        }

        private float GetAimAngle()
        {
            var dx = Raylib.GetMousePosition().X - X;
            var dy = Raylib.GetMousePosition().Y - Y;
            var theta = MathF.Atan2(dy, dx);
            return theta;
        }
        private (float X, float Y) GetCrosshairCoords()
        {
            var theta = GetAimAngle();
            var chX = X + MathF.Cos(theta) * (Radius * 5);
            var chY = Y + MathF.Sin(theta) * (Radius * 5);
            return (chX, chY);
        }

        private void Move(float x, float y, float worldWidth, float worldHeight)
        {
            X += x;
            Y += y;

            if (X < Radius) X = Radius;
            if (Y < Radius) Y = Radius;
            if (X > worldWidth - Radius) X = worldWidth - Radius;
            if (Y > worldHeight - Radius)
            {
                Y = worldHeight - Radius;
                _isJumping = false;
            }
        }

        #endregion Movement
    }
}

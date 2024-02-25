// https://www.raylib.com/
// https://github.com/ChrisDill/Raylib-cs
// dotnet add package Raylib-cs

using Kz.Engine.General;
using Kz.Engine.Trigonometry;
using Raylib_cs;
using System.Numerics;
using Color = Raylib_cs.Color;

public enum WormDir
{
    Right,
    Left,
}

public enum WormState
{
    Still,
    Moving,
    Jumping,
}

public class Worm
{
    public float X { get; set; }
    public float Y { get; set; }

    public float Size { get; set; }

    public float AimAngle { get; set; }

    public WormDir Direction { get; set; }
    public WormState State { get; set; }

    private float _speed = 1.5f;

    private int _spriteIndex;
    private int _frameIndex;
    private float _frameSpeed = 0.15f;
    private float _frameTime = 0.0f;
    private int _frameDir = 1;
    private int _maxFrames = 3;

    private Texture2D _sprites;
    private Shader _shader;

    private int _shaderShadeLocation;
    private float[] _shaderShade = [];

    private float _velocityY = 0.0f;
    private float _gravity = 0.25f;
    private float _jumpVelocity = -5.5f;

    public Worm()
    {
        State = WormState.Still;

        _sprites = Raylib.LoadTexture("Resources\\Worm.png");
        _shader = Raylib.LoadShader("", "Resources\\Worm.frag");

        _shaderShadeLocation = Raylib.GetShaderLocation(_shader, "shade");
        var color = Color.Blue;
        _shaderShade = [color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f];
    }

    public void Update()
    {
        //if (State == WormState.Jumping)
        //{
        Y += _velocityY;
        if (Y > 512)
        {
            Y = 512;
            //State = WormState.Still;
        }

        _velocityY += _gravity;
            //if (_velocityY > 0.0f) _velocityY = 0.0f;
            //}

            //
            // Calculate FrameIndex
            //
        if (State == WormState.Still)
        {
            _frameIndex = 1;
            _frameTime = 0.0f;
            _frameDir = 1;
        }
        else
        {
            _frameTime += Raylib.GetFrameTime();
            if (_frameTime > _frameSpeed)
            {
                _frameTime = 0.0f;
                _frameIndex += _frameDir;
                if (_frameIndex >= _maxFrames)
                {
                    _frameIndex = _maxFrames - 2;
                    _frameDir = -_frameDir;
                }
                else if (_frameIndex < 0)
                {
                    _frameIndex = 1;
                    _frameDir = -_frameDir;
                }
            }
        }

        //
        // Calculate SpriteIndex
        //
        if (Direction == WormDir.Right)
        {
            // two ranges: 3pi/2 to 2pi and 0 to pi/4
            // 3pi/2 to 2pi is indexes 2-6
            // 0 to pi/4 is indexes 0 - 2
            if (AimAngle >= TrigConsts.THREE_PI_OVER_TWO && AimAngle < TrigConsts.TWO_PI)
            {
                _spriteIndex = (int)Utils.RangeMap(AimAngle, TrigConsts.THREE_PI_OVER_TWO, TrigConsts.TWO_PI, 6, 2);
            }
            else if (AimAngle >= 0 && AimAngle <= TrigConsts.PI_OVER_FOUR)
            {
                _spriteIndex = (int)Utils.RangeMap(AimAngle, 0, TrigConsts.PI_OVER_FOUR, 2, 0);
            }
        }
        else if (Direction == WormDir.Left)
        {
            _spriteIndex = (int)Utils.RangeMap(AimAngle, TrigConsts.THREE_PI_OVER_FOUR, TrigConsts.THREE_PI_OVER_TWO, 0, 6);
        }

        _spriteIndex = Math.Clamp(_spriteIndex, 0, 6);
    }

    

    public void Render()
    {
        //Raylib.DrawCircle((int)X, (int)Y, Size, Color.DarkGreen);

        var xx = X + MathF.Cos(AimAngle) * Size * 5.0f;
        var yy = Y + MathF.Sin(AimAngle) * Size * 5.0f;

        Raylib.DrawRectangleLines((int)xx, (int)yy, 10, 10, Color.Red);

        Raylib.DrawText($"SpriteIndex: {_spriteIndex}", 10, 10, 20, Color.RayWhite);
        Raylib.DrawText($"FrameIndex: {_frameIndex}", 10, 40, 20, Color.RayWhite);
        Raylib.DrawText($"State: {State}", 10, 70, 20, Color.RayWhite);

        //Raylib.DrawTexture(_sprites, 500, 10, Color.RayWhite);

        //
        // render sprite
        //
        Raylib.SetShaderValue(_shader, _shaderShadeLocation, _shaderShade, ShaderUniformDataType.Vec4);

        Raylib.BeginShaderMode(_shader);
        var spriteX = _frameIndex * 20;
        var spriteY = _spriteIndex * 20;
        var source = new Rectangle(spriteX, spriteY, Direction == WormDir.Right ? 20 : -20, 20);
        var dest = new Rectangle(X, Y, Size * 2, Size * 2);
        var origin = new Vector2(Size, Size);
        Raylib.DrawTexturePro(_sprites, source, dest, origin, 0.0f, Color.White);
        Raylib.EndShaderMode();
    }

    public void MoveRight()
    {
        State = WormState.Moving;

        X += _speed;

        if (Direction == WormDir.Left)
            AimAngle = TrigUtil.MirrorAngle(AimAngle);
        Direction = WormDir.Right;
    }

    public void MoveLeft()
    {
        State = WormState.Moving;

        X -= _speed;

        if (Direction == WormDir.Right)
            AimAngle = TrigUtil.MirrorAngle(AimAngle);

        Direction = WormDir.Left;
    }

    private float lastAngle = 0;

    public void Aim(float amount)
    {
        if (Direction == WormDir.Right)
        {
            AimAngle += amount;
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
            AimAngle -= amount;

            // constrain angle between 3pi/2 and 3pi/4
            if (AimAngle > TrigConsts.THREE_PI_OVER_TWO) { AimAngle = TrigConsts.THREE_PI_OVER_TWO; }
            if (AimAngle < TrigConsts.THREE_PI_OVER_FOUR) { AimAngle = TrigConsts.THREE_PI_OVER_FOUR; }
        }
    }

    public void Jump()
    {
        //if (State != WormState.Jumping)
        //{
        //State = WormState.Jumping;
        _velocityY = _jumpVelocity;
        //}
    }
}

internal class Program
{
    public static void Main()
    {
        //
        // Initialization
        //
        Raylib.InitWindow(1024, 1024, ".: POC :.");
        Raylib.SetTargetFPS(60);

        var worm = new Worm();
        worm.X = 512;
        worm.Y = 512;
        worm.Size = 25;
        worm.Direction = WormDir.Right;

        //
        // MAIN RENDER LOOP
        //
        while (!Raylib.WindowShouldClose())    // Detect window close button or ESC key
        {
            //
            // PROCESS INPUTS
            //
            if (Raylib.IsKeyDown(KeyboardKey.Left))
            {
                worm.MoveLeft();
            }
            else if (Raylib.IsKeyDown(KeyboardKey.Right))
            {
                worm.MoveRight();
            }

            if (Raylib.IsKeyUp(KeyboardKey.Left) && !Raylib.IsKeyDown(KeyboardKey.Right)) { worm.State = WormState.Still; }
            if (Raylib.IsKeyUp(KeyboardKey.Right) && !Raylib.IsKeyDown(KeyboardKey.Left)) { worm.State = WormState.Still; }

            if (Raylib.IsKeyPressed(KeyboardKey.Space)) worm.Jump();

            if (Raylib.IsKeyDown(KeyboardKey.Up))
            {
                var angle = TrigUtil.DegreesToRadians(1.0f);
                worm.Aim(-angle);
            }
            else if (Raylib.IsKeyDown(KeyboardKey.Down))
            {
                var angle = TrigUtil.DegreesToRadians(1.0f);
                worm.Aim(angle);
            }

            //
            // UPDATE STUFF
            //

            worm.Update();

            //
            // RENDER STUFF
            //
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            // draw some ground
            Raylib.DrawRectangle(0, 537, 1024, (int)(512 - worm.Size), Color.DarkBrown);

            worm.Render();

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
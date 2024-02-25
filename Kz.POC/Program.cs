// https://www.raylib.com/
// https://github.com/ChrisDill/Raylib-cs
// dotnet add package Raylib-cs

using Kz.Engine.General;
using Kz.Engine.Trigonometry;
using Kz.POC;
using Raylib_cs;
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
}

public class Worm
{
    public float X { get; set; }
    public float Y { get; set; }

    // dimension of the sprite
    public float Size { get; set; }

    public float AimAngle { get; set; }

    public WormDir Direction { get; set; }
    public WormState State { get; set; }

    private float _speed = 1.5f;

    private Sprite _sprite;

    private float _velocityY = 0.0f;
    private float _gravity = 0.40f;
    private float _jumpVelocity = -10.5f;

    private bool _isJumping = false;

    private List<(int X, int Y)> _edgePixelsRight;
    private List<(int X, int Y)> _edgePixelsLeft;

    public Worm()
    {
        State = WormState.Still;
        var config = new SpriteConfig
        {
            Filename = "Resources\\Worm.png",
            FragShaderFilename = "Resources\\Worm.frag",
            MaxFrames = 3,
            MaxAnimations = 7,
            FrameSpeed = 0.15f,
            DefaultFrameIndex = 1,
            Width = 20,
            Height = 20,
            Tint = Color.Green,            
        };
        _sprite = new Sprite(config);

        _edgePixelsRight = _sprite.GetSpriteEdges(1, 2);
        _edgePixelsLeft = new List<(int X, int Y)>();
        for (var i = 0; i < _edgePixelsRight.Count; i++)
        {            
            var x = 20 -_edgePixelsRight[i].X - 1;
            var y = _edgePixelsRight[i].Y;
            _edgePixelsLeft.Add((x, y)); 
        }        
    }

    public void Update()
    {                
        Y += _velocityY;
        if (Y > 512)
        {
            Y = 512;
            _isJumping = false;            
        }

        _velocityY += _gravity;
        if (_velocityY > 100) _velocityY = 100; // ?? Better way to do this...keep it from continuing to climb
        

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
                spriteIndex = (int)Utils.RangeMap(AimAngle, TrigConsts.THREE_PI_OVER_TWO, TrigConsts.TWO_PI, 6, 2);
            }
            else if (AimAngle >= 0 && AimAngle <= TrigConsts.PI_OVER_FOUR)
            {
                spriteIndex = (int)Utils.RangeMap(AimAngle, 0, TrigConsts.PI_OVER_FOUR, 2, 0);
            }
        }
        else if (Direction == WormDir.Left)
        {
            spriteIndex = (int)Utils.RangeMap(AimAngle, TrigConsts.THREE_PI_OVER_FOUR, TrigConsts.THREE_PI_OVER_TWO, 0, 6);
        }

        spriteIndex = Math.Clamp(spriteIndex, 0, 6);
        _sprite.SetSpriteAnimationIndex(spriteIndex);
    }

    public void Render()
    {
        _sprite.Render((int)X, (int)Y, (int)Size, (int)Size, Direction == WormDir.Left, false);

        // crosshairs
        var xx = X + MathF.Cos(AimAngle) * Size * 5.0f;
        var yy = Y + MathF.Sin(AimAngle) * Size * 5.0f;
        Raylib.DrawRectangleLines((int)xx, (int)yy, 10, 10, Color.Red);

        // bounding box
        //Raylib.DrawCircle((int)(X - Size / 2.0f), (int)(Y - Size / 2.0f), 2.0f, Color.Purple);
        //Raylib.DrawRectangleLines(
        //    (int)(X - Size), (int)(Y - Size), 
        //    (int)(Size), (int)(Size), 
        //    Color.Purple);

        // edge pixels
        if (Direction == WormDir.Right)
        {
            for (var i = 0; i < _edgePixelsRight.Count; i++)
            {
                // 10 is half the width of the actual sprite image
                // 20 is the width of the actual sprite image
                //      center at origin           scale            offset
                var x = (_edgePixelsRight[i].X - 10) * (Size / 20.0f) - (Size / 2.0f);
                var y = (_edgePixelsRight[i].Y - 10) * (Size / 20.0f) - (Size / 2.0f);
                //Raylib.DrawPixel((int)(x), (int)(y), Color.Purple);
                Raylib.DrawPixel((int)(x + X), (int)(y + Y), Color.Purple);
            }
        } else
        {
            for (var i = 0; i < _edgePixelsLeft.Count; i++)
            {
                // 10 is half the width of the actual sprite image
                // 20 is the width of the actual sprite image
                //      center at origin           scale            offset
                var x = (_edgePixelsLeft[i].X - 10) * (Size / 20.0f) - (Size / 2.0f);
                var y = (_edgePixelsLeft[i].Y - 10) * (Size / 20.0f) - (Size / 2.0f);
                //Raylib.DrawPixel((int)(x), (int)(y), Color.Purple);
                Raylib.DrawPixel((int)(x + X), (int)(y + Y), Color.Purple);
            }
        }
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
        if (_isJumping) return;

        _velocityY = _jumpVelocity;
        _isJumping = true;        
    }

    public void Cleanup()
    {
        _sprite.Cleanup();
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
        worm.Size = 50;
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

        worm.Cleanup();
        Raylib.CloseWindow();
    }
}
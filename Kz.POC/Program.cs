// https://www.raylib.com/
// https://github.com/ChrisDill/Raylib-cs
// dotnet add package Raylib-cs

using Kz.POC;
using Raylib_cs;
using Color = Raylib_cs.Color;

public enum WormDir
{
    Right,
    Left,
}

public class Worm
{
    public float X { get; set; }
    public float Y { get; set; }

    public float Size { get; set; }

    public float AimAngle { get; set; }

    public WormDir Direction { get; set; }

    private float _speed = 10;

    public Worm()
    {
    }

    public void Render()
    {
        Raylib.DrawCircle((int)X, (int)Y, Size, Color.DarkGreen);

        var xx = X + MathF.Cos(AimAngle) * Size * 5.0f;
        var yy = Y + MathF.Sin(AimAngle) * Size * 5.0f;

        Raylib.DrawRectangleLines((int)xx, (int)yy, 10, 10, Color.Red);


        //Raylib.DrawText($"Angle: {AimAngle}", 10, 10, 20, Color.RayWhite);
        
    }    

    public void MoveRight()
    {
        X += _speed;

        if (Direction == WormDir.Left)
            AimAngle = (float)MirrorAngle(AimAngle);
        Direction = WormDir.Right;
    }

    public void MoveLeft()
    {
        X -= _speed;

        if (Direction == WormDir.Right)
            AimAngle = (float)MirrorAngle(AimAngle);

        Direction = WormDir.Left;
    }

    private float lastAngle = 0;
    public void Aim(float amount)
    {
        var ThreePiOverTwo = (3.0f * MathF.PI) / 2.0f;
        var ThreePiOverFour = (3.0f * MathF.PI) / 4.0f;
        var PiOverFour = MathF.PI / 4.0f;
                        
        if (Direction == WormDir.Right)
        {
            AimAngle += amount;
            AimAngle = NormalizeAngle(AimAngle);

            // constrain angle between 3pi/2 and pi/4
            var isInRange = (AimAngle >= ThreePiOverTwo || AimAngle <= PiOverFour);
            if (!isInRange) AimAngle -= amount;
        }
        else if (Direction == WormDir.Left)
        {
            AimAngle -= amount;
            
            // constrain angle between 3pi/2 and 3pi/4
            if (AimAngle > ThreePiOverTwo) { AimAngle = ThreePiOverTwo; }
            if (AimAngle < ThreePiOverFour) { AimAngle = ThreePiOverFour; }
        }
    }

    /// <summary>
    /// Normalize the angle between 0 and 2π
    /// </summary>    
    public float NormalizeAngle(float angle)
    {
        var normalized = angle % (2 * MathF.PI);
        if (normalized < 0) normalized += 2 * MathF.PI;

        return normalized;
    }

    public float MirrorAngle(float angle)
    {
        var normalized = NormalizeAngle(angle);        

        // Reflect the angle across the Y-axis
        if (normalized <= MathF.PI) return MathF.PI - normalized;
        else return 3 * MathF.PI - normalized; // Adjust if beyond π radians
    }
}


internal class Program
{

    

    public static void Main()
    {
        var worm = new Worm();
        worm.X = 512; 
        worm.Y = 512;
        worm.Size = 25;
        worm.Direction = WormDir.Right;


        Console.WriteLine($"3pi/2 = {(3.0f * MathF.PI) / 2.0f}");
        Console.WriteLine($"3pi/4 = {(3.0f * MathF.PI) / 4.0f}");
        Console.WriteLine($"pi/4 = {MathF.PI / 4.0f}");


        //
        // Initialization
        //        
        Raylib.InitWindow(1024, 1024, ".: POC :.");
        Raylib.SetTargetFPS(60);

        SpriteLoader.LoadSpritesheet("Worm.json");

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
            
            if (Raylib.IsKeyDown(KeyboardKey.Up))
            {
                var angle = 1.0f * MathF.PI / 180.0f;
                worm.Aim(-angle);
            }
            else if (Raylib.IsKeyDown(KeyboardKey.Down))
            {
                var angle = 1.0f * MathF.PI / 180.0f;
                worm.Aim(angle);
            }

            //
            // UPDATE STUFF
            //

            // TODO

            //
            // RENDER STUFF
            //
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            worm.Render();

            Raylib.EndDrawing();
        }
        
        Raylib.CloseWindow();
    }
}

    
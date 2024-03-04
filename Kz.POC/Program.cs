// https://www.raylib.com/
// https://github.com/ChrisDill/Raylib-cs
// dotnet add package Raylib-cs

using Kz.Engine.Trigonometry;
using Kz.POC;
using Raylib_cs;
using Color = Raylib_cs.Color;

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
        worm.Size = 75;
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
            Raylib.DrawRectangle(0, 512, 1024, 512, Color.DarkBrown);

            worm.Render();

            Raylib.EndDrawing();
        }

        worm.Cleanup();
        Raylib.CloseWindow();
    }
}
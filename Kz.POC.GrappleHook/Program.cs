// https://www.raylib.com/
// https://github.com/ChrisDill/Raylib-cs
// dotnet add package Raylib-cs

using Kz.POC.GrappleHook;
using Raylib_cs;
using Color = Raylib_cs.Color;





internal class Program
{
    public static void Main()
    {
        //
        // Initialization
        //
        Raylib.InitWindow(1024, 1024, ".: POC - Grapple Hook :.");
        Raylib.SetTargetFPS(60);

        var worm = new Worm(500, 500, 25);

        //
        // MAIN RENDER LOOP
        //
        while (!Raylib.WindowShouldClose())    // Detect window close button or ESC key
        {
            //
            // PROCESS INPUTS
            //
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                // do something...
            }

            worm.ProcessInputs(1024, 1024);

            //
            // UPDATE STUFF
            //

            worm.Update(1024, 1024);

            //
            // RENDER STUFF
            //
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            worm.Render();

            Raylib.EndDrawing();
        }

        worm.Cleanup();

        Raylib.CloseWindow();
    }
}
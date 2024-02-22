// https://www.raylib.com/
// https://github.com/ChrisDill/Raylib-cs
// dotnet add package Raylib-cs

using Kz.Liero;
using Raylib_cs;
using Color = Raylib_cs.Color;

internal class Program
{
    public static void Main()
    {
        //
        // Initialization
        //
        var settings = new WindowSettings(1920, 1440, 4);

        Raylib.InitWindow(settings.WindowWidth, settings.WindowHeight, ".: Liero :.");
        Raylib.SetTargetFPS(60);
        
        //
        // Setup Game
        //
        var game = new Game(settings, 400, 400);

        //
        // MAIN RENDER LOOP
        //
        while (!Raylib.WindowShouldClose())    // Detect window close button or ESC key
        {            
            ProcessInputs(game);

            Update(game);

            Render(settings, game);            
        }

        game.End();
        Raylib.CloseWindow();
    }

    private static void Render(WindowSettings settings, IGame game)
    {
        game.Render();

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        RenderTextureToWindow(game.Texture, settings.WindowWidth, settings.WindowHeight);

        Raylib.DrawFPS(10, 10);

        Raylib.EndDrawing();
    }

    private static void Update(IGame game)
    {
        game.Update();
    }

    private static void ProcessInputs(IGame game)
    {
        game.ProcessInputs();
    }

    #region Helpers

    private static void RenderToTexture(RenderTexture2D target)
    {
        Raylib.BeginTextureMode(target);
        Raylib.ClearBackground(Color.Black);

        // TODO

        Raylib.EndTextureMode();
    }

    private static void RenderTextureToWindow(RenderTexture2D target, int windowWidth, int windowHeight)
    {
        var src = new Rectangle(0, 0, target.Texture.Width, -target.Texture.Height);
        var dest = new Rectangle(0, 0, windowWidth, windowHeight);
        Raylib.DrawTexturePro(
            target.Texture,
            src,
            dest,
            new System.Numerics.Vector2(0.0f, 0.0f),
            0,
            Color.White);
    }

    #endregion Helpers
}
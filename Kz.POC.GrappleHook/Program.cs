// https://www.raylib.com/
// https://github.com/ChrisDill/Raylib-cs
// dotnet add package Raylib-cs

using Kz.Engine.DataStructures;
using Kz.POC.GrappleHook;
using Raylib_cs;
using Color = Raylib_cs.Color;

/*
SpringForce = -k * x
    :where k = constant and represent scale of the force
    :where x = represents displacement of the spring (difference between rest length and current length)
        = currentLength - restLength

*/


public class Bob
{
    public Vector2f Location { get; set; }
    public Vector2f Velocity { get; set; }
    
    public Bob(float x, float y)
    {
        Location = new Vector2f(x, y);
        Velocity = Vector2f.Zero;        
    }

    public void Update()
    {        
        Location += Velocity;

        //if (Location.X < 25f) Location.X = 25f;
        //if (Location.X > 1024 - 25f) Location.X = 1024 - 25f;
        //if (Location.Y < 25f) Location.Y = 25f;
        //if (Location.Y > 1024 - 25f) Location.Y = 1024 - 25f;        
    }


    public void Render()
    {
        Raylib.DrawCircle((int)Location.X, (int)Location.Y, 25.0f, Color.Green);               
    }    
}


internal class Program
{
    public static void Main()
    {
        var _gravity = new Vector2f(0.0f, 1.5f);

        //
        // Initialization
        //
        Raylib.InitWindow(1024, 1024, ".: POC - Grapple Hook :.");
        Raylib.SetTargetFPS(30);

        var worm = new Worm(500, 500, 25);

        //var bob = new Bob(256.0f, 500.0f);

        //var spring = new Spring(200.0f, 0.01f);
        //spring.Anchor = new Vector2f(512f, 512f);
                        

        //
        // MAIN RENDER LOOP
        //
        while (!Raylib.WindowShouldClose())    // Detect window close button or ESC key
        {
            //
            // PROCESS INPUTS
            //
            if (Raylib.IsMouseButtonDown(MouseButton.Right))
            {
                //bob.Location.X = Raylib.GetMousePosition().X;
                //bob.Location.Y = Raylib.GetMousePosition().Y;
                //bob.Velocity = Vector2f.Zero;
            }

            worm.ProcessInputs(1024, 1024);


            //
            // UPDATE STUFF
            //

            worm.Update(1024, 1024);
            //bob.Velocity += _gravity;
            //var springForce = spring.GetForce(bob.Location);
            //bob.Velocity += springForce;
            //bob.Update();

            //bob.Velocity *= 0.99f; // dampening

            //Constrain(bob, spring, 50.0f, 1000.0f);
            
            //
            // RENDER STUFF
            //
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            worm.Render();
            
            //Raylib.DrawLine((int)spring.Anchor.X, (int)spring.Anchor.Y, (int)bob.Location.X, (int)bob.Location.Y, Color.Brown);
            //bob.Render();


            Raylib.EndDrawing();
        }

        worm.Cleanup();

        Raylib.CloseWindow();
    }

    private static void Constrain(Bob bob, Spring spring, float min, float max)
    {
        var dir = bob.Location - spring.Anchor;
        var currentLength = dir.Magnitude();

        if (currentLength < min)
        {
            dir = dir.Normal() * min;
            bob.Location = spring.Anchor + dir;
            bob.Velocity = Vector2f.Zero;
        }
        else if (currentLength > max)
        {
            dir = dir.Normal() * max;
            bob.Location = spring.Anchor + dir;
            bob.Velocity = Vector2f.Zero;
        }
    }
}
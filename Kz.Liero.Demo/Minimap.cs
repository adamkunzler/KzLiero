using Raylib_cs;

namespace Kz.Liero
{
    public class Minimap
    {
        private RenderTexture2D _target;
        public RenderTexture2D Target => _target;

        private int _width;
        public int Width => _width;

        private int _height;
        public int Height => _height;

        public Minimap(int width, int height)
        {                
            _target = Raylib.LoadRenderTexture(width, height);

            _width = width;
            _height = height;
        }
        
        public void Render(World world)
        {
            //var stepX = Math.Ceiling(world.WorldWidth / _width / 1.0f);
            //var stepY = Math.Ceiling(world.WorldHeight / _height / 1.0f);

            var stepX = world.WorldWidth / (float)_width;
            var stepY = world.WorldHeight / (float)_height;

            Raylib.BeginTextureMode(_target);
            Raylib.ClearBackground(Color.Black);
                        
            Raylib.DrawRectangleLines(0, 0, _width, _height, Color.Black);

            for(var y = 0; y < _height; y++)
            {
                for(var x = 0; x < _width; x++)
                {
                    var xx = x * stepX;
                    var yy = y * stepY;

                    var dirt = world.DirtAt((int)xx, (int)yy);
                    if (dirt == null) continue;

                    var color = (dirt.Value.IsActive) ? dirt.Value.Color : Background.DefaultColor;                    
                    Raylib.DrawPixel(x, y, color);
                }
            }

            var px1 = (world.Player1Position.X  / world.WorldWidth) * _width;
            var py1 = (world.Player1Position.Y / world.WorldHeight) * _height;            
            Raylib.DrawPixel((int)px1, (int)py1, world.Player1Color);

            var px2 = (world.Player2Position.X / world.WorldWidth) * _width;
            var py2 = (world.Player2Position.Y / world.WorldHeight) * _height;
            Raylib.DrawPixel((int)px2, (int)py2, world.Player2Color);

            Raylib.EndTextureMode();
        }
    }
}
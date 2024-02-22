using Raylib_cs;

namespace Kz.Liero
{
    public class RaylibHelper
    {
        public static void RenderTexture(RenderTexture2D target, int destWidth, int destHeight)
        {
            var src = new Rectangle(0, 0, target.Texture.Width, -target.Texture.Height);
            var dest = new Rectangle(0, 0, destWidth, destHeight);
            Raylib.DrawTexturePro(
                target.Texture,
                src,
                dest,
                new System.Numerics.Vector2(0.0f, 0.0f),
                0,
                Color.White);            
        }

        public static void RenderTexture(
            RenderTexture2D target, 
            int srcX, int srcY, int srcWidth, int srcHeight,
            int destX, int destY, int destWidth, int destHeight, 
            Color tint)
        {
            var src = new Rectangle(srcX, srcY, srcWidth, -srcHeight);
            var dest = new Rectangle(destX, destY, destWidth, destHeight);

            Raylib.DrawTexturePro(
                target.Texture,
                src,
                dest,
                new System.Numerics.Vector2(0.0f, 0.0f),
                0,
                tint);
        }

    }
}
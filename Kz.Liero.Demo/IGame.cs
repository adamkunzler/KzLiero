using Raylib_cs;

namespace Kz.Liero
{
    public interface IGame
    {
        RenderTexture2D Texture { get; }

        void Update();

        void Render();

        void ProcessInputs();

        void End();
    }
}
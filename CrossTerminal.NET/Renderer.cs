using System.Drawing;
using System.Numerics;
using TrippyGL;

namespace CrossTerminal.NET
{
    internal class Renderer
    {
        private readonly SimpleShaderProgram _shaderProgram;
        private readonly TextureBatcher _batch;
        public Renderer(GraphicsDevice graphicsDevice)
        {

            _shaderProgram = SimpleShaderProgram.Create<VertexColorTexture>(graphicsDevice, 0, 0, true);

            _batch = new TextureBatcher(graphicsDevice);
            _batch.SetShaderProgram(_shaderProgram);
            OnViewportChanged(graphicsDevice);
        }

        public void OnViewportChanged(GraphicsDevice graphicsDevice)
        {
            _shaderProgram.Projection = 
                Matrix4x4.CreateOrthographicOffCenter(
                    0, 
                    graphicsDevice.Viewport.Width, 
                    graphicsDevice.Viewport.Height, 
                    0, 
                    0, 
                    1);


        }


        public void Begin() => _batch.Begin();

        public void End() => _batch.End();

        public void Draw(object texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 scale, float depth)
        {
            var tex = (Texture2D)texture;

            _batch.Draw(
                tex,
                pos,
                src,
                new Color4b(color.R, color.G, color.B, color.A),
                scale,
                rotation,
                Vector2.Zero,
                depth);
        }

        public void DrawString(Font font, string text, Vector2 pos, Color color)
        {
            font.Draw(_batch, text, pos, color);
        }

    }
}

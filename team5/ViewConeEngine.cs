using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace team5
{
    public class ViewConeEngine
    {
        public Game1 Game;
        private VertexBuffer VertexBuffer;
        private IndexBuffer IndexBuffer;
        private BasicEffect BasicEffect;


        public ViewConeEngine(Game1 game)
        {
            Game = game;
        }

        public void LoadContent(ContentManager content, float angle)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[]
            {
                 new VertexPositionColor(new Vector3(0, 0.25f, 0), Color.Green),
                 new VertexPositionColor(new Vector3(0.5f, 0, 0), Color.Green),
                 new VertexPositionColor(new Vector3(0.52f, 0.1f, 0), Color.Green),
                 new VertexPositionColor(new Vector3(0.53f, 0.2f, 0), Color.Green),
                 new VertexPositionColor(new Vector3(0.53f, 0.3f, 0), Color.Green),
                 new VertexPositionColor(new Vector3(0.52f, 0.4f, 0), Color.Green),
                 new VertexPositionColor(new Vector3(0.5f, 0.5f, 0), Color.Green)
            };

            VertexBuffer = new VertexBuffer(Game.GraphicsDevice, VertexPositionTexture.VertexDeclaration,
                                vertices.Length, BufferUsage.None);

            VertexBuffer.SetData(vertices);
            short[] indices = new short[] { 0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5, 0, 5, 6 };
            IndexBuffer = new IndexBuffer(Game.GraphicsDevice, typeof(short), indices.Length, BufferUsage.None);
            IndexBuffer.SetData(indices);

            //shader
            BasicEffect = new BasicEffect(Game.GraphicsDevice);
        }

        public void Draw(Vector2 pos)
        {
            GraphicsDevice device = Game.GraphicsDevice;

            BasicEffect.World = Game.Transforms.ModelMatrix;
            BasicEffect.View = Game.Transforms.ViewMatrix;
            BasicEffect.Projection = Game.Transforms.ProjectionMatrix;
            BasicEffect.VertexColorEnabled = true;

            device.SetVertexBuffer(VertexBuffer);
            device.Indices = IndexBuffer;

            foreach (EffectPass pass in BasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 5);
            }
        }

    }
}

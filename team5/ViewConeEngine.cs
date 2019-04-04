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
        private Effect ConeEffect;
        private const int Triangles = 5;

        public ViewConeEngine(Game1 game)
        {
            Game = game;
        }

        public void LoadContent(ContentManager content)
        {
            // This is stupid but monogame requires a bogus vertex buffer. Fuck you, monogame!
            VertexPosition[] vertices = new VertexPosition[Triangles*3];
            for(int i=0; i<vertices.Length; i++)
                vertices[i] = new VertexPosition(new Vector3(0, 0, 0));
            VertexBuffer = new VertexBuffer(Game.GraphicsDevice, VertexPosition.VertexDeclaration,
                                            Triangles*3, BufferUsage.None);
            VertexBuffer.SetData(vertices);
            // Create shader
            ConeEffect = content.Load<Effect>("Shaders/cone");
        }

        public void Draw(float radius, float angle1, float angle2)
        {
            GraphicsDevice device = Game.GraphicsDevice;

            ConeEffect.CurrentTechnique = ConeEffect.Techniques["Cone"];
            ConeEffect.Parameters["projectionMatrix"].SetValue(Game.Transforms.ProjectionMatrix);
            ConeEffect.Parameters["viewMatrix"].SetValue(Game.Transforms.ViewMatrix);
            ConeEffect.Parameters["modelMatrix"].SetValue(Game.Transforms.ModelMatrix);
            ConeEffect.Parameters["angles"].SetValue(new Vector2(angle1, angle2));
            ConeEffect.Parameters["radius"].SetValue(radius);
            ConeEffect.Parameters["triangles"].SetValue(Triangles);

            device.SetVertexBuffer(VertexBuffer);
            device.BlendState = BlendState.AlphaBlend;
            foreach (EffectPass pass in ConeEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, Triangles);
            }
        }
        
        public void Draw(Vector2 position, float radius, float angle1, float angle2)
        {
            Game.Transforms.Push();
            Game.Transforms.Translate(position);
            Draw(radius, angle1, angle2);
            Game.Transforms.Pop();
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace team5
{
    public class ParticleEmitter
    {
        private Random Random;
        public Vector2 EmitterLocation { get; set; }
        private List<Particle> Particles = new List<Particle>();
        private List<Texture2D> Textures = new List<Texture2D>();
        private SpriteBatch SpriteBatch;
        private Game1 Game;

        private readonly RasterizerState NoCull;


        public ParticleEmitter(Game1 game)
        {
            Game = game;
            Random = game.RNG;
            NoCull = new RasterizerState { CullMode = CullMode.None };
        }

        private Particle GenerateNewParticle()
        {
            Texture2D texture = Textures[Random.Next(Textures.Count)];
            Vector2 position = EmitterLocation;
            Vector2 velocity = new Vector2(
                1f * (float)(Random.NextDouble() * 2 - 1),
                1f * (float)(Random.NextDouble() * 2 - 1));
            float angle = 0;
            float angularVelocity = 0.1f * (float)(Random.NextDouble() * 2 - 1);
            Color color = new Color(165, 42, 42);
            float size = 0.7f;
            float ttl = 1f;

            return new Particle(texture, EmitterLocation, velocity, angle, angularVelocity, color, size, ttl);
        }

        public void LoadContent(ContentManager content)
        {
            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
            Textures.Add(Game.TextureCache["particle"]);
        }
        
        public void UnloadContent()
        {
            SpriteBatch.Dispose();
            Textures.ForEach((tex)=>tex.Dispose());
            Textures.Clear();
        }

        public void Update()
        {
            int total = 1;

            for (int i = 0; i < total; i++)
            {
                Particles.Add(GenerateNewParticle());
            }

            for (int particle = 0; particle < Particles.Count; particle++)
            {
                Particles[particle].Update();
                if (Particles[particle].TTL <= 0)
                {
                    Particles.RemoveAt(particle);
                    particle--;
                }
            }
        }

        public void Draw()
        {
            Game.Transforms.PushView();
            Game.Transforms.ResetView();

            SpriteBatch.Begin();
            for (int i = 0; i < Particles.Count; i++)
            {
                Particles[i].Draw(SpriteBatch);
            }
            SpriteBatch.End();
            Game.Transforms.PopView();

            Game.GraphicsDevice.RasterizerState = NoCull;
        }
    }

}

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
        private List<Particle> particles;
        private List<Texture2D> textures;
        private SpriteBatch SpriteBatch;
        private Game1 Game;

        private readonly RasterizerState NoCull;


        public ParticleEmitter(Game1 game)
        {
            Game = game;
            this.particles = new List<Particle>();
            Random = game.RNG;
            NoCull = new RasterizerState { CullMode = CullMode.None };
        }


        private Particle GenerateNewParticle()
        {
            Texture2D texture = textures[Random.Next(textures.Count)];
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
            textures = new List<Texture2D>();
            textures.Add(content.Load<Texture2D>("Textures/particle"));

        }

        public void Update()
        {
            int total = 1;

            for (int i = 0; i < total; i++)
            {
                particles.Add(GenerateNewParticle());
            }

            for (int particle = 0; particle < particles.Count; particle++)
            {
                particles[particle].Update();
                if (particles[particle].TTL <= 0)
                {
                    particles.RemoveAt(particle);
                    particle--;
                }
            }
        }

        public void Draw()
        {
            Game.Transforms.PushView();
            Game.Transforms.ResetView();

            SpriteBatch.Begin();
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].Draw(SpriteBatch);
            }
            SpriteBatch.End();
            Game.Transforms.PopView();

            Game.GraphicsDevice.RasterizerState = NoCull;
        }
    }

}

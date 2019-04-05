﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    class StaticCamera : Entity
    {
        private AnimatedSprite Sprite;
        private ConeEntity ViewCone;
        
        public StaticCamera(Vector2 position, Game1 game) : base(game)
        {
            Position = position;
            Sprite = new AnimatedSprite(null, game, new Vector2(Chunk.TileSize, Chunk.TileSize));
            ViewCone = new ConeEntity(game);
            ViewCone.FromDegrees(225, 32);
            ViewCone.Radius = Chunk.TileSize * 10;
        }
        
        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = content.Load<Texture2D>("Textures/camera");
            Sprite.Add("idle", 0, 4, 1.0);
        }
        
        public override void Update(GameTime gameTime, Chunk chunk)
        {
            float dt = Game1.DeltaT;
            Sprite.Update(dt);
            ViewCone.UpdatePosition(Position);
            ViewCone.Update(gameTime, chunk);
        }
        
        public override void Draw(GameTime gameTime)
        {
            ViewCone.Draw(gameTime);
            Sprite.Draw(Position);
        }

        public override RectangleF GetBoundingBox()
        {
            throw new System.NotImplementedException();
        }

        public override bool Collide(Entity source, float timestep, out int direction, out float time, out bool corner)
        {
            throw new System.NotImplementedException();
        }

        public override bool Contains(Vector2 point)
        {
            throw new System.NotImplementedException();
        }
    }
}

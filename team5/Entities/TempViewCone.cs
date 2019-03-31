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
    class TempViewCone : ConeEntity
    {
        private AnimatedSprite Sprite;

        private int Direction;


        public TempViewCone(Game1 game) : base(game)
        {
            Sprite = new AnimatedSprite(null, game, new Vector2(69, 71));
            Radius = 69;
        }


        public void faceLeft()
        {
            if (Direction != -1)
            {
                Direction = -1;
                Sprite.Direction = -1;
                Angle1 = 5.0F / 6 * (float)Math.PI;
                Angle2 = 7.0F / 6 * (float)Math.PI;
            }
        }

        public void faceRight()
        {
            if (Direction != 1)
            {
                Direction = 1;
                Sprite.Direction = 1;
                Angle1 = 11.0F / 6 * (float)Math.PI;
                Angle2 = 1.0F / 6 * (float)Math.PI;
            }
        }

        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = content.Load<Texture2D>("Textures/Viewconetemp");
            Sprite.Add("idle", 0, 1, 1.0);
        }

        public override void Update(GameTime gameTime, Chunk chunk)
        {
            base.Update(gameTime, chunk);

            if (!chunk.Level.Player.IsHiding)
            {
                if (Collide(chunk.Level.Player, Game1.DeltaT, out int dirction, out float time, out bool corner))
                {
                    chunk.Die(chunk.Level.Player);
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if(Direction == 1)
            {
                Sprite.Draw(Position + new Vector2(35,0));
            }
            else
            {
                Sprite.Draw(Position - new Vector2(35, 0));
            }
            
        }
    }
}

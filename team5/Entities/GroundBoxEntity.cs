using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace team5.Entities
{
    class GroundBoxEntity : BoxEntity
    {
        public bool moveRight = false;
        public bool moveLeft = false;
        public bool jump = false;
        public bool jumpkeydown = false;

        public int longjump = 0;
        public int longjumpTime = 15;

        public float maxVel = 200;
        public float accelRate = 600;
        public float jumpSpeed = 200;
        public float longjumpSpeed = 200;

        public GroundBoxEntity(Vector2 position, Game1 game) : base(game)
        {
            this.position = position;
            size = new Point(10, 10);
            Texture2D dummyTexture;
            dummyTexture = new Texture2D(game.GraphicsDevice, 10, 10);
            Color[] colors = new Color[10 * 10];
            for (int i = 0; i < 100; ++i)
            {
                colors[i] = Color.Green;
            }
            dummyTexture.SetData(colors);
            drawer = new AnimatedSprite(dummyTexture, 1, 1, game.SpriteBatch);
        }

        public virtual void WallAction(int direction)
        {

        }

        public override void Update(GameTime gameTime, Chunk chunk)
        {
            base.Update(gameTime, chunk);

            int direction = 0;
            float time;

            Entity[] target;

            if (chunk != null)
            {
                if (moveRight && velocity.X < maxVel)
                {
                    velocity.X = Math.Min(maxVel, velocity.X + accelRate * Game1.DELTAT);
                }
                if (moveLeft && -velocity.X < maxVel)
                {
                    velocity.X = Math.Max(-maxVel, velocity.X - accelRate * Game1.DELTAT);
                }

                velocity.Y += Game1.DELTAT * Game1.GRAVITY;

                float deltat = Game1.DELTAT;

                bool collided = false;

                while (chunk.collideSolid(this, deltat, out direction, out time, out target))
                {
                    collided = true;
                    if ((direction & Chunk.DOWN) != 0)
                    {
                        velocity.Y = target[0].velocity.Y;
                        position.Y = target[0].getBoundingBox().Top - size.Y;
                    }
                    if ((direction & Chunk.UP) != 0)
                    {
                        float relVel = velocity.Y - target[0].velocity.Y;
                        velocity.Y = target[0].velocity.Y - (relVel / 3);
                        position.Y = target[0].getBoundingBox().Bottom;
                    }
                    if ((direction & Chunk.LEFT) != 0)
                    {
                        velocity.X = target[1].velocity.X;
                        WallAction(direction);
                        position.X = target[1].getBoundingBox().Right;
                    }
                    if ((direction & Chunk.RIGHT) != 0)
                    {
                        velocity.X = target[1].velocity.X;
                        WallAction(direction);
                        position.X = target[1].getBoundingBox().Left - size.X;
                    }
                    position += velocity * time * deltat;

                    if ((direction & Chunk.DOWN) != 0)
                    {
                        if (jump)
                        {
                            velocity.Y -= jumpSpeed;
                            longjump = longjumpTime;
                        }
                        if (!(moveLeft || moveRight || jump))
                        {
                            velocity.X = 0;
                        }
                    }

                    deltat = (1 - time) * deltat;
                }

                WallAction(direction);

                if(longjump < longjumpTime && longjump > 0)
                {
                    if (jumpkeydown)
                    {
                        velocity.Y -= deltat * longjumpSpeed; 
                    }
                }

                position += velocity * deltat;
            }
            else
            {

            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    class Player : BoxEntity
    {
        const bool canRepeatWallJump = false;
        const bool canDoubleJump = false;

        public bool moveRight = false;
        public bool moveLeft = false;
        public bool jumpkeydown = true;
        private bool jumpkeywasup = false;
        public bool jump = false;
        private bool hasWallJumped = false;
        private bool hasDoubleJumped = false;

        public int longjump = 0;

        public const float maxVel = 200;
        public const float accelRate = 600;
        public const float jumpSpeed = 400;
        public const float groundFriction = 0.0001F;
        public const float airFriction = 0.5F;
        public static readonly float stepGroundFriction = (float)Math.Pow(groundFriction, Game1.DELTAT);
        public static readonly float stepAirFriction = (float)Math.Pow(airFriction, Game1.DELTAT);

        public Player(Vector2 position, Game1 game):base(game)
        {
            this.position = position;
            size = new Point(10,10);
            Texture2D dummyTexture;
            dummyTexture = new Texture2D(game.GraphicsDevice, 10, 10);
            Color[] colors = new Color[10*10];
            for(int i = 0; i < 100; ++i)
            {
                colors[i] = Color.Green;
            }
            dummyTexture.SetData(colors);
            drawer = new AnimatedSprite(dummyTexture, 1, 1, game.SpriteBatch);
        }

        public override void Update(GameTime gameTime, Chunk chunk)
        {
            //base.Update(gameTime, chunk);

            jump = jumpkeydown && jumpkeywasup;

            int direction;
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
                    if ((direction & Chunk.DOWN) != 0) {
                        velocity.Y = target[0].velocity.Y;
                        position.Y = target[0].getBoundingBox().Top - size.Y;
                        hasDoubleJumped = false;
                        hasWallJumped = false;
                    }
                    if ((direction & Chunk.UP) != 0)
                    {
                        float relVel = velocity.Y - target[0].velocity.Y;
                        velocity.Y = target[0].velocity.Y - (relVel/3);
                        position.Y = target[0].getBoundingBox().Bottom;
                    }
                    if ((direction & Chunk.LEFT) != 0)
                    {
                        velocity.X = target[1].velocity.X;
                        velocity.Y = velocity.Y * (float)Math.Pow(groundFriction, Game1.DELTAT);
                        position.X = target[1].getBoundingBox().Right;
                    }
                    if ((direction & Chunk.RIGHT) != 0)
                    {
                        velocity.X = target[1].velocity.X;
                        velocity.Y = velocity.Y * (float)Math.Pow(groundFriction, Game1.DELTAT);
                        position.X = target[1].getBoundingBox().Left - size.X;
                    }
                    position += velocity * time*deltat;

                    if((direction & Chunk.DOWN) != 0)
                    {
                        if (jump)
                        {
                            jumpkeydown = false;
                            velocity.Y -= jumpSpeed;
                            longjump = 15;
                        }
                        if(!(moveLeft || moveRight || jump))
                        {
                            velocity.X = 0;
                        }
                    }
                    else
                    {
                        if (jump && (!hasWallJumped || canRepeatWallJump) && (direction & Chunk.RIGHT) != 0)
                        {
                            velocity.Y -= jumpSpeed;
                            velocity.X = -maxVel;
                            hasWallJumped = true;
                            jump = false;
                        }
                        if(jump && !hasWallJumped && (direction & Chunk.LEFT) != 0)
                        {
                            velocity.Y -= jumpSpeed;
                            velocity.X = maxVel;
                            hasWallJumped = true;
                            jump = false;
                        }
                    }

                    deltat = (1 - time) * deltat;
                }

                if (!collided && jump && (!hasDoubleJumped && canDoubleJump))
                {
                    velocity.Y = Math.Min(velocity.Y,Math.Max(-jumpSpeed, velocity.Y-jumpSpeed));
                    longjump = 15;
                    hasDoubleJumped = true;
                }
                
                position += velocity * deltat;
                velocity = velocity * stepAirFriction;
            }
            else
            {

            }

            jumpkeywasup = !jumpkeydown;
        }
    }
}

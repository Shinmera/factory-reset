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
		public bool moveRight = false;
		public bool moveLeft = false;
		public bool jump = false;

		public const float maxVel = 100;
		public const float accelRate = 300;
		public const float jumpSpeed = 100;
		public const float friction = 0.5F;
		public static readonly float stepFriction = (float)Math.Pow(0.5F, Game1.DELTAT);

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
			base.Update(gameTime, chunk);

			int direction;
			float time;

			Entity[] target;

			if (chunk != null)
			{
				velocity.Y += Game1.DELTAT * Game1.GRAVITY;

				if (chunk.collideSolid(this, Game1.DELTAT, out direction, out time, out target))
				{
					if ((direction & Chunk.DOWN) != 0) {
						velocity.Y = 0;
						position.Y = target[0].getBoundingBox().Top - size.Y;
					}
					if ((direction & Chunk.UP) != 0)
					{
						velocity.Y = 0;
						position.Y = target[0].getBoundingBox().Bottom;
					}
					if ((direction & Chunk.LEFT) != 0)
					{
						velocity.X = 0;
						position.Y = target[1].getBoundingBox().Right;
					}
					if ((direction & Chunk.RIGHT) != 0)
					{
						position.Y = target[1].getBoundingBox().Left - size.X;
						velocity.X = 0;
					}
					position += velocity * Game1.DELTAT;

					if((direction & Chunk.DOWN) != 0)
					{
						if (moveRight && velocity.X < maxVel)
						{
							velocity.X = Math.Min(maxVel, velocity.X + accelRate * Game1.DELTAT);
						}
						if (moveLeft && -velocity.X < maxVel)
						{
							velocity.X = Math.Max(-maxVel, velocity.X - accelRate * Game1.DELTAT);
						}
						if (jump)
						{
							velocity.Y -= jumpSpeed;
						}
						if(!(moveLeft || moveRight || jump))
						{
							velocity.X = velocity.X * stepFriction;
							if(Math.Abs(velocity.X) < 1F)
							{
								velocity.X = 0;
							}
						}
					}
				}
				else
				{
					position += velocity * Game1.DELTAT;
				}
			}
			else
			{

			}
		}
	}
}

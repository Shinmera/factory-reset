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

		public int longjump = 0;

		public const float maxVel = 200;
		public const float accelRate = 600;
		public const float jumpSpeed = 400;
		public const float groundFriction = 0.01F;
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
			base.Update(gameTime, chunk);

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

				while (chunk.collideSolid(this, deltat, out direction, out time, out target))
				{
					if ((direction & Chunk.DOWN) != 0) {
						velocity.Y = target[0].velocity.Y;
						position.Y = target[0].getBoundingBox().Top - size.Y;
					}
					if ((direction & Chunk.UP) != 0)
					{
						velocity.Y = target[0].velocity.Y;
						position.Y = target[0].getBoundingBox().Bottom;
					}
					if ((direction & Chunk.LEFT) != 0)
					{
						velocity.X = target[1].velocity.X;
						position.X = target[1].getBoundingBox().Right;
					}
					if ((direction & Chunk.RIGHT) != 0)
					{
						velocity.X = target[1].velocity.X;
						position.X = target[1].getBoundingBox().Left - size.X;
					}
					position += velocity * time*deltat;

					if((direction & Chunk.DOWN) != 0)
					{
						if (jump)
						{
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
						velocity = velocity * stepAirFriction;
					}

					deltat = (1 - time) * deltat;
				}

				position += velocity * deltat;
				velocity = velocity * stepAirFriction;
			}
			else
			{

			}
		}
	}
}

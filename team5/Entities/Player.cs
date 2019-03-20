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

			Entity target;

			if (chunk != null)
			{
				velocity.Y += Game1.GRAVITY;
				if (chunk.collideSolid(this, Game1.DELTAT, out direction, out time, out target))
				{
					switch (direction)
					{
						case Chunk.DOWN:
							velocity.Y = 0;
							position.Y = target.getBoundingBox().Top - size.Y;
							position.X += velocity.X * Game1.DELTAT;
							break;
						case Chunk.UP:
							velocity.Y = 0;
							position.Y = target.getBoundingBox().Bottom;
							position.X += velocity.X * Game1.DELTAT;
							break;
						case Chunk.LEFT:
							velocity.X = 0;
							position.Y = target.getBoundingBox().Right;
							position.Y += velocity.Y * Game1.DELTAT;
							break;
						case Chunk.RIGHT:
							position.Y = target.getBoundingBox().Left - size.X;
							velocity.X = 0;
							position.Y += velocity.Y * Game1.DELTAT;
							break;
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

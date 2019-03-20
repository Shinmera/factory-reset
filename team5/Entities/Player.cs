using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5
{
	class Player : BoxEntity
	{

		public void moveLeft()
		{

		}

		public Player(Vector2 position)
		{
			this.position = position;
		}

		public override void Update(GameTime gameTime, Chunk chunk)
		{
			base.Update(gameTime, chunk);

			int direction;
			float time;

			Entity target;

			chunk.collideSolid(this, Game1.physicstimestep, out direction, out time, out target);
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5
{
	abstract class Entity
	{
		protected Vector2 velocity;
		protected Vector2 position;

		public abstract Rectangle getBoundingBox();

		public virtual void Update(GameTime gameTime)
		{

		}

		public virtual void Draw(GameTime gameTime)
		{

		}

		//Assume source is always a box
		public virtual bool collide(Entity source, out int direction, out Vector2 position)
		{
			direction = -1;
			position = new Vector2();
			return false;
		}
	}
}

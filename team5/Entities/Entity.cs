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
		public Vector2 velocity;
		public Vector2 position;

		public abstract Rectangle getBoundingBox();

		public virtual void Update(GameTime gameTime)
		{

		}

		public virtual void Draw(GameTime gameTime)
		{

		}

		//Assume source is always a box
		public abstract bool collide(Entity source, float timestep, out int direction, out Vector2 position);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5
{
	class Entity
	{

		Rectangle boundingBox;
		Vector2 velocity;
		Vector2 position;

		public virtual void Update(GameTime gameTime)
		{

		}

		public virtual void Draw(GameTime gameTime)
		{

		}

		//Assume source is always a box
		public virtual bool collide(Entity source, ref int direction, ref Vector2 position)
		{
			return false;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5.Entities
{
	abstract class BoxEntity : Entity
	{
		protected Rectangle boundingBox;

		public override Rectangle getBoundingBox()
		{
			return boundingBox;
		}

		public override bool collide(Entity source, float timestep, out int direction, out Vector2 position)
		{
			Rectangle motionBB;
			motionBB.X = source.getBoundingBox().Left + Math.Floor(Math.Min(0,));

			direction = -1;
			position = new Vector2();
			return false;
		}
	}
}

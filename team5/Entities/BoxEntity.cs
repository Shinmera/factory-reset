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
	}
}

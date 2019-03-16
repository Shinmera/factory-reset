using System;
using System.Collections;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace team5
{
	class Level
	{
		public static int EMPTY = 0;
		public static int SOLIDPLATFORM = 1;

		public int[][] tileset;

		//Viewcones, intelligence
		ArrayList CollidingEntities;

		//Enemies, background objects
		ArrayList NonCollidingEntities;

		//things that will stop you like moving platforms (which are not part of the tileset)
		ArrayList SolidEntities;



		public Level()
		{
			ArrayList SolidEntities = new ArrayList();
		}

		public void Update(GameTime gameTime)
		{
			foreach (var entity in SolidEntities)
			{
				((Entity)entity).Update(gameTime);
			}
		}

		public void Draw(GameTime gameTime)
		{

		}

		public bool collide(Entity source, out int direction, out Entity target, out Vector2 position, float timestep)
		{
			direction = -1;
			target = null;
			position = new Vector2();
			return false;
		}
	}
}

using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace team5
{
	class Chunk
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

		SpriteBatch spriteBatch;


		public Chunk(SpriteBatch spriteBatch, int [][] tileset)
		{
			this.tileset = tileset;
			ArrayList SolidEntities = new ArrayList();
			this.spriteBatch = spriteBatch;
		}

		public void Update(GameTime gameTime)
		{
			foreach (var entity in SolidEntities)
			{
				((Entity)entity).Update(gameTime, this);
			}
		}

		public void Draw(GameTime gameTime)
		{

		}

		public static int UP = 1;
		public static int RIGHT = 2;
		public static int DOWN = 3;
		public static int LEFT = 4;

		//TODO: Tile collisions
		public bool collideSolid(Entity source, float timestep, out int direction, out float time, out Entity target)
		{

			time = float.PositiveInfinity;
			direction = -1;
			target = null;

			foreach (var entity in SolidEntities)
			{
				float tempTime;
				int tempDirection;
				if (((BoxEntity)entity).collide(source, timestep, out tempDirection, out tempTime)){
					if (tempTime < time)
					{
						time = tempTime;
						direction = tempDirection;
						target = (BoxEntity)entity;
					}
				}
			}

			if (target != null)
				return true;

			return false;
		}
	}
}

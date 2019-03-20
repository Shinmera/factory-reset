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
	class Level
	{
		public ArrayList chunks;
		public Chunk activeChunk;
		public Player player;

		private bool chunkTrans = false;

		public Level()
		{

		}

		//TESTING ONLY
		public Level(Game1 game)
		{
			player = new Player(new Vector2(200, 50), game);
			activeChunk = new Chunk(game, player);
		}

		public void Update(GameTime gameTime)
		{
			activeChunk.Update(gameTime);
			if (chunkTrans)
			{
				player.Update(gameTime, null);
			}
		}

		public void Draw(GameTime gameTime)
		{
			activeChunk.Draw(gameTime);
		}
	}
}

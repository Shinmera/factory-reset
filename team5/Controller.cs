using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace team5
{
	class Controller
	{
		Player player;

		public void setPlayer(Player player)
		{
			this.player = player;
		}

		public Controller(Game1 game)
		{

		}

		public void Update()
		{
			KeyboardState state = Keyboard.GetState();

			player.moveLeft = state.IsKeyDown(Keys.A);
			player.moveRight = state.IsKeyDown(Keys.D);
			player.jump = state.IsKeyDown(Keys.W);
		}
	}
}

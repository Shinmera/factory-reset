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
        Player Player;

        public void SetPlayer(Player player)
        {
            this.Player = player;
        }

        public Controller(Game1 game)
        {

        }

        public void Update()
        {
            KeyboardState state = Keyboard.GetState();

            Player.MoveLeft = state.IsKeyDown(Keys.A);
            Player.MoveRight = state.IsKeyDown(Keys.D);
            Player.JumpKeyDown = state.IsKeyDown(Keys.W);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5
{
    class DialogBox : TextBox
    {
        public DialogBox(string text, string font, float sizePx, Game1 game, Level parent, Vector2 position = default(Vector2)) : base(text, font, sizePx, game, parent, "WalkieTalkieSprite", position, Chunk.Down)
        {
            LeftPadding = 65;
            RightPadding = 5;
            TopPadding = 55;
            BottomPadding = 5;
        }

        public override void Draw()
        {
            base.Draw();

        }
    }
}

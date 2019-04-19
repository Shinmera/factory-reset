using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    class DialogBox : TextBox
    {
        private bool EnterKeyWasUp = false;
        private string[] TextArray;
        private int CurText = 0;
        private int CurNumLines;
        private int CurLine = 0;
        private float LineHeight;
        private float MaxLines;

        public DialogBox(string[] text, string font, float sizePx, Game1 game, Level parent, Vector2 position = default(Vector2)) : base("", font, sizePx, game, parent, "WalkieTalkieSprite", position, Chunk.Down)
        {
            LeftPadding = 65;
            RightPadding = 5;
            TopPadding = 55;
            BottomPadding = 5;

            TextArray = text;

            MaxLines = (Background.Height - TopPadding - BottomPadding) / sizePx;

            SetText(TextArray[0]);

            CurNumLines = Text.Split('\n',StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public override void Update()
        {
            if ((EnterKeyWasUp && Game.Controller.Enter))
            {
                if(CurLine + MaxLines < CurNumLines)
                {
                    CurLine += (int)Math.Floor(MaxLines - 0.3F);
                    TextOffset = CurLine * SizePx;
                }
                else
                {
                    ++CurText;
                    if(CurText>= TextArray.Length)
                    {
                        Level.ClosePopup();
                    }
                    else
                    {
                        SetText(TextArray[CurText]);
                        CurNumLines = Text.Split('\n').Length;
                        CurLine = 0;
                        TextOffset = 0;
                    }
                }
            }

            EnterKeyWasUp = !Game.Controller.Enter;
        }

        public override void Draw()
        {
            base.Draw();

        }
    }
}

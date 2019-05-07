using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5
{
    class ButtonPrompt:BoxEntity
    {
        public readonly TextEngine.Button[] Button;

        public const int PromptCount = 5;

        private static Dictionary<uint, TextEngine.Button[]> Buttons;

        private const float ButtonWidth = 18;

        static ButtonPrompt()
        {
            Buttons = new Dictionary<uint, TextEngine.Button[]>
            {
                { 0x1, new TextEngine.Button[] { TextEngine.Button.R2 } },
                { 0x2, new TextEngine.Button[] { TextEngine.Button.L }},
                { 0x3, new TextEngine.Button[] { TextEngine.Button.A } },
                { 0x4, new TextEngine.Button[] { TextEngine.Button.AUR } },
                { 0x5, new TextEngine.Button[] { TextEngine.Button.AD } },
                { 0x6, new TextEngine.Button[] { TextEngine.Button.R2, TextEngine.Button.AUR } }
            };
        }

        private ButtonPrompt(Game1 game, TextEngine.Button[] button, Vector2 position) : base(game, new Vector2(Chunk.TileSize*2F))
        {
            Button = button;
            Position = position;
        }

        public static ButtonPrompt GetPrompt(Game1 game, uint id, Vector2 position)
        {
            return new ButtonPrompt(game, Buttons[id], position);
        }

        public void DrawPrompt()
        {
            for(int i = 0; i < Button.Length; ++i)
            {
                Vector2 buttonPos = Game.TextEngine.TranslateToWindow(Position + new Vector2(0 - ButtonWidth / 2 * Button.Length + i * ButtonWidth, 36));
                Game.TextEngine.QueueButton(Button[i], buttonPos);
            }
            
        }
    }
}

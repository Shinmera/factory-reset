using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace team5
{
    class DialogBox : TextBox
    {
        private string[] TextArray;
        private int CurText = 0;
        private int CurNumLines;
        private int CurLine = 0;
        private float MaxLines;
        private float LetterTimer = 0;
        private int CurMaxLetters;

        private float timeBeforeSkip;
        private float timeBeforeSkipDuration = 0.3f;

        public static float timePerLetter = 0.02f;

        private Texture2D AvatarTexture;
        private AnimatedSprite Avatar;
        private Vector2 AvatarOffset = new Vector2(-197, 38);

        protected static Dictionary<string, Texture2D> Sprites;

        public DialogBox(string[] text, string font, float sizePx, Game1 game, Level parent, Vector2 position = default(Vector2)) : base("", font, sizePx, game, parent, "WalkieTalkie", position, Chunk.Down)
        {
            LeftPadding = 65;
            RightPadding = 5;
            TopPadding = 5;
            BottomPadding = 5;

            AvatarTexture = Sprites["WalkieTalkieSprite"];

            Avatar = new AnimatedSprite(AvatarTexture, game, new Vector2(AvatarTexture.Bounds.Width, AvatarTexture.Bounds.Height));
            Avatar.Add("idle", 0, 1, 100);
            Avatar.Play("idle");

            TextArray = text;

            MaxLines = (Background.Height - TopPadding - BottomPadding) / sizePx;

            SetText(TextArray[0]);
            CurLetters = 1;
            CurMaxLetters = Text.Length;
            CurNumLines = Text.Split('\n',StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static new void LoadStaticContent(ContentManager content)
        {
            Sprites = new Dictionary<string, Texture2D>
            {
                { "WalkieTalkieSprite", content.Load<Texture2D>("Textures/walkie_talkie") },
                { "WalkieTalkieBlank", content.Load<Texture2D>("Textures/walkie_talkie_Blank") }
            };
        }

        public override void Update()
        {
            if(timeBeforeSkip > 0)
            {
                timeBeforeSkip -= Game1.DeltaT;
            }
            if(CurLetters < CurMaxLetters)
            {
                LetterTimer += Game1.DeltaT;

                while(LetterTimer >= timePerLetter)
                {
                    LetterTimer -= timePerLetter;
                    ++CurLetters;
                }

                if(CurLetters >= CurMaxLetters)
                {
                    CurLetters = CurMaxLetters;
                    timeBeforeSkip = timeBeforeSkipDuration;
                }
                
            }

            if ((!Game.Controller.Advance && Game.Controller.Was.Advance))
            {
                if (CurLetters < CurMaxLetters)
                {
                    CurLetters = CurMaxLetters;
                }
                else if(timeBeforeSkip > 0)
                {
                    timeBeforeSkip = 0;
                }
                else if (CurLine + MaxLines < CurNumLines)
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
                        CurLetters = 1;
                        CurMaxLetters = Text.Length;
                        LetterTimer = 0;
                    }
                }
            }
        }

        public override void Draw()
        {
            base.Draw();

            Vector2 center = GetPos();

            center += AvatarOffset;

            float scale = Game.GraphicsDevice.Viewport.Width / Level.Camera.GetTargetSize().X * 0.5F;

            Game.Transforms.PushView();
            Game.Transforms.ResetView();
            Game.Transforms.ScaleView(scale);
            Game.Transforms.TranslateView(scale * center);

            Avatar.Draw();

            Game.Transforms.PopView();
        }
    }
}

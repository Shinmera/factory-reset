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
        private string[] CurLines;
        private int CurText = 0;
        private int CurNumLines;
        private int CurLine = 0;
        private float MaxLines;
        private float LetterTimer = 0;
        private int CurMaxLetters;

        private float TimeBeforeSkip;
        private float TimeBeforeSkipDuration = 0.4f;
        public static float TimePerLetter = 0.02f;

        private SoundEngine.Sound CallSound;

        private Texture2D AvatarTexture;
        private AnimatedSprite Avatar;
        private Vector2 AvatarOffset = new Vector2(-197, 38);

        public DialogBox(string[] text, string font, float sizePx, Game1 game, Level parent, Vector2 position = default(Vector2)) : base("", font, sizePx, game, parent, "WalkieTalkie", position, Chunk.Down)
        {
            LeftPadding = 65;
            RightPadding = 5;
            TopPadding = 5;
            BottomPadding = 5;

            AvatarTexture = game.TextureCache["walkie_talkie"];

            Avatar = new AnimatedSprite(AvatarTexture, game, new Vector2(AvatarTexture.Bounds.Width, AvatarTexture.Bounds.Height));
            Avatar.Add("idle", 0, 1, 100);
            Avatar.Play("idle");

            TextArray = text;

            MaxLines = (Background.Height - TopPadding - BottomPadding) / sizePx;

            SetText(TextArray[0]);
            CurLetters = 1;
            //CurMaxLetters = Text.Length;
            CurLines = Text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            CurNumLines = CurLines.Length;

            CurMaxLetters = 0;
            for(int i = 0; i < Math.Min(Math.Floor(MaxLines),CurNumLines); ++i)
            {
                CurMaxLetters++;
                CurMaxLetters += CurLines[i].Length;
            }
            CurMaxLetters--;
        }

        public override void Update()
        {
            if(CallSound == null) {
                CallSound = Game.SoundEngine.Play("Player_WalkieTalk1", 1, false);
            }

            if (TimeBeforeSkip > 0)
            {
                TimeBeforeSkip -= Game1.DeltaT;
            }
            if(CurLetters < CurMaxLetters)
            {
                LetterTimer += Game1.DeltaT;

                while(LetterTimer >= TimePerLetter && CurLetters < CurMaxLetters)
                {
                    LetterTimer -= TimePerLetter;
                    ++CurLetters;
                }

                if(CurLetters >= CurMaxLetters)
                {
                    CurLetters = CurMaxLetters;
                    if(TimePerLetter > 0)
                        TimeBeforeSkip = TimeBeforeSkipDuration;
                }

                if(CurLetters < Math.Max(CurMaxLetters-5,2))
                    PlaySound();
            }
            else
            {
                if(TimeBeforeSkip <= 0)
                {
                    CallSound.Stopped = true;
                }
            }

            if(Game.Controller.Call && !Game.Controller.Was.Call)
            {
                Game.SoundEngine.Play("Player_WalkieEnd");
                CallSound.Stopped = true;
                Level.ClosePopup();
                return;
            }

            if ((!Game.Controller.Advance && Game.Controller.Was.Advance))
            {
                if (CurLetters < CurMaxLetters)
                {
                    CurLetters = CurMaxLetters;
                    CallSound.Stopped = true;
                }
                else if(TimeBeforeSkip > 0)
                {
                    TimeBeforeSkip = 0;
                }
                else if (CurLine + MaxLines < CurNumLines)
                {
                    CurLine += (int)Math.Floor(MaxLines);

                    CurMaxLetters = 0;
                    for (int i = 0; i < Math.Min(CurLine + Math.Floor(MaxLines), CurNumLines); ++i)
                    {
                        CurMaxLetters++;
                        CurMaxLetters += CurLines[i].Length;
                    }
                    CurMaxLetters--;

                    TextOffset = CurLine * SizePx;
                }
                else
                {
                    ++CurText;

                    if (CurText>= TextArray.Length)
                    {
                        CallSound.Stopped = true;
                        Game.SoundEngine.Play("Player_WalkieEnd");
                        Level.ClosePopup();
                    }
                    else
                    {
                        Game.SoundEngine.Play("UI_Button", 1);

                        PlaySound();

                        SetText(TextArray[CurText]);
                        CurLines = Text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                        CurNumLines = CurLines.Length;
                        CurLine = 0;
                        TextOffset = 0;
                        CurLetters = 1;
                        //CurMaxLetters = Text.Length;

                        CurMaxLetters = 0;
                        for (int i = 0; i < Math.Min(CurLine + Math.Floor(MaxLines), CurNumLines); ++i)
                        {
                            CurMaxLetters++;
                            CurMaxLetters += CurLines[i].Length;
                        }
                        CurMaxLetters--;
                        LetterTimer = 0;
                    }
                }
            }
        }


        public void PlaySound()
        {
            if (CallSound.Stopped)
            {
                CallSound = Game.SoundEngine.Play("Player_WalkieTalk1", 1, false);
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

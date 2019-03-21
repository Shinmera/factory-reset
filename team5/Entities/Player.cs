using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    class Player : GroundBoxEntity
    {
        const bool CanRepeatWallJump = false;
        const bool CanDoubleJump = false;

        const int Width = 10;
        const int Height = 10;

        private bool JumpKeyWasUp = false;
        private bool HasWallJumped = false;
        private bool HasDoubleJumped = false;

        public const float PlayerMaxVel = 200;
        public const float PlayerAccelRate = 600;
        public const float PlayerJumpSpeed = 200;
        public const float PlayerLongJumpSpeed = 300;
        public const float PlayerLongJumpTime = 15*Game1.DeltaT;
        public const float PlayerGroundFriction = 0.0001F;
        public const float PlayerAirFriction = 0.5F;

        public static readonly float PlayerStepGroundFriction = (float)Math.Pow(PlayerGroundFriction, Game1.DeltaT);
        public static readonly float PlayerStepAirFriction = (float)Math.Pow(PlayerAirFriction, Game1.DeltaT);

        public Player(Vector2 position, Game1 game):base(position,game, new Point(Width, Height))
        {
            Texture2D dummyTexture;
            dummyTexture = new Texture2D(game.GraphicsDevice, 10, 10);
            Color[] colors = new Color[10*10];
            for(int i = 0; i < 100; ++i)
            {
                colors[i] = Color.Green;
            }
            dummyTexture.SetData(colors);
            Drawer = new AnimatedSprite(dummyTexture, 1, 1, game.SpriteBatch);

            MaxVel = PlayerMaxVel;
            AccelRate = PlayerAccelRate;
            JumpSpeed = PlayerJumpSpeed;
            LongJumpSpeed = PlayerJumpSpeed;
            StepAirFriction = PlayerStepAirFriction;
        }

        public override void WallAction(int direction)
        {
            base.WallAction(direction);
            if(Velocity.Y > 0)
                Velocity.Y *= PlayerStepGroundFriction;

            if (Jump && (!HasWallJumped || CanRepeatWallJump) && (direction & Chunk.Right) != 0)
            {
                Velocity.Y -= JumpSpeed;
                Velocity.X = -MaxVel;
                HasWallJumped = true;
                Jump = false;
            }
            if (Jump && (!HasWallJumped || CanRepeatWallJump) && (direction & Chunk.Left) != 0)
            {
                Velocity.Y -= JumpSpeed;
                Velocity.X = MaxVel;
                HasWallJumped = true;
                Jump = false;
            }
        }

        public override void OnTouchGround()
        {
            base.OnTouchGround();
            HasDoubleJumped = false;
            HasWallJumped = false;
        }

        public override void Update(GameTime gameTime, Chunk chunk)
        {

            Jump = JumpKeyDown && JumpKeyWasUp;

            base.Update(gameTime, chunk);

            JumpKeyWasUp = !JumpKeyDown;

        }
    }
}

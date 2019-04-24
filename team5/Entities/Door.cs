using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace team5
{
    class Door : BoxEntity
    {
        private AnimatedSprite Sprite;

        public enum EState
        {
            Open,
            Opening,
            Closed,
            Closing
        };

        private float Timer = 1;
        private const float OpenCloseTime = 1;

        private EState State = EState.Closing;

        public Door(Vector2 position, Game1 game) : base(game, new Vector2(Chunk.TileSize * 3, Chunk.TileSize))
        {
            Position = position + Vector2.UnitY * (Chunk.TileSize / 2);
            Sprite = new AnimatedSprite(null, game, new Vector2(16, 32));
        }

        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = Game.TextureCache["door"];
            Sprite.Add("open", 0, 1, 1.0);
            Sprite.Add("closing", 2, 9, 0.7, -1, 4);
            Sprite.Add("opening", 2, 9, 0.7, -1, 0);
            Sprite.Add("crash", 9, 15, 0.6, -1, 0);
            Sprite.Add("closed", 15, 16, 1.0);
        }

        public void Interact(Chunk chunk)
        {
            switch (State)
            {
                case EState.Closed:
                    State = EState.Opening;
                    Sprite.Play("opening");
                    break;
                case EState.Open:
                    State = EState.Closing;
                    Sprite.Play("closing");
                    chunk.SolidTiles[chunk.GetTileLocation(Position - Vector2.UnitY * Chunk.TileSize / 2)] = (uint)Chunk.Colors.SolidPlatform;
                    chunk.SolidTiles[chunk.GetTileLocation(Position + Vector2.UnitY * Chunk.TileSize / 2)] = (uint)Chunk.Colors.SolidPlatform;
                    break;
            }
        }

        public override void Update(Chunk chunk)
        {
            base.Update(chunk);

            switch (State)
            {
                case EState.Closed:
                case EState.Open:
                    break;
                case EState.Closing:
                    Timer += Game1.DeltaT;
                    if(Timer >= OpenCloseTime)
                    {
                        Timer = 0;
                        Sprite.Play("closed");
                        State = EState.Closed;
                    }
                    break;
                case EState.Opening:
                    Timer += Game1.DeltaT;
                    if (Timer >= OpenCloseTime)
                    {
                        Timer = 0;
                        Sprite.Play("open");
                        State = EState.Open;
                        chunk.SolidTiles[chunk.GetTileLocation(Position - Vector2.UnitY * Chunk.TileSize / 2)] = (uint)Chunk.Colors.BackgroundWall;
                        chunk.SolidTiles[chunk.GetTileLocation(Position + Vector2.UnitY * Chunk.TileSize / 2)] = (uint)Chunk.Colors.BackgroundWall;
                    }
                    break;
            }

            Sprite.Update(Game1.DeltaT);
        }

        public override void Draw()
        {
            Sprite.Draw(Position);
        }

        public override void Respawn(Chunk chunk)
        {
            Sprite.Play("closed");
            State = EState.Closed;
            chunk.SolidTiles[chunk.GetTileLocation(Position)] = (uint)Chunk.Colors.SolidPlatform;
            chunk.SolidTiles[chunk.GetTileLocation(Position + Vector2.UnitY * Chunk.TileSize)] = (uint)Chunk.Colors.SolidPlatform;
        }
    }
}

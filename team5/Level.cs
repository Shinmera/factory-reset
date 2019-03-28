using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;

namespace team5
{
    class Level
    {
        public ArrayList Chunks;
        public Chunk ActiveChunk;
        public Player Player;

        private bool ChunkTrans = false;

        public Level()
        {

        }

        //TESTING ONLY
        public Level(Game1 game)
        {
            Player = new Player(new Vector2(200, 50), game);
            ActiveChunk = new Chunk(game, Player, "Chunks/TestChunk");
        }
        
        public void LoadContent(ContentManager content)
        {
            ActiveChunk.LoadContent(content);
        }

        public void Update(GameTime gameTime)
        {
            ActiveChunk.Update(gameTime);
            if (ChunkTrans)
            {
                Player.Update(gameTime, null);
            }
        }

        public void Draw(GameTime gameTime)
        {
            ActiveChunk.Draw(gameTime);
        }
    }
}

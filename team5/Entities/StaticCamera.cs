using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace team5
{
    class StaticCamera : Entity, IEnemy
    {
        protected AnimatedSprite Sprite;
        protected ConeEntity ViewCone;
        
        public StaticCamera(Vector2 position, float degrees, Game1 game) : base(game)
        {
            Position = position;
            Sprite = new AnimatedSprite(null, game, new Vector2(Chunk.TileSize, Chunk.TileSize));
            ViewCone = new ConeEntity(game);
            ViewCone.FromDegrees(degrees, 33);
            ViewCone.Radius = Chunk.TileSize * 999;
            ViewCone.UpdatePosition(position);
        }
        
        public override void LoadContent(ContentManager content)
        {
            Sprite.Texture = Game.TextureCache["camera"];
            Sprite.Add("idle", 0, 4, 1.0);
        }
        
        public override void Update(Chunk chunk)
        {
            float dt = Game1.DeltaT;
            Sprite.Update(dt);
            ViewCone.UpdatePosition(Position);
            ViewCone.Update(chunk);
        }
        
        public override void Draw()
        {
            ViewCone.Draw();
            Game.Transforms.Push();
            Game.Transforms.Rotate(ViewCone.Middle);
            Sprite.Draw(Position);
            Game.Transforms.Pop();
        }

        public override RectangleF GetBoundingBox()
        {
            throw new System.NotImplementedException();
        }

        public override bool Collide(Entity source, float timestep, out int direction, out float time, out bool corner)
        {
            direction = 0;
            time = 0;
            corner = false;
            return false;
        }

        public override bool Contains(Vector2 point)
        {
            return false;
        }

        public void HearSound(Vector2 Position, float volume, Chunk chunk)
        {
        }

        public void Alert(Vector2 Position, Chunk chunk)
        {
            ViewCone.SetColor(ConeEntity.AlertColor);
        }

        public void ClearAlarm(Chunk chunk)
        {
            ViewCone.SetColor(ConeEntity.ClearColor);
        }
    }
}

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace team5
{
    public class Particle
    {
        public float dt = Game1.DeltaT;
        public Texture2D Texture { get; set; }
        public Vector2 Position;
        public Vector2 Velocity;
        public float Angle;
        public float AngularVelocity;
        public Color Color;
        public float Size;
        public float TTL;
        private const float Gravity = 30; 

        public Particle(Texture2D texture, Vector2 position, Vector2 velocity,
            float angle, float angularVelocity, Color color, float size, float ttl)
        {
            Texture = texture;
            Position = position;
            Velocity = velocity;
            Angle = angle;
            AngularVelocity = angularVelocity;
            Color = color;
            Size = size;
            TTL = ttl;
        }

        public void Update()
        {
            TTL -= dt;
            Velocity.Y += dt * Gravity;
            Position += Velocity;
            Angle += AngularVelocity;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle sourceRectangle = new Rectangle(0, 0, Texture.Width, Texture.Height);
            Vector2 origin = new Vector2(Texture.Width / 2, Texture.Height / 2);

            spriteBatch.Draw(Texture, Position, sourceRectangle, Color,
                Angle, origin, Size, SpriteEffects.None, 0f);
        }

    }
}

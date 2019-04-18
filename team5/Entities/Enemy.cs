using Microsoft.Xna.Framework;

namespace team5
{
    interface IEnemy {
        void HearSound(Vector2 Position, float volume);
        void Alert(Vector2 Position, Chunk chunk);
    }
}

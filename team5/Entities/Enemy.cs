using Microsoft.Xna.Framework;

namespace team5
{
    interface IEnemy {
        void HearSound(Vector2 Position, float volume, Chunk chunk);
        void Alert(Vector2 Position, Chunk chunk);
        void ClearAlarm(Chunk chunk);
    }
}

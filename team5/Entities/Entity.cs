using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5
{
    abstract class Entity:GameObject
    {
        public Vector2 Position = new Vector2();

        public Entity(Game1 game):base(game)
        {
        }

        /// <summary>
        ///   Returns a Bounding Box over the Entity. The entire Entity must fit inside the Box.
        /// </summary>
        public abstract RectangleF GetBoundingBox();

        /// <summary>
        ///   Tests collision from a given entity. In general, only the source entity's motion will be considered.
        /// </summary>
        /// <param name="source">The other entity with which to test collsion</param>
        /// <param name="timestep">A copy of the timestep size</param>
        /// <param name="direction">Returns the direction of collisions. Mostly used by Box Entities, will typically be zero otherwise.</param>
        /// <param name="time">For moving Entities, returns at which point in time the collision occured. Specified in multiples of the timestep (will always be in [0,1])</param>
        /// <param name="corner">In the case of Box entities, returns true iff the collision was exactly on a corner. Used to allow smooth movement across multiple boxes.</param>
        public abstract bool Collide(Entity source, float timestep, out int direction, out float time, out bool corner);

        /// <summary>
        ///   Tests if a point is inside an Entity.
        /// </summary>
        public abstract bool Contains(Vector2 point);
    }
}

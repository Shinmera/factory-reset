using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace team5
{
	abstract class BoxEntity : Entity
	{
		protected Point size;

		public BoxEntity(Game1 game):base(game)
		{
		}

		public override Rectangle getBoundingBox()
		{
			return new Rectangle((int)Math.Floor(position.X), (int)Math.Floor(position.Y), size.X, size.Y);
		}


		//Standard swept AABB
		//Source: https://www.gamedev.net/articles/programming/general-and-gameplay-programming/swept-aabb-collision-detection-and-response-r3084
		public override bool collide(Entity source, float timestep, out int direction, out float time)
		{
			if (source is BoxEntity)
				return collideBox((BoxEntity)source, timestep, out direction, out time, getBoundingBox());
			else
			{
				direction = -1;
				time = -1;
				return false;
			}
		}

		public static bool collideBox(BoxEntity source, float timestep, out int direction, out float time, Rectangle target)
		{
			Rectangle motionBB;


			Rectangle sourceBB = source.getBoundingBox();
			Vector2 sourceMotion = source.velocity * timestep;

			motionBB.X = sourceBB.X + (int)Math.Floor(Math.Min(0.0, sourceMotion.X));
			motionBB.Y = sourceBB.Y + (int)Math.Floor(Math.Min(0.0, sourceMotion.Y));
			motionBB.Width = sourceBB.Width + (int)Math.Ceiling(Math.Max(0.0, sourceMotion.X));
			motionBB.Height = sourceBB.Height + (int)Math.Ceiling(Math.Max(0.0, sourceMotion.Y));

			if (!motionBB.Intersects(target))
			{
				direction = -1;
				time = -1;
				return false;
			}

			Vector2 InvEntry;
			Vector2 InvExit;

			if (sourceMotion.X > 0.0f)
			{
				InvEntry.X = target.X - (sourceBB.X + sourceBB.Width);
				InvExit.X = (target.X + target.Width) - sourceBB.X;
			}
			else
			{
				InvEntry.X = (target.X + target.Width) - sourceBB.X;
				InvExit.X = target.X - (sourceBB.X + sourceBB.Width);
			}

			if (sourceMotion.Y > 0.0f)
			{
				InvEntry.Y = target.Y - (sourceBB.Y + sourceBB.Width);
				InvExit.Y = (target.Y + target.Width) - sourceBB.Y;
			}
			else
			{
				InvEntry.Y = (target.Y + target.Width) - sourceBB.Y;
				InvExit.Y = target.Y - (sourceBB.Y + sourceBB.Width);
			}

			Vector2 Entry;
			Vector2 Exit;

			if (sourceMotion.X == 0.0f)
			{
				Entry.X = float.NegativeInfinity;
				Exit.X = float.PositiveInfinity;
			}
			else
			{
				Entry.X = InvEntry.X / sourceMotion.X;
				Exit.X = InvExit.X / sourceMotion.X;
			}

			if (sourceMotion.Y == 0.0f)
			{
				Entry.Y = float.NegativeInfinity;
				Exit.Y = float.PositiveInfinity;
			}
			else
			{
				Entry.Y = InvEntry.Y / sourceMotion.Y;
				Exit.Y = InvExit.Y / sourceMotion.Y;
			}

			float entryTime = Math.Max(Entry.X, Entry.Y);
			float exitTime = Math.Min(Exit.X, Exit.Y);

			if (entryTime > exitTime || Entry.X < 0.0f && Entry.Y < 0.0f || Entry.X > 1 || Entry.Y > 1)
			{
				direction = 0;
				time = 0;
				return false;
			}
			else
			{
				// calculate normal of collided surface
				if (Entry.X > Entry.Y)
				{
					if (sourceMotion.X < 0.0f)
					{
						direction = Chunk.LEFT;
					}
					else
					{
						direction = Chunk.RIGHT;
					}
				}
				else
				{
					if (sourceMotion.Y < 0.0f)
					{
						direction = Chunk.UP;
					}
					else
					{
						direction = Chunk.DOWN;
					}
				}

				// return the time of collision
				time = entryTime;
				return true;
			}

		}
	}
}

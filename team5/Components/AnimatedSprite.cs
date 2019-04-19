using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace team5
{   
    //Basic animation class for sprites
    public class AnimatedSprite
    {
        private struct Animation
        {
            public string Name;
            /// <summary>The index of the starting frame of this animation.</summary>
            public int Start;
            /// <summary>The index of the frame after the end of this animation.</summary>
            public int End;
            /// <summary>The duration in seconds for a single frame at standard speed.</summary>
            public float FrameTime;
            /// <summary>The index of the first frame after a loop.</summary>
            public int LoopStart;
            /// <summary>The next animation to transition to when this one is done, or -1 to loop the animation</summary>
            public int Next;
            
            public Animation(string name, int start, int stop, double duration, int loopStart = -1, int next = -1)
            {
                Name = name;
                Start = start;
                End = stop;
                FrameTime = (float)(duration/(stop-start));
                LoopStart = (loopStart < 0)? start : loopStart;
                Next = next;
            }
        }

        /// <summary>The Sprite Map of the animation</summary>
        public Texture2D Texture { get; set; }
        /// <summary>A reference to the singleton parent game</summary>
        private Game1 Game;

        public float Speed = 1;
        /// <summary>The direction the sprite is currently facing</summary>
        public float Direction = 1;
        /// <summary>Time passed since last animation frame</summary>
        private float TimeAccumulator = 0;
        /// <summary>The current Frame being rendered</summary>
        public int Frame = 0;
        /// <summary>The direction of frame playback</summary>
        public int FrameStep = +1;
        /// <summary>The animation currently being run</summary>
        private Animation Anim;
        /// <summary>A List of animations that can be rendered by this sprite</summary>
        private List<Animation> Animations = new List<Animation>();
        /// <summary>The size of each animation frame</summary>
        public Vector2 FrameSize { get; }

        public AnimatedSprite(Texture2D texture, Game1 game) 
            : this(texture, game, new Vector2(Chunk.TileSize, Chunk.TileSize))
        { }

        public AnimatedSprite(Texture2D texture, Game1 game, Vector2 frameSize)
        {
            Texture = texture;
            Game = game;
            FrameSize = frameSize;
        }

        /// <summary>
        ///   Add an animation for this sprite to render.
        /// </summary>
        public void Add(string name, int start, int stop, double duration, int loopStart = -1, int next = -1)
        {
            Animations.Add(new Animation(name, start, stop, duration, loopStart, next));
            if(Animations.Count == 1) Anim = Animations[0];
        }

        /// <summary>
        ///   Switch to a specific animation by ID
        /// </summary>
        public void Play(int idx)
        {
            Animation next = Animations[idx];
            if(!EqualityComparer<Animation>.Default.Equals(Anim, next))
            {
                Anim = next;
                Frame = Anim.Start;
                TimeAccumulator = 0;
                FrameStep = +1;
            }
        }

        /// <summary>
        ///   Switch to a specific animation by Name
        /// </summary>
        public void Play(string name)
        {
            Play(Animations.FindIndex(a => a.Name.Equals(name)));
        }

        /// <summary>
        ///   Restart from the first frame of the animation
        /// </summary>
        public void Reset()
        {
            Frame = Anim.Start;
        }

        /// <summary>
        ///   Call to Draw the Sprite in its current state.
        /// </summary>
        public void Draw(Vector2 position)
        {
            Game.Transforms.Push();
            Game.Transforms.Scale(Direction, 1);
            Game.Transforms.Translate(position);
            int width = (int)FrameSize.X;
            int height = (int)FrameSize.Y;

            Vector4 source = new Vector4(width * Frame, 0, width, height);
            Game.SpriteEngine.Draw(Texture, source);
            Game.Transforms.Pop();
        }

        public void Draw()
        {
            Draw(new Vector2(0, 0));
        }

        public void Update(float dt)
        {
            // Calculate number of frames to advance
            TimeAccumulator += dt;
            int frameInc = (int)System.Math.Floor(TimeAccumulator / Anim.FrameTime);
            TimeAccumulator = TimeAccumulator % Anim.FrameTime;
            // Step frames and check bounds
            Frame += frameInc * FrameStep;
            if(Anim.End <= Frame)
            {
                int oversteppedFrames = Anim.End - Frame;
                // Handle animation transition logic.
                if(Anim.Next == -1)
                    Frame = Anim.LoopStart + oversteppedFrames;
                else
                {
                    Anim = Animations[Anim.Next];
                    Frame = Anim.Start + oversteppedFrames;
                }
            } else if(Frame < Anim.Start)
            {
                int oversteppedFrames = Anim.Start - Frame;
                if(Anim.Next == -1)
                    Frame = Anim.End - oversteppedFrames;
                else
                {
                    FrameStep = +1;
                    Anim = Animations[Anim.Next];
                    Frame = Anim.Start + oversteppedFrames;
                }
            }
        }
    }
}

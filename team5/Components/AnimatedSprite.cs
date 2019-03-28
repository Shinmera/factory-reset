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
            
            public Animation(string name, int start, int frames, double duration, int loopOffset = 0, int next = -1)
            {
                Name = name;
                Start = start;
                End = start+frames;
                FrameTime = (float)(duration/frames);
                LoopStart = start+loopOffset; 
                Next = next;
            }
        }
        
        public Texture2D Texture { get; set; }
        private SpriteEngine Engine;
        public float Speed = 1;
        private float TimeAccumulator = 0;
        private int Frame = 0;
        private Animation Anim;
        private List<Animation> Animations = new List<Animation>();
        private Vector2 FrameSize;

        public AnimatedSprite(Texture2D texture, SpriteEngine engine) : this(texture, engine, new Vector2(Chunk.TileSize, Chunk.TileSize))
        { }

        public AnimatedSprite(Texture2D texture, SpriteEngine engine, Vector2 frameSize)
        {
            Texture = texture;
            Engine = engine;
            FrameSize = frameSize;
        }
        
        public void Add(string name, int start, int frames, double duration, int loopOffset = 0, int next = -1)
        {
            Animations.Add(new Animation(name, start, frames, duration, loopOffset, next));
            if(Animations.Count == 1) Anim = Animations[0];
        }
        
        public void Play(int idx)
        {
            if(!EqualityComparer<Animation>.Default.Equals(Anim, Animations[idx]))
            {
                Anim = Animations[idx];
                Frame = Anim.Start;
                TimeAccumulator = 0;
            }
        }
        
        public void Play(string name)
        {
            Play(Animations.FindIndex(a => a.Name.Equals(name)));
        }

        public void Draw(Vector2 position)
        {
            int width = (int)FrameSize.X;
            int height = (int)FrameSize.Y;

            Rectangle source = new Rectangle(width * Frame, 0, width, height);
            Engine.Draw(Texture, source, position);
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
            Frame += frameInc;
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
            }
        }
    }
}

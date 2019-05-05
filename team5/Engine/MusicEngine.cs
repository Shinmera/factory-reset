using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using System;

namespace team5
{   
    public class MusicEngine
    {
        public static float Volume = 0.5f;
        private readonly Dictionary<string, Track> SoundCache = new Dictionary<string, Track>();
        private readonly Stack<Track> PlayStack = new Stack<Track>();
        private readonly Game1 Game;
        private ContentManager Content;
        
        public class Track : IDisposable
        {
            private readonly MusicEngine Engine;
            public readonly string Name;
            public readonly Song Song;
            public readonly TimeSpan LoopTime;
            private TimeSpan PauseTime;
            
            private bool IsStopped = true;
            public bool Stopped { 
                get { return IsStopped; }
                set {
                    if(value == IsStopped) return;
                    IsStopped = value;
                    if (value){
                        // Check if top
                        MediaPlayer.Stop();
                        Engine.PlayStack.Pop();
                        if(Engine.PlayStack.TryPeek(out var top))
                           top.Play();
                    }
                    else
                        Play();
                }
            }

            private bool IsPaused = false;
            public bool Paused { 
                get { return IsPaused; }
                set {
                    IsPaused = value;
                    if(value)
                    {
                        PauseTime = MediaPlayer.PlayPosition;
                        MediaPlayer.Pause();
                    }
                    else
                    {
                        MediaPlayer.Resume();
                    }
                }
            }
            
            public Track(MusicEngine engine, string name, Song song, float loopTime = 0)
            {
                this.Engine = engine;
                this.Name = name;
                this.Song = song;
                if(0 < loopTime)
                    this.LoopTime = new TimeSpan((int)(loopTime*1e-7));
            }
            
            public void Play()
            {
                if (Engine.PlayStack.TryPeek(out var top))
                {
                     if (top != this)
                    {
                        top.Paused = true;
                        Engine.PlayStack.Push(this);
                    }
                }
                else
                    Engine.PlayStack.Push(this);
                IsStopped = false;
                IsPaused = false;
                if (PauseTime == null)
                    MediaPlayer.Play(Song);
                else
                    MediaPlayer.Play(Song, PauseTime);
                MediaPlayer.Volume = MusicEngine.Volume;
            }
            
            public void Update()
            {
                if(Paused) return;
                
                if(Engine.PlayStack.Peek() == this && Song.Duration <= MediaPlayer.PlayPosition)
                {
                    if(LoopTime != null)
                        MediaPlayer.Play(Song, LoopTime);
                    else
                        Stopped = true;
                }
            }
            
            public void Dispose()
            {
                Song.Dispose();
            }
        }
        
        public MusicEngine(Game1 game)
        {
            Game = game;
        }
        
        public void LoadContent(ContentManager content)
        {
            Content = content;
        }
        
        public void UnloadContent()
        {
            Clear();
            foreach(var entry in SoundCache)
                entry.Value.Dispose();
            SoundCache.Clear();
        }
        
        public void Load(string name)
        {
            Load(name, name, 0);
        }
        
        public void Load(string name, string file, float loopTime)
        {
            if(!SoundCache.ContainsKey(name)){
                Game1.Log("MusicEngine","Loading {0}/{1}", name, file);
                Song song = Content.ReadAsset<Song>("Music/"+file);
                SoundCache.Add(name, new Track(this, name, song, loopTime));
                // Callback to advance load screen
                Game.AdvanceLoad();
            }
        }
        
        public Track Play(string name)
        {
            Track track = SoundCache[name];
            track.Play();
            return track;
        }
        
        public void Clear()
        {
            PlayStack.Clear();
            MediaPlayer.Stop();
        }
        
        public void Update()
        {
            foreach(var track in PlayStack)
                track.Update();
        }
        
        public void Stop()
        {
            if(PlayStack.TryPeek(out var top))
                top.Stopped = true;
        }
        
        public bool Paused
        {
            get { return PlayStack.Count == 0 || PlayStack.Peek().Paused; }
            set {
                if(PlayStack.TryPeek(out var top))
                     top.Paused = value;
            }
        }
    }
}

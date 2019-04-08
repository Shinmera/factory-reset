using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.Linq;

namespace team5
{
    public class SoundEngine
    {
        private Game1 Game;
        private ContentManager Content;
        private AudioListener Listener;
        private readonly Dictionary<string, SoundEffect> SoundCache = new Dictionary<string, SoundEffect>();
        private readonly List<Sound> ActiveSounds = new List<Sound>();
        
        public class Sound
        {
            private SoundEngine Parent;
            readonly SoundEffect Effect;
            readonly SoundEffectInstance Instance;
            readonly AudioEmitter Emitter;

            public Sound(SoundEngine parent, SoundEffect effect, Vector2 position)
            {
                Parent = parent;
                Effect = effect;
                Instance = effect.CreateInstance();
                Emitter = new AudioEmitter();
                Position = position;
                Instance.Play();
                Parent.ActiveSounds.Add(this);
            }
            
            public bool Stopped
            {
                get { return Instance.State == SoundState.Stopped; }
                set {
                    if(value)
                        Instance.Stop();
                    else if(Stopped)
                    {
                        Instance.Play();
                        Parent.ActiveSounds.Add(this);
                    }
                }
            }
            
            public bool Paused
            {
                get { return Instance.State == SoundState.Paused; }
                set {
                    if(value) Instance.Pause();
                    else if(Paused) Instance.Resume();
                }
            }
            
            public bool Loop
            {
                get { return Instance.IsLooped; }
                set { Instance.IsLooped = value; }
            }
            
            public Vector2 Position
            {
                get { return new Vector2(Emitter.Position.X, Emitter.Position.Y); }
                set { 
                    Emitter.Position = new Vector3(value.X, value.Y, 0);
                    Update();
                }
            }
            
            public void Update()
            {
                Instance.Apply3D(Parent.Listener, Emitter);
            }
        }
        
        public SoundEngine(Game1 game)
        {
            Game = game;
            SoundEffect.DistanceScale = 2000f;
        }
        
        public void LoadContent(ContentManager content)
        {
            Content = content;
            Listener = new AudioListener();
            foreach(var sound in SoundCache.Keys.ToList())
            {
                if(SoundCache[sound] == null){
                    SoundCache[sound] = content.Load<SoundEffect>("Sounds/"+sound);
                }
            }
        }
        
        public void RequestForLoad(string effect)
        {
            if(!SoundCache.ContainsKey(effect))
                SoundCache.Add(effect, null);
        }
        
        public bool Paused
        {
            set {
                ActiveSounds.ForEach(sound => { sound.Paused = value; });
            }
        }
        
        public Sound Play(string effect, Vector2 position)
        {
            return new Sound(this, SoundCache[effect], position);
        }
        
        public bool Listen(Vector2 position, out Vector2 source)
        {
            // FIXME: Implement sound hearing
            source = position;
            return false;
        }
        
        public void Update(Vector2 listener)   
        {
            Listener.Position = new Vector3(listener.X, listener.Y, 100);
            ActiveSounds.RemoveAll(sound => {
                    sound.Update();
                    return sound.Stopped;
                });
        }
    }
}

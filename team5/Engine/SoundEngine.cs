using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System;

namespace team5
{   
    /// <summary>
    ///   See http://sol.gfxile.net/soloud/concepts3d.html#attenuation
    ///   See https://docs.unrealengine.com/en-US/Engine/Audio/DistanceModelAttenuation
    /// </summary>
    using Attenuation = Func<float, float, float, float, float>;
    public sealed class Attenuations{
        /// <summary>
        ///   Constant volume, meaning no distance attenuation.
        /// </summary>
        public static Attenuation None = (dist, min, max, roll)=>
            1.0f;
        
        /// <summary>
        ///   Inverse distance attenuation.
        ///   The higher the rolloff, the steeper the volume drop.
        ///   The rolloff factor should be above 1.
        /// </summary>
        public static Attenuation Inverse = (dist, min, max, roll)=>
            min / (min+roll*(dist-min));
        
        /// <summary>
        ///   Linear distance attenuation.
        ///   The rolloff simply sets the maximal reduction in volume.
        ///   The rolloff factor should be in [0, 1].
        /// </summary>
        public static Attenuation Linear = (dist, min, max, roll)=>
            1.0f - roll * (dist-min) / (max-min);
        
        /// <summary>
        ///   Exponential distance attenuation.
        ///   The rolloff is the exponent factor.
        ///   The rolloff factor should be above 1.
        /// </summary>
        public static Attenuation Exponential = (dist, min, max, roll)=>
            (float)Math.Pow(dist / min, -roll);
        
        /// <summary>
        ///   Inverse exponential distance attenuation.
        ///   The higher the rolloff factor, the steeper the curve towards max.
        ///   The rolloff factor should be above 1.
        /// </summary>
        public static Attenuation InverseExponential = (dist, min, max, roll)=>
            1.0f - (float)Math.Pow((dist-min) / (max-min), roll);
        
        /// <summary>
        ///   Inverse Logarithmic distance attenuation.
        ///   The higher the rolloff factor, the steeper the curve.
        ///   The rolloff factor should be above 1.
        /// </summary>
        public static Attenuation InverseLogarithmic = (dist, min, max, roll)=>
            1.0f / (float)Math.Log(Math.Max(0.0000000001, Math.Min(0.9999999, (dist-min)/(max-min))), roll);
    }
    
    public class SoundEngine
    {
        /// <summary>The range in which there is no panning and no attenuation.</summary>
        public const float DeadZone = 2 * Chunk.TileSize;
        /// <summary>The range in which sound is panned. Note the panning is always linear.</summary>
        public const float MidRange = 10 * Chunk.TileSize;
        /// <summary>The range up to which sound is audible.</summary>
        public const float AudibleDistance = 50 * Chunk.TileSize;
        /// <summary>The rolloff factor for the attenuation function.</summary>
        public const float Rolloff = 1f;
        /// <summary>The attenuation function used for sound distance volume scaling.</summary>
        public static readonly Attenuation Attenuation = Attenuations.InverseExponential;

        private Vector2 Listener;
        
        /// <summary>Master volume adjustment</summary>
        public static float Volume = 0.6f;
        
        private Game1 Game;
        private ContentManager Content;
        private readonly Dictionary<string, SoundPool> SoundCache = new Dictionary<string, SoundPool>();
        private readonly List<Sound> ActiveSounds = new List<Sound>();
        
        private class SoundPool : IDisposable
        {
            public readonly string Name;
            public readonly SoundEffect[] Effects;
            private int LastIndex = -1;
            
            public SoundPool(string name, SoundEffect[] effects)
            {
                Name = name;
                Effects = effects;
            }
            
            public SoundEffect Next()
            {
                // Cycle through effects to avoid repetition.
                LastIndex = (LastIndex+1) % Effects.Length;
                return Effects[LastIndex];
            }
            
            public void Dispose()
            {
                foreach(var sound in Effects){
                    Game1.Log("SoundEngine","Unloading {0}/{1}", Name, sound);
                    sound.Dispose();
                }
            }
        }
        
        public class Sound : IDisposable
        {
            private SoundEngine SoundEngine;
            readonly SoundEffect Effect;
            readonly SoundEffectInstance Instance;
            public float RelativeVolume;
            public Vector2 Position = new Vector2(0,0);

            public Sound(SoundEngine soundEngine, SoundEffect effect, Vector2 position, float relativeVolume=1, bool loop=false)
            {
                RelativeVolume = relativeVolume;
                SoundEngine = soundEngine;
                Effect = effect;
                Instance = effect.CreateInstance();
                Position = position;
                Loop = loop;
                Update();
                Instance.Play();
                SoundEngine.ActiveSounds.Add(this);
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
                        SoundEngine.ActiveSounds.Add(this);
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
            
            public float getRelativeVolumeAt(Vector2 position)
            {
                float clamp(float l, float x, float u) { return (x < l) ? l : (u < x) ? u : x; }
                Vector2 direction = Position - position;
                float distance = clamp(DeadZone, direction.Length(), AudibleDistance);
                return clamp(0, Attenuation(distance, DeadZone, AudibleDistance, Rolloff), 1) * RelativeVolume;
            }

            public void Update()
            {
                float clamp(float l, float x, float u) { return (x < l) ? l : (u < x) ? u : x; }
                
                Vector2 direction = Position - SoundEngine.Listener;
                float distance = clamp(DeadZone, direction.Length(), AudibleDistance);
                float panFactor = clamp(0, (distance-DeadZone)/(MidRange-DeadZone), 1);
                float attenuation = clamp(0, Attenuation(distance, DeadZone, AudibleDistance, Rolloff), 1);
                Instance.Volume = distance >= AudibleDistance ? 0 : attenuation * SoundEngine.Volume * Game1.Volume * RelativeVolume;
                Instance.Pan = Math.Sign(direction.X)*panFactor;
            }
            
            public void Dispose()
            {
                Instance.Dispose();
            }
        }
        
        public SoundEngine(Game1 game)
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
            foreach (var entry in SoundCache)
                entry.Value.Dispose();
            SoundCache.Clear();
        }
        
        public void Load(string effect)
        {
            Load(effect, effect);
        }
        
        public void Load(string name, params object[] assets)
        {
            if(!SoundCache.ContainsKey(name)){
                SoundEffect[] effects = new SoundEffect[assets.Length];
                for(int i=0; i<effects.Length; ++i)
                {
                    Game1.Log("SoundEngine","Loading {0}/{1}", name, assets[i]);
                    effects[i] = Content.ReadAsset<SoundEffect>("Sounds/"+assets[i]);
                    // Callback to advance load screen
                    Game.AdvanceLoad();
                }
                SoundCache.Add(name, new SoundPool(name, effects));
            }
        }
        
        public bool Paused
        {
            set {
                ActiveSounds.ForEach(sound => { sound.Paused = value; });
            }
        }
        
        public Sound Play(string effect, float volume=1, bool loop=false)
        {
            return Play(effect, Listener, volume, loop);
        }
        
        public Sound Play(string effect, Vector2 position, float volume=1, bool loop=false)
        {
            var sound = new Sound(this, SoundCache[effect].Next(), position, volume, loop);
            return sound;
        }
        
        public bool Listen(Vector2 position, out Vector2 source)
        {
            source = position;
            return false;
        }
        
        public void Clear()
        {
            foreach(var sound in ActiveSounds){
                sound.Stopped = true;
                sound.Dispose();
            }
            ActiveSounds.Clear();
        }
        
        public void Update(Vector2 listener)   
        {
            Listener.X = listener.X;
            Listener.Y = listener.Y;
            ActiveSounds.RemoveAll(sound => {
                    sound.Update();
                    if(sound.Stopped)
                    {
                        sound.Dispose();
                        return true;
                    }
                    return false;
                });
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
        public static readonly Attenuation Attenuation = Attenuations.Exponential;
        
        public Vector2 Listener;
        
        private Game1 Game;
        private ContentManager Content;
        private readonly Dictionary<string, SoundEffect> SoundCache = new Dictionary<string, SoundEffect>();
        private readonly List<Sound> ActiveSounds = new List<Sound>();
        
        public class Sound
        {
            private SoundEngine SoundEngine;
            readonly SoundEffect Effect;
            readonly SoundEffectInstance Instance;
            public Vector2 Position = new Vector2(0,0);

            public Sound(SoundEngine soundEngine, SoundEffect effect)
            {
                SoundEngine = soundEngine;
                Effect = effect;
                Instance = effect.CreateInstance();
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
            
            public void Update()
            {
                float clamp(float l, float x, float u) { return (x < l) ? l : (u < x) ? u : x; }
                
                Vector2 direction = Position - SoundEngine.Listener;
                float distance = clamp(DeadZone, direction.Length(), AudibleDistance);
                float panFactor = clamp(0, (distance-DeadZone)/(MidRange-DeadZone), 1);
                float attenuation = clamp(0, Attenuation(distance, DeadZone, AudibleDistance, Rolloff), 1);
                
                Instance.Volume = attenuation;
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
        
        public void Load(string effect)
        {
            if(!SoundCache.ContainsKey(effect))
                SoundCache.Add(effect, Content.Load<SoundEffect>("Sounds/"+effect));
        }
        
        public bool Paused
        {
            set {
                ActiveSounds.ForEach(sound => { sound.Paused = value; });
            }
        }
        
        public Sound Play(string effect)
        {
            return new Sound(this, SoundCache[effect]);
        }
        
        public Sound Play(string effect, Vector2 position)
        {
            return new Sound(this, SoundCache[effect]){
                Position = position
            };
        }
        
        public bool Listen(Vector2 position, out Vector2 source)
        {
            // FIXME: Implement sound hearing
            source = position;
            return false;
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

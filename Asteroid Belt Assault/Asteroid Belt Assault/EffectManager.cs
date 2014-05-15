using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;
using ProjectMercury.Renderers;

namespace Asteroid_Belt_Assault
{
    public class ShaderParameters
    {
        public Dictionary<string, object> fxparams;

        public ShaderParameters()
        {
            fxparams = new Dictionary<string, object>();
        }

        public void SetValue (string name, float value) {
            fxparams[name] = value;
        }

        public void SetValue (string name, Vector2 value) {
            fxparams[name] = value;
        }
    }

    public class ShaderAnimation {
        public string shader;
        public ShaderParameters start;
        public ShaderParameters middle;
        public ShaderParameters end;
        private DateTime startTime, endTime;
        private int duration;
        private float progression;
        public bool Finished = false;

        public ShaderAnimation(string effectname, int duration)  // Duration in ms
        {
            shader = effectname;
            progression = 0;
            this.duration = duration;

            startTime = System.DateTime.Now;
            endTime = startTime.AddMilliseconds(duration); // (duration * 1000);
        }
        
        public int Duration {
            get { return duration; }
            set { duration = value; if (duration <= 0) duration = 1; }
            }

        public void Update()
        {
            DateTime curTime = DateTime.Now;
            progression = (float)(curTime.Ticks - startTime.Ticks) / (float)(endTime.Ticks - startTime.Ticks);

            if (curTime > endTime)
                Finished = true;

            ShaderParameters sa, sb;

            sa = start;
            sb = end;

            if (end == null)
            {
                sb = start;
            }

            if (middle != null)
            {
                sa = start;
                sb = middle;

                if (progression > 0.5f)
                {
                    sa = middle;
                    sb = end;
                    progression -= 0.5f;
                }
            }

            progression *= 2f;

            if (!Finished)
            foreach (string key in start.fxparams.Keys) {

                Type fxtyp = sa.fxparams[key].GetType();

                if (fxtyp == typeof(float)) {
                    float value = (float)sa.fxparams[key];

                    if (sb != null)
                    if (sb.fxparams.ContainsKey(key))
                    {
                        value = MathHelper.Lerp(value, (float)sb.fxparams[key], progression); 
                    }

                    Effect eshader = EffectManager.Shader(shader);
                    eshader.Parameters[key].SetValue(value);

                }
                else if (fxtyp == typeof(Vector2))
                {
                    Vector2 value = (Vector2)sa.fxparams[key];

                    if (sb != null)
                    if (sb.fxparams.ContainsKey(key))
                    {
                        Vector2 endp = (Vector2)(sb.fxparams[key]);
                        value = new Vector2(MathHelper.Lerp(value.X, endp.X, progression), MathHelper.Lerp(value.Y, endp.Y, progression));

                        EffectManager.Shader(shader).Parameters[key].SetValue(value);
                    }
                }

            }
        }
    }

    static class EffectManager
    {
        static ContentManager pContent;
        static Renderer pRenderer;

        private static Dictionary<string, ParticleEffect> effects;
        private static Dictionary<string, Effect> shaders;
        public static List<ShaderAnimation> animations;

        public static void Initialize (GraphicsDeviceManager graphics, ContentManager Content)
        {
            pContent = Content;
            effects = new Dictionary<string, ParticleEffect>();
            shaders = new Dictionary<string, Microsoft.Xna.Framework.Graphics.Effect>();
            animations = new List<ShaderAnimation>();

            pRenderer = new SpriteBatchRenderer
            {               
                GraphicsDeviceService = graphics
            };
        }

        private static void LoadEffect(string effectname)
        {
            effects.Add(effectname, new ParticleEffect());

            effects[effectname] = pContent.Load<ParticleEffect>(@"EffectLibrary\" + effectname);
            effects[effectname].LoadContent(pContent);
            effects[effectname].Initialise();

        }

        private static void LoadShader(string shadername)
        {
            Effect fx = pContent.Load<Effect>(@"Shaders\" + shadername);
            shaders.Add(shadername, fx);


        }

        public static void TriggerAnimation(string shader, int duration, ShaderParameters start, ShaderParameters middle, ShaderParameters end)
        {
            bool exists = false;
            foreach (ShaderAnimation ani in animations)
            {
                if (ani.shader == shader)
                {
                    exists = true;
                }
            }

            if (!exists)
            {
                ShaderAnimation animation = new ShaderAnimation(shader, duration);
                animation.start = start;
                animation.middle = middle;
                animation.end = end;

                try
                {
                    animations.Add(animation);
                }
                catch
                {
                }
            }
            /*
            rippleEffect.Parameters["center"].SetValue(new Vector2(0.8f, 0.8f));   // new Vector2(0.8f,0.8f)
            rippleEffect.Parameters["amplitude"].SetValue(0.01f);  // 0.1
            rippleEffect.Parameters["frequency"].SetValue(10);
            rippleEffect.Parameters["phase"].SetValue(floodWaveTicks);
            */
        }

        public static void LoadContent()
        {
            LoadEffect("BasicExplosion");
            LoadEffect("MeteroidCollision"); 
            LoadEffect("MeteroidExplode");
            LoadEffect("ShipSmokeTrail");
            LoadEffect("StarTrail");
            LoadEffect("MagicTrail");
            LoadEffect("StarFireImpact");
            LoadEffect("BasicExplosionWithHalo");
            LoadEffect("BasicExplosionWithTrails2");
            LoadEffect("Ship Cannon Fire");
            LoadEffect("Enemy Cannon Fire");
            LoadEffect("ShieldsUp");
            LoadEffect("ShieldBounce");
            LoadEffect("PulseTracker");

            pRenderer.LoadContent(pContent);


        }

        public static ParticleEffect Effect(string effectname)
        {
            return effects[effectname];
        }

        public static Effect Shader(string shadername)
        {
            return shaders[shadername];
        }

        public static void Update(GameTime gameTime)
        {
            float SecondsPassed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (string key in effects.Keys)
            {
                effects[key].Update(SecondsPassed);
            }

            for (int i = animations.Count-1; i >= 0; i--)
            {
                animations[i].Update();
                if (animations[i].Finished)
                {
                    animations.RemoveAt(i);
                }
            }
        }

        public static void Draw()
        {
            foreach (string key in effects.Keys)
            {
                pRenderer.RenderEffect(effects[key]);
            }
        }

    }
}

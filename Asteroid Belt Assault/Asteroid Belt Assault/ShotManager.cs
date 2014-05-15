using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Asteroid_Belt_Assault
{
    class ShotManager
    {
        public List<Sprite> Shots = new List<Sprite>();
        private Rectangle screenBounds;

        private static Texture2D Texture;
        private Rectangle InitialFrame;
        private static int FrameCount;
        private float shotSpeed;
        private static int CollisionRadius;

        private bool playerFired = false;

        public ShotManager(
            Texture2D texture,
            Rectangle initialFrame,
            int frameCount,
            int collisionRadius,
            float shotSpeed,
            Rectangle screenBounds)
        {
            Texture = texture;
            InitialFrame = initialFrame;
            FrameCount = frameCount;
            CollisionRadius = collisionRadius;
            this.shotSpeed = shotSpeed;
            this.screenBounds = screenBounds;
            
        }

        public void FireShot(
            Vector2 location,
            Vector2 velocity,
            bool playerFired)
        {
            Sprite thisShot = new Sprite(
                location,
                Texture,
                InitialFrame,
                velocity);

            thisShot.Velocity *= shotSpeed;
            this.playerFired = playerFired;

            Vector2 vec = thisShot.Velocity;
            vec.Normalize();

            thisShot.Rotation = (float)Math.Atan2(vec.Y, vec.X);

            for (int x = 1; x < FrameCount; x++)
            {
                thisShot.AddFrame(new Rectangle(
                    InitialFrame.X + (InitialFrame.Width * x),
                    InitialFrame.Y,
                    InitialFrame.Width,
                    InitialFrame.Height));
            }
            thisShot.CollisionRadius = CollisionRadius;
            Shots.Add(thisShot);

            if (playerFired)
            {

                SoundManager.PlayPlayerShot();
            }
            else
            {

                SoundManager.PlayEnemyShot();
            }
        }

        public void Update(GameTime gameTime)
        {
            for (int x = Shots.Count - 1; x >= 0; x--)
            {
                
                Shots[x].Update(gameTime);

                if (playerFired)
                    EffectManager.Effect("Enemy Cannon Fire").Trigger(Shots[x].Center + (-Shots[x].Velocity * 0.05f));
                else
                    EffectManager.Effect("Ship Cannon Fire").Trigger(Shots[x].Center);

                if (!screenBounds.Intersects(Shots[x].Destination))
                {
                    Shots.RemoveAt(x);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Sprite shot in Shots)
            {
                if (playerFired) 
                    shot.Draw(spriteBatch);
            }
        }

    
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BloomPostprocess;

namespace Asteroid_Belt_Assault
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        enum GameStates { TitleScreen, Playing, PlayerDead, GameOver };
        GameStates gameState = GameStates.TitleScreen;
        Texture2D titleScreen;
        Texture2D spriteSheet;
        Texture2D crosshairs;
        Texture2D Ship;

        List<StarField> starField;
        AsteroidManager asteroidManager;
        PlayerManager playerManager;
        EnemyManager enemyManager;
        ExplosionManager explosionManager;

        BloomComponent bloom;

        CollisionManager collisionManager;

        SpriteFont pericles14;

        private float playerDeathDelayTime = 10f;
        private float playerDeathTimer = 0f;
        private float titleScreenTimer = 0f;
        private float titleScreenDelayTime = 1f;

        private int playerStartingLives = 3;
        private Vector2 playerStartLocation = new Vector2(390, 550);
        private Vector2 scoreLocation = new Vector2(20, 10);
        private Vector2 livesLocation = new Vector2(20, 25);


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            titleScreen = Content.Load<Texture2D>(@"Textures\TitleScreen");
            spriteSheet = Content.Load<Texture2D>(@"Textures\spriteSheet");
            crosshairs = Content.Load<Texture2D>(@"Textures\crosshairs64");
            Ship = Content.Load<Texture2D>(@"Textures\Ship");

            Song song = Content.Load<Song>(@"Sounds\song");  // Put the name of your song in instead of "song_title"
            MediaPlayer.Play(song);

            bloom = new BloomComponent(this);
            bloom.Initialize();
         
            bloom.Settings = new BloomSettings(null, 0.25f, 4, 2, 1, 1.5f, 1);

            starField = new List<StarField>();

            Random rand = new Random();
            for (int i = 0; i < 5; i++)
            {
                int bright = rand.Next(32, 32 + i * 40);

                starField.Add(  new StarField(
                    this.Window.ClientBounds.Width,
                    this.Window.ClientBounds.Height,
                    (5-i)*50,
                    new Vector2(0, 10 + i * 100),
                    spriteSheet,
                    new Rectangle(5, 257, 8, 7), // new Rectangle(0, 450, 2, 2)
                    new Color(bright + rand.Next(0, 10), bright, bright + rand.Next(0, 50))));
            }

            asteroidManager = new AsteroidManager(
                10,
                spriteSheet,
                new Rectangle(0, 0, 50, 50),
                20,
                this.Window.ClientBounds.Width,
                this.Window.ClientBounds.Height);

            playerManager = new PlayerManager(
                Ship,    
                new Rectangle(0, 0, 64, 64),   
                1,
                new Rectangle(
                    0,
                    0,
                    this.Window.ClientBounds.Width,
                    this.Window.ClientBounds.Height));

            enemyManager = new EnemyManager(
                spriteSheet,
                new Rectangle(0, 200, 50, 50),
                6,
                playerManager,
                new Rectangle(
                    0,
                    0,
                    this.Window.ClientBounds.Width,
                    this.Window.ClientBounds.Height));

            explosionManager = new ExplosionManager(
                spriteSheet,
                new Rectangle(0, 100, 50, 50),
                3,
                new Rectangle(0, 450, 2, 2));

            collisionManager = new CollisionManager(
                asteroidManager,
                playerManager,
                enemyManager,
                explosionManager);

            SoundManager.Initialize(Content);

            pericles14 = Content.Load<SpriteFont>(@"Fonts\Pericles14");


            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private void resetGame()
        {
            playerManager.playerSprite.Location = playerStartLocation;
            foreach (Sprite asteroid in asteroidManager.Asteroids)
            {
                asteroid.Location = new Vector2(-500, -500);
            }
            enemyManager.Enemies.Clear();
            enemyManager.Active = true;
            playerManager.PlayerShotManager.Shots.Clear();
            enemyManager.EnemyShotManager.Shots.Clear();
            playerManager.Destroyed = false;
        }
        
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            bloom.Update(gameTime);

            switch (gameState)
            {
                case GameStates.TitleScreen:
                    titleScreenTimer +=
                        (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (titleScreenTimer >= titleScreenDelayTime)
                    {
                        if ((Keyboard.GetState().IsKeyDown(Keys.Space)) ||
                            (GamePad.GetState(PlayerIndex.One).Buttons.A ==
                            ButtonState.Pressed))
                        {
                            playerManager.LivesRemaining = playerStartingLives;
                            playerManager.PlayerScore = 0;
                            resetGame();
                            gameState = GameStates.Playing;
                        }
                    }
                    break;

                case GameStates.Playing:

                    for (int i = 0; i < starField.Count; i++)
                    {
                        starField[i].Update(gameTime);
                    }
                    asteroidManager.Update(gameTime);
                    playerManager.Update(gameTime);
                    enemyManager.Update(gameTime);
                    explosionManager.Update(gameTime);
                    collisionManager.CheckCollisions();

                    if (playerManager.Destroyed)
                    {
                        playerDeathTimer = 0f;
                        enemyManager.Active = false;
                        playerManager.LivesRemaining--;
                        if (playerManager.LivesRemaining < 0)
                        {
                            gameState = GameStates.GameOver;
                        }
                        else
                        {
                            gameState = GameStates.PlayerDead;
                        }
                    }

                    break;

                case GameStates.PlayerDead:
                    playerDeathTimer +=
                        (float)gameTime.ElapsedGameTime.TotalSeconds;

                    for (int i = 0; i < starField.Count; i++)
                    {
                        starField[i].Update(gameTime);
                    }
                    asteroidManager.Update(gameTime);
                    enemyManager.Update(gameTime);
                    playerManager.PlayerShotManager.Update(gameTime);
                    explosionManager.Update(gameTime);

                    if (playerDeathTimer >= playerDeathDelayTime)
                    {
                        resetGame();
                        gameState = GameStates.Playing;
                    }
                    break;

                case GameStates.GameOver:
                    playerDeathTimer +=
                        (float)gameTime.ElapsedGameTime.TotalSeconds;
                    for (int i = 0; i < starField.Count; i++)
                    {
                        starField[i].Update(gameTime);
                    }
                    asteroidManager.Update(gameTime);
                    enemyManager.Update(gameTime);
                    playerManager.PlayerShotManager.Update(gameTime);
                    explosionManager.Update(gameTime);
                    if (playerDeathTimer >= playerDeathDelayTime)
                    {
                        gameState = GameStates.TitleScreen;
                    }
                    break;

            }

            MouseState ms = Mouse.GetState();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            

           

           

            if (gameState == GameStates.TitleScreen)
            {
                GraphicsDevice.Clear(Color.Black);

                spriteBatch.Begin();
                spriteBatch.Draw(titleScreen,
                    new Rectangle(0, 0, this.Window.ClientBounds.Width,
                        this.Window.ClientBounds.Height),
                        Color.White);
                spriteBatch.End();
            }

            if ((gameState == GameStates.Playing) ||
                (gameState == GameStates.PlayerDead) ||
                (gameState == GameStates.GameOver))
            {
                spriteBatch.Begin();

                bloom.BeginDraw();

                GraphicsDevice.Clear(Color.Black);

                for (int i = 0; i < starField.Count; i++)
                {
                    starField[i].Draw(spriteBatch);
                }
                asteroidManager.Draw(spriteBatch);

                playerManager.PlayerShotManager.Draw(spriteBatch);
                explosionManager.Draw(spriteBatch);
                playerManager.Draw(spriteBatch);

                base.Draw(gameTime);
                spriteBatch.End();

                bloom.Draw(gameTime);

                spriteBatch.Begin();

                
                enemyManager.Draw(spriteBatch);
                
                
                
                

                spriteBatch.DrawString(
                    pericles14,
                    "Score: " + playerManager.PlayerScore.ToString(),
                    scoreLocation,
                    Color.White);

                if (playerManager.LivesRemaining >= 0)
                {
                    spriteBatch.DrawString(
                        pericles14,
                        "Ships Remaining: " +
                            playerManager.LivesRemaining.ToString(),
                        livesLocation,
                        Color.White);
                }

                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);
                
                MouseState ms = Mouse.GetState();
                spriteBatch.Draw(crosshairs, new Rectangle((int)ms.X - 32, (int)ms.Y - 32, 64, 64), new Rectangle(5* 64, 6 * 64, 64, 64), Color.Cyan);//fav:(1,4)(5,6)

                

                spriteBatch.End();
            }

            if ((gameState == GameStates.GameOver))
            {
                spriteBatch.Begin();

                spriteBatch.DrawString(
                    pericles14,
                    "G A M E  O V E R !",
                    new Vector2(
                        this.Window.ClientBounds.Width / 2 -
                          pericles14.MeasureString("G A M E  O V E R !").X / 2,
                        50),
                    Color.White);

                spriteBatch.End();

            }


            

            base.Draw(gameTime);
        }

    }
}

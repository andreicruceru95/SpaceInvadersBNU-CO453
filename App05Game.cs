﻿using App05MonoGame.Managers;
using App05MonoGame.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace App05MonoGame
{
    /// <summary>
    /// This game creates a variety of sprites as an example.  
    /// There is no game to play yet. The spaceShip and the 
    /// asteroid can be used for a space shooting game, the player, 
    /// the coin and the enemy could be used for a pacman
    /// style game where the player moves around collecting
    /// random coins and the enemy tries to catch the player.
    /// </summary>
    /// <authors>
    /// Derek Peacock & Andrei Cruceru
    /// </authors>
    public class App05Game : Game
    {
        // Constants

        public const int HD_Height = 720;
        public const int HD_Width = 1280;

        // Variables

        private GraphicsDeviceManager graphicsManager;
        private GraphicsDevice graphicsDevice;
        private SpriteBatch spriteBatch;

        private SpriteFont arialFont;
        private SpriteFont calibriFont;

        private Texture2D backgroundImage;
        private SoundEffect flameEffect;

        private CoinsController coinsController;

        private PlayerSprite shipSprite;
        private Sprite asteroidSprite;

        private AnimatedPlayer playerSprite;
        private AnimatedSprite coinSprite;
        private AnimatedSprite enemySprite;

        private int score;
        private int health;
        public App05Game()
        {
            graphicsManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            coinsController = new CoinsController();
        }

        /// <summary>
        /// Setup the game window size to 720P 1280 x 720 pixels
        /// Simple fixed playing area with no camera or scrolling
        /// </summary>
        protected override void Initialize()
        {
            graphicsManager.PreferredBackBufferWidth = HD_Width;
            graphicsManager.PreferredBackBufferHeight = HD_Height;

            graphicsManager.ApplyChanges();

            graphicsDevice = graphicsManager.GraphicsDevice;

            score = 0;
            health = 100;

            base.Initialize();
        }

        /// <summary>
        /// use Content to load your game images, fonts,
        /// music and sound effects
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            backgroundImage = Content.Load<Texture2D>(
                "backgrounds/green_background720p");

            // Load Music and SoundEffects

            SoundController.LoadContent(Content);
            SoundController.PlaySong("Adventure");
            flameEffect = SoundController.GetSoundEffect("Flame");

            // Load Fonts

            arialFont = Content.Load<SpriteFont>("fonts/arial");
            calibriFont = Content.Load<SpriteFont>("fonts/calibri");

            // suitable for asteroids type game

            SetupSpaceShip();
            SetupAsteroid();

            // animated sprites suitable for pacman type game

            SetupAnimatedPlayer();
            SetupEnemy();

            Texture2D coinSheet = Content.Load<Texture2D>("Actors/coin_copper");
            coinsController.CreateCoin(graphicsDevice, coinSheet);
        }

        /// <summary>
        /// This is a single image sprite that rotates
        /// and move at a constant speed in a fixed direction
        /// </summary>
        private void SetupAsteroid()
        {
            Texture2D asteroid = Content.Load<Texture2D>(
               "Actors/Stones2Filled_01");

            asteroidSprite = new Sprite(asteroid, 1200, 500);
            asteroidSprite.Direction = new Vector2(-1, 0);
            asteroidSprite.Speed = 100;

            asteroidSprite.Rotation = MathHelper.ToRadians(3);
            asteroidSprite.RotationSpeed = 2f;

        }

        /// <summary>
        /// This is a Sprite that can be controlled by a
        /// player using Rotate Left = A, Rotate Right = D, 
        /// Forward = Space
        /// </summary>
        private void SetupSpaceShip()
        {
            Texture2D ship = Content.Load<Texture2D>(
               "Actors/GreenShip");

            shipSprite = new PlayerSprite(ship, 200, 500);
            shipSprite.Direction = new Vector2(1, 0);
            shipSprite.Speed = 200;
            shipSprite.DirectionControl = DirectionControl.Rotational;
        }


        /// <summary>
        /// This is a Sprite with four animations for the four
        /// directions, up, down, left and right
        /// </summary>
        private void SetupAnimatedPlayer()
        {
            Texture2D sheet4x3 = Content.Load<Texture2D>("Actors/rsc-sprite-sheet1");

            AnimationManager manager = new AnimationManager(graphicsDevice, sheet4x3, 4, 3);

            string[] keys = new string[] { "Down", "Left", "Right", "Up" };
            manager.CreateAnimationGroup(keys);

            playerSprite = new AnimatedPlayer();
            manager.AppendAnimationsTo(playerSprite);

            playerSprite.CanWalk = true;
            playerSprite.Scale = 2.0f;

            playerSprite.Position = new Vector2(200, 200);
            playerSprite.Speed = 200;
            playerSprite.Direction = new Vector2(1, 0);

            playerSprite.Rotation = MathHelper.ToRadians(0);
            playerSprite.RotationSpeed = 0f;
        }

        /// <summary>
        /// This is an enemy Sprite with four animations for the four
        /// directions, up, down, left and right.  Has no intelligence!
        /// </summary>
        private void SetupEnemy()
        {
            Texture2D sheet4x3 = Content.Load<Texture2D>("Actors/rsc-sprite-sheet3");

            AnimationManager manager = new AnimationManager(graphicsDevice, sheet4x3, 4, 3);

            string[] keys = new string[] { "Down", "Left", "Right", "Up" };

            manager.CreateAnimationGroup(keys);

            enemySprite = new AnimatedSprite();
            manager.AppendAnimationsTo(enemySprite);

            enemySprite.Scale = 2.0f;
            enemySprite.PlayAnimation("Left");

            enemySprite.Position = new Vector2(1000, 200);
            enemySprite.Direction = new Vector2(-1, 0);
            enemySprite.Speed = 50;

            enemySprite.Rotation = MathHelper.ToRadians(0);
        }


        /// <summary>
        /// Called 60 frames/per second and updates the positions
        /// of all the drawable objects
        /// </summary>
        /// <param name="gameTime">
        /// Can work out the elapsed time since last call if
        /// you want to compensate for different frame rates
        /// </param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            // Update Asteroids

            shipSprite.Update(gameTime);
            asteroidSprite.Update(gameTime);

            if (shipSprite.HasCollided(asteroidSprite) && shipSprite.IsAlive)
            {
                flameEffect.Play();

                shipSprite.IsActive = false;
                shipSprite.IsAlive = false;
                shipSprite.IsVisible = false;
            }

            // Update Chase Game

            playerSprite.Update(gameTime);
            coinsController.Update(gameTime);
            enemySprite.Update(gameTime);

            coinsController.HasCollided(playerSprite);

            if (playerSprite.HasCollided(enemySprite))
            {
                playerSprite.IsActive = false;
                playerSprite.IsAlive = false;
                //playerSprite.IsVisible = false;
                enemySprite.IsActive = false;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Called 60 frames/per second and Draw all the 
        /// sprites and other drawable images here
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LawnGreen);

            spriteBatch.Begin();

            spriteBatch.Draw(backgroundImage, Vector2.Zero, Color.White);

            // Draw asteroids game

            shipSprite.Draw(spriteBatch);
            asteroidSprite.Draw(spriteBatch);

            // Draw Chase game

            playerSprite.Draw(spriteBatch);
            coinsController.Draw(spriteBatch);
            enemySprite.Draw(spriteBatch);

            DrawGameStatus(spriteBatch);
            DrawGameFooter(spriteBatch);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        /// <summary>
        /// Display the name fo the game and the current score
        /// and health of the player at the top of the screen
        /// </summary>
        public void DrawGameStatus(SpriteBatch spriteBatch)
        {
            Vector2 topLeft = new Vector2(4, 4);
            string status = $"Score = {score:##0}";

            spriteBatch.DrawString(arialFont, status, topLeft, Color.White);

            string game = "Coin Chase";
            Vector2 gameSize = arialFont.MeasureString(game);
            Vector2 topCentre = new Vector2((HD_Width/2 - gameSize.X/2), 4);
            spriteBatch.DrawString(arialFont, game, topCentre, Color.White);

            string healthText = $"Health = {health}%";
            Vector2 healthSize = arialFont.MeasureString(healthText);
            Vector2 topRight = new Vector2(HD_Width - (healthSize.X + 4), 4);
            spriteBatch.DrawString(arialFont, healthText, topRight, Color.White);

        }

        /// <summary>
        /// Display the Module, the authors and the application name
        /// at the bottom of the screen
        /// </summary>
        public void DrawGameFooter(SpriteBatch spriteBatch)
        {
            int margin = 20;

            string names = "Derek & Andrei";
            string app = "App05: MonogGame";
            string module = "BNU CO453-2020";

            Vector2 namesSize = calibriFont.MeasureString(names);
            Vector2 appSize = calibriFont.MeasureString(app);

            Vector2 bottomCentre = new Vector2((HD_Width - namesSize.X)/ 2, HD_Height - margin);
            Vector2 bottomLeft = new Vector2(margin, HD_Height - margin);
            Vector2 bottomRight = new Vector2(HD_Width - appSize.X - margin, HD_Height - margin);

            spriteBatch.DrawString(calibriFont, names, bottomCentre, Color.Yellow);
            spriteBatch.DrawString(calibriFont, module, bottomLeft, Color.Yellow);
            spriteBatch.DrawString(calibriFont, app, bottomRight, Color.Yellow);

        }
    }
}

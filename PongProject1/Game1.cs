using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PongProject1
{
    
    internal struct SettingsStruct
    {
        internal Keys player1DownKey = Keys.S;
        internal Keys player1UpKey = Keys.W;
        internal Keys player2DownKey = Keys.Down;
        internal Keys player2UpKey = Keys.Up;
        internal Keys pauseKey = Keys.Escape;
        internal Keys confirmKey = Keys.Enter;

        internal int defaultPaddleHeight = 100;
        internal int defaultPaddleSpeed = 300;
        internal int defaultPaddleDistanceFromEdge = 40;
        internal int defaultMaxLives = 3;

        internal float defaultStartingVelocity = 200;
        internal float defaultVelocityIncrement = 10;
        internal float defaultMinServeAngle = 60;
        internal float defaultMaxServeAngle = 120;
        internal float defaultMinBounceAngle = 40;
        internal float defaultMaxBounceAngle = 140;

        public SettingsStruct()
        {
            
        }
    }
    
    public class Game1 : Game
    {
        internal SettingsStruct Settings = new SettingsStruct();
        internal const string ballFileName = "ball";
        private const string lifeIconFileName = "ball";
        internal const string player1PaddleFileName = "bluePaddle";
        internal const string player2PaddleFileName = "redPaddle";
        internal int screenWidth;
        internal int screenHeight;
        private const string pausedText = "GAME PAUSED";
        
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D lifeIcon;
        private SpriteFont arialFont;
        // declares struct and sets default values
        

        private Menu menu;
        private Ball ball;
        private Paddle player1Paddle;
        private Paddle player2Paddle;
        private int player1Lives;
        private int player2Lives;

        private bool menuOpen = true;
        private bool gamePaused;
        private bool pauseKeyPressedLastFrame;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            screenWidth = _graphics.PreferredBackBufferWidth;
            screenHeight = _graphics.PreferredBackBufferHeight;
        }

        protected override void Initialize()
        {
            menu = new Menu(this);
            menu.InitializeMenu();
            Window.Title = "Pong";
            
            // required vars: 
            // screen width / height
            // settings
            // 
            
            ball = new Ball(this);
            player1Paddle = new Paddle(this, Settings.player1UpKey, Settings.player1DownKey, true);
            player2Paddle = new Paddle(this, Settings.player2UpKey, Settings.player2DownKey, false);

            menu.InitializeMenu();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            ball.Load(Content, ballFileName, new[] { player1Paddle, player2Paddle });

            player1Paddle.Load(Content, player1PaddleFileName);
            player2Paddle.Load(Content, player2PaddleFileName);
            lifeIcon = Content.Load<Texture2D>(lifeIconFileName);
            arialFont = Content.Load<SpriteFont>("arialFont");
            base.LoadContent();
        }

        internal void StartGame()
        {
            menuOpen = false;
            player1Lives = Settings.defaultMaxLives;
            player2Lives = Settings.defaultMaxLives;
            player1Paddle.Active = true;
            player1Paddle.Visible = true;
            player2Paddle.Active = true;
            player2Paddle.Visible = true;
            ball.Respawn();
            player1Paddle.Reset();
            player2Paddle.Reset();
        }

        internal void ExitGame()
        {
            Exit();
        }

        protected void QuitGame()
        {
            menuOpen = true;
            player1Paddle.Active = false;
            player1Paddle.Visible = false;
            player2Paddle.Active = false;
            player2Paddle.Visible = false;
            ball.Active = false;
            ball.Visible = false;
        }
        protected override void Update(GameTime gameTime)
        {
            if (menuOpen)
            {
                menu.UpdateMenu();
                base.Update(gameTime);
                return;
            }

            if (Keyboard.GetState().IsKeyDown(Settings.pauseKey) && !pauseKeyPressedLastFrame)
            {
                gamePaused = !gamePaused;
                pauseKeyPressedLastFrame = true;
            }
            else if (!Keyboard.GetState().IsKeyDown(Settings.pauseKey))
            {
                pauseKeyPressedLastFrame = false;
            }

            if (gamePaused)
            {
                base.Update(gameTime);
                return;
            }
            
            ball.Update(gameTime);

            // if (Keyboard.GetState().IsKeyDown(pauseKey))
            // {
            //     QuitGame();
            //     StartGame();
            // }

            player1Paddle.Update(gameTime);
            player2Paddle.Update(gameTime);

            if (ball.PlayerHasScored)
            {
                if (ball.ScoringPlayer == 1)
                {
                    player2Lives--;
                    if (player2Lives <= 0)
                    {
                        // Code for handeling game over. Place a proper game over screen (as part of some method) here
                        QuitGame();
                        base.Update(gameTime);
                        return;
                    }
                }
                else
                {
                    player1Lives--;
                    if (player1Lives <= 0)
                    {
                        // Code for handeling game over. Place a proper game over screen (as part of some method) here
                        QuitGame();
                        base.Update(gameTime);
                        return;
                    }
                }



                // Code for handeling resetting the field (a timeout???) here
                ball.Respawn();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            if (menuOpen)
                menu.DrawMenu(_spriteBatch, arialFont);
            
            ball.Draw(gameTime, _spriteBatch);
            player1Paddle.Draw(gameTime, _spriteBatch);
            player2Paddle.Draw(gameTime, _spriteBatch);

            // Draw lives of player 1
            for (int i = 0; i < player1Lives; i++)
            {
                _spriteBatch.Draw(lifeIcon, new Vector2(lifeIcon.Width * i, 0), Color.White);
            }
            // Draw lives of player 2
            for (int i = 0; i < player2Lives; i++)
            {
                _spriteBatch.Draw(lifeIcon, new Vector2(_graphics.PreferredBackBufferWidth - lifeIcon.Width * (i + 1), 0), Color.White);
            }

            if (gamePaused)
            {
                Vector2 textSize = arialFont.MeasureString(pausedText);
                Vector2 textPosition = new Vector2(screenWidth - textSize.X, screenHeight - textSize.Y) / 2;
                _spriteBatch.DrawString(arialFont, pausedText, textPosition, Color.Black);
            }
            
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PongProject1
{
    internal enum PlayerMode {}
    internal struct SettingsStruct
    {
        internal Keys menuDownKey = Keys.Down;
        internal Keys menuUpKey = Keys.Up;
        internal Keys debugModeKey = Keys.Insert;
        internal Keys player1DownKey = Keys.S;
        internal Keys player1UpKey = Keys.W;
        internal Keys player2DownKey = Keys.Down;
        internal Keys player2UpKey = Keys.Up;
        internal Keys pauseKey = Keys.P;
        internal Keys quitKey = Keys.Escape;
        internal Keys quickStartKey = Keys.Home;

        internal readonly int defaultPaddleHeight = 100;
        internal readonly int defaultPaddleSpeed = 300;
        internal readonly int defaultPaddleDistanceFromEdge = 40;

        internal readonly float defaultStartingVelocity = 240;
        internal readonly float defaultVelocityIncrement = 30;
        internal readonly float defaultMinServeAngle = 60;
        internal readonly float defaultMaxServeAngle = 120;
        internal readonly float defaultMinBounceAngle = 40;
        internal readonly float defaultMaxBounceAngle = 140;

        public SettingsStruct()
        {
            
        }
    }
    
    public class Game1 : Game
    {
        internal SettingsStruct Settings = new ();
        private const string ballFileName = "ball";
        private const string lifeIconFileName = "ball";
        private const string player1PaddleFileName = "bluePaddle";
        private const string player2PaddleFileName = "redPaddle";
        internal readonly int screenWidth;
        internal readonly int screenHeight;
        private const string pausedText = "GAME PAUSED";
        
        private readonly GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        private Texture2D lifeIcon;
        public SpriteFont arialFont;
        // declares struct and sets default values

        private Menu menu;
        internal Ball ball;
        private Paddle player1Paddle;
        private Paddle player2Paddle;
        private int player1Lives;
        private int player2Lives;
        
        private bool menuOpen = true;
        private bool gamePaused;
        private bool pauseKeyPressedLastFrame;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            screenWidth = graphics.PreferredBackBufferWidth;
            screenHeight = graphics.PreferredBackBufferHeight;
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

            menu.InitializeMenu();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            lifeIcon = Content.Load<Texture2D>(lifeIconFileName);
            arialFont = Content.Load<SpriteFont>("arialFont");
            base.LoadContent();
        }

        internal void StartGame(int livesPlayer1, int livesPlayer2, int typePlayer1, int typePlayer2)
        {
            
            ball = new Ball(this);
            player1Paddle = new Paddle(this, Settings.player1UpKey, Settings.player1DownKey, true, typePlayer1);
            player2Paddle = new Paddle(this, Settings.player2UpKey, Settings.player2DownKey, false, typePlayer2);
            player1Paddle.Load(Content, player1PaddleFileName);
            player2Paddle.Load(Content, player2PaddleFileName);
            ball.Load(Content, ballFileName, new[] { player1Paddle, player2Paddle });
            
            menuOpen = false;
            player1Lives = livesPlayer1;
            player2Lives = livesPlayer2;
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

        private void QuitGame()
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
            
            if (Keyboard.GetState().IsKeyDown(Settings.quitKey))
            {
                ExitGame();
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
                        menu.GameOver("Player 1");
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
                        menu.GameOver("Player 2");
                        QuitGame();
                        base.Update(gameTime);
                        return;
                    }
                }



                // Code for handling resetting the field (a timeout???) here
                ball.Respawn();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            if (menuOpen)
                menu.DrawMenu();

            if (!menuOpen)
            {
                ball.Draw(gameTime, spriteBatch);
                player1Paddle.Draw(gameTime, spriteBatch);
                player2Paddle.Draw(gameTime, spriteBatch);
                
                // Draw lives of player 1
                for (int i = 0; i < player1Lives; i++)
                {
                    spriteBatch.Draw(lifeIcon, new Vector2(lifeIcon.Width * i, 0), Color.White);
                }
                // Draw lives of player 2
                for (int i = 0; i < player2Lives; i++)
                {
                    spriteBatch.Draw(lifeIcon, new Vector2(graphics.PreferredBackBufferWidth - lifeIcon.Width * (i + 1), 0), Color.White);
                }
            }

            if (gamePaused)
            {
                Vector2 textSize = arialFont.MeasureString(pausedText);
                Vector2 textPosition = new Vector2(screenWidth - textSize.X, screenHeight - textSize.Y) / 2;
                spriteBatch.DrawString(arialFont, pausedText, textPosition, Color.Black);
            }
            
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
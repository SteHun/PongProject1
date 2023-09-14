using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PongProject1
{
    public class Game1 : Game
    {
        private const int defaultPaddleHeight = 100;
        private const int defaultPaddleSpeed = 300;
        private const int defaultPaddleDistanceFromEdge = 40;
        private const int defaultMaxLives = 3;

        private const float defaultStartingVelocity = 200;
        private const float defaultVelocityIncrement = 10;
        private const float defaultMinServeAngle = 60;
        private const float defaultMaxServeAngle = 120;
        private const float defaultMinBounceAngle = 40;
        private const float defaultMaxBounceAngle = 140;

        private const string ballFileName = "ball";
        private const string lifeIconFileName = "ball";
        private const string player1PaddleFileName = "bluePaddle";
        private const string player2PaddleFileName = "redPaddle";
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D lifeIcon;
        private SpriteFont arialFont;

        private Keys player1DownKey = Keys.S;
        private Keys player1UpKey = Keys.W;
        private Keys player2DownKey = Keys.Down;
        private Keys player2UpKey = Keys.Up;
        private Keys pauseKey = Keys.Escape;
        private Keys confirmKey = Keys.Enter;

        private Menu menu;
        private Ball ball;
        private Paddle player1Paddle;
        private Paddle player2Paddle;
        private int player1Lives;
        private int player2Lives;

        private bool menuOpen = true;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            menu = new Menu(this);
            menu.InitializeMenu();
            Window.Title = "Pong";
            ball = new Ball(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight, defaultStartingVelocity,
                defaultMinServeAngle, defaultMaxServeAngle, defaultMinBounceAngle, defaultMaxBounceAngle, defaultVelocityIncrement);
            int windowHeight = _graphics.PreferredBackBufferHeight;
            player1Paddle = new Paddle(new Vector2(defaultPaddleDistanceFromEdge, (float)(windowHeight - defaultPaddleHeight) / 2),
                defaultPaddleHeight, windowHeight, player1UpKey, player1DownKey, defaultPaddleSpeed, true);
            player2Paddle = new Paddle(new Vector2(_graphics.PreferredBackBufferWidth - defaultPaddleDistanceFromEdge, (float)(windowHeight - defaultPaddleHeight) / 2),
                defaultPaddleHeight, windowHeight, player2UpKey, player2DownKey, defaultPaddleSpeed, false);
            
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
            player1Lives = defaultMaxLives;
            player2Lives = defaultMaxLives;
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
            
            ball.Update(gameTime);

            // if (Keyboard.GetState().IsKeyDown(pauseKey))
            // {
            //     QuitGame();
            //     StartGame();
            // }

            player1Paddle.Update(gameTime);
            player2Paddle.Update(gameTime);

            if (ball.PlayerHasScoared)
            {
                if (ball.ScoaringPlayer == 1)
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
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
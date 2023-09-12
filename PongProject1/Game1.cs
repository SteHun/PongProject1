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

        private const float defaultStartingVelocity = 200;
        private const float defaultMinServeAngle = 60;
        private const float defaultMaxServeAngle = 120;

        private const string ballFileName = "ball";
        private const string player1PaddleFileName= "bluePaddle";
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Keys player1DownKey = Keys.S;
        private Keys player1UpKey = Keys.W;
        private Keys player2DownKey = Keys.Down;
        private Keys player2UpKey = Keys.Up;
        private Keys pauseKey = Keys.Escape;
        private Keys confirmKey = Keys.Enter;

        private Ball ball;
        private Paddle player1Paddle;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            Window.Title = "Pong";
            ball = new Ball(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight, defaultStartingVelocity, defaultMinServeAngle, defaultMaxServeAngle);
            int windowHeight = _graphics.PreferredBackBufferHeight;
            player1Paddle = new Paddle(new Vector2(defaultPaddleDistanceFromEdge, (windowHeight - defaultPaddleHeight) / 2), 
                defaultPaddleHeight, windowHeight, player1UpKey, player1DownKey, defaultPaddleSpeed, true);
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            ball.Load(Content, ballFileName, new Paddle[] {player1Paddle});
            ball.Respawn();

            player1Paddle.Load(Content, player1PaddleFileName);
            player1Paddle.Active = true;
            player1Paddle.Visible = true;
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {

            ball.Update(gameTime);

            if (Keyboard.GetState().IsKeyDown(pauseKey))
            {
                ball.Respawn();
            }

            player1Paddle.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            ball.Draw(gameTime, _spriteBatch);
            player1Paddle.Draw(gameTime, _spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
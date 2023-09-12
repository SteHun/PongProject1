using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PongProject1
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Keys player1DownKey = Keys.S;
        private Keys player1UpKey = Keys.W;
        private Keys player2DownKey = Keys.Down;
        private Keys player2UpKey = Keys.Up;
        private Keys pauseKey = Keys.Escape;
        private Keys confirmKey = Keys.Enter;

        private Ball ball;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            Window.Title = "Pong";

            ball = new Ball(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            ball.Load(Content);
            ball.Respawn();
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {

            ball.Update(gameTime);

            if (Keyboard.GetState().IsKeyDown(pauseKey))
            {
                ball.Respawn();
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            ball.Draw(gameTime, _spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
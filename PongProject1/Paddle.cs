using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace PongProject1
{
    internal class Paddle
    {
        internal Vector2 Position;
        internal bool Active = false;
        internal bool Visible = false;
        internal float Speed;
        internal int height;
        internal int width;
        internal bool IsFacingRight;

        private int screenHeight;
        private Texture2D texture;
        private Vector2 startingPosition;
        private Keys upKey;
        private Keys downKey;
        internal Paddle(Vector2 startingPosition, int height, int screenHeight, Keys upKey, Keys downKey, float speed, bool isFacingRight)
        {
            this.startingPosition = startingPosition;
            this.height = height;
            this.screenHeight = screenHeight;
            this.upKey = upKey;
            this.downKey = downKey;
            Speed = speed;
            Position = startingPosition;
            IsFacingRight = isFacingRight;
        }
        
        internal Paddle(Game1 game, Keys upKey, Keys downKey, bool isFacingRight)
        {
            this.height = Game1.defaultPaddleHeight;
            this.screenHeight = game.screenHeight;
            this.upKey = upKey;
            this.downKey = downKey;
            Speed = Game1.defaultPaddleSpeed;
            Position = startingPosition;
            IsFacingRight = isFacingRight;
            if (isFacingRight)
            {
                this.startingPosition = new Vector2(
                    Game1.defaultPaddleDistanceFromEdge, 
                    (float)(game.screenHeight - Game1.defaultPaddleHeight) / 2);
            }
            else
            {
                this.startingPosition = new Vector2(
                    game.screenWidth - Game1.defaultPaddleDistanceFromEdge, 
                    (float)(game.screenHeight - Game1.defaultPaddleHeight) / 2);
            }
            
        }
        internal void Reset()
        {
            Position = startingPosition;
        }
        internal void Load(ContentManager content, string paddleFileName)
        {
            texture = content.Load<Texture2D>(paddleFileName);
            if (!IsFacingRight)
            {
                startingPosition.X -= texture.Width;
                Position = startingPosition;
            }
            width = texture.Width;
        }

        internal void Update(GameTime gameTime)
        {
            if (!Active)
                return;
            float totalMovement = 0;
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(upKey))
            {
                totalMovement -= Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (state.IsKeyDown(downKey))
            {
                totalMovement += Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            Position.Y += totalMovement;
            if (Position.Y < 0)
                Position.Y = 0;
            else if (Position.Y + height > screenHeight)
                Position.Y = screenHeight - height;
        }

        internal void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
        {
            if (!Visible)
                return;
            _spriteBatch.Draw(texture, new Rectangle((int)MathF.Round(Position.X), (int)MathF.Round(Position.Y), texture.Width, height), Color.White);

        }
    }
}

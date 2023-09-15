using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace PongProject1
{
    internal class Paddle
    {
        internal int type;
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
        internal Paddle(Vector2 startingPosition, int height, int screenHeight, Keys upKey, Keys downKey, float speed, bool isFacingRight, int playerType)
        {
            type = playerType;
            this.startingPosition = startingPosition;
            this.height = height;
            this.screenHeight = screenHeight;
            this.upKey = upKey;
            this.downKey = downKey;
            Speed = speed;
            Position = startingPosition;
            IsFacingRight = isFacingRight;
        }
        
        internal Paddle(Game1 game, Keys upKey, Keys downKey, bool isFacingRight, int playerType)
        {
            type = playerType;
            this.height = game.Settings.defaultPaddleHeight;
            this.screenHeight = game.screenHeight;
            this.upKey = upKey;
            this.downKey = downKey;
            Speed = game.Settings.defaultPaddleSpeed;
            Position = startingPosition;
            IsFacingRight = isFacingRight;
            if (isFacingRight)
            {
                this.startingPosition = new Vector2(
                    game.Settings.defaultPaddleDistanceFromEdge, 
                    (float)(game.screenHeight - game.Settings.defaultPaddleHeight) / 2);
            }
            else
            {
                this.startingPosition = new Vector2(
                    game.screenWidth - game.Settings.defaultPaddleDistanceFromEdge, 
                    (float)(game.screenHeight - game.Settings.defaultPaddleHeight) / 2);
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
            float totalMovement;
            switch (type)
            {
                case 0: //human
                    totalMovement = HumanUpdate(gameTime);
                    break;
                case 1: //easy AI
                    totalMovement = EasyAIUpdate(gameTime);
                    break;
                default: //hard AI or some other problem
                    totalMovement = HardAIUpdate(gameTime);
                    break;
            }
            Position.Y += totalMovement;
            if (Position.Y < 0)
                Position.Y = 0;
            else if (Position.Y + height > screenHeight)
                Position.Y = screenHeight - height;
        }

        private float HumanUpdate(GameTime gameTime)
        {
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

            return totalMovement;
        }

        internal float EasyAIUpdate(GameTime gameTime)
        {
            // TODO make easy AI
            return 0;
        }
        
        internal float HardAIUpdate(GameTime gameTime)
        {
            // TODO make hard AI
            return 0;
        }

        internal void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
        {
            if (!Visible)
                return;
            _spriteBatch.Draw(texture, new Rectangle((int)MathF.Round(Position.X), (int)MathF.Round(Position.Y), texture.Width, height), Color.White);

        }
    }
}

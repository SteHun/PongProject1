using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace PongProject1
{
    internal class Ball
    {
        private Game1 game;

        public bool Active;
        public bool Visible;
        public bool PlayerHasScored;
        public byte ScoringPlayer;
        private Vector2 startingPosition;
        internal Vector2 Position;
        internal Vector2 Velocity;
        internal Texture2D Texture;
        private float minServeAngle;
        private float maxServeAngle;
        private float minBounceAngle;
        private float maxBounceAngle;
        private int totalBounces;
        private Paddle[] paddles;
        internal Ball(Game1 game)
        {
            this.game = game;
            //convert to radians
            this.minServeAngle = game.Settings.defaultMinServeAngle * (MathF.PI / 180);
            this.maxServeAngle = game.Settings.defaultMaxServeAngle * (MathF.PI / 180);
            this.minBounceAngle = game.Settings.defaultMinBounceAngle * (MathF.PI / 180);
            this.maxBounceAngle = game.Settings.defaultMaxBounceAngle * (MathF.PI/ 180);
        }

        internal Ball(Game1 game, Vector2 startPos)
        {
            this.game = game;
            startingPosition = startPos;
            //convert to radians
            this.minServeAngle = game.Settings.defaultMinServeAngle * (MathF.PI / 180);
            this.maxServeAngle = game.Settings.defaultMaxServeAngle * (MathF.PI / 180);
            this.minBounceAngle = game.Settings.defaultMinBounceAngle * (MathF.PI / 180);
            this.maxBounceAngle = game.Settings.defaultMaxBounceAngle * (MathF.PI/ 180);
        }


        internal void Load(ContentManager content, string textureFileName, Paddle[] paddlesImport)
        {
            Texture = content.Load<Texture2D>(textureFileName);
            // if startingPosition wasn't explicitly set, set it to the centre
            if (startingPosition == default(Vector2))
                startingPosition = new Vector2(game.screenWidth - Texture.Width, game.screenHeight - Texture.Height) / 2;
            paddles = paddlesImport;
        }

        internal void Update(GameTime gameTime)
        {
            if (!Active)
            {
                return;
            }

            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            // bounce the ball
            if (Position.Y <= 0)
            {
                Velocity.Y *= -1;
                Position.Y = 0;
            }
            else if (Position.Y + Texture.Height >= game.screenHeight)
            {
                Velocity.Y *= -1;
                Position.Y = game.screenHeight - Texture.Height;
            }

            if (Position.X <= 0)
            {
                Active = false;
                PlayerHasScored = true;
                ScoringPlayer = 2;
            }
            if (Position.X + Texture.Width >= game.screenWidth)
            {
                Active = false;
                PlayerHasScored = true;
                ScoringPlayer = 1;
            }
            float? collisionResult = CheckCollision();
            if (!collisionResult.HasValue)
            {
                return;
            }

            totalBounces++;
            float angle;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (Velocity.X < 0)
            {
                angle = MapValue(minBounceAngle, maxBounceAngle, (float)collisionResult);
            }
            else
            {
                angle = MapValue(minBounceAngle + MathF.PI, maxBounceAngle + MathF.PI, (float)-collisionResult);
            }

            Velocity = new Vector2(MathF.Sin(angle), MathF.Cos(angle)) * (game.Settings.defaultStartingVelocity + totalBounces * game.Settings.defaultVelocityIncrement);
        }

        internal void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
        {
            if (!Visible)
                return;
            _spriteBatch.Draw(Texture, Position, Color.White);
        }

        internal void Respawn()
        {
            Random rng = new Random();
            Position = startingPosition;
            Active = true;
            Visible = true;
            PlayerHasScored = false;
            ScoringPlayer = 0;
            totalBounces = 0;
            float angle;
            // Choose the player that will be served the ball
            if (rng.Next(2) == 0)
            {
                //set a random angle
                angle = (float)rng.NextDouble() * (maxServeAngle - minServeAngle) + minServeAngle;
            }
            else
            {
                angle = (float)rng.NextDouble() * (maxServeAngle - minServeAngle) + minServeAngle + MathF.PI;
            }
            Velocity = new Vector2(MathF.Sin(angle), MathF.Cos(angle)) * game.Settings.defaultStartingVelocity;
        }

        // This method returns -1 to 1 depending on where the ball hits the paddle. It returns null with no collision
        private float? CheckCollision()
        {
            bool isMovingRight = Velocity.X > 0;
            foreach(Paddle paddle in paddles)
            {
                // (I know this can be done in fewer steps, but this is more readable
                if (isMovingRight && paddle.IsFacingRight)
                    continue;
                if (!isMovingRight && !paddle.IsFacingRight)
                    continue;
                // is the ball in the correct x position for it it hit this paddle? 
                if (!(Position.X + Texture.Width >= paddle.Position.X && Position.X <= paddle.Position.X + paddle.width))
                    continue;
                // in the ball in the correct y position for it to hit this paddle?
                if (!(Position.Y + Texture.Height >= paddle.Position.Y && Position.Y <= paddle.Position.Y + paddle.height))
                    continue;
                // the paddle has passed all checks and is in a collision with the ball
                // TEMP!!! for now it returns 0 to bounce the ball straight. Replace later
                float ballMiddleYPos = Position.Y + (float)Texture.Height / 2;
                float paddleMiddleYPos = paddle.Position.Y + (float)paddle.height / 2;
                Debug.WriteLine($"distance from middle: {ballMiddleYPos - paddleMiddleYPos}|result: {(ballMiddleYPos - paddleMiddleYPos) / ((float)paddle.height / 2)}");
                return (paddleMiddleYPos - ballMiddleYPos) / ((float)paddle.height / 2);
            }
            return null;
        }

        // chooses a value between min and max based on the value (with range -1 to 1)
        private static float MapValue(float min, float max, float value)
        {
            // value can't be higher than 1 or lower than -1
            if (value < 0)
                value = MathF.Max(-1, value);
            else
                value = MathF.Min(1, value);
            float half = (max + min) / 2;
            float result = half + (half - min) * value;
            return result;
        }
    }

}

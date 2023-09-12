using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace PongProject1
{
    internal class Ball
    {


        private int screenWidth, screenHeight;
        public bool Active = false;
        public bool Visible = false;
        public bool PlayerHasScoared = false;
        public byte ScoaringPlayer = 0;
        private Vector2 startingPosition;
        internal Vector2 Position;
        internal Vector2 Velocity;
        private Texture2D texture;
        private float startingVelocity;
        private float minServeAngle;
        private float maxServeAngle;
        private float minBounceAngle;
        private float maxBounceAngle;
        private Paddle[] paddles;
        internal Ball(int screenWidth, int screenHeight, float startingVelocity, float minServeAngle, float maxServeAngle, float minBounceAngle, float maxBounceAngle)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.startingVelocity = startingVelocity;

            //convert to radians
            this.minServeAngle = minServeAngle * (MathF.PI / 180);
            this.maxServeAngle = maxServeAngle * (MathF.PI / 180);
            this.minBounceAngle = minBounceAngle * (MathF.PI / 180);
            this.maxBounceAngle = maxBounceAngle* (MathF.PI/ 180);
        }

        internal void Load(ContentManager content, string textureFileName, Paddle[] paddles)
        {
            texture = content.Load<Texture2D>(textureFileName);
            startingPosition = new Vector2(screenWidth - texture.Width, screenHeight - texture.Height) / 2;
            this.paddles = paddles;
        }

        internal void Update(GameTime gameTime)
        {
            if (!Active)
                return;
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            // bounce the ball
            if (Position.Y <= 0 || Position.Y + texture.Height >= screenHeight)
                Velocity.Y *= -1;

            if (Position.X <= 0)
            {
                Active = false;
                PlayerHasScoared = true;
                ScoaringPlayer = 2;
            }
            if (Position.X + texture.Width >= screenWidth)
            {
                Active = false;
                PlayerHasScoared = true;
                ScoaringPlayer = 1;
            }
            float? colissionResult = CheckColission();
            if (colissionResult.HasValue)
            {
                float angle;
                if (Velocity.X < 0)
                    angle = MapValue(minBounceAngle, maxBounceAngle, (float)colissionResult);
                else
                    angle = MapValue(minBounceAngle + MathF.PI, maxBounceAngle + MathF.PI, (float)-colissionResult);
                Velocity = new Vector2(MathF.Sin(angle), MathF.Cos(angle)) * startingVelocity;
            }
        }

        internal void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
        {
            if (!Visible)
                return;
            _spriteBatch.Draw(texture, Position, Color.White);
        }

        internal void Respawn()
        {
            Random rng = new Random();
            Position = startingPosition;
            Active = true;
            Visible = true;
            PlayerHasScoared = false;
            ScoaringPlayer = 0;
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
            Velocity = new Vector2(MathF.Sin(angle), MathF.Cos(angle)) * startingVelocity;
        }

        // This method returns -1 to 1 depending on where the ball hits the paddle. It returns null with no colission
        private float? CheckColission()
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
                if (!(Position.X + texture.Width >= paddle.Position.X && Position.X <= paddle.Position.X + paddle.width))
                    continue;
                // in the ball in the correct y position for it to hit this paddle?
                if (!(Position.Y + texture.Height >= paddle.Position.Y && Position.Y <= paddle.Position.Y + paddle.height))
                    continue;
                // the paddle has passed all checks and is in a colission with the ball
                // TEMP!!! for now it returns 0 to bounce the ball straingt. Replace later
                float ballMiddleYPos = Position.Y + texture.Height / 2;
                float paddleMiddleYPos = paddle.Position.Y + paddle.height / 2;
                Debug.WriteLine($"distance from middle: {ballMiddleYPos - paddleMiddleYPos}|result: {(ballMiddleYPos - paddleMiddleYPos) / (paddle.height / 2)}");
                return (paddleMiddleYPos - ballMiddleYPos) / (paddle.height / 2);
            }
            return null;
        }

        // chooses a value between min and max based on the value (with range -1 to 1)
        private float MapValue(float min, float max, float value)
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

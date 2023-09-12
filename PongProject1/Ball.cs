using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection.Metadata;

namespace PongProject1
{
    internal class Ball
    {


        private int screenWidth, screenHeight;
        public bool Active = false;
        public bool Visible = false;
        private Vector2 startingPosition;
        internal Vector2 Position;
        internal Vector2 Velocity;
        private Texture2D texture;
        private float startingVelocity;
        private float minServeAngle;
        private float maxServeAngle;
        internal Ball(int screenWidth, int screenHeight, float startingVelocity, float minServeAngle, float maxServeAngle)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.startingVelocity = startingVelocity;

            //convert to radians
            this.minServeAngle = minServeAngle * (MathF.PI / 180);
            this.maxServeAngle = maxServeAngle * (MathF.PI/ 180);
        }

        internal void Load(ContentManager content, string textureFileName)
        {
            texture = content.Load<Texture2D>(textureFileName);
            startingPosition = new Vector2(screenWidth - texture.Width, screenHeight - texture.Height) / 2;
        }

        internal void Update(GameTime gameTime)
        {
            if (!Active)
                return;
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            // bounce the ball
            if (Position.Y <= 0 || Position.Y + texture.Height >= screenHeight)
                Velocity.Y *= -1;
            if (Position.X <= 0 || Position.X + texture.Width>= screenWidth)
                Velocity.X *= -1;
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
    }

}

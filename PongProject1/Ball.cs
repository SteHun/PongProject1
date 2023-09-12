using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection.Metadata;

namespace PongProject1
{
    internal class Ball
    {
        private const float startingVelocity = 200;
        private float minServeAngle = 60;
        private float maxServeAngle = 120;


        private int screenWidth, screenHeight;
        private bool active = false;
        private bool visible = false;
        private Vector2 startingPosition;
        internal Vector2 Position;
        internal Vector2 Velocity;
        private Texture2D texture;
        internal Ball(int screenWidth, int screenHeight)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;

            //convert to radians
            minServeAngle = minServeAngle * (MathF.PI / 180);
            maxServeAngle = maxServeAngle * (MathF.PI/ 180);
        }

        internal void Load(ContentManager Content)
        {
            texture = Content.Load<Texture2D>("ball");
            startingPosition = new Vector2(screenWidth - texture.Width, screenHeight - texture.Height) / 2;
        }

        internal void Update(GameTime gameTime)
        {
            if (!active)
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
            if (!visible)
                return;
            _spriteBatch.Draw(texture, Position, Color.White);
        }

        internal void Respawn()
        {
            Random rng = new Random();
            Position = startingPosition;
            active = true;
            visible = true;
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
            int _ = 5;
        }
        
        internal void MakeVisible()
        {
            visible = true;
        }

        internal void Deactivate()
        {
            active = false;
            visible = false;
        }
    }

}

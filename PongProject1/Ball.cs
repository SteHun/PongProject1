using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace PongProject1
{
    internal class Ball
    {
        private Game1 game;

        //Miscellaneous
        public bool Active;
        public bool Visible;
        public bool PlayerHasScored;
        public byte ScoringPlayer;
        private bool sharpAngle; //Used to check if the player returns the ball in a manner that has a high Y velocity
        
        //Ball variables
        private Vector2 startingPosition;
        internal Vector2 Position;
        private double activationTime;
        internal Vector2 Velocity;
        internal Texture2D Texture;
        internal SoundEffect BonkSFX;
        internal SoundEffect SharpShotSFX;
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
            minServeAngle = game.Settings.defaultMinServeAngle * (MathF.PI / 180);
            maxServeAngle = game.Settings.defaultMaxServeAngle * (MathF.PI / 180);
            minBounceAngle = game.Settings.defaultMinBounceAngle * (MathF.PI / 180);
            maxBounceAngle = game.Settings.defaultMaxBounceAngle * (MathF.PI/ 180);
        }

        //Load textures, set positions and put paddles in array
        internal void Load(ContentManager content, string textureFileName, Paddle[] paddlesImport)
        {
            Texture = content.Load<Texture2D>(textureFileName);
            BonkSFX = content.Load<SoundEffect>("bonk");
            SharpShotSFX = content.Load<SoundEffect>("CrowdOh");
            // if startingPosition wasn't explicitly set, set it to the centre
            if (startingPosition == default)
                startingPosition = new Vector2(game.screenWidth - Texture.Width, game.screenHeight - Texture.Height) / 2;
            paddles = paddlesImport;
        }

        internal void Update(GameTime gameTime)
        {
            //If the ball isn't in play or the wait time between new serves hasn't passed yet
            if (!Active || gameTime.TotalGameTime.TotalMilliseconds < activationTime)
            {
                return; //Don't run the script
            }

            //Update ball position
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // bounce the ball if it hits the ceiling / floor
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

            //Check if a player has scored
            if (Position.X <= 0)
            {
                Active = false;
                PlayerHasScored = true;
                ScoringPlayer = 2;
                //TODO play scoring sound
                if (sharpAngle)
                {
                    
                }
                else
                {
                    
                }
            }
            if (Position.X + Texture.Width >= game.screenWidth)
            {
                Active = false;
                PlayerHasScored = true;
                ScoringPlayer = 1;
                //TODO Play scoring sound
                if (sharpAngle)
                {
                    
                }
                else
                {
                    
                }
            }
            
            //Check if a paddle touches the ball
            float? collisionResult = CheckCollision();
            if (!collisionResult.HasValue)
            {
                return; //Do nothing if no paddles are hitting the ball
            }

            BonkSFX.Play(MediaPlayer.Volume, MathF.Min((Velocity.Length() * 0.25f) / game.Settings.defaultStartingVelocity - 1, 1), 0);
            
            totalBounces++;
            float angle;
            sharpAngle = false;
            //Change the angle of the ball
            if (Velocity.X < 0)
            {
                angle = MapValue(minBounceAngle, maxBounceAngle, (float)collisionResult);
                if ((angle < minBounceAngle * 1.1 || angle > maxBounceAngle * 0.9) && Velocity.Length() / game.Settings.defaultStartingVelocity >= 1.8)
                {
                    sharpAngle = true;
                    Debug.WriteLine("sharp!");
                }
            }
            else
            {
                angle = MapValue(minBounceAngle + MathF.PI, maxBounceAngle + MathF.PI, (float)-collisionResult);
                if ((angle < minBounceAngle + MathF.PI * 1.1 || angle > maxBounceAngle + MathF.PI * 0.9) && Velocity.Length() / game.Settings.defaultStartingVelocity >= 1.8)
                {
                    sharpAngle = true;
                    Debug.WriteLine("sharp!");

                }
            }

            if (sharpAngle)
                SharpShotSFX.Play();
            Console.WriteLine(angle);
            //TODO calculate what counts as sharpAngle


            //Increase the speed of the ball
            Velocity = new Vector2(MathF.Sin(angle), MathF.Cos(angle)) * (game.Settings.defaultStartingVelocity + totalBounces * game.Settings.defaultVelocityIncrement);
        }

        //Update the visuals position of the ball
        internal void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
        {
            if (!Visible)
                return;
            _spriteBatch.Draw(Texture, Position, Color.White);
        }

        //Reset the values of the ball if someone has scored and both players are still alive
        internal void Respawn() //This function fills in the real Respawn function with base parameters
        {
            Respawn(new GameTime(new TimeSpan(0), new TimeSpan(0)));
        }
        internal void Respawn(GameTime gameTime)
        {
            Random rng = new Random();
            //Set the time before the ball starts moving
            if (gameTime.TotalGameTime.TotalMilliseconds > 0)
                activationTime = gameTime.TotalGameTime.TotalMilliseconds + game.Settings.defaultBallRespawnTime;
            
            //Reset values
            Position = startingPosition;
            Active = true;
            Visible = true;
            PlayerHasScored = false;
            ScoringPlayer = 0;
            totalBounces = 0;
            
            // Choose the player that will be served the ball
            float angle;
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
            
            //Run for all paddles loaded in the game
            foreach(Paddle paddle in paddles)
            {
                //Only run the rest of the code if the ball is moving towards the paddle
                if (isMovingRight && paddle.IsFacingRight)
                    continue;
                if (!isMovingRight && !paddle.IsFacingRight)
                    continue;
                
                // is the ball in the correct x position for it to hit this paddle? 
                if (!(Position.X + Texture.Width >= paddle.Position.X && Position.X <= paddle.Position.X + paddle.width))
                    continue;
                // in the ball in the correct y position for it to hit this paddle?
                if (!(Position.Y + Texture.Height >= paddle.Position.Y && Position.Y <= paddle.Position.Y + paddle.height))
                    continue;
                
                // the paddle has passed all checks and is in a collision with the ball
                float ballMiddleYPos = Position.Y + (float)Texture.Height / 2;
                float paddleMiddleYPos = paddle.Position.Y + (float)paddle.height / 2;
                
                //Returns a number between -1 and 1
                return (paddleMiddleYPos - ballMiddleYPos) / ((float)paddle.height / 2);
            }
            return null;
        }

        // chooses a value between min and max based on the value (with range -1 to 1)
        //Enter a min and max return value, the result is min if value == -1, the result is max if value == 1
        //If value is between -1 and 1 result is the ratio of value to min or max (depending on positive or negative)
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

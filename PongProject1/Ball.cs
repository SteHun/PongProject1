using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
 
namespace PongProject1
{
    internal class Ball
    {
        #region Initialization
        #region Variables
        private Game1 game;

        //Miscellaneous
        public bool Active;
        public bool Visible;
        public bool PlayerHasScored;
        public byte ScoringPlayer;
        private bool sharpShot; //Used to check if the player returns the ball in a manner that has a high Y velocity
        
        //Ball variables
        private Vector2 startingPosition;
        internal Vector2 Position;
        internal float TrueYPosition
        {
            get { return Position.Y + (float)Texture.Height / 2; }
        }
        private double activationTime;
        internal Vector2 Velocity;
        internal Texture2D Texture;
        internal SoundEffect BonkSFX;
        internal SoundEffect ScoreSFX;
        internal SoundEffect SharpShotSFX;
        internal SoundEffect SharpShotScoreSFX;
        internal SoundEffect SharpShotDefendSFX;
        private SoundEffectInstance crowdSoundEffectInstance;
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
        #endregion

        //Load textures, set positions and put paddles in array
        internal void Load(ContentManager content, string textureFileName, Paddle[] paddlesImport)
        {
            //Sprites
            Texture = content.Load<Texture2D>(textureFileName);
            
            //Sound effects
            BonkSFX = content.Load<SoundEffect>("bonk");
            ScoreSFX = content.Load<SoundEffect>("CrowdSlowClap");
            SharpShotSFX = content.Load<SoundEffect>("CrowdOh");
            SharpShotScoreSFX = content.Load<SoundEffect>("CrowdYay");
            SharpShotDefendSFX = content.Load<SoundEffect>("CrowdAw");
            
            // if startingPosition wasn't explicitly set, set it to the centre
            if (startingPosition == default)
                startingPosition = new Vector2(game.screenWidth - Texture.Width, game.screenHeight - Texture.Height) / 2;
            paddles = paddlesImport;
        }
        #endregion

        #region Update method
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
            float requiredSpeedRatio = 4f;
            if (Position.X <= 0) //Player 2 scores
            {
                //Flags to update score and start next serve
                Active = false;
                PlayerHasScored = true;
                ScoringPlayer = 2;
                
                //Plays a different sound effect depending on if the point was scored with a 'sharp shot' or the ball was travelling fast enough
                if (sharpShot || Velocity.Length() / game.Settings.defaultStartingVelocity >= requiredSpeedRatio)
                    SharpShotScoreSFX.Play();
                else //Default scoring sound
                    ScoreSFX.Play();
            }
            if (Position.X + Texture.Width >= game.screenWidth) //Player 1 scores
            {
                //Flags to update score and start next serve
                Active = false;
                PlayerHasScored = true;
                ScoringPlayer = 1;
                
                //Plays a different sound effect depending on if the point was scored with a 'sharp shot' or the ball was travelling fast enough
                if (sharpShot || Velocity.Length() / game.Settings.defaultStartingVelocity >= requiredSpeedRatio)
                    SharpShotScoreSFX.Play();
                else //Default scoring sound
                    ScoreSFX.Play();
            }
            
            
            
            //Check if a paddle touches the ball
            float? collisionResult = CheckCollision();
            if (!collisionResult.HasValue)
            {
                return; //Do nothing if no paddles are hitting the ball
            }

            //Play a sound effect if the ball is hit by a paddle
            BonkSFX.Play(MediaPlayer.Volume, MathF.Min(Velocity.Length() * 0.25f / game.Settings.defaultStartingVelocity - 1, 1), 0);
            
            //Play a sound effect if the previous hit was a sharp shot (this means a player has defended against a 'hard' ball)
            if (sharpShot)
            {
                StopCrowdSFX();
                crowdSoundEffectInstance = SharpShotDefendSFX.CreateInstance();
                crowdSoundEffectInstance.Play();
            }

            totalBounces++;
            float angle;
            sharpShot = false;
            
            //Change the angle of the ball
            if (Velocity.X < 0)
            {
                angle = MapValue(minBounceAngle, maxBounceAngle, (float)collisionResult);
                
                //If a ball is hit with great speed in the Y axis then it is called a 'sharp shot' (This is most likely a harder ball to defend against)
                if ((angle < minBounceAngle * 1.1 || angle > maxBounceAngle * 0.9) && Velocity.Length() / game.Settings.defaultStartingVelocity >= 1.8)
                {
                    sharpShot = true;
                }
            }
            else
            {
                angle = MapValue(minBounceAngle + MathF.PI, maxBounceAngle + MathF.PI, (float)-collisionResult);
                
                //If a ball is hit with great speed in the Y axis then it is called a 'sharp shot' (This is most likely a harder ball to defend against)
                if ((angle < minBounceAngle + MathF.PI * 1.1 || angle > maxBounceAngle + MathF.PI * 0.9) && Velocity.Length() / game.Settings.defaultStartingVelocity >= 1.8)
                {
                    sharpShot = true;
                }
            }

            if (sharpShot)
            {
                StopCrowdSFX();
                crowdSoundEffectInstance = SharpShotSFX.CreateInstance();
                crowdSoundEffectInstance.Play();
            }

            //Increase the speed of the ball
            Velocity = new Vector2(MathF.Sin(angle), MathF.Cos(angle)) * (game.Settings.defaultStartingVelocity + totalBounces * game.Settings.defaultVelocityIncrement);
        }

        private void StopCrowdSFX()
        {
            //crowsSoundEffectInstance may be null at this point. The ? prevents this from executing if it is null
            crowdSoundEffectInstance?.Stop();
        }

        //Update the visuals position of the ball
        internal void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            spriteBatch.Draw(Texture, Position, Color.White);
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
            sharpShot = false;
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
        #endregion

        #region Other methods
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
        #endregion
    }

}

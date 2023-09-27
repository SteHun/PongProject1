using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace PongProject1
{
    internal class Paddle
    {
        //Paddle related variables
        private readonly int type;
        internal Vector2 Position;
        internal bool Active = false;
        internal bool Visible = false;
        private float Speed;
        internal readonly int height;
        internal int width;
        internal bool IsFacingRight; //Used to check if the paddle is on the left or right side of the screen
        private float AIerror; //Offset from predicted ball position in y axis, used to make AI not hit every ball perfectly in the middle (and make EasyAI make mistakes)

        //Miscellaneous variables
        private readonly Ball ball;
        private readonly int screenHeight;
        private readonly int screenWidth;
        private Texture2D texture;
        private Vector2 startingPosition;
        private readonly Keys upKey;
        private readonly Keys downKey;
        private SettingsStruct Settings;
        
        private const string paddleRedFileName = "Paddle_red";
        private const string paddleBlueFileName = "Paddle_blue";
        private Texture2D visibleTexture;
        
        //Paddle constructor
        internal Paddle(Game1 game, Keys upKey, Keys downKey, bool isFacingRight, int playerType)
        {
            ball = game.ball;
            type = playerType;
            height = game.Settings.defaultPaddleHeight;
            screenHeight = game.screenHeight;
            screenWidth = game.screenWidth;
            this.upKey = upKey;
            this.downKey = downKey;
            Speed = game.Settings.defaultPaddleSpeed;
            Position = startingPosition;
            IsFacingRight = isFacingRight;
            Settings = game.Settings;
            if (isFacingRight)
            {
                startingPosition = new Vector2(
                    game.Settings.defaultPaddleDistanceFromEdge, 
                    (float)(game.screenHeight - game.Settings.defaultPaddleHeight) / 2);
            }
            else
            {
                startingPosition = new Vector2(
                    game.screenWidth - game.Settings.defaultPaddleDistanceFromEdge, 
                    (float)(game.screenHeight - game.Settings.defaultPaddleHeight) / 2);
            }
            
        }
        
        //Runs upon game starting in Game1.StartGame()
        internal void Reset()
        {
            Position = startingPosition;
        }
        
        //Loads in the visual aspects of the paddle, including its starting position
        internal void Load(ContentManager content, string paddleFileName)
        {
            texture = content.Load<Texture2D>(paddleFileName);
            
            
            if (IsFacingRight)
            {
                visibleTexture = content.Load<Texture2D>(paddleRedFileName);
            }
            else
            {
                visibleTexture = content.Load<Texture2D>(paddleBlueFileName);
                startingPosition.X -= texture.Width;
                Position = startingPosition;
            }
            
            width = texture.Width;
        }

        #region Update and Draw methods
        //Handles the frame by frame inputs of the paddle
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
                case 2: //hard AI
                    totalMovement = HardAIUpdate(gameTime);
                    break;
                case 3: //Impossible AI
                    totalMovement = ImpossibleAIUpdate(gameTime);
                    break;
                default: //Defaults to being controlled by human if error occurs
                    Console.WriteLine($"{type} is not a recognised AI difficulty");
                    totalMovement = HumanUpdate(gameTime);
                    break;
            }
            
            //Updates the position of the paddle and does the height collision check
            Position.Y += totalMovement;
            if (Position.Y < 0)
                Position.Y = 0;
            else if (Position.Y + height > screenHeight)
                Position.Y = screenHeight - height;
        }
        
        //Updates the visuals of the paddle
        internal void Draw(SpriteBatch spriteBatch, string theme)
        {
            if (!Visible)
                return;

            if (theme == "Light")
            {
                spriteBatch.Draw(texture, new Rectangle((int)MathF.Round(Position.X), (int)MathF.Round(Position.Y), texture.Width, height), Color.White);
            }
            else //Theme is equal to Dark
            {
                Rectangle positionRect = new Rectangle(
                    (int)MathF.Round(Position.X - (float)(visibleTexture.Width - texture.Width) / 2),
                    (int)MathF.Round(Position.Y - (float)(visibleTexture.Height - texture.Height) / 2),
                    visibleTexture.Width, visibleTexture.Height
                );
                spriteBatch.Draw(visibleTexture, positionRect, Color.White);
            }
        }
        #endregion

        #region  Input handeling
        //The player controller of the paddle
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

        #region AI
        //The AI when easy mode is selected
        private float EasyAIUpdate(GameTime gameTime)
        {
            // if the ball is facing the AI it moves to it with an error margin
            if ((IsFacingRight && ball.Velocity.X > 0) || (!IsFacingRight && ball.Velocity.X < 0))
            {
                SetNewAIError(1.5f * (ball.Velocity.Length() / Settings.defaultStartingVelocity));
                return 0; //If the ball is going away from the AI, the AI will do nothing
            }
            float targetPosition = ball.TrueYPosition - (float)height / 2 + AIerror;
            
            //Turn the targetPosition into a movement
            if (Position.Y > targetPosition)
            {
                return -MathF.Min(Speed * (float)gameTime.ElapsedGameTime.TotalSeconds, Position.Y - targetPosition);
            }
            return MathF.Min(Speed * (float)gameTime.ElapsedGameTime.TotalSeconds, targetPosition - Position.Y);
        }
        
        //The AI when hard mode is selected
        private float HardAIUpdate(GameTime gameTime)
        {
            float totalMovement = 0;
            float targetPosition; //Aims to get the ball in the middle of the paddle
            int distanceBeforeReaction = 180; //Makes the AI wait a bit before moving to the position of the ball coming it's way
            //Conditions for hard AI paddle to move
            bool leftSideCheck = IsFacingRight && ball.Velocity.X < 0 && ball.Position.X < screenWidth - distanceBeforeReaction;
            bool rightSideCheck = !IsFacingRight && ball.Velocity.X > 0 && ball.Position.X > distanceBeforeReaction;
            
            // if the ball is facing the AI it moves to it
            if (leftSideCheck || rightSideCheck)
            {
                targetPosition = GetPredictedBallPosition(gameTime) - (float)height / 2 + AIerror;
            }
            else //If the ball is moving away from the AI then the AI doesn't do anything
            {
                SetNewAIError(1); //AI error is used just to add variety to the returns, it actually makes the AI stronger
                return 0;
            }
            
            //Turn the targetPosition into a movement
            if (Position.Y > targetPosition)
            {
                totalMovement -= MathF.Min(Speed * (float)gameTime.ElapsedGameTime.TotalSeconds, Position.Y - targetPosition);
            }
            else if (Position.Y < targetPosition)
            {
                totalMovement += MathF.Min(Speed * (float)gameTime.ElapsedGameTime.TotalSeconds, targetPosition - Position.Y);
            }
            return totalMovement;
        }
        
        private float ImpossibleAIUpdate(GameTime gameTime)
        {
            float totalMovement = 0;
            float targetPosition;
            
            //Calculate where the ball is going to be
            if ((IsFacingRight && ball.Velocity.X < 0) || (!IsFacingRight && ball.Velocity.X > 0)) //If the ball is facing your way
            {
                targetPosition = GetPredictedBallPosition(gameTime) - (float)height / 2 + AIerror;
            }
            else //Moves to the middle of the screen if the ball is moving towards the opponent (from the middle it's easier to react)
            {
                targetPosition = (float)(screenHeight - height) / 2;
                SetNewAIError(1); //AI error is used just to add variety to the returns, it actually makes the AI stronger
            }
            
            //Turns the targetPosition into a movement
            if (Position.Y > targetPosition)
            {
                totalMovement -= MathF.Min(Speed * (float)gameTime.ElapsedGameTime.TotalSeconds, Position.Y - targetPosition);
            }
            else if (Position.Y < targetPosition)
            {
                totalMovement += MathF.Min(Speed * (float)gameTime.ElapsedGameTime.TotalSeconds, targetPosition - Position.Y);
            }
            return totalMovement;
        }

        //Used to calculate where the ball will be when at the X value of the paddle
        //Used to calculate targetPosition in the AI difficulty methods
        private float GetPredictedBallPosition(GameTime gameTime)
        {
            //Creates a fake ball and updates it's position until it would be where the ball will be when ball X == paddle X
            //The Y position of the fake ball is where the AI wants to move to
            FakeBall fakeBall = new FakeBall(ball, screenHeight);
            float leftXpos, rightXpos;
            if (IsFacingRight)
            {
                leftXpos = Position.X;
                rightXpos = screenWidth - Position.X;
            }
            else
            {
                rightXpos = Position.X;
                leftXpos = screenWidth - Position.X;
            }

            while (fakeBall.Position.X >= leftXpos && fakeBall.Position.X <= rightXpos)
            {
                // NOTE: This could be inaccurate
                // This assumes the game runs at a consistent framerate
                fakeBall.Update(gameTime);
            }
            return fakeBall.TrueYPosition;
        }

        //The Easy difficulty of the AI has a margin for error to make mistakes, calculated by this method
        private void SetNewAIError(float errorOvershoot)
        {
            Random rng = new Random();
            
            // set rng between a range of -0.5 * height - errorMargin and 0.5 * height + errorMargin
            AIerror = ((float)rng.NextDouble() - 0.5f) * (height * 0.8f + errorOvershoot);
        }
        #endregion
        #endregion
    }

    #region Fake ball
    //Used to calculate where the ball will be when crossing the x axis of a paddle
    internal class FakeBall
    {
        internal Vector2 Position;
        internal float TrueYPosition
        {
            get { return Position.Y + (float)height / 2; }
        }
        internal Vector2 Velocity;
        private int screenHeight;
        private int height;
        internal FakeBall(Ball ball, int screenHeight)
        {
            this.screenHeight = screenHeight;
            Position = ball.Position;
            Velocity = ball.Velocity;
            height = ball.Texture.Height;
        }

        //Simulates the movement of the ball
        internal void Update(GameTime gameTime)
        {
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            // bounce the ball
            if (Position.Y <= 0)
            {
                Velocity.Y *= -1;
                Position.Y = 0;
            }
            else if (Position.Y + height >= screenHeight)
            {
                Velocity.Y *= -1;
                Position.Y = screenHeight - height;
            }
        }
    }
    #endregion
}

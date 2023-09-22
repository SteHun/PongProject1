using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

/* ----EXTRA FEATURES----
- A menu to start the game and to change settings
- The option to select keybinds
- The option to select a theme
- 3 difficulties of computer players
- Sound effects
- Pause
- Exiting a match before it is finished
(there may be more I forgot) 
 */

namespace PongProject1
{
    internal struct SettingsStruct
    {
        //Keybinds
        internal Keys menuDownKey = Keys.S;
        internal Keys menuUpKey = Keys.W;
        internal Keys menuSelectKey = Keys.Enter;
        internal Keys player1DownKey = Keys.S;
        internal Keys player1UpKey = Keys.W;
        internal Keys player2DownKey = Keys.Down;
        internal Keys player2UpKey = Keys.Up;
        internal Keys pauseKey = Keys.P;
        internal Keys quitKey = Keys.Escape;
        internal Keys quickStartKey = Keys.F1;

        //Paddle related variables
        internal readonly int defaultPaddleHeight = 96;
        internal readonly int defaultPaddleSpeed = 300;
        internal readonly int defaultPaddleDistanceFromEdge = 40;

        //Ball related variables
        internal readonly float defaultStartingVelocity = 240;
        internal readonly float defaultVelocityIncrement = 30;
        internal readonly float defaultMinServeAngle = 60;
        internal readonly float defaultMaxServeAngle = 120;
        internal readonly float defaultMinBounceAngle = 40;
        internal readonly float defaultMaxBounceAngle = 140;
        internal readonly double defaultBallRespawnTime = 3000;
        public SettingsStruct()
        {
            
        }
    }
    
    public class Game1 : Game
    {
        //Initialization of files and application
        internal SettingsStruct Settings = new ();
        private const string ballFileName = "ball"; //Old ball
        private const string biggerBallFileName = "Bigger_Ball";
        private const string neonBallFileName = "Neon_ball";
        private const string lifeIconFileName = "ball";
        private const string player1PaddleFileName = "bluePaddle";
        private const string player2PaddleFileName = "redPaddle";
        private const string paddleSizeFileName = "Paddle_size";
        internal readonly int screenWidth;
        internal readonly int screenHeight;
        private const string pausedText = "GAME PAUSED";
        
        private readonly GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        private Texture2D lifeIcon;
        public SpriteFont arialFont;
        private SoundEffect winSFX;
        public Color backgroundColor;

        //Game settings and values
        private Menu menu;
        internal Ball ball;
        private Paddle player1Paddle;
        private Paddle player2Paddle;
        private int player1Lives;
        private int player2Lives;
        
        //Checks used to see if we are in game
        private bool menuOpen = true;
        private bool gamePaused;
        private bool pauseKeyPressedLastFrame;

        public Game1()
        {
            //Initialization of files and application
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            screenWidth = graphics.PreferredBackBufferWidth;
            screenHeight = graphics.PreferredBackBufferHeight;
        }

        #region  Initialization
        protected override void Initialize()
        {
            //Start in the menu
            menu = new Menu(this);
            menu.InitializeMenu();
            
            //Give application a name
            Window.Title = "Pong by Philip and Stefan";
            
            base.Initialize();
        }

        //Load textures
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            lifeIcon = Content.Load<Texture2D>(lifeIconFileName);
            arialFont = Content.Load<SpriteFont>("arialFont");
            winSFX = Content.Load<SoundEffect>("win");
            base.LoadContent();
        }
        #endregion

        #region Match state methods
        internal void StartGame(int livesPlayer1, int livesPlayer2, int typePlayer1, int typePlayer2)
        {
            //Setup ball and paddles according to settings
            ball = new Ball(this);
            player1Paddle = new Paddle(this, Settings.player1UpKey, Settings.player1DownKey, true, typePlayer1);
            player2Paddle = new Paddle(this, Settings.player2UpKey, Settings.player2DownKey, false, typePlayer2);
            if (menu.theme[menu.themeIndex] == "Light")
            {
                player1Paddle.Load(Content, player1PaddleFileName);
                player2Paddle.Load(Content, player2PaddleFileName);
                
                ball.Load(Content, biggerBallFileName, new[] {player1Paddle, player2Paddle});
            }
            else
            {
                player1Paddle.Load(Content, paddleSizeFileName);
                player2Paddle.Load(Content, paddleSizeFileName);
                
                ball.Load(Content, neonBallFileName, new[] {player1Paddle, player2Paddle});
            }
            
            //Old ball, remove if satisfied with changes
            //ball.Load(Content, ballFileName, new[] { player1Paddle, player2Paddle });
            
            //Set up other settings
            menuOpen = false;
            player1Lives = livesPlayer1;
            player2Lives = livesPlayer2;
            player1Paddle.Active = true;
            player1Paddle.Visible = true;
            player2Paddle.Active = true;
            player2Paddle.Visible = true;
            ball.Respawn();
            player1Paddle.Reset();
            player2Paddle.Reset();
        }

        //Exit() can only be accessed by Game1.cs, ExitGame() is project wide
        internal void ExitGame()
        {
            Exit();
        }

        //When stopping a match (doesn't exit the program)
        private void QuitMatch()
        {
            //Menu handling
            menuOpen = true;
            menu.menuIndex = 0;
            menu.noInputWaitTime = 360;
            gamePaused = false;
            //Only go to main menu if not on the winner screen
            if(menu.menuState != Menu.MenuState.Winner)
            {
                menu.menuState = Menu.MenuState.MainMenu;
            }
            
            //Disabling paddles and ball
            player1Paddle.Active = false;
            player1Paddle.Visible = false;
            player2Paddle.Active = false;
            player2Paddle.Visible = false;
            ball.Active = false;
            ball.Visible = false;
        }
        #endregion

        #region Update and Draw Method
        protected override void Update(GameTime gameTime)
        {
            //Don't run rest of Update() if menu is open
            if (menuOpen)
            {
                menu.UpdateMenu();
                base.Update(gameTime);
                return;
            }
            
            //Exit program if pressing shortcut
            if (Keyboard.GetState().IsKeyDown(Settings.quitKey) && !menu.quitHeld)
            {
                menu.quitHeld = true;
                QuitMatch();
            }

            //Pause game if pressing shortcut
            if (Keyboard.GetState().IsKeyDown(Settings.pauseKey) && !pauseKeyPressedLastFrame)
            {
                gamePaused = !gamePaused;
                pauseKeyPressedLastFrame = true;
            }
            else if (!Keyboard.GetState().IsKeyDown(Settings.pauseKey))
            {
                pauseKeyPressedLastFrame = false; 
            }

            //Don't run rest of Update() if game is paused
            if (gamePaused)
            {
                base.Update(gameTime);
                return;
            }
            
            //Update ball
            ball.Update(gameTime);

            //Update paddle
            player1Paddle.Update(gameTime);
            player2Paddle.Update(gameTime);

            //Check if a player has scored
            if (ball.PlayerHasScored)
            {
                //If player 1 has scored
                if (ball.ScoringPlayer == 1)
                {
                    player2Lives--;
                    if (player2Lives <= 0) //If the opposing player lost all their lives
                    {
                        menu.GameOver("Player 1");
                        QuitMatch();
                        winSFX.Play();
                        base.Update(gameTime);
                        return;
                    }
                }
                else //If player 2 has scored
                {
                    player1Lives--;
                    if (player1Lives <= 0) //If the opposing player lost all their lives
                    {
                        menu.GameOver("Player 2");
                        QuitMatch();
                        winSFX.Play();
                        base.Update(gameTime);
                        return;
                    }
                }

                //In case of error: If a player has scored but not player 1 or 2
                ball.Respawn(gameTime);
            }
            
            //Protecting copyright
            if (!(Window.Title.ToLower().Contains("philip") && Window.Title.ToLower().Contains("stefan")))
                throw (new Exception("The program has thrown an error to protect copyright!"));
            base.Update(gameTime);
        }

        //Visual updates
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(backgroundColor);
            spriteBatch.Begin();
            //If the menu is open draw menu elements
            if (menuOpen)
            {
                menu.DrawMenu();
            }
            else //A match is in process
            {
                //Draw ball and paddles
                player1Paddle.Draw(spriteBatch, menu.theme[menu.themeIndex]);
                player2Paddle.Draw(spriteBatch, menu.theme[menu.themeIndex]);
                ball.Draw(gameTime, spriteBatch);
                
                // Draw lives of player 1
                for (int i = 0; i < player1Lives; i++)
                {
                    spriteBatch.Draw(lifeIcon, new Vector2(lifeIcon.Width * i, 0), Color.White);
                }
                // Draw lives of player 2
                for (int i = 0; i < player2Lives; i++)
                {
                    spriteBatch.Draw(lifeIcon, new Vector2(graphics.PreferredBackBufferWidth - lifeIcon.Width * (i + 1), 0), Color.White);
                }
            }

            //If the game is paused
            if (gamePaused)
            {
                //Show a pause screen
                Vector2 textSize = arialFont.MeasureString(pausedText);
                Vector2 textPosition = new Vector2(screenWidth - textSize.X, screenHeight - textSize.Y) / 2;
                spriteBatch.DrawString(arialFont, pausedText, textPosition, Color.Black);
            }
            
            spriteBatch.End();

            base.Draw(gameTime);
        }
        #endregion
    }
}
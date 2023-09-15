using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace PongProject1
{
    public class Menu //TODO make this class to be accessed by Game1 by changing core methods
    {
        private Game1 game;

        //Menu things
        private byte menuIndex; //Keeps track of what the player has selected in the menu
        private MenuState menuState; //What menu the player is currently in
        private Vector2 menuTextTopLeftPosition; //The position where the first element of the menu is drawn
        private Vector2 menu2ndRowTopLeftPosition; //The position where the first element of the 2nd row (in case it exists) of the menu is drawn
        private Vector2 menuTextGap; //The gap between every element of text
        private short noInputWaitTime; //A delay before a tip on menu controls is shown on the main menu in frames
        private byte quitWaitTime; //A delay before the quit button can be pressed in frames 

        //Menu toggle lists
        private string[] playerType = {"Human", "AI Easy", "AI Hard"};
        private int[] lives = { 1, 2, 3, 4, 5 };
        private byte player1TypeIndex; //Indexes are used to remember what the player has selected in a list
        private byte player1LivesIndex;
        private byte player2TypeIndex;
        private byte player2LivesIndex;

        //Keybinds
        private byte variablePosition; //This is used to indicate what keybind is being changed
        private Keys menuUpKey;
        private Keys menuDownKey;
        private Keys menuSelectKey;
        private Keys menuQuitKey;
        private Keys debugModeKey;
        private Keys quickStartKey;
        private Keys pauseKey;
        private Keys player1UpKey;
        private Keys player1DownKey;
        private Keys player2UpKey;
        private Keys player2DownKey;

        private bool menuUpHeld; //Used for menu movement
        private bool menuDownHeld; //Used for menu movement
        private bool menuSelectHeld;
        private bool debugModeHeld;
        private bool quickStartHeld;
        private bool pauseHeld;
        
        private string winningPlayer = "";

        public Menu(Game1 game)
        {
            this.game = game;
        }

        public void InitializeMenu()
        {
            //Menu setup
            menuIndex = 0;
            menuState = MenuState.MainMenu;
            menuTextTopLeftPosition = new Vector2(50, 50);
            menu2ndRowTopLeftPosition = new Vector2(350, 50);
            menuTextGap = new Vector2(0, 35);
            noInputWaitTime = 900; //In frames (60 fps, so 15 seconds)

            //Default keybinds setup
            variablePosition = 0;
            menuUpKey = Keys.W;
            menuDownKey = Keys.S;
            menuSelectKey = Keys.Enter;
            menuQuitKey = Keys.Escape;
            debugModeKey = Keys.O;
            quickStartKey = Keys.I;
            pauseKey = Keys.P;
            player1UpKey = Keys.W;
            player1DownKey = Keys.S;
            player2UpKey = Keys.I;
            player2DownKey = Keys.K;
        }
        
        public void UpdateMenu()
        {
            MenuMovement();
            CheckForKeybind();

            //TODO Expand and finish other menus
            //TODO (missing how to play, and the functionality of music and sound effect toggles (if we are to add those))

            if(quitWaitTime > 0) quitWaitTime--;
            if (noInputWaitTime > 0) noInputWaitTime--;
            if (Keyboard.GetState().IsKeyDown(menuUpKey) || Keyboard.GetState().IsKeyDown(menuDownKey)) //Removes tip on how to use menu if menu button is pressed
            {
                noInputWaitTime = 900;
            }
        }

        public void DrawMenu()
        {
            switch (menuState)
            {
                case MenuState.MainMenu:
                    MenuString("Start", 0);
                    MenuString("Settings", 1);
                    MenuString("Hot to play", 2);
                    MenuString("Quit", 3);

                    //The tooltip message if the player doesn't move in the main menu for 15 seconds
                    if (noInputWaitTime == 0)
                    {
                        game.spriteBatch.DrawString(game.arialFont, $"Use {menuUpKey.ToString()} and {menuDownKey.ToString()} to move in the menu ",
                            new Vector2(50, 350), Color.Gray);
                        game.spriteBatch.DrawString(game.arialFont, $"and {menuSelectKey.ToString()} to select an option",
                            new Vector2(50, 385), Color.Gray);
                    }
                    break;

                case MenuState.Lobby:
                    //First row (What rules/handicap you are selecting)
                    MenuString("Start", 0);
                    MenuString("Player 1", 1);
                    MenuString("Lives", 2);
                    MenuString("Player 2", 3);
                    MenuString("Lives", 4);
                    MenuString("Back", 5);
                    
                    //Second row (The value of the rule/setting)
                    //(Start text here)
                    MenuString2ndRow(playerType[player1TypeIndex], 1);
                    MenuString2ndRow(lives[player1LivesIndex].ToString(), 2);
                    MenuString2ndRow(playerType[player2TypeIndex], 3);
                    MenuString2ndRow(lives[player2LivesIndex].ToString(), 4);
                    //(Back text here)
                    break;

                case MenuState.Settings:
                    MenuString("Controls", 0);
                    MenuString("Music", 1);
                    MenuString("Sound effects", 2);
                    MenuString("Back", 3);
                    break;

                case MenuState.Controls:
                    //First row (what the keybinds do)
                    MenuString("Menu move up", 0);
                    MenuString("Menu move down", 1);
                    MenuString("Menu select", 2);
                    MenuString("Quick exit", 3);
                    MenuString("Enter debug mode", 4);
                    MenuString("Quick start mode", 5);
                    MenuString("Pause", 6);
                    MenuString("Player 1 up", 7);
                    MenuString("Player 1 down", 8);
                    MenuString("Player 2 up", 9);
                    MenuString("Player 2 down", 10);
                    MenuString("Back", 11);
                    
                    //Second row (what the keybinds are)
                    MenuString2ndRow(menuUpKey.ToString(), 0);
                    MenuString2ndRow(menuDownKey.ToString(), 1);
                    MenuString2ndRow(menuSelectKey.ToString(), 2);
                    MenuString2ndRow(menuQuitKey.ToString(), 3);
                    MenuString2ndRow(debugModeKey.ToString(), 4);
                    MenuString2ndRow(quickStartKey.ToString(), 5);
                    MenuString2ndRow(pauseKey.ToString(), 6);
                    MenuString2ndRow(player1UpKey.ToString(), 7);
                    MenuString2ndRow(player1DownKey.ToString(), 8);
                    MenuString2ndRow(player2UpKey.ToString(), 9);
                    MenuString2ndRow(player2DownKey.ToString(), 10);
                    break;

                case MenuState.SelectingKey:
                    game.spriteBatch.DrawString(game.arialFont, "Press the key you want to set as the keybind for this control", new Vector2(200, 400), Color.White);
                    break;

                case MenuState.Winner:
                    game.spriteBatch.DrawString(game.arialFont, $"{winningPlayer} has won!", new Vector2(200, 300), Color.White);
                    break;
            }
        }

        #region Menu Methods
        private Color MenuEntryColor(byte menuPosition) //Highlights text if selected
        {
            if (menuPosition == menuIndex)
            {
                return Color.Yellow;
            }

            return Color.White;
        }

        private byte GetMenuLength() //Gets the byte value of the last element in the current menu enum
        {
            byte menuLength = 0;
            switch (menuState)
            {
                case MenuState.MainMenu:
                    menuLength = (byte)Enum.GetNames(typeof(MainMenu)).Length;
                    break;
                case MenuState.Lobby:
                    menuLength = (byte)Enum.GetNames(typeof(Lobby)).Length;
                    break;
                case MenuState.Settings:
                    menuLength = (byte)Enum.GetNames(typeof(Settings)).Length;
                    break;
                case MenuState.Controls:
                    menuLength = (byte)Enum.GetNames(typeof(Controls)).Length;
                    break;

            }

            menuLength--; //Menu starts counting at 0, so length is 1 more than highest value element, this corrects for that
            return menuLength;
        }

        private void MenuMovement() //Handles all the movement in menus
        {
            //In case of bugs
            if (menuIndex > GetMenuLength())
            {
                menuIndex = 0;
            }

            //Quick quit key
            if (Keyboard.GetState().IsKeyDown(menuQuitKey) && quitWaitTime == 0 && menuState != MenuState.SelectingKey)
            {
                game.ExitGame();
            }

            if (Keyboard.GetState().IsKeyDown(quickStartKey) && !quickStartHeld)
            {
                quickStartHeld = true;
                game.StartGame(lives[player1LivesIndex], lives[player2LivesIndex]);
            }

            //Move to a different menu / some other action when select key is pressed
            if (Keyboard.GetState().IsKeyDown(menuSelectKey) && !menuSelectHeld)
            {
                menuSelectHeld = true;
                MenuEffect();
            }

            //Move a selection higher and wrap around if necessary
            if (Keyboard.GetState().IsKeyDown(menuUpKey) && !menuUpHeld)
            {
                menuUpHeld = true;

                if (menuIndex == 0)
                {
                    menuIndex = GetMenuLength();
                }
                else
                {
                    menuIndex--;
                }
            }

            //Move a selection lower and wrap around if necessary
            if (Keyboard.GetState().IsKeyDown(menuDownKey) && !menuDownHeld)
            {
                menuDownHeld = true;
                if (menuIndex == GetMenuLength()) //Checks if menuIndex is at the last element in the enum
                {
                    menuIndex = 0;
                }
                else
                {
                    menuIndex++;
                }
            }

            //Keyboard.GetStart().IsKeyDown is active when a key is held down, this reset makes the movement in menu's smoother
            //by requiring the player to let go of the key to continue moving in the same direction in a menu
            if (Keyboard.GetState().IsKeyUp(menuUpKey))
            {
                menuUpHeld = false;
            }
            
            if (Keyboard.GetState().IsKeyUp(menuDownKey))
            {
                menuDownHeld = false;
            }
            
            if (Keyboard.GetState().IsKeyUp(menuSelectKey))
            {
                menuSelectHeld = false;
            }

            if (Keyboard.GetState().IsKeyUp(quickStartKey))
            {
                quickStartHeld = false;
            }
        }

        private void MenuEffect() //Handles the effect of pressing the select button over a menu element
        {
            switch (menuState)
            {
                case MenuState.MainMenu:
                    switch (menuIndex)
                    {
                        case (byte)MainMenu.Start:
                            menuState = MenuState.Lobby;
                            break;
                        case (byte)MainMenu.Settings:
                            menuState = MenuState.Settings;
                            break;
                        case (byte)MainMenu.HowToPlay:
                            //Add how to play screen/text
                            menuState = MenuState.HowToPlay;
                            break;
                        case (byte)MainMenu.Quit:
                            game.ExitGame();
                            break;
                    }
                    menuIndex = 0;
                    break;

                case MenuState.Lobby:
                    switch (menuIndex)
                    {
                        case (byte)Lobby.Start:
                            game.StartGame(lives[player1LivesIndex], lives[player2LivesIndex]);
                            
                            break;
                        case (byte)Lobby.Player1Player:
                            player1TypeIndex = (byte)ToggleNext(playerType, player1TypeIndex);
                            break;
                        case (byte)Lobby.Player1Lives:
                            player1LivesIndex = (byte)ToggleNext(lives, player1LivesIndex);
                            break;
                        case (byte)Lobby.Player2Player:
                            player2TypeIndex = (byte)ToggleNext(playerType, player2TypeIndex);
                            break;
                        case (byte)Lobby.Player2Lives:
                            player2LivesIndex = (byte)ToggleNext(lives, player2LivesIndex);
                            break;
                        case (byte)Lobby.Back:
                            menuState = MenuState.MainMenu;
                            menuIndex = 0;
                            break;
                    }
                    break;

                case MenuState.Settings:
                    switch (menuIndex)
                    {
                        case (byte)Settings.Controls:
                            menuState = MenuState.Controls;
                            menuIndex = 0;
                            break;
                        case (byte)Settings.Music:
                            //Add togglable music
                            break;
                        case (byte)Settings.SoundEffects:
                            //Add togglable sound effects
                            break;
                        case (byte)Settings.Back:
                            menuState = MenuState.MainMenu;
                            menuIndex = 0;
                            break;
                    }
                    break;

                case MenuState.Controls:
                    switch (menuIndex)
                    {
                        case (byte)Controls.MenuMoveUp:
                            variablePosition = 0;
                            break;
                        case (byte)Controls.MenuMoveDown:
                            variablePosition = 1;
                            break;
                        case (byte)Controls.MenuSelect:
                            variablePosition = 2;
                            break;
                        case (byte)Controls.QuickQuit:
                            variablePosition = 3;
                            break;
                        case (byte)Controls.QuickStartMode:
                            variablePosition = 4;
                            break;
                        case (byte)Controls.DebugMode:
                            variablePosition = 5;
                            break;
                        case (byte)Controls.Pause:
                            variablePosition = 6;
                            break;
                        case (byte)Controls.Player1Up:
                            variablePosition = 7;
                            break;
                        case (byte)Controls.Player1Down:
                            variablePosition = 8;
                            break;
                        case (byte)Controls.Player2Up:
                            variablePosition = 9;
                            break;
                        case (byte)Controls.Player2Down:
                            variablePosition = 10;
                            break;
                        case (byte)Controls.Back:
                            menuState = MenuState.Settings;
                            menuIndex = 0;
                            return;
                    }
                    
                    menuState = MenuState.SelectingKey;
                    break;

                case MenuState.Winner:
                    menuState = MenuState.MainMenu;
                    menuIndex = 0;
                    break;
            }
        }
        #endregion

        private void CheckForKeybind()
        {
            if (menuState != MenuState.SelectingKey) return;
            Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();
            if (pressedKeys.Length <= 0) return;
            if (menuSelectHeld) return;

            switch (variablePosition)
            {
                case 0:
                    menuUpKey = pressedKeys[0];
                    menuIndex++; //Shifts menu index because menuUpHeld will be false and cause an accidental movement of selected menu element
                    break;
                case 1:
                    menuDownKey = pressedKeys[0];
                    menuIndex--; //Shifts menu index because menuDownHeld will be false and cause an accidental movement of selected menu element
                    break;
                case 2:
                    menuSelectKey = pressedKeys[0];
                    break;
                case 3:
                    menuQuitKey = pressedKeys[0];
                    quitWaitTime = 30;
                    break;
            }

            menuSelectHeld = true;
            menuState = MenuState.Controls;
        }

        private int ToggleNext(int[] array, byte index) //Get next element in array (and loop if necessary)
        {
            if (index == array.Length - 1)
            {
                return 0;
            }

            return index+1;
        }

        private int ToggleNext(string[] array, byte index) //Get next element in array (and loop if necessary)
        {
            if (index == array.Length - 1)
            {
                return 0;
            }

            return index + 1;
        }

        public void GameOver(string winner)
        {
            menuState = MenuState.Winner;
            menuIndex = 0;
            winningPlayer = winner;
        }

        //This method is used to more quickly type DrawString(), which has multiple parameters that are constantly the same
        private void MenuString(string text, byte index)
        {
            game.spriteBatch.DrawString(game.arialFont, text, menuTextTopLeftPosition + menuTextGap*index, MenuEntryColor(index));
        }

        //Same but with the second row vector
        private void MenuString2ndRow(string text, byte index)
        {
            game.spriteBatch.DrawString(game.arialFont, text, menu2ndRowTopLeftPosition + menuTextGap*index, MenuEntryColor(index));
        }

        #region Menu Enums

        enum MenuState : byte //What menu is currently selected
        {
            MainMenu,
            Lobby,
            Settings,
            HowToPlay,
            Controls,
            SelectingKey,
            Winner,
        }

        enum MainMenu : byte //All selectable options within MainMenu
        {
            Start,
            Settings,
            HowToPlay,
            Quit
        }

        enum Lobby : byte //All selectable options just before a match (can select opponent, handicap and rules)
        {
            Start,
            Player1Player, //Human or AI (and what difficulty)
            Player1Lives,
            Player2Player, //Human or AI (and what difficulty)
            Player2Lives,
            Back,
        }

        enum Settings : byte //All selectable options within Settings
        {
            Controls,
            Music, //If we add this
            SoundEffects, //If we add this
            Back,
        }

        enum Controls : byte
        {
            MenuMoveUp,
            MenuMoveDown,
            MenuSelect,
            QuickQuit,
            DebugMode,
            QuickStartMode,
            Pause,
            Player1Up,
            Player1Down,
            Player2Up,
            Player2Down,
            Back,
        }
        #endregion

    }
}
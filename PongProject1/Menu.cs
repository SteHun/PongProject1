using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
// ReSharper disable InconsistentNaming

namespace PongProject1
{
    public class Menu
    {
        private readonly Game1 game;

        //Menu things
        public byte menuIndex; //Keeps track of what the player has selected in the menu
        public MenuState menuState; //What menu the player is currently in
        private Vector2 menuTextTopLeftPosition; //The position where the first element of the menu is drawn
        private Vector2 menu2ndRowTopLeftPosition; //The position where the first element of the 2nd row (in case it exists) of the menu is drawn
        private Vector2 menuTextGap; //The gap between every element of text
        private Color textColor; //The color of the text when not highlighted
        private short noInputWaitTime; //A delay before a tip on menu controls is shown on the main menu in frames
        private byte quitWaitTime; //A delay before the quit button can be pressed in frames 

        //Menu toggle lists
        private readonly string[] playerType = {"Human", "AI Easy", "AI Hard", "AI Impossible"};
        private readonly int[] lives = {1, 2, 3, 4, 5};
        private readonly string[] theme = { "Light", "Dark" };
        private readonly float[] volume = {0f, 0.25f, 0.5f, 0.75f, 1f};
        private byte player1TypeIndex; //Indexes are used to remember what the player has selected in a list
        private byte player1LivesIndex = 2;
        private byte player2TypeIndex;
        private byte player2LivesIndex = 2;
        private byte themeIndex;
        private byte soundEffectsIndex = 4; //Starts at 100% volume

        //Keybinds
        private byte variablePosition; //This is used to indicate what keybind is being changed
        private Keys menuUpKey; //These are menu specific keybinds, other keybinds are stored in Game1 Settings struct
        private Keys menuDownKey;
        private Keys menuSelectKey;
        private Keys quickStartKey;

        //Checks if key is being held down, used to avoid inputting action every frame and instead only on first frame of press
        private bool menuUpHeld;
        private bool menuDownHeld;
        private bool menuSelectHeld;
        private bool quickStartHeld;
        public bool quitHeld;
        
        private string winningPlayer;

        public Menu(Game1 game)
        {
            this.game = game;
        }

        #region Initialize method
        public void InitializeMenu()
        {
            SoundEffect.MasterVolume = volume[soundEffectsIndex];
            
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
            quickStartKey = Keys.F1;
            
            ChangeTheme();
        }
        #endregion

        #region Update method
        public void UpdateMenu()
        {
            //Controls movement and runs code if a keybind is pressed
            MenuMovement();
            CheckForKeybind();

            //Update timers
            if(quitWaitTime > 0) quitWaitTime--;
            if (noInputWaitTime > 0) noInputWaitTime--;
            
            //Give a popup text if the player hasn't moved on the main menu for a while (indicating they don't know the controls)
            if (Keyboard.GetState().IsKeyDown(menuUpKey) || Keyboard.GetState().IsKeyDown(menuDownKey)) //Removes tip on how to use menu if menu button is pressed
            {
                noInputWaitTime = 900;
            }
        }
        #endregion

        #region Draw method
        //Contains all the text that needs to be displayed depending on the active menu
        //MenuString() is a shorter version of spriteBatch.DrawString() with a lot of default values filled in
        //The index is used for positioning of text and to keep track what to highlight
        public void DrawMenu()
        {
            switch (menuState)
            {
                case MenuState.MainMenu:
                    MenuString("Start", 0);
                    MenuString("Settings", 1);
                    MenuString("Quit", 2);

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
                    MenuString("Theme", 1);
                    MenuString("Sound FX", 2);
                    MenuString("Back", 3);
                    
                    MenuString2ndRow(theme[themeIndex], 1);
                    MenuString2ndRow($"{volume[soundEffectsIndex]*100}%", 2);
                    break;

                case MenuState.Controls:
                    //First row (what the keybinds do)
                    MenuString("Menu move up", 0);
                    MenuString("Menu move down", 1);
                    MenuString("Menu select", 2);
                    MenuString("Quick exit", 3);
                    MenuString("Quick start mode", 4);
                    MenuString("Pause", 5);
                    MenuString("Player 1 up", 6);
                    MenuString("Player 1 down", 7);
                    MenuString("Player 2 up", 8);
                    MenuString("Player 2 down", 9);
                    MenuString("Back", 10);
                    
                    //Second row (what the keybinds are)
                    MenuString2ndRow(menuUpKey.ToString(), 0);
                    MenuString2ndRow(menuDownKey.ToString(), 1);
                    MenuString2ndRow(menuSelectKey.ToString(), 2);
                    MenuString2ndRow(game.Settings.quitKey.ToString(), 3);
                    MenuString2ndRow(quickStartKey.ToString(), 4);
                    MenuString2ndRow(game.Settings.pauseKey.ToString(), 5);
                    MenuString2ndRow(game.Settings.player1UpKey.ToString(), 6);
                    MenuString2ndRow(game.Settings.player1DownKey.ToString(), 7);
                    MenuString2ndRow(game.Settings.player2UpKey.ToString(), 8);
                    MenuString2ndRow(game.Settings.player2DownKey.ToString(), 9);
                    break;

                case MenuState.SelectingKey:
                    game.spriteBatch.DrawString(game.arialFont, "Press the key you want to set \nas the keybind for this control", new Vector2(100, 320), textColor);
                    break;

                case MenuState.Winner:
                    game.spriteBatch.DrawString(game.arialFont, $"{winningPlayer} has won!", new Vector2(200, 300), textColor);
                    break;
            }
        }
        #endregion

        #region Menu Methods
        private Color MenuEntryColor(byte menuPosition) //Highlights text if selected
        {
            if (menuPosition == menuIndex)
            {
                return Color.Yellow;
            }

            return textColor;
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
                    menuLength = 12; //Controls does not contain an enum element for every element in the list, so hardcoded
                    break;
            }

            menuLength--; //Menu starts counting at 0, so length is 1 more than highest value element, this corrects for that
            return menuLength;
        }

        private void MenuMovement() //Handles all the movement in menus
        {
            //Shouldn't happen, but in case of bugs
            if (menuIndex > GetMenuLength())
            {
                menuIndex = 0;
            }

            //Quick quit key
            if (Keyboard.GetState().IsKeyDown(game.Settings.quitKey) && quitWaitTime == 0 && menuState != MenuState.SelectingKey && !quitHeld)
            {
                game.ExitGame();
            }

            //Quick start match key
            if (Keyboard.GetState().IsKeyDown(quickStartKey) && !quickStartHeld)
            {
                quickStartHeld = true;
                game.StartGame(lives[player1LivesIndex], lives[player2LivesIndex], player1TypeIndex, player2TypeIndex);
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

            if (Keyboard.GetState().IsKeyUp(game.Settings.quitKey))
            {
                quitHeld = false;
            }
        }

        //Handles the effect of pressing the select button over a menu element
        private void MenuEffect() 
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
                            game.StartGame(lives[player1LivesIndex], lives[player2LivesIndex], player1TypeIndex, player2TypeIndex);
                            
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
                        case (byte)Settings.Theme:
                            themeIndex = (byte)ToggleNext(theme, themeIndex);
                            ChangeTheme();
                            break;
                        case (byte)Settings.SoundEffects:
                            soundEffectsIndex = (byte)ToggleNext(volume, soundEffectsIndex);
                            SoundEffect.MasterVolume = volume[soundEffectsIndex]; //Actually changes volume
                            break;
                        case (byte)Settings.Back:
                            menuState = MenuState.MainMenu;
                            menuIndex = 0;
                            break;
                    }
                    break;

                case MenuState.Controls:
                    if(menuIndex == 10) //Back button
                    {
                        menuState = MenuState.Settings;
                        menuIndex = 0;
                        return;
                    }

                    variablePosition = menuIndex;
                    menuState = MenuState.SelectingKey;
                    break;

                case MenuState.Winner:
                    menuState = MenuState.MainMenu;
                    menuIndex = 0;
                    break;
            }
        }
        #endregion

        #region Other methods
        private void CheckForKeybind() //Used to check what the new key is for a keybind
        {
            //Return if not supposed to be selecting a key
            if (menuState != MenuState.SelectingKey) return;
            Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();
            if (pressedKeys.Length <= 0) return;
            if (menuSelectHeld) return; //Without this every key is automatically set to the select key

            //Find out which variable that holds the keybind the new key should be assigned to
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
                    game.Settings.quitKey = pressedKeys[0];
                    quitWaitTime = 30;
                    break;
                case 4:
                    quickStartKey = pressedKeys[0];
                    break;
                case 5:
                    game.Settings.pauseKey = pressedKeys[0];
                    break;
                case 6:
                    game.Settings.player1UpKey = pressedKeys[0];
                    break;
                case 7:
                    game.Settings.player1DownKey = pressedKeys[0];
                    break;
                case 8:
                    game.Settings.player2UpKey = pressedKeys[0];
                    break;
                case 9:
                    game.Settings.player2DownKey = pressedKeys[0];
                    break;
            }

            //Move back to controls menu
            menuSelectHeld = true;
            quitHeld = true;
            menuState = MenuState.Controls;
        }

        #region ToggleNext method variants
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
        
        private int ToggleNext(float[] array, byte index) //Get next element in array (and loop if necessary)
        {
            if (index == array.Length - 1)
            {
                return 0;
            }

            return index + 1;
        }
        #endregion

        public void GameOver(string winner) //When a match is finished move to the winner screen
        {
            menuState = MenuState.Winner;
            menuIndex = 0;
            winningPlayer = winner;
        }

        private void ChangeTheme()
        {
            switch (theme[themeIndex])
            {
                case "Light":
                    game.backgroundColor = Color.CornflowerBlue;
                    textColor = Color.White;
                    break;
                case "Dark":
                    game.backgroundColor = Color.Black;
                    textColor = Color.LightGray;
                    break;
            }
        }

        #region MenuString methods
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
        #endregion
        #endregion
        
        #region Menu Enums

        public enum MenuState : byte //What menu is currently selected
        {
            MainMenu,
            Lobby,
            Settings,
            Controls,
            SelectingKey,
            Winner,
        }

        enum MainMenu : byte //All selectable options within MainMenu
        {
            Start,
            Settings,
            Quit
        }

        enum Lobby : byte //All selectable options just before a match (can select opponent and amount of lives)
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
            Theme,
            SoundEffects,
            Back,
        }
        
        #endregion
    }
}
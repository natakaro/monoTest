using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Input
{
    [Flags]
    public enum MouseButtons
    {
        //None = 0x0,
        LeftButton = 0x1,
        MiddleButton = 0x2,
        RightButton = 0x4,
        XButton1 = 0x8,
        XButton2 = 0x10
    }
    public static class MouseStateExtension
    {
        public static bool IsButtonPressed(this MouseState state, MouseButtons button)
        {
            return (ButtonState)(typeof(MouseState).GetProperty(button.ToString()).GetValue(state, null)) == ButtonState.Pressed;
        }

        public static bool IsButtonReleased(this MouseState state, MouseButtons button)
        {
            return (ButtonState)(typeof(MouseState).GetProperty(button.ToString()).GetValue(state, null)) == ButtonState.Released;
        }
    }

    /// <summary>
    /// Helper for reading input from keyboard, gamepad, and touch input. This class 
    /// tracks both the current and previous state of the input devices, and implements 
    /// query methods for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    public class InputState
    {
        public KeyboardState CurrentKeyboardState;
        public MouseState CurrentMouseState;

        public KeyboardState LastKeyboardState;
        public MouseState LastMouseState;

        /// <summary>
        /// Constructs a new input state.
        /// </summary>
        public InputState()
        {
            CurrentKeyboardState = new KeyboardState();
            CurrentMouseState = new MouseState();

            LastKeyboardState = new KeyboardState();
            LastMouseState = new MouseState();
        }

        /// <summary>
        /// Reads the latest state user input.
        /// </summary>
        public void Update()
        {
            LastKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();

            LastMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();
        }

        /// <summary>
        /// Helper for checking if a key was pressed during this update. The
        /// controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a keypress
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsKeyPressed(Keys key)
        {
            return CurrentKeyboardState.IsKeyDown(key);
        }

        public bool IsMouseButtonPressed(MouseButtons mouseButton)
        {
            return CurrentMouseState.IsButtonPressed(mouseButton);
        }

        public bool IsLeftMouseButtonPressed
        {
            get { return (CurrentMouseState.LeftButton == ButtonState.Pressed); }
        }

        public bool IsRightMouseButtonPressed
        {
            get { return (CurrentMouseState.RightButton == ButtonState.Pressed); }
        }

        /// <summary>
        /// Helper for checking if a key was newly pressed during this update. The
        /// controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a keypress
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsNewKeyPress(Keys key)
        {
                return (CurrentKeyboardState.IsKeyDown(key) &&
                        LastKeyboardState.IsKeyUp(key));
        }

        public bool IsNewMouseClick(MouseButtons mouseButton)
        {
            return (CurrentMouseState.IsButtonReleased(mouseButton) &&
                    LastMouseState.IsButtonPressed(mouseButton));
        }

        public bool IsNewLeftMouseClick
        {
            get { return (CurrentMouseState.LeftButton == ButtonState.Released && LastMouseState.LeftButton == ButtonState.Pressed); }
        }

        public bool IsNewRightMouseClick
        {
            get { return (CurrentMouseState.RightButton == ButtonState.Released && LastMouseState.RightButton == ButtonState.Pressed); }
        }

        public bool IsNewMiddleMouseClick
        {
            get { return (CurrentMouseState.MiddleButton == ButtonState.Pressed && LastMouseState.MiddleButton == ButtonState.Released); }
        }

        public bool IsNewMouseScrollUp
        {
            get { return (CurrentMouseState.ScrollWheelValue > LastMouseState.ScrollWheelValue); }
        }

        public bool IsNewMouseScrollDown
        {
            get { return (CurrentMouseState.ScrollWheelValue < LastMouseState.ScrollWheelValue); }
        }
    }
}

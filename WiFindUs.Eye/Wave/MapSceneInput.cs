using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Input;
using WaveEngine.Common.Math;
using WaveEngine.Framework;
using WaveEngine.Framework.Managers;
using WaveEngine.Framework.Services;
using WiFindUs.Extensions;

namespace WiFindUs.Eye.Wave
{
    public class MapSceneInput : SceneBehavior
    {
        [Flags]
        public enum MouseButtons
        {
            None = 0,
            Left = 1,
            Middle = 2,
            Right = 4
        };
        [Flags]
        public enum KeyboardModifiers
        {
            None = 0,
            Shift = 1,
            Alt = 2,
            Control = 4
        };
        public class MapSceneMouseEventArgs
        {
            public readonly MouseButtons Button;
            public readonly ButtonState State;
            public readonly int X;
            public readonly int Y;
            public readonly KeyboardModifiers Modifiers;
            public readonly MapScene Scene;
            public bool Handled = false;

            public MapSceneMouseEventArgs(MapScene scene, ref MouseState state, MouseButtons button, KeyboardModifiers modifiers)
            {
                Scene = scene;
                Button = button;
                State = GetButtonState(ref state, button);
                X = state.X;
                Y = state.Y;
                Modifiers = modifiers;
            }
        };
        public event Action<MapSceneMouseEventArgs> MousePressed, MouseHeld, MouseReleased;

        private KeyboardState oldKeyboardState;
        private MouseState oldMouseState;
        private MapScene mapScene;
        private Input input;
        
        protected override void ResolveDependencies()
        {
            mapScene = Scene as MapScene;
            input = WaveServices.Input;
            oldKeyboardState = input.KeyboardState;
            oldMouseState = input.MouseState;
        }

        protected override void Update(TimeSpan gameTime)
        {
            if (mapScene == null || input == null)
                return;

            //initial state
            Vector3 moveDelta = new Vector3();
            float zoomDelta = 0;
            float tiltDelta = 0;
            KeyboardModifiers modifiers = GetModifiers(ref input.KeyboardState);

            //check debug hotkey
            if (WasKeyPressed(Keys.F2))
                mapScene.DebugMode = !mapScene.DebugMode;

            //mouse button events
            //presses
            if (MousePressed != null)
            {
                if (WasMousePressed(MouseButtons.Left))
                    MousePressed(new MapSceneMouseEventArgs(mapScene, ref input.MouseState, MouseButtons.Left, modifiers));
                if (WasMousePressed(MouseButtons.Middle))
                    MousePressed(new MapSceneMouseEventArgs(mapScene, ref input.MouseState, MouseButtons.Middle, modifiers));
                if (WasMousePressed(MouseButtons.Right))
                    MousePressed(new MapSceneMouseEventArgs(mapScene, ref input.MouseState, MouseButtons.Right, modifiers));
            }
            //holds
            if (MouseHeld != null)
            {
                if (WasMouseHeld(MouseButtons.Left))
                    MouseHeld(new MapSceneMouseEventArgs(mapScene, ref input.MouseState, MouseButtons.Left, modifiers));
                if (WasMouseHeld(MouseButtons.Middle))
                    MouseHeld(new MapSceneMouseEventArgs(mapScene, ref input.MouseState, MouseButtons.Middle, modifiers));
                if (WasMouseHeld(MouseButtons.Right))
                    MouseHeld(new MapSceneMouseEventArgs(mapScene, ref input.MouseState, MouseButtons.Right, modifiers));
            }
            //releases
            if (MouseReleased != null)
            {
                if (WasMouseReleased(MouseButtons.Left))
                    MouseReleased(new MapSceneMouseEventArgs(mapScene, ref input.MouseState, MouseButtons.Left, modifiers));
                if (WasMouseReleased(MouseButtons.Middle))
                    MouseReleased(new MapSceneMouseEventArgs(mapScene, ref input.MouseState, MouseButtons.Middle, modifiers));
                if (WasMouseReleased(MouseButtons.Right))
                    MouseReleased(new MapSceneMouseEventArgs(mapScene, ref input.MouseState, MouseButtons.Right, modifiers));
            }

            //camera panning with keyboard arrows
            if (!modifiers.HasFlag(KeyboardModifiers.Shift) && !modifiers.HasFlag(KeyboardModifiers.Control))
            {
                if (WasKeyHeld(Keys.Left))
                    moveDelta.X -= 1.0f;
                if (WasKeyHeld(Keys.Right))
                    moveDelta.X += 1.0f;
                if (WasKeyHeld(Keys.Up))
                    moveDelta.Z -= 1.0f;
                if (WasKeyHeld(Keys.Down))
                    moveDelta.Z += 1.0f;
                if (!moveDelta.LengthSquared().Tolerance(0.0f,0.01f))
                {
                    moveDelta.Normalize();
                    moveDelta *= (float)gameTime.TotalSeconds * 1000.0f;
                }
            }
            //camera panning with middle mouse
            if (WasMouseHeld(MouseButtons.Middle))
            {
                float zoomDiff = 1.0f + mapScene.CameraZoom;
                moveDelta.X += (oldMouseState.X - input.MouseState.X) * zoomDiff;
                moveDelta.Z += (oldMouseState.Y - input.MouseState.Y) * zoomDiff;
            }
            //tilting camera with left + right mouse
            else if (WasMouseHeld(MouseButtons.Left) && WasMouseHeld(MouseButtons.Right))
                tiltDelta = (float)(oldMouseState.Y - input.MouseState.Y) * 0.01f;

            //zooming with mousewheel
            if (input.MouseState.Wheel != oldMouseState.Wheel)
                zoomDelta -= (float)(input.MouseState.Wheel - oldMouseState.Wheel) * 0.05f;

            //tilting camera with shift + up/down
            if (modifiers == KeyboardModifiers.Shift)
            {
                if (WasKeyHeld(Keys.Up))
                    zoomDelta -= 0.05f;
                if (WasKeyHeld(Keys.Down))
                    zoomDelta += 0.05f;
            }

            //tilting camera with control + up/down
            if (modifiers == KeyboardModifiers.Control)
            {
                if (WasKeyHeld(Keys.Up))
                    tiltDelta -= (float)gameTime.TotalSeconds;
                if (WasKeyHeld(Keys.Down))
                    tiltDelta += (float)gameTime.TotalSeconds;
            }

            //apply changes
            mapScene.CameraAutoUpdate = false;
            if (!moveDelta.LengthSquared().Tolerance(0.0f,0.01f))
                mapScene.CameraTarget += moveDelta;
            mapScene.CameraZoom += zoomDelta;
            mapScene.CameraTilt += tiltDelta;
            mapScene.CameraAutoUpdate = true;

            //state
            oldKeyboardState = input.KeyboardState;
            oldMouseState = input.MouseState;
        }

        /////////////////////////////////////////////////////////////////////
        // KEYBOARD BUTTONS
        /////////////////////////////////////////////////////////////////////

        private bool WasKeyPressed(Keys key)
        {
            return input.KeyboardState.IsKeyPressed(key) && !oldKeyboardState.IsKeyPressed(key);
        }

        private bool WasKeyHeld(Keys key)
        {
            return input.KeyboardState.IsKeyPressed(key) && oldKeyboardState.IsKeyPressed(key);
        }

        private bool WasKeyReleased(Keys key)
        {
            return !input.KeyboardState.IsKeyPressed(key) && oldKeyboardState.IsKeyPressed(key);
        }

        private static KeyboardModifiers GetModifiers(ref KeyboardState state)
        {
            return (state.IsKeyPressed(Keys.LeftShift) || state.IsKeyPressed(Keys.RightShift) ? KeyboardModifiers.Shift : KeyboardModifiers.None)
                | (state.IsKeyPressed(Keys.LeftAlt) || state.IsKeyPressed(Keys.RightAlt) ? KeyboardModifiers.Alt : KeyboardModifiers.None)
                | (state.IsKeyPressed(Keys.LeftControl) || state.IsKeyPressed(Keys.RightControl) ? KeyboardModifiers.Control : KeyboardModifiers.None);
        }

        /////////////////////////////////////////////////////////////////////
        // MOUSE BUTTONS
        /////////////////////////////////////////////////////////////////////

        private static ButtonState GetButtonState(ref MouseState state, MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.Left:
                    return state.LeftButton;
                case MouseButtons.Middle:
                    return state.MiddleButton;
                case MouseButtons.Right:
                    return state.RightButton;
            }
            return ButtonState.Release;
        }

        private bool WasMousePressed(MouseButtons button)
        {
            return GetButtonState(ref input.MouseState, button) == ButtonState.Pressed
                && GetButtonState(ref oldMouseState, button) == ButtonState.Release;
        }

        private bool WasMouseHeld(MouseButtons button)
        {
            return GetButtonState(ref input.MouseState, button) == ButtonState.Pressed
                && GetButtonState(ref oldMouseState, button) == ButtonState.Pressed;
        }

        private bool WasMouseReleased(MouseButtons button)
        {
            return GetButtonState(ref input.MouseState, button) == ButtonState.Release
                && GetButtonState(ref oldMouseState, button) == ButtonState.Pressed;
        }

        private static MouseButtons GetButtons(ref MouseState state)
        {
            return (state.LeftButton == ButtonState.Pressed ? MouseButtons.Left : MouseButtons.None)
                | (state.MiddleButton == ButtonState.Pressed ? MouseButtons.Middle : MouseButtons.None)
                | (state.RightButton == ButtonState.Pressed ? MouseButtons.Right : MouseButtons.None);
        }
    }
}

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

namespace WiFindUs.Eye.Wave
{
    public class MapSceneInputBehaviour : SceneBehavior
    {
        public event Action<Marker[]> SceneClicked;
        
        private KeyboardState oldKeyboardState;
        private MouseState oldMouseState;
        private System.Windows.Forms.Cursor cursor = null;
        private MapScene mapScene;
        private Input input;
        
        protected override void ResolveDependencies()
        {
            mapScene = Scene as MapScene;
            input = WaveServices.Input;
        }

        protected override void Update(TimeSpan gameTime)
        {
            if (mapScene == null || input == null)
                return;

            //initial state
            cursor = null;
            mapScene.CameraAutoUpdate = false;

            //left mouse click
            if (LeftMousePressed(ref input.MouseState, ref oldMouseState) && SceneClicked != null)
                SceneClicked(mapScene.MarkersFromScreenRay(input.MouseState.X, input.MouseState.Y));

            //arrows and WSAD for pan
            Vector3 moveDelta = new Vector3();
            if (IsLeft(ref input))
                moveDelta.X -= 1;
            if (IsForward(ref input))
                moveDelta.Z -= 1;
            if (IsRight(ref input))
                moveDelta.X += 1;
            if (IsBack(ref input))
                moveDelta.Z += 1;
            if (moveDelta.LengthSquared() > 0.0f)
            {
                moveDelta.Normalize();
                moveDelta *= (float)gameTime.TotalSeconds * 1000.0f;
            }

            //middle mouse drag for pan
            if (MiddleMousePressedOrHeld(ref input.MouseState, ref oldMouseState))
            {
                float zoomDiff = 1.0f + ((float)mapScene.CameraZoom / 100.0f);
                moveDelta.X += (oldMouseState.X - input.MouseState.X) * zoomDiff;
                moveDelta.Z += (oldMouseState.Y - input.MouseState.Y) * zoomDiff;
                cursor = System.Windows.Forms.Cursors.Hand;
            }
            else if (//left + right buttons for tilting
                LeftMousePressedOrHeld(ref input.MouseState, ref oldMouseState)
                    && RightMousePressedOrHeld(ref input.MouseState, ref oldMouseState))
            {
                mapScene.CameraTilt += (int)(oldMouseState.Y - input.MouseState.Y);
                cursor = System.Windows.Forms.Cursors.SizeNS;
            }

            //do panning
            if (moveDelta.LengthSquared() > 0.0f)
                mapScene.CameraTarget += moveDelta;

            //mouse wheel for zooming and tilting
            if (input.MouseState.Wheel != oldMouseState.Wheel)
                 mapScene.CameraZoom -= (input.MouseState.Wheel - oldMouseState.Wheel) * 5;

            //check debug hotkey
            if (oldKeyboardState.F2 == ButtonState.Release && input.KeyboardState.F2 == ButtonState.Pressed)
                mapScene.DebugMode = !mapScene.DebugMode;

            //state
            if (cursor != mapScene.HostControl.Cursor)
                mapScene.HostControl.Cursor = cursor ?? System.Windows.Forms.Cursors.Default;
            oldKeyboardState = input.KeyboardState;
            oldMouseState = input.MouseState;
            mapScene.CameraAutoUpdate = true;
        }

        /////////////////////////////////////////////////////////////////////
        // KEYBOARD BUTTONS
        /////////////////////////////////////////////////////////////////////

        private bool IsShift(ref Input input)
        {
            return input.KeyboardState.RightShift == ButtonState.Pressed
                || input.KeyboardState.LeftShift == ButtonState.Pressed;
        }

        private bool IsLeft(ref Input input)
        {
            return input.KeyboardState.Left == ButtonState.Pressed
                || input.KeyboardState.A == ButtonState.Pressed;
        }

        private bool IsRight(ref Input input)
        {
            return input.KeyboardState.Right == ButtonState.Pressed
                || input.KeyboardState.D == ButtonState.Pressed;
        }

        private bool IsForward(ref Input input)
        {
            return input.KeyboardState.Up == ButtonState.Pressed
                || input.KeyboardState.W == ButtonState.Pressed;
        }

        private bool IsBack(ref Input input)
        {
            return input.KeyboardState.Down == ButtonState.Pressed
                || input.KeyboardState.S == ButtonState.Pressed;
        }

        /////////////////////////////////////////////////////////////////////
        // LEFT MOUSE BUTTON
        /////////////////////////////////////////////////////////////////////

        private bool IsLeftMouse(ref MouseState state)
        {
            return state.LeftButton == ButtonState.Pressed;
        }

        private bool LeftMousePressed(ref MouseState state, ref MouseState oldState)
        {
            return IsLeftMouse(ref state) && !IsLeftMouse(ref oldState);
        }

        private bool LeftMouseHeld(ref MouseState state, ref MouseState oldState)
        {
            return IsLeftMouse(ref state) && IsLeftMouse(ref oldState);
        }

        private bool LeftMousePressedOrHeld(ref MouseState state, ref MouseState oldState)
        {
            return LeftMousePressed(ref state, ref oldState) || LeftMouseHeld(ref state, ref oldState);
        }

        private bool LeftMouseReleased(ref MouseState state, ref MouseState oldState)
        {
            return !IsLeftMouse(ref state) && IsLeftMouse(ref oldState);
        }

        /////////////////////////////////////////////////////////////////////
        // MIDDLE MOUSE BUTTON
        /////////////////////////////////////////////////////////////////////

        private bool IsMiddleMouse(ref MouseState state)
        {
            return state.MiddleButton == ButtonState.Pressed;
        }

        private bool MiddleMousePressed(ref MouseState state, ref MouseState oldState)
        {
            return IsMiddleMouse(ref state) && !IsMiddleMouse(ref oldState);
        }

        private bool MiddleMouseHeld(ref MouseState state, ref MouseState oldState)
        {
            return IsMiddleMouse(ref state) && IsMiddleMouse(ref oldState);
        }

        private bool MiddleMousePressedOrHeld(ref MouseState state, ref MouseState oldState)
        {
            return MiddleMousePressed(ref state, ref oldState) || MiddleMouseHeld(ref state, ref oldState);
        }

        private bool MiddleMouseReleased(ref MouseState state, ref MouseState oldState)
        {
            return !IsMiddleMouse(ref state) && IsMiddleMouse(ref oldState);
        }

        /////////////////////////////////////////////////////////////////////
        // MIDDLE MOUSE BUTTON
        /////////////////////////////////////////////////////////////////////

        private bool IsRightMouse(ref MouseState state)
        {
            return state.RightButton == ButtonState.Pressed;
        }

        private bool RightMousePressed(ref MouseState state, ref MouseState oldState)
        {
            return IsRightMouse(ref state) && !IsRightMouse(ref oldState);
        }

        private bool RightMouseHeld(ref MouseState state, ref MouseState oldState)
        {
            return IsRightMouse(ref state) && IsRightMouse(ref oldState);
        }

        private bool RightMousePressedOrHeld(ref MouseState state, ref MouseState oldState)
        {
            return RightMousePressed(ref state, ref oldState) || RightMouseHeld(ref state, ref oldState);
        }

        private bool RightMouseReleased(ref MouseState state, ref MouseState oldState)
        {
            return !IsRightMouse(ref state) && IsRightMouse(ref oldState);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Input;
using WaveEngine.Common.Math;
using WaveEngine.Framework;
using WaveEngine.Framework.Services;

namespace WiFindUs.Eye.Wave
{
    public class MapSceneInputBehaviour : SceneBehavior
    {        
        private KeyboardState oldKeyboardState;
        private MouseState oldMouseState;
        
        protected override void ResolveDependencies()
        {

        }

        protected override void Update(TimeSpan gameTime)
        {
            MapScene mapScene = Scene as MapScene;
            Input input = WaveServices.Input;
            if (mapScene == null || input == null)
                return;

            //turn off camera auto updates
            mapScene.CameraAutoUpdate = false;

            //arrows and WSAD for pan
            Vector3 moveDelta = new Vector3();
            if (IsLeft(input))
                moveDelta.X -= 1;
            if (IsForward(input))
                moveDelta.Z -= 1;
            if (IsRight(input))
                moveDelta.X += 1;
            if (IsBack(input))
                moveDelta.Z += 1;
            if (moveDelta.LengthSquared() > 0.0f)
            {
                moveDelta.Normalize();
                moveDelta *= (float)gameTime.TotalSeconds * 1000.0f;
            }

            //middle mouse drag for pan
            if (input.MouseState.MiddleButton == ButtonState.Pressed
                && oldMouseState.MiddleButton == ButtonState.Pressed)
            {

                float zoomDiff = 1.0f + ((float)mapScene.CameraZoom / 100.0f);
                moveDelta.X += (oldMouseState.X - input.MouseState.X) * zoomDiff;
                moveDelta.Z += (oldMouseState.Y - input.MouseState.Y) * zoomDiff;
            }
            else if (//left + right buttons for tilting
                input.MouseState.LeftButton == ButtonState.Pressed
                    && oldMouseState.LeftButton == ButtonState.Pressed
                    && input.MouseState.RightButton == ButtonState.Pressed
                    && oldMouseState.RightButton == ButtonState.Pressed)
            {
                mapScene.CameraTilt += (int)(oldMouseState.Y - input.MouseState.Y);
            }

            //do panning
            if (moveDelta.LengthSquared() > 0.0f)
                mapScene.CameraTarget += moveDelta;

            //mouse wheel for zooming and tilting
            if (input.MouseState.Wheel != oldMouseState.Wheel)
                 mapScene.CameraZoom -= (input.MouseState.Wheel - oldMouseState.Wheel) * 5;

            //state
            oldKeyboardState = input.KeyboardState;
            oldMouseState = input.MouseState;
            mapScene.CameraAutoUpdate = true;
        }

        private bool IsShift(Input input)
        {
            return input.KeyboardState.RightShift == ButtonState.Pressed
                || input.KeyboardState.LeftShift == ButtonState.Pressed;
        }

        private bool IsLeft(Input input)
        {
            return input.KeyboardState.Left == ButtonState.Pressed
                || input.KeyboardState.A == ButtonState.Pressed;
        }

        private bool IsRight(Input input)
        {
            return input.KeyboardState.Right == ButtonState.Pressed
                || input.KeyboardState.D == ButtonState.Pressed;
        }
        private bool IsForward(Input input)
        {
            return input.KeyboardState.Up == ButtonState.Pressed
                || input.KeyboardState.W == ButtonState.Pressed;
        }

        private bool IsBack(Input input)
        {
            return input.KeyboardState.Down == ButtonState.Pressed
                || input.KeyboardState.S == ButtonState.Pressed;
        }
    }
}

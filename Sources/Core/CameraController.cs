using Silk.NET.Input;
using System;
using System.Numerics;

namespace GLSample.Core
{
    public class CameraController
    {
        public Camera Target { get; }
        public float MovementSpeed { get; set; } = 1.0f;

        private bool _rightPressed;
        private bool _leftPressed;
        private bool _upPressed;
        private bool _downPressed;
        private bool _forwardPressed;
        private bool _backwardPressed;

        public CameraController(Camera target)
        {
            Target = target; 
        }

        public void OnKeyDown(Key key)
        {
            switch (key)
            {
                case Key.D: _rightPressed = true; break;
                case Key.A: _leftPressed = true; break;
                case Key.Space: _upPressed = true; break;
                case Key.ShiftLeft: _downPressed = true; break;
                case Key.W: _forwardPressed = true; break;
                case Key.S: _backwardPressed = true; break;
            }
        }

        public void OnKeyUp(Key key) 
        {
            switch (key)
            {
                case Key.D: _rightPressed = false; break;
                case Key.A: _leftPressed = false; break;
                case Key.Space: _upPressed = false; break;
                case Key.ShiftLeft: _downPressed = false; break; 
                case Key.W: _forwardPressed = false; break;
                case Key.S: _backwardPressed = false; break;
            }
        }




        public void Update(float deltaTime)
        {
            var movement = Vector3.Zero;
            if (_rightPressed) movement += Vector3.UnitX;
            if (_leftPressed) movement -= Vector3.UnitX;
            if (_upPressed) movement += Vector3.UnitY;
            if (_downPressed) movement -= Vector3.UnitY;
            if (_forwardPressed) movement -= Vector3.UnitZ;
            if (_backwardPressed) movement += Vector3.UnitZ;

            movement = Vector3.Transform(movement, Target.Transform.Rotation);
            Target.Transform.Position += movement * MovementSpeed * deltaTime;
        }

        public void OnMouseMove(Vector2 delta)
        {
            // TODO
        }
    }
}

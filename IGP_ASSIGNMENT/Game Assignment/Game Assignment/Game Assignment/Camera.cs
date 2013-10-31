using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;


namespace Game_Assignment
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Camera : Microsoft.Xna.Framework.GameComponent
    {
        //Camera matrices
        public Matrix view;
        public Matrix projection;
        public Vector3 position;
        public Vector3 direction;
        public Vector3 up;
        public float nearPlane = 1f;
        public float farPlane = 20000f;
        public float speed = 2.5f;
        public float currentPitch = 0;
        public float currentYaw = 0;
        public bool orthoganal = false;
        public bool updateFrustrum = true;
        public float width;
        public float height;
        public MouseState prevMouseState;
        public KeyboardState prevKeyboard;
        public BoundingFrustum frustum;

        public Camera(Game game, Vector3 pos, Vector3 target, Vector3 up)
            : base(game)
        {
            position = pos;
            direction = target - pos;
            direction.Normalize();
            this.up = up;
            CreateLookAt();
            width = (float)Game.Window.ClientBounds.Width;
            height = (float)Game.Window.ClientBounds.Height;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)Game.Window.ClientBounds.Width / (float)Game.Window.ClientBounds.Height, nearPlane, farPlane);
            frustum = new BoundingFrustum(view * projection);
        }
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            checkKeyboardMouse();
            prevMouseState = Mouse.GetState();
            prevKeyboard = Keyboard.GetState();
            CreateLookAt();
            if (orthoganal)
            {
                projection = Matrix.CreateOrthographic(width, height, nearPlane, farPlane);
            }
            if (updateFrustrum)
            {
                frustum = new BoundingFrustum(view * projection);
            }
            base.Update(gameTime);
        }
        private void CreateLookAt()
        {
            direction.Normalize();
            view = Matrix.CreateLookAt(position, position + direction, up);
        }
        private void checkKeyboardMouse()
        {
            if (!prevKeyboard.IsKeyDown(Keys.G) && Keyboard.GetState().IsKeyDown(Keys.G))
            {
                if (updateFrustrum)
                    updateFrustrum = false;
                else
                    updateFrustrum = true;
            }
            //move forward and backward
            if (!orthoganal)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    position += direction * speed;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.S))
                {
                    position -= direction * speed;
                }
            }
            else
            {
                if (Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    width /= 1.05f;
                    height /= 1.05f;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.S))
                {
                    width *= 1.05f;
                    height *= 1.05f;
                }
            }

            // Move side to side
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                position += Vector3.Cross(up, direction) * speed;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                position -= Vector3.Cross(up, direction) * speed;
            }
            //able to move camera freely if shift is held down
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                ((Game1)Game).IsMouseVisible = false;
                float pitchAngle = (MathHelper.PiOver4 / 150) * (Mouse.GetState().Y - prevMouseState.Y);
                direction = Vector3.Transform(direction, Matrix.CreateFromAxisAngle(Vector3.Cross(up, direction), pitchAngle));
                currentPitch += pitchAngle;

                float yawAngle = (-MathHelper.PiOver4 / 150) * (Mouse.GetState().X - prevMouseState.X);
                direction = Vector3.Transform(direction, Matrix.CreateFromAxisAngle(up, yawAngle));
                currentYaw += yawAngle;

                Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);
            }
            else
                ((Game1)Game).IsMouseVisible = true;

            //toggle ortho or perspective viewing
            if (!prevKeyboard.IsKeyDown(Keys.C) && Keyboard.GetState().IsKeyDown(Keys.C))
            {
                if (!orthoganal)
                {
                    projection = Matrix.CreateOrthographic(width, height, nearPlane, farPlane);
                    orthoganal = true;
                }
                else
                {
                    projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)Game.Window.ClientBounds.Width / (float)Game.Window.ClientBounds.Height, nearPlane, farPlane);
                    orthoganal = false;
                }
            }
        }

    }
}

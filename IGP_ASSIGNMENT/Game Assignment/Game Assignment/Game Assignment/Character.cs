﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game_Assignment
{
    /*public enum characterState
    {
        IDLE,
        FOLLOW,
        JUMP
    }*/

    public class Character : BasicModel
    {
        public Vector3 resetPosition;
        public Vector3 desiredVelocity;
        public Vector3 steering;
        public float mass;
        public float maxSpeed;
        public float jumpForce;
        //public characterState npcState = characterState.FOLLOW;
        public StateMachine npcState;
        //public BoundingBox skyChecker;
        public float maxJumpHeight;
        public Vector3 target;
        public int pathIndex = 1;

        public Character(Model m, int xPos, int yPos, Game1 game)
            : base(m, game)
        {

            position = new Vector3(xPos * 117.3f, yPos * 88.7f, 0);
            rotationY = MathHelper.ToRadians(180);
            npcState = new CharacterStateMachine(this, CharacterStateMachine.States.FOLLOW);
        }
        //Changed to allow for changing rotation, skychecker, and chase to be more effective...
        public override void Update(Camera camera)
        {
            //if (rotationY == 0) //right
            //{
            //    skyChecker = new BoundingBox(new Vector3(box.Max.X, box.Min.Y, box.Min.Z),
            //    new Vector3(box.Max.X + (distanceThreshold(this)), (box.Min.Y + maxJumpHeight), box.Max.Z));
            //}
            //else //left
            //{
            //    skyChecker = new BoundingBox(new Vector3(box.Min.X - (distanceThreshold(this)), box.Min.Y, box.Min.Z),
            //       new Vector3(box.Min.X, (box.Min.Y + maxJumpHeight), box.Max.Z));
            //}

            if (this == game.follower)
            {

                //AI Behaviour
                switch (npcState.getCurrentState())
                {
                    case StateMachine.States.FOLLOW:
                        if (game.aStar.paths.Count > 1)
                        {
                            target = game.aStar.paths[1];
                            game.aStar.state = AStar.SearchState.IDLE;
                        }

                        if (calculateDistance(game.chosen) > width(game.chosen))
                        {
                            follow();
                        }
                        else if (calculateDistance(game.chosen) < width(game.chosen))
                        {
                            velocity.X -= (velocity.X * 0.1f);
                        }

                        if (target.Y > position.Y && target.Y < (position.Y + maxJumpHeight))
                        {
                            npcState.changeState(StateMachine.States.JUMP);
                        }
                        break;
                    case StateMachine.States.IDLE:
                        velocity.X = 0;
                        break;
                    case StateMachine.States.JUMP:
                        if (canJump)
                            jump();
                        npcState.changeState(StateMachine.States.FOLLOW);
                        break;
                }
            }

            //all characters behaviours           
            position += velocity;
            if (velocity.X > 0)
            {
                rotationY = MathHelper.Pi;
            }
            else if (velocity.X < 0)
            {
                rotationY = 0;
            }

            backFeet = new Vector3(box.Min.X + 2.5f, box.Min.Y - 5, 0);
            frontFeet = new Vector3(box.Max.X - 2.5f, box.Min.Y - 5, 0);
        }
  
        public void follow()
        {
            if (game.collisionBoxToBoxes(box, game.levelBoxes) == Game1.collisionType.NO_COLLISION)
            {
                steering = seek(target, 0);
                velocity.X += steering.X / 5.5f;
            }
        }

        public float calculateDistance(Character character)
        {
            Vector3 tempDirection = position - character.position;
            tempDirection.Normalize();
            Ray ray = new Ray(character.position, tempDirection);
            float distance = 0;
            float? rayDistance = ray.Intersects(box);
            if (rayDistance.HasValue == true)
                distance = (float)rayDistance;
            return distance;
        }

		public float width(Character character)
        {
            float width = (character.box.Max.X - character.box.Min.X);
            return width;
        }

        public float height(Character character)
        {
            float height = (character.box.Max.X - character.box.Min.X);
            return height;
        }

        public void keyCheck()
        {
            //move right
            if (game.prevKeyboard.IsKeyDown(Keys.Right) &&
                game.keyboard.IsKeyDown(Keys.Right))
            {
                if (Math.Abs(velocity.X) <= maxSpeed)
                {
                    rotationY = 0;
                    velocity += Vector3.Right * 0.5f;
                }
            }
            //move left
            else if (game.prevKeyboard.IsKeyDown(Keys.Left) &&
                game.keyboard.IsKeyDown(Keys.Left))
            {
                if (Math.Abs(velocity.X) <= maxSpeed)
                {
                    rotationY = MathHelper.Pi;
                    velocity += Vector3.Left * 0.5f;
                }
            }

            //stop if !left || !right
            if (game.prevKeyboard.IsKeyDown(Keys.Right) && !game.keyboard.IsKeyDown(Keys.Right) ||
                game.prevKeyboard.IsKeyDown(Keys.Left) && !game.keyboard.IsKeyDown(Keys.Left))
            {
                Vector3 tempVel = velocity;
                tempVel.X = 0;
                velocity = tempVel;
            }
            //NEW! to ensure that the character is on the ground before it can jump
            //jumping
            if (game.keyboard.IsKeyDown(Keys.Up) && !game.prevKeyboard.IsKeyDown(Keys.Up))
            {
                if (canJump || game.collisionBoxToBoxes(box,game.levelBoxes) == Game1.collisionType.BOTTOM)
                    jump();
            }

            //stay command toggle
            if (game.keyboard.IsKeyDown(Keys.Q) && !game.prevKeyboard.IsKeyDown(Keys.Q))
            {
                if (game.follower.npcState.getCurrentState() == StateMachine.States.FOLLOW)
                {
                    game.follower.npcState.changeState(StateMachine.States.IDLE);
                    game.chosen.npcState.changeState(StateMachine.States.IDLE);
                }
                else if (game.follower.npcState.getCurrentState() == StateMachine.States.IDLE)
                {
                    game.follower.npcState.changeState(StateMachine.States.FOLLOW);
                    game.chosen.npcState.changeState(StateMachine.States.FOLLOW);
                }
            }

            //grab objects
            if (game.keyboard.IsKeyDown(Keys.F) && !game.prevKeyboard.IsKeyDown(Keys.F))
            {
                BoundingBox grabBox = new BoundingBox();
                if (rotationY == 0)
                {
                    grabBox = new BoundingBox(new Vector3(box.Max.X, box.Min.Y, box.Min.Z),
                       new Vector3((box.Max.X + width(this)), box.Max.Y, box.Max.Z));
                }
                else if (rotationY == MathHelper.Pi)
                {
                    grabBox = new BoundingBox(new Vector3((box.Min.X - width(this)), box.Min.Y, box.Min.Z),
                       new Vector3(box.Min.X, box.Max.Y, box.Max.Z));
                }

                if (grabBox.Intersects(game.follower.box) || this.box.Intersects(game.follower.box))
                {
                    Console.WriteLine("im grabbing your ass");
                    game.follower.npcState.changeState(StateMachine.States.IDLE);
                }
            }

        }

        public void jump()
        {
            Vector3 tempVelocity = velocity;
            tempVelocity.Y = jumpForce;
            velocity = tempVelocity;
            canJump = false;
        }

        public Vector3 seek(Vector3 target, float slowRadius)
        {
            Vector3 force;
            float tempDistance;
            desiredVelocity = target - position;
            tempDistance = desiredVelocity.Length();
            desiredVelocity.Normalize();

            if (tempDistance <= slowRadius)
            {
                desiredVelocity *= (maxSpeed * tempDistance / slowRadius);
            }
            else
            {
                desiredVelocity *= maxSpeed;
            }

            force = desiredVelocity - velocity;
            return force;
        }
    }
}

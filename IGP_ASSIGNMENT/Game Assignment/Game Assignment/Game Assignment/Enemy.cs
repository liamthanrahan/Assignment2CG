﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game_Assignment
{
    public class Enemy : BasicModel
    {
        public enum enemyState
        {
            IDLE,
            ATTACK,
            CHASE,
            THROW,
            PATROL
        }
        public enemyState state;
        public Ray vision;
        public Vector3 spawnPosition;
        public bool rangedAttackEnabled = false;
        public bool chasingPlayer = false;
        //NEW place holders. allows for easy editing
        public Vector3 chaseThreshold;
        public float attackThreshold;
        //Left and Right wall for a patrol
        public BoundingBox leftWall;
        public BoundingBox rightWall;
        //boolean for patrols
        public bool patrolByWalls;
        public bool patrolBySetWalls;
        public bool patrolBySetWallsWithThreshold;
        public float patrolThreshold;
        

        public Enemy(Model m, int xPos, int yPos, Game1 game)
            : base(m, game)
        {
            position = new Vector3(xPos * 117.3f, yPos * 88.7f, 0);
            scaleValue = 35;
            rotationY = MathHelper.ToRadians(180);
            vision = new Ray(position, Vector3.UnitX);
            
            attackThreshold = 100;
            patrolByWalls = false;
            patrolBySetWalls = true;
            patrolBySetWallsWithThreshold = true;
            spawnPosition = position;
            patrolThreshold = 500;
        }
        //NEW updated the structure for CHASE, still trying to figure out attack and throw.
        public override void Update(Camera camera)
        {
            float? rayLength = game.collisionRayToBox(vision, game.chosen.box);
            chaseThreshold = game.chosen.position - position;

            if ((game.modelCollidingWith(vision) is Character) && rayLength < attackThreshold)
                state = enemyState.ATTACK;
            else if (rayLength <= chaseThreshold.Length() && rayLength >= attackThreshold && game.modelCollidingWith(vision) is Character)
            {
                chasingPlayer = true;
                state = enemyState.CHASE;
            }
            else if (!(game.modelCollidingWith(vision) is Character) && (rayLength <= chaseThreshold.Length() || rayLength == null))
                state = enemyState.PATROL;
            if (!chasingPlayer)
            {
                switch (state)
                {
                    case enemyState.IDLE:
                        break;
                    case enemyState.ATTACK:
                        if(game.chosen.position.X - position.X > 0)
                            game.chosen.position += Vector3.Right * 3;
                        else
                            game.chosen.position += Vector3.Left * 3;
                        break;
                    case enemyState.CHASE:
						Vector3 temp = Vector3.Normalize(chaseThreshold);
	                    temp.Y = 0;
	                    temp.Z = 0;
	                    position += temp;
	                    break;
                    case enemyState.THROW:
                        break;
                    case enemyState.PATROL:
                        if (patrolByWalls)
                        {
                            if (rotationY == MathHelper.ToRadians(180))
                            {
                                position += Vector3.Left;
                                vision.Direction = Vector3.Left;
                            }
                            else if (rotationY == 0)
                            {
                                position += Vector3.Right;
                                vision.Direction = Vector3.Right;
                            }
                            if (game.collisionBoxToBoxes(box, game.levelBoxes) == Game1.collisionType.RIGHT)
                                rotationY = 0;
                            else if (game.collisionBoxToBoxes(box, game.levelBoxes) == Game1.collisionType.LEFT)
                                rotationY = MathHelper.ToRadians(180);
                        }
                        else if (patrolBySetWalls)
                        {
                            if (patrolBySetWallsWithThreshold)
                            {
                                leftWall = new BoundingBox(new Vector3(position.X - patrolThreshold - 50, position.Y - 50, position.Z - 20), 
                                    new Vector3(position.X - patrolThreshold, position.Y + 50, position.Z + 20));
                                rightWall = new BoundingBox(new Vector3(position.X + patrolThreshold, position.Y - 50, position.Z - 20), 
                                    new Vector3(position.X + patrolThreshold + 50, position.Y + 50, position.Z + 20));
                            }
                            else
                            {
                                leftWall = new BoundingBox();
                                rightWall = new BoundingBox();
                            }

                            if (box.Intersects(leftWall))
                                rotationY = 0;
                            else if (box.Intersects(rightWall))
                                rotationY = MathHelper.ToRadians(180);
                        }
                        break;
                }
            }
            chasing();//NEW
            if (rotationY == MathHelper.ToRadians(180))
            {
                vision = new Ray(position, Vector3.Left);
            }
            else if (rotationY == 0)
            {
                vision = new Ray(position, Vector3.Right);
            }
            DebugShapeRenderer.AddLine(position, position + vision.Direction * 200, Color.DarkBlue);
            position += velocity;
            backFeet = new Vector3(box.Min.X, box.Min.Y - 5, 0);
            frontFeet = new Vector3(box.Max.X, box.Min.Y - 5, 0);
        }
        //NEW
        public void chasing()
        {
            if (chasingPlayer)
            {
                Vector3 temp = 2 * Vector3.Normalize(game.chosen.position - position);
                temp.Y = 0;
                temp.Z = 0;
                if (temp.X > 0)
                    rotationY = 0;
                else if (temp.X < 0)
                    rotationY = MathHelper.ToRadians(180);
                position += temp;
            }
            if (Math.Abs(game.chosen.position.X - position.X) > chaseThreshold.Length() || (!(game.modelCollidingWith(vision) is Character) && game.collisionRayToBox(vision, game.chosen.box) <=100))
                chasingPlayer = false;
        }
    }
}

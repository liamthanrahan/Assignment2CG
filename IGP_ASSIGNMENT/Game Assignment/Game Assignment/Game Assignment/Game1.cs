using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Game_Assignment
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public enum collisionType
        {
            NO_COLLISION,
            TOP,
            LEFT,
            RIGHT,
            BOTTOM,
        }

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont debugFont;
        public ModelManager modelManager;
        public SoundManager soundManager;
        public Vector3 GRAVITY = Vector3.Down * 0.098f * 5f;
        public KeyboardState prevKeyboard;
        public KeyboardState keyboard;
        public Camera camera { get; protected set; }
        public GraphicsDevice device;
        public bool isFollowingPlayer = true;
        public bool isLevelLoaded = false;
        public Character chosen;
        public Character follower;
        public List<BoundingBox> levelBoxes = new List<BoundingBox>();
        public List<BasicModel> reloadModels = new List<BasicModel>();
        public AStar aStar;
        public QuadTree quadTree;
		public int score = 0;
        public float fpsTimer;
        public float FPS;
        public int frameCount;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            device = graphics.GraphicsDevice;
            camera = new Camera(this, new Vector3(0, 0, 750), -Vector3.UnitZ, Vector3.Up);
            Components.Add(camera);
            modelManager = new ModelManager(this);
            Components.Add(modelManager);
            soundManager = new SoundManager(this);
            Components.Add(soundManager);
            base.Initialize();
            reloadModels = modelManager.models;
        }

        protected void loadLevel()
        {
            device = graphics.GraphicsDevice;
            Components.Clear();

            camera = new Camera(this, new Vector3(0, 0, 750), -Vector3.UnitZ, Vector3.Up);
            Components.Add(camera);

            modelManager = new ModelManager(this);

            modelManager.models = reloadModels;
            Components.Add(modelManager);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        /// 
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            debugFont = Content.Load<SpriteFont>("debugFont");

            // Set cullmode to none
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rs;

            DebugShapeRenderer.Initialize(GraphicsDevice);

            foreach (Human ch in modelManager.models.OfType<Human>())
            {
                chosen = ch;
                chosen.resetPosition = chosen.position;
                foreach (Dwarf df in modelManager.models.OfType<Dwarf>())
                {
                    follower = df;
                    follower.position = chosen.position - Vector3.UnitX * 40;
                    follower.resetPosition = follower.position;
                }
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 

        protected override void Update(GameTime gameTime)
        {
            //killing and death
            
            BasicModel basic = enemyCollidingWith(chosen.box);
            if(basic != null)
            {
                collisionType type = collisionBoxToBox(chosen.box, basic.box);
                if ( type == collisionType.TOP)
                {
                    //remove enemy
                    modelManager.models.Remove(basic);
                    score += 200;
                }
                else if (type != collisionType.TOP && type != collisionType.NO_COLLISION)
                {
                    reloadWorld();
                }
            }
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (!isLevelLoaded)
            {
                loadLevelBoxes();
                aStar = new AStar(modelManager.widthOfMap, modelManager.heightOfMap, (int)modelManager.widthOfMap, (int)modelManager.heightOfMap, this);
                quadTree = new QuadTree(1, new BoundingBox(new Vector3(0, -modelManager.heightOfMap * 88.7f, 0), new Vector3(modelManager.widthOfMap * 117.3f, 0, 0)), this);

                if (isLevelLoaded)
                {
                    Components.Add(quadTree);
                    Components.Add(aStar);
                }
            }
            else
            {
                fpsTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                keyboard = Keyboard.GetState();

                if (!prevKeyboard.IsKeyDown(Keys.Enter) && keyboard.IsKeyDown(Keys.Enter))
                {
                    if (aStar.state == AStar.SearchState.SEARCHING)
                    {
                        aStar.searchStep();
                    }
                }

                if (frameCount % 20 == 0 &&
                     aStar.state != AStar.SearchState.SEARCHING &&
                     follower.npcState.getCurrentState() != StateMachine.States.IDLE)
                {
                    aStar.reset();
                    aStar.openList.Add(aStar.setStartNode(follower));
                    aStar.endNode = aStar.nodeModelIn(chosen);
                    foreach (BasicModel bm in modelManager.models)
                    {
                        if (bm is GroundModel)
                        {
                            aStar.closedList.Add(aStar.nodeModelIn(bm));
                        }
                    }
                    aStar.state = AStar.SearchState.SEARCHING;
                }

                
                frameCount++;
                if (fpsTimer >= 1)
                {
                    FPS = frameCount;
                    frameCount = 0;
                    fpsTimer = 0;
                }
                //keyboard check for chosen character
                chosen.keyCheck();
             
                //bounds the PC's and enemies to gravity and the level
                chosen.checkCollision();
                follower.checkCollision();
                foreach (Enemy en in modelManager.models.OfType<Enemy>())
                {
                    en.Update(camera);
                    en.checkCollision();
                }

                //debug shnuff
                if (!prevKeyboard.IsKeyDown(Keys.Z) && keyboard.IsKeyDown(Keys.Z))
                {
                    Console.WriteLine("stateFollower: " + follower.npcState);
                    Console.WriteLine("newLine___" + chosen.ToString());
                }

                //Switch your character
                if (!prevKeyboard.IsKeyDown(Keys.E) && keyboard.IsKeyDown(Keys.E) && chosen is Human)
                {
                    foreach (Dwarf dwarf in modelManager.models.OfType<Dwarf>())
                    {
                        follower = chosen;
                        chosen = dwarf;
                    }
                }

                else if (!prevKeyboard.IsKeyDown(Keys.E) && keyboard.IsKeyDown(Keys.E) && chosen is Dwarf)
                {
                    foreach (Human human in modelManager.models.OfType<Human>())
                    {
                        follower = chosen;
                        chosen = human;
                    }
                }

                //toggle follow camera
                if (!prevKeyboard.IsKeyDown(Keys.X) && keyboard.IsKeyDown(Keys.X))
                {
                    if (!isFollowingPlayer)
                    {
                        isFollowingPlayer = true;
                    }
                    else
                    {
                        isFollowingPlayer = false;
                    }
                }

                //follow character maybe
                if (isFollowingPlayer)
                {
                    Vector3 tempCamPos = camera.position;
                    tempCamPos.X = chosen.position.X;
                    tempCamPos.Y = chosen.position.Y - 50;
                    camera.position = tempCamPos;
                }

                //reset characters to original position
                if (keyboard.IsKeyDown(Keys.R))
                {
                    reloadWorld();
                    SoundEffectInstance se = (SoundEffectInstance)soundManager.soundBank["dwarfStep"];
                    se.Play();
                }
                if (keyboard.IsKeyDown(Keys.T))
                {
                    SoundEffectInstance se = (SoundEffectInstance)soundManager.soundBank["humanStep"];
                    se.Play();
                }
                prevKeyboard = keyboard;
            }

            base.Update(gameTime);
        }

        private void reloadWorld()
        {
            modelManager.loadWorld();
            foreach (Human ch in modelManager.models.OfType<Human>())
            {
                chosen = ch;
                foreach (Dwarf df in modelManager.models.OfType<Dwarf>())
                {
                    follower = df;
                    follower.position = chosen.position - Vector3.UnitX * 40;
                }
            }
        }
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            //GraphicsDevice.BlendState = BlendState.Opaque;
            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            DebugShapeRenderer.AddBoundingBox(chosen.box, Color.HotPink);
            DebugShapeRenderer.AddLine(chosen.backFeet, chosen.backFeet + Vector3.Up * (chosen.box.Max.Y - chosen.box.Min.Y), Color.OldLace);
            DebugShapeRenderer.AddLine(chosen.frontFeet, chosen.frontFeet + Vector3.Up * (chosen.box.Max.Y - chosen.box.Min.Y), Color.OldLace);
            DebugShapeRenderer.AddBoundingBox(follower.box, Color.Magenta);
            DebugShapeRenderer.AddLine(follower.backFeet + Vector3.UnitZ * 50, follower.backFeet + Vector3.Up * (follower.box.Max.Y - follower.box.Min.Y) + Vector3.UnitZ * 50, Color.OldLace);
            DebugShapeRenderer.AddLine(follower.frontFeet + Vector3.UnitZ * 50, follower.frontFeet + Vector3.Up * (follower.box.Max.Y - follower.box.Min.Y) + Vector3.UnitZ * 50, Color.OldLace);

            foreach (Enemy enemy in modelManager.models.OfType<Enemy>())
            {
                DebugShapeRenderer.AddBoundingBox(enemy.box, Color.Red);
            }

            if (aStar != null)
            {
                foreach (AStar.Node node in aStar.gridBoxes)
                {
                    //DebugShapeRenderer.AddBoundingBox(node.box, Color.BlanchedAlmond);
                }
                foreach (AStar.Node node in aStar.closedList)
                {
                    //DebugShapeRenderer.AddBoundingBox(node.box, Color.LightGoldenrodYellow);
                    if (node.hasParent())
                    {
                        DebugShapeRenderer.AddLine(node.centre(), node.currentParent.centre(), Color.Red);
                    }
                }
                foreach (AStar.Node node in aStar.openList)
                {
                    if (node != null)
                    {
                        DebugShapeRenderer.AddLine((node.centre() + new Vector3(-10, 0, 0)), (node.centre() + new Vector3(10, 0, 0)), Color.DeepPink);
                        DebugShapeRenderer.AddLine((node.centre() + new Vector3(0, -10, 0)), (node.centre() + new Vector3(0, 10, 0)), Color.DeepPink);
                    }
                }

                foreach (Vector3 targetNode in aStar.paths)
                {
                    DebugShapeRenderer.AddLine((targetNode + new Vector3(-10, 0, 0)), (targetNode + new Vector3(10, 0, 0)), Color.CadetBlue);
                    DebugShapeRenderer.AddLine((targetNode + new Vector3(0, -10, 0)), (targetNode + new Vector3(0, 10, 0)), Color.CadetBlue);
                }
                DebugShapeRenderer.AddBoundingFrustum(camera.frustum, Color.Firebrick);
                if (quadTree != null)
                {
                    foreach (QuadTree qt in QuadTree.leavesInsideBound)
                    {
                        DebugShapeRenderer.AddBoundingBox(qt.boundingBox, Color.Violet);
                    }
                }
                //spriteBatch.Begin();
                //foreach (AStar.Node node in aStar.gridBoxes)
                //{
                //    string h = string.Copy("H: " + node.hScore + "\nG: " + node.gScore + "\nF: " + node.fScore());// + "\nY:" + aStar.nodeArrayIndex(aStar.gridBoxes, node));
                //    Vector3 projectedPosMax = GraphicsDevice.Viewport.Project(node.box.Max, camera.projection, camera.view, Matrix.Identity);
                //    Vector3 projectedPosMin = GraphicsDevice.Viewport.Project(node.box.Min, camera.projection, camera.view, Matrix.Identity);
                //    Vector2 screenPos = new Vector2(projectedPosMin.X + 10, projectedPosMax.Y);
                //    //spriteBatch.DrawString(debugFont, test, screenPos, Color.White);    
                //    spriteBatch.DrawString(debugFont, h, screenPos, Color.White);
                //}
                //spriteBatch.End();
            }

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            DebugShapeRenderer.Draw(gameTime, camera.view, camera.projection);

            base.Draw(gameTime);
            showDebug();
			showScore();
        }

        #region Functions
        private void loadLevelBoxes()
        {
            int count = modelManager.models.Count;
            foreach (GroundModel gm in modelManager.models.OfType<GroundModel>())
            {
                if (!isLevelLoaded)
                {
                    BoundingBox box = gm.box;
                    if (box.Min != Vector3.Zero && box.Max != Vector3.Zero)
                        levelBoxes.Add(box);
                    if (levelBoxes.Count == count)
                        isLevelLoaded = true;
                }
            }
        }

        //checks where the collision is happening
        public collisionType collisionBoxToBoxes(BoundingBox box, List<BoundingBox> boxes)
        {
            for (int i = 0; i < boxes.Count; i++)
            {
                if (box.Intersects(boxes[i]))
                {
                    if (box.Max.X > boxes[i].Min.X && box.Min.X < boxes[i].Max.X && (box.Max.Y + box.Min.Y) / 2 > boxes[i].Max.Y)
                        return collisionType.TOP;
                    else if ((box.Max.X + box.Min.X) / 2 > boxes[i].Min.X && (box.Max.X + box.Min.X) / 2 < boxes[i].Max.X && (box.Max.Y + box.Min.Y) / 2 < boxes[i].Max.Y)
                        return collisionType.BOTTOM;
                    else if ((box.Max.X + box.Min.X) / 2 > boxes[i].Max.X)
                        return collisionType.RIGHT;
                    else if ((box.Max.X + box.Min.X) / 2 < boxes[i].Min.X)
                        return collisionType.LEFT;

                }
            }
            return collisionType.NO_COLLISION;
        }
        //SAME AS ABOVE METHOD BUT WITH ONLY ONE BOX!
        public collisionType collisionBoxToBox(BoundingBox box1, BoundingBox box2)
        {
                if (box1.Intersects(box2))
                {
                    if (box1.Max.X > box2.Min.X && box1.Min.X < box2.Max.X && (box1.Max.Y + box1.Min.Y) / 2 > box2.Max.Y)
                        return collisionType.TOP;
                    else if ((box1.Max.X + box1.Min.X) / 2 > box2.Min.X && (box1.Max.X + box1.Min.X) / 2 < box2.Max.X && (box1.Max.Y + box1.Min.Y) / 2 < box2.Max.Y)
                        return collisionType.BOTTOM;
                    else if ((box1.Max.X + box1.Min.X) / 2 > box2.Max.X)
                        return collisionType.RIGHT;
                    else if ((box1.Max.X + box1.Min.X) / 2 < box2.Min.X)
                        return collisionType.LEFT;

                }
            return collisionType.NO_COLLISION;
        }

        //returns the box index of colliding box
        public BoundingBox boxCollidingWith(BoundingBox box, List<BoundingBox> boxes)
        {
            for (int i = 0; i < boxes.Count; i++)
            {
                if (box.Intersects(boxes[i]))
                    return boxes[i];
            }
            return new BoundingBox(Vector3.Zero, Vector3.Zero);
        }

        //collision check between ray and bounding boxes in a list
        public float? collisionRayToBoxes(Vector3 position, List<BoundingBox> boxes)
        {
            Ray ray = new Ray(position, -Vector3.UnitY);
            for (int i = 0; i < boxes.Count; i++)
            {
                float? intersects = ray.Intersects(boxes[i]);
                if (intersects.HasValue)
                {
                    return intersects;
                }
            }
            return null;
        }
        //check ray collisino with single box
        public float? collisionRayToBox(Ray r, BoundingBox box)
        {                    
                float? intersects = r.Intersects(box);
                if (intersects.HasValue)
                {
                    return intersects;
                }
            
            return null;
        }

        public BasicModel modelCollidingWith(Ray r)
        {
            BasicModel tempModel = null;
            float smallestValue = float.MaxValue;
            foreach (BasicModel bm in modelManager.models)
            {
                float? intersects = r.Intersects(bm.box);
                if (intersects.HasValue)
                {
                    if (intersects.Value < smallestValue)
                    {
                        smallestValue = intersects.Value;
                        tempModel = bm;
                    }
                }
            }
            return tempModel;
        }

        public BasicModel enemyCollidingWith(BoundingBox b)
        {
            foreach (BasicModel bm in modelManager.models)
            {
                if (b.Intersects(bm.box) && bm is Enemy)
                {
                    return bm;
                }
            }
            return null;
        }

        private void showScore()
        {
            string scoreString = string.Copy("Score: " + score);
            spriteBatch.Begin();
            spriteBatch.DrawString(debugFont, scoreString, new Vector2(10, 20), Color.White);
            spriteBatch.End();
        }
        private void showDebug()
        {
            string test = string.Copy("follower: " + follower.GetType() + ", state: " + follower.npcState);
            string front = "";
            foreach (Enemy e in modelManager.models.OfType<Enemy>())
            {
                front = string.Copy("enemy: " + collisionRayToBox(e.vision, chosen.box));
            }
            string back = string.Copy("back: " + collisionRayToBoxes(follower.backFeet, levelBoxes));
            string positionInText = string.Format("Position of character: ({0:0.0}, {1:0.0})", follower.position.X, follower.position.Y);
            string velocityInText = string.Format("Velocity of character: ({0:0.0}, {1:0.0})", follower.velocity.X, follower.velocity.Y);
            string jump = string.Copy("canJump = " + chosen.canJump);
            string cameraFollow = string.Copy("camera follow? " + isFollowingPlayer);
            string chosenChar = string.Copy("Selected: " + chosen.GetType());
            string rotation = string.Copy("rotation: " + chosen.rotationY);
            string path = string.Copy("pathIndex: " + follower.pathIndex);
            string aStarInText = string.Copy("A* State: " + aStar.state);
            string targets = string.Copy("target: " + follower.target.Y + ", jumpHeight: " + (follower.position.Y + follower.maxJumpHeight));

            //string frameRateString = string.Format("time: ({0:0.0})", frameRate);
            //string scoreInText = string.Copy("GAME SCORE: " + gameScore);

            spriteBatch.Begin();
            //spriteBatch.DrawString(debugFont, positionInText, new Vector2(10, 0), Color.White);
            spriteBatch.DrawString(debugFont, velocityInText, new Vector2(10, 20), Color.White);
            spriteBatch.DrawString(debugFont, jump, new Vector2(10, 40), Color.White);
            spriteBatch.DrawString(debugFont, cameraFollow, new Vector2(10, 60), Color.White);
            spriteBatch.DrawString(debugFont, front, new Vector2(10, 80), Color.White);
            spriteBatch.DrawString(debugFont, back, new Vector2(175, 80), Color.White);
            spriteBatch.DrawString(debugFont, path, new Vector2(10, 100), Color.White);
            spriteBatch.DrawString(debugFont, test, new Vector2(10, 120), Color.White);
            spriteBatch.DrawString(debugFont, rotation, new Vector2(10, 140), Color.White);
            spriteBatch.DrawString(debugFont, aStarInText, new Vector2(10, 160), Color.White);
            spriteBatch.DrawString(debugFont, targets, new Vector2(10, 180), Color.White);
            spriteBatch.DrawString(debugFont, positionInText, new Vector2(10, 0), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 1);
            //spriteBatch.DrawString(debugFont, frameRateString, new Vector2(10, 180), Color.White);
            //spriteBatch.DrawString(debugFont, scoreInText, new Vector2(10, 200), Color.Red);
            spriteBatch.End();
        }
        public int numberOfCollisions(BoundingBox box, List<BoundingBox> boxes)
        {
            int count = 0;
            for (int i = 0; i < boxes.Count; i++)
            {
                if (box.Intersects(boxes[i]))
                    count++;
            }
            return count;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkinnedModel;

namespace Game_Assignment
{
    public class BasicModel
    {
        public Model model { get; protected set; }
        protected Matrix world = Matrix.Identity;
        public Matrix[] transforms;
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 backFeet;
        public Vector3 frontFeet;
        public float scaleValue;
        public float rotationY = 0;
        public float rotationX = 0;
        public float rotationZ = 0;
        public bool canJump = true;
        public BoundingBox box;
        public Game1 game;
        public AnimationPlayer animationPlayer;
        public SkinningData skinningData;

        public BasicModel(Model m, Game1 game)
        {
            model = m;
            this.game = game;
            transforms = new Matrix[model.Bones.Count];
            skinningData = model.Tag as SkinningData;
            if (skinningData != null)
            {
                animationPlayer = new AnimationPlayer(skinningData);
            }
        }

        public virtual void Update(Camera camera)
        {
        }

        public virtual void Draw(Camera camera, GraphicsDevice device)
        {
            model.Root.Transform = Matrix.Identity *
                Matrix.CreateScale(scaleValue) *
                Matrix.CreateRotationX(rotationX) *
                Matrix.CreateRotationY(rotationY) *
                Matrix.CreateRotationZ(rotationZ) *
                Matrix.CreateTranslation(position);
            if (skinningData != null)
            {
                Matrix[] bones = animationPlayer.GetSkinTransforms();

                model.CopyAbsoluteBoneTransformsTo(transforms);
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (SkinnedEffect se in mesh.Effects)
                    {
                        se.SetBoneTransforms(bones);
                        se.World = transforms[mesh.ParentBone.Index];
                        se.Projection = camera.projection;
                        se.View = camera.view;
                        se.AmbientLightColor = Vector3.One;
                        se.DiffuseColor = Vector3.One;
                        se.EnableDefaultLighting();
                        box = UpdateBoundingBox(model, se.World);
                    }
                    foreach (QuadTree qt in QuadTree.leavesInsideBound)
                    {
                        if (qt.objects.Contains(this))
                        {
                            mesh.Draw();
                        }
                    }
                }
            }
        }

        public virtual Matrix GetWorld()
        {
            return world;
        }
        protected BoundingBox UpdateBoundingBox(Model model, Matrix worldTransform)
        {
            // Initialize minimum and maximum corners of the bounding box to max and min values
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // For each mesh of the model
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Vertex buffer parameters
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = meshPart.NumVertices * vertexStride;

                    // Get vertex data as float
                    float[] vertexData = new float[vertexBufferSize / sizeof(float)];
                    meshPart.VertexBuffer.GetData<float>(vertexData);

                    // Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
                    for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
                    {
                        Vector3 transformedPosition = Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]), worldTransform);

                        min = Vector3.Min(min, transformedPosition);
                        max = Vector3.Max(max, transformedPosition);
                    }
                }
            }
            max.Z += 50;
            // Create and return bounding box
            return new BoundingBox(min, max);
        }
        public void checkCollision()
        {
            //resolves collisions when on top of level
            if (game.collisionBoxToBoxes(box, game.levelBoxes) == Game1.collisionType.TOP)
            {
                float depthOfContact = box.Min.Y - game.boxCollidingWith(box, game.levelBoxes).Max.Y;
                Vector3 tempPos = position;
                tempPos.Y -= depthOfContact - 0.5f;
                position = tempPos;
                Vector3 tempVelocity = velocity;
                tempVelocity.Y = 0;
                velocity = tempVelocity;
                canJump = true;
                if (this == game.follower)
                    Console.WriteLine("TOP");
            }
            //resolves collisions when under level
            else if (game.collisionBoxToBoxes(box, game.levelBoxes) == Game1.collisionType.BOTTOM)
            {
                Game1.collisionType testCol = game.collisionBoxToBoxes(box, game.levelBoxes);
                float depthOfContact = box.Max.Y - game.boxCollidingWith(box, game.levelBoxes).Min.Y;
                Vector3 tempPos = position;
                tempPos.Y -= depthOfContact + 0.5f;
                position = tempPos;
                Vector3 tempVel = velocity;
                tempVel.Y = 0;
                velocity = tempVel;
                if (this == game.follower)
                    Console.WriteLine("BOTTOM");
            }
            //resolves collisions when left of level
            else if (game.collisionBoxToBoxes(box, game.levelBoxes) == Game1.collisionType.LEFT)
            {
                float depthOfContact = box.Max.X - game.boxCollidingWith(box, game.levelBoxes).Min.X;
                Vector3 tempPos = position;
                tempPos.X -= depthOfContact + 0.5f;
                position = tempPos;
                Vector3 tempVel = velocity;
                tempVel.X = 0;
                velocity = tempVel;
                if (this == game.follower)
                    Console.WriteLine("LEFT");
            }
            //resolves collisions when right of level
            else if (game.collisionBoxToBoxes(box, game.levelBoxes) == Game1.collisionType.RIGHT)
            {
                float depthOfContact = box.Min.X - game.boxCollidingWith(box, game.levelBoxes).Max.X;
                Vector3 tempPos = position;
                tempPos.X -= depthOfContact - 0.5f;
                position = tempPos;
                Vector3 tempVel = velocity;
                tempVel.X = 0;
                velocity = tempVel;
                if (this == game.follower)
                    Console.WriteLine("RIGHT");
            }

            //apply gravity when jumping or not colliding with the floor
            if (game.collisionRayToBoxes(backFeet, game.levelBoxes) != 0 &&
                game.collisionRayToBoxes(frontFeet, game.levelBoxes) != 0)
            {
                velocity += game.GRAVITY;
            }
        }
    }
}

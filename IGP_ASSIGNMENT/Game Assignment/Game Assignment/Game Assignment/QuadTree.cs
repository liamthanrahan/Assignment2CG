using System;
using System.Collections.Generic;
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
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class QuadTree : Microsoft.Xna.Framework.GameComponent
    {
        public BoundingBox boundingBox;
        public List<BasicModel> objects;
        public int maxObjects;
        public QuadTree parent;
        public QuadTree topLeft;
        public QuadTree topRight;
        public QuadTree bottomLeft;
        public QuadTree bottomRight;
        public static List<QuadTree> leavesInsideBound = new List<QuadTree>();

         public QuadTree(int maxObjects, BoundingBox box, Game game)
             :base(game)
        {
            this.maxObjects = maxObjects;
            box.Min.Z = -100;
            box.Max.Z = 100;
            boundingBox = box;
            objects = new List<BasicModel>(maxObjects);            
        }
        //create leaves
        public QuadTree(int maxObjects, Vector3 position, Vector3 scale, Game game)
            :this(maxObjects, new BoundingBox(position - scale * 0.5f, position + scale * 0.5f), game)
        {

        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            removeChildren();            
            foreach (BasicModel bm in ((Game1)Game).modelManager.models)
            {
                addObject(bm);
            }
            getLeavesInsideFrustum(((Game1)Game).camera.frustum);

            base.Update(gameTime);
        }

        //returns if this quadtree is a leaf
        public bool isLeaf
        {
            get { return topLeft == null; }
        }
        //counts leaves in this quadtree
        public int leafCount
        {
            get
            {
                return isLeaf ? 4 : (topLeft.leafCount + topRight.leafCount + bottomLeft.leafCount + bottomRight.leafCount);
            }
        }
        //counts all objects in it's children or self
        public int objectCount
        {
            get
            {
                return isLeaf ? objects.Count : (topLeft.objects.Count + topRight.objects.Count + bottomLeft.objects.Count + bottomRight.objects.Count);
            }
        }
        //add objects to this quadtree
        public QuadTree addObject(BasicModel bm)
        {
            QuadTree result = null;
            if (boundingBox.Contains(bm.position) == ContainmentType.Contains)
            {
                if (topLeft == null)
                {
                    if (objects.Count < maxObjects)
                    {
                        objects.Add(bm);
                        return this;
                    }

                    //splits the box into 4 
                    split();

                    if (topLeft == null)
                    {
                        maxObjects *= 2;
                        return addObject(bm);
                    }
                }

                result = topLeft.addObject(bm);
                if (result == null)
                {
                    result = topRight.addObject(bm);
                    if (result == null)
                    {
                        result = bottomLeft.addObject(bm);
                        if (result == null)
                        {
                            result = bottomRight.addObject(bm);
                        }
                    }
                }
            }
            return result;
        }
        
        public void removeObject(BasicModel bm)
        {
            QuadTree leaf = findLeaf(bm.position);
            if (leaf != null)
            {
                leaf.objects.Remove(bm);
            }
        }

        public QuadTree objectMoved(BasicModel bm, QuadTree prevNode)
        {
            QuadTree result = null;
            if (boundingBox.Contains(bm.position) == ContainmentType.Contains)
            {
                if (topLeft == null)
                {
                    if (this != prevNode)
                    {
                        if (prevNode != null)
                        {
                            prevNode.objects.Remove(bm);
                        }
                        return addObject(bm);
                    }
                    return this;
                }

                if ((result = topLeft.objectMoved(bm, prevNode)) == null)
                {
                    if((result = topRight.objectMoved(bm, prevNode)) == null)
                    {
                        if ((result = bottomLeft.objectMoved(bm, prevNode)) == null)
                        {
                            result = bottomRight.objectMoved(bm, prevNode);
                        }
                    }
                }
            }
            return result;
        }

        public QuadTree findLeaf(Vector3 position)
        {
            if (boundingBox.Contains(position) == ContainmentType.Contains)
            {
                if (topLeft == null)
                {
                    return this;
                }
                else
                {
                    QuadTree result;
                    if ((result = topLeft.findLeaf(position)) == null)
                    {
                        if ((result = topRight.findLeaf(position)) == null)
                        {
                            if ((result = bottomLeft.findLeaf(position)) == null)
                            {
                                result = bottomRight.findLeaf(position);
                            }
                        }
                    }
                }
            }
            return null;
        }

        public List<QuadTree> getLeavesInsideFrustum(BoundingFrustum frustum)
        {
            leavesInsideBound.Clear();
            addLeavesInsideFrustum(frustum);
            return leavesInsideBound;
        }

        public void addLeavesInsideFrustum(BoundingFrustum frustum)
        {
            if (frustum.Contains(boundingBox) != ContainmentType.Disjoint)
            {
                if (topLeft == null)
                {
                    leavesInsideBound.Add(this);
                }
                else
                {
                    topLeft.addLeavesInsideFrustum(frustum);
                    topRight.addLeavesInsideFrustum(frustum);
                    bottomLeft.addLeavesInsideFrustum(frustum);
                    bottomRight.addLeavesInsideFrustum(frustum);
                }
            }
        }

        public void split()
        {
            float halfScaleX = (boundingBox.Max.X - boundingBox.Min.X) * 0.5f;
            float halfScaleY = (boundingBox.Max.Y - boundingBox.Min.Y) * 0.5f;
            Vector3 halfScale = new Vector3(halfScaleX, halfScaleY, 0);
            float qtrScaleX = halfScaleX * 0.5f;
            float qtrScaleY = halfScaleY * 0.5f;

            if (qtrScaleX != 0 && qtrScaleY != 0)
            {
                Vector3 topLeftPos = boundingBox.Min + new Vector3(qtrScaleX, qtrScaleY, 0);
                Vector3 topRightPos = boundingBox.Min + new Vector3(qtrScaleX + halfScaleX, qtrScaleY, 0);
                Vector3 bottomLeftPos = boundingBox.Min + new Vector3(qtrScaleX,  qtrScaleY + halfScaleY, 0);
                Vector3 bottomRightPos = boundingBox.Min + new Vector3(qtrScaleX + halfScaleX, qtrScaleY + halfScaleY, 0);

                topLeft = new QuadTree(maxObjects, topLeftPos, halfScale,this.Game);
                topRight = new QuadTree(maxObjects, topRightPos, halfScale, this.Game);
                bottomLeft = new QuadTree(maxObjects, bottomLeftPos, halfScale, this.Game);
                bottomRight = new QuadTree(maxObjects, bottomRightPos, halfScale, this.Game);

                topLeft.parent = this;
                topRight.parent = this;
                bottomLeft.parent = this;
                bottomRight.parent = this;

                reassignObjects();
                objects.Clear();
            }
        }

        public void reassignObjects()
        {
            foreach (BasicModel bm in objects)
            {
                if (topLeft.addObject(bm) == null)
                {
                    if (topRight.addObject(bm) == null)
                    {
                        if(bottomLeft.addObject(bm) == null)
                        {
                            bottomRight.addObject(bm);
                        }
                    }
                }
            }
        }

        public void removeChildren()
        {
            leavesInsideBound.Remove(this.topLeft);
            leavesInsideBound.Remove(this.topRight);
            leavesInsideBound.Remove(this.bottomLeft);
            leavesInsideBound.Remove(this.bottomRight);
            this.topLeft = null;
            this.topRight = null;
            this.bottomLeft = null;
            this.bottomRight = null;
        }
    }
}
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
    public class AStar : Microsoft.Xna.Framework.GameComponent
    {
        public enum SearchState
        {
            IDLE,
            SEARCHING,
            NO_PATH,
            PATH_FOUND,
        }

        public class Node
        {
            public int hScore;
            public int gScore;
            public Node currentParent;
            public Node prevParent;
            public BoundingBox box;


            public Node(BoundingBox box)
            {
                this.box = box;
            }

            public Node()
            {
                // TODO: Complete member initialization
            }

            public Vector3 centre()
            {
                return new Vector3((box.Min.X + box.Max.X) / 2, (box.Min.Y + box.Max.Y) / 2, (box.Min.Z + box.Max.Z) / 2);
            }

            public int fScore()
            {
                return gScore + hScore;
            }

            public bool hasParent()
            {
                return currentParent != null;
            }
        }

        public Node[,] gridBoxes;
        public List<Node> openList = new List<Node>();
        public List<Node> closedList = new List<Node>();
        public int horizontalGrids;
        public int verticalGrids;
        public Node endNode;
        public Node startNode;
        public Node pathNode;
        public SearchState state;
        public Node currentSearchNode = new Node();
        public List<Vector3> paths;
        public List<Vector3> prevPaths;
        public List<BoundingBox> boxes;

        //uses the size of the bounding box and splits it up into horizontal and vertical grids by hGrids and vGrids 
        public AStar(float width, float height, int hGrids, int vGrids, Game game)
            : base(game)
        {
            paths = new List<Vector3>();
            horizontalGrids = hGrids;
            verticalGrids = vGrids;
            gridBoxes = new Node[hGrids, vGrids];
            boxes = new List<BoundingBox>();
            float tempWidth = width * 117.3f;
            float tempHeight = height * 88.7f;
            float widthOfGrid = tempWidth / hGrids;
            float heightOfGrid = tempHeight / vGrids;
            int y = 0;
            for (float i = 0; i <= (int)tempHeight - 1; i += heightOfGrid)
            {
                int x = 0;
                for (float j = 0; j <= (int)tempWidth - 1; j += widthOfGrid)
                {

                    BoundingBox grid = new BoundingBox(new Vector3(j - widthOfGrid / 2, (-i - heightOfGrid / 2), -50), new Vector3((j + widthOfGrid / 2), -i + heightOfGrid / 2, 50));
                    gridBoxes[x, y] = new Node(grid);
                    boxes.Add(grid);
                    x++;
                }
                y++;
            }
        }

        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            switch (state)
            {
                case SearchState.SEARCHING:
                    {
                        while (state != SearchState.PATH_FOUND)
                        {
                            if (endNode != null)
                            {
                                searchStep();
                            }
                            if (state == SearchState.NO_PATH)
                                break;
                        }
                        break;
                    }
                case SearchState.PATH_FOUND:
                    {
                        createFinalPath();
                        break;
                    }
                case SearchState.NO_PATH:
                    {
                        break;
                    }
                case SearchState.IDLE:
                    {
                        break;
                    }
            }
            base.Update(gameTime);
        }

        public void setClosedGrids(BasicModel bm)
        {
            foreach (Node node in gridBoxes)
            {
                if (node.box.Contains(bm.position) == ContainmentType.Contains)
                {
                    closedList.Add(node);
                }
            }
        }

        public Node setStartNode(BasicModel bm)
        {
            foreach (Node node in gridBoxes)
            {
                if (node.box.Contains(bm.position) == ContainmentType.Contains)
                {
                    startNode = node;
                    return node;
                }
            }
            return null;
        }

        public int nodeListIndex(List<Node> nodes, Node node)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (node.Equals(nodes[i]))
                {
                    return i;
                }
            }
            return 0;
        }

        public Tuple<int, int> nodeArrayIndex(Node node)
        {
            for (int i = 0; i < gridBoxes.GetLength(0); i++)
            {
                for (int j = 0; j < gridBoxes.GetLength(1); j++)
                {
                    if (node.Equals(gridBoxes[i, j]))
                    {
                        return new Tuple<int, int>(i, j);
                    }
                }
            }
            return null;
        }

        public int stepDistance(Node node1, Node node2)
        {
            int distanceX = (int)Math.Abs(node1.centre().X - node2.centre().X);
            int distanceY = (int)Math.Abs(node1.centre().Y - node2.centre().Y);

            return (int)Math.Sqrt(Math.Pow(distanceX, 2) + Math.Pow(distanceY, 2));
        }

        public int heuristic(Node node)
        {
            return stepDistance(node, endNode);
        }

        public bool inMap(int x, int y)
        {
            return (x >= 0 && x < horizontalGrids && y >= 0 && y < verticalGrids);
        }

        public bool isClosed(int x, int y)
        {
            foreach (Node node in closedList)
            {
                if (gridBoxes[x, y].Equals(node))
                {
                    return true;
                }
            }
            return false;
        }

        public bool isOpen(int x, int y)
        {
            return inMap(x, y) && !isClosed(x, y);
        }

        public IEnumerable<Node> nearNodes(Node node)
        {
            int nodeX = nodeArrayIndex(node).Item1;
            int nodeY = nodeArrayIndex(node).Item2;
            if (isOpen(nodeX, nodeY + 1))
            {
                gridBoxes[nodeX, nodeY + 1].currentParent = node;
                yield return gridBoxes[nodeX, nodeY + 1];
            }
            if (isOpen(nodeX, nodeY - 1))
            {
                gridBoxes[nodeX, nodeY - 1].currentParent = node;
                yield return gridBoxes[nodeX, nodeY - 1];
            }
            if (isOpen(nodeX + 1, nodeY))
            {
                gridBoxes[nodeX + 1, nodeY].currentParent = node;
                yield return gridBoxes[nodeX + 1, nodeY];
            }
            if (isOpen(nodeX - 1, nodeY))
            {
                gridBoxes[nodeX - 1, nodeY].currentParent = node;
                yield return gridBoxes[nodeX - 1, nodeY];
            }
            if (isOpen(nodeX + 1, nodeY + 1))
            {
                gridBoxes[nodeX + 1, nodeY + 1].currentParent = node;
                yield return gridBoxes[nodeX + 1, nodeY + 1];
            }
            if (isOpen(nodeX - 1, nodeY - 1))
            {
                gridBoxes[nodeX - 1, nodeY - 1].currentParent = node;
                yield return gridBoxes[nodeX - 1, nodeY - 1];
            }
            if (isOpen(nodeX + 1, nodeY - 1))
            {
                gridBoxes[nodeX + 1, nodeY - 1].currentParent = node;
                yield return gridBoxes[nodeX + 1, nodeY - 1];
            }
            if (isOpen(nodeX - 1, nodeY + 1))
            {
                gridBoxes[nodeX - 1, nodeY + 1].currentParent = node;
                yield return gridBoxes[nodeX - 1, nodeY + 1];
            }
        }

        public bool inList(List<Node> list, Node node)
        {
            foreach (Node listNode in list)
            {
                if (node.Equals(listNode))
                {
                    return true;
                }
            }
            return false;
        }

        public void searchStep()
        {
            Node newNode;

            bool foundNewNode = selectNextNode(out newNode);
            if (foundNewNode)
            {
                Vector3 currentPos = newNode.centre();

                Node bestNode = new Node();
                bestNode.hScore = int.MaxValue;
                foreach (Node tempNode in nearNodes(newNode))
                {
                    int x = nodeArrayIndex(tempNode).Item1;
                    int y = nodeArrayIndex(tempNode).Item2;
                    y++;

                    int prevGscore = int.MaxValue;
                    if (tempNode.prevParent != null)
                    {
                        prevGscore = (stepDistance(tempNode, tempNode.prevParent) + tempNode.prevParent.gScore);
                        //prevGscore += prevGscore/((int)nodeArrayIndex(gridBoxes, tempNode).Item2 + 1);
                    }
                    tempNode.gScore = (stepDistance(tempNode, tempNode.currentParent) + tempNode.currentParent.gScore);
                    //tempNode.gScore += tempNode.gScore / ((int)nodeArrayIndex(gridBoxes, tempNode).Item2 + 1);
                    tempNode.hScore = heuristic(tempNode);
                    if (prevGscore < tempNode.gScore)
                    {
                        tempNode.currentParent = tempNode.prevParent;
                    }
                    if (!inList(openList, tempNode) && !inList(closedList, tempNode))
                    {
                        openList.Add(tempNode);

                        if (tempNode.fScore() < bestNode.fScore())
                        {
                            bestNode = tempNode;
                        }
                    }
                    if (tempNode.hasParent())
                    {
                        tempNode.prevParent = tempNode.currentParent;
                    }

                }
                if (currentPos == endNode.centre())
                {
                    state = SearchState.PATH_FOUND;
                }

                openList.Remove(newNode);
                closedList.Add(newNode);
                currentSearchNode = newNode;
            }
            else
            {
                state = SearchState.NO_PATH;
            }
        }

        public bool selectNextNode(out Node node)
        {
            node = new Node();
            node.hScore = int.MaxValue;
            bool success = false;
            if (openList.Count > 0)
            {
                foreach (Node openNode in openList)
                {
                    if (openNode != null)
                    {
                        if (openNode.fScore() <= node.fScore())
                        {
                            if (openNode.fScore() < node.fScore())
                            {
                                success = true;
                                node = openNode;
                            }
                        }
                    }
                }
            }
            return success;
        }

        public void createFinalPath()
        {
            //paths.Clear();
            pathNode = endNode;
            while (pathNode.hasParent())
            {
                Vector3 tempPos = pathNode.centre();
                float target = Math.Abs(pathNode.box.Max.Y - pathNode.box.Min.Y);
                target /= 2.5f;
                tempPos.Y -= target;
                paths.Add(tempPos);
                pathNode = pathNode.currentParent;
            }
            paths.Reverse();
        }

        public void reset()
        {
            state = SearchState.IDLE;
            openList.Clear();
            closedList.Clear();
            paths.Clear();
            foreach (Node node in gridBoxes)
            {
                node.hScore = 0;
                node.gScore = 0;
                node.currentParent = null;
                node.prevParent = null;
            }
        }

        public Node nodeModelIn(BasicModel bm)
        {
            foreach (Node node in gridBoxes)
            {
                if (node.box.Contains(bm.position) == ContainmentType.Contains)
                    return node;
            }
            return null;
        }
    }
}
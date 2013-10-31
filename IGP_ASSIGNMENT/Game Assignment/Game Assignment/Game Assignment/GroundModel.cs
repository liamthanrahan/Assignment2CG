using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game_Assignment
{
    public enum groundType
    {
        FLAT,
        LEFT_RAMP,
        RIGHT_RAMP
    }
    public class GroundModel : BasicModel
    {
        public groundType type;        
        
        public GroundModel(Model m, int xPos, int yPos, groundType type, Game1 game)
            : base(m, game)
        {
            position = new Vector3(xPos * 117.3f, yPos * 88.7f, 0);
            scaleValue = 30.0f;
            rotationX = 0;
            rotationY = MathHelper.ToRadians(-90.0f);
            rotationZ = MathHelper.ToRadians(-90.0f);
            this.type = type;
        }
        public float top()
        {
            return box.Max.Y;
        }
        public float bottom()
        {
            return box.Min.Y;
        }
        public float Left()
        {
            return box.Min.X;
        }
        public float Right()
        {
            return box.Max.X;
        }
    }
}

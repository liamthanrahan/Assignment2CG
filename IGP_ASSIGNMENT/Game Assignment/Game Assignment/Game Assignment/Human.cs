using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SkinnedModel;

namespace Game_Assignment
{
    public class Human : Character
    {
        public Human(Model m, int xPos, int yPos, Game1 game)
            : base(m, xPos, yPos, game)
        {            
            scaleValue = 20;
            maxSpeed = 5;
            jumpForce = 9;
            rotationY = MathHelper.ToRadians(90);
            rotationX = MathHelper.ToRadians(-90);
            AnimationClip clip = skinningData.AnimationClips["Throw"];
            animationPlayer.StartClip(clip);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SkinnedModel;

namespace Game_Assignment
{
    public class Dwarf : Character
    {
        public Dwarf(Model m, int xPos, int yPos, Game1 game)
            : base(m, xPos, yPos, game)
        {
            scaleValue = 27;
            maxSpeed = 7;
            jumpForce = 7;
            rotationY = MathHelper.ToRadians(90);
            rotationX = MathHelper.ToRadians(-90);
            AnimationClip clip = skinningData.AnimationClips["Idle"];
            animationPlayer.StartClip(clip);
        }
    }
}

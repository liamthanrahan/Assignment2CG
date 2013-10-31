﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game_Assignment
{
    public class Dwarf : Character
    {
        public Dwarf(Model m, int xPos, int yPos, Game1 game)
            : base(m, xPos, yPos, game)
        {
            scaleValue = 13;
            maxSpeed = 7;
            jumpForce = 10;
            rotationX += MathHelper.ToRadians(90);
            maxJumpHeight = -(jumpForce * jumpForce) / (2 * game.GRAVITY.Y);
        }
    }
}
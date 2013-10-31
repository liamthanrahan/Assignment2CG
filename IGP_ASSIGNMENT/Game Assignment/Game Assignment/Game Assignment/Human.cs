﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game_Assignment
{
    public class Human : Character
    {
        public Human(Model m, int xPos, int yPos, Game1 game)
            : base(m, xPos, yPos, game)
        {            
            scaleValue = 30;
            maxSpeed = 5;
            jumpForce = 14;
            maxJumpHeight = -(jumpForce * jumpForce) / (2 * game.GRAVITY.Y);
        }
    }
}

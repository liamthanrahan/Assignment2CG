﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;
using Game_Assignment_Library;

namespace Game_Assignment
{
    public class ModelManager : DrawableGameComponent
    {
        public List<BasicModel> models = new List<BasicModel>();
        public int heightOfMap;
        public int widthOfMap;

        public ModelManager(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
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

        protected override void LoadContent()
        {
            // Use configuration file to assemble world
            loadWorld();
            base.LoadContent();
        }

        public void loadWorld()
        {
            models.Clear();
            ConfigSettings config = Game.Content.Load<ConfigSettings>(@"Config\levelconfig");
            BehaviourConfig behaviourConfig = Game.Content.Load<BehaviourConfig>(@"Config\behaviourconfig");

            int counterX = 0;
            int counterY = 0;
            int counterXMax = int.MinValue;

            while (counterY < config.modelmap.GetLength(0))
            {
                while (counterX < config.modelmap[counterY].Length)
                {
                    addModel(config.modelmap[counterY][counterX], counterX, counterY);
                    counterX++;
                    if (counterX > counterXMax)
                    {
                        counterXMax = counterX;
                    }
                }
                counterX = 0;
                counterY++;
            }
            widthOfMap = counterXMax;
            heightOfMap = counterY;
        }

        private void addModel(char modelSymbol, int xPos, int yPos)
        {
            switch (modelSymbol)
            {
                case '|': models.Add(new GroundModel(Game.Content.Load<Model>(@"Models\Level\groundmodel"), xPos, -yPos, groundType.FLAT, (Game1)Game));
                    break;

                case '/': models.Add(new GroundModel(Game.Content.Load<Model>(@"Models\Level\groundmodelleft"), xPos, -yPos, groundType.RIGHT_RAMP, (Game1)Game));
                    break;

                case '\\': models.Add(new GroundModel(Game.Content.Load<Model>(@"Models\Level\groundmodelright"), xPos, -yPos, groundType.LEFT_RAMP, (Game1)Game));
                    break;

                case 'P':
                    models.Add(new Human(Game.Content.Load<Model>(@"Models\PC\basicGentleman"), xPos, -yPos, (Game1)Game));
                    models.Add(new Dwarf(Game.Content.Load<Model>(@"Models\PC\basicDwarf"), (xPos - 1), -yPos, (Game1)Game));
                    break;

                case 'E':
                    models.Add(new Enemy(Game.Content.Load<Model>(@"Models\Enemy\basicMonster"), xPos, -yPos, (Game1)Game));
                    break;

                case 'B':
                    models.Add(new Enemy(Game.Content.Load<Model>(@"Models\Enemy\basicMonster"), xPos, -yPos, (Game1)Game));
                    break;

                case '0': break;

                default: break;
            }
            
        }

        public override void Update(GameTime gameTime)
        {
            // Loop through all models and call Update
            for (int i = 0; i < models.Count; ++i)
            {
                models[i].Update(((Game1)Game).camera);
            }
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            // Loop through and draw each model

            foreach (BasicModel bm in models)
            {
                bm.Draw(((Game1)Game).camera, ((Game1)Game).device);
            }
            base.Draw(gameTime);
        }

    }
}

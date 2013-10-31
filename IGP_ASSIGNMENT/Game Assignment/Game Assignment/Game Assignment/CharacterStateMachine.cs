using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Assignment
{
    class CharacterStateMachine : StateMachine
    {
        public enum States { IDLE, FOLLOW, JUMP }

        public CharacterStateMachine(BasicModel owner, States initialState) 
            : base(owner)
        {           
        }

        public void Update()
        {
            //Character behaviour logic
        }
    }
}

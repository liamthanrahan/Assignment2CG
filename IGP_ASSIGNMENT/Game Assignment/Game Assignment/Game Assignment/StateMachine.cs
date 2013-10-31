using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Assignment
{
    public class StateMachine
    {
        public enum States { IDLE, ATTACK, FOLLOW, JUMP, CHASE, THROW, PATROL }
        private States currentState;
        BasicModel owner;

        public StateMachine(BasicModel owner)
        {
            this.owner = owner;
            currentState = States.IDLE;
        }


        public void Update()
        {
            // check for change or transition
            // Change state
            // Change behaviour
        }

        public States getCurrentState()
        {
            return currentState;
        }

        public void changeState(States nextState)
        {
            this.currentState = nextState;
        }
    }
}

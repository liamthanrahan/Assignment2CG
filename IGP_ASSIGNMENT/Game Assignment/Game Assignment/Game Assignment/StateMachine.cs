using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Assignment
{
    public interface StateMachine
    {
        public enum States { IDLE, ATTACK, FOLLOW, JUMP, CHASE, THROW, PATROL }
        private States currentState;
        BasicModel owner;

        public StateMachine(BasicModel owner, States initialState)
        {
            this.owner = owner;
            currentState = initialState;
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

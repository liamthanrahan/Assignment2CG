using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Assignment
{
    class EnemyStateMachine : StateMachine
    {
        public enum States { IDLE, ATTACK, CHASE, THROW, PATROL }

        public EnemyStateMachine(BasicModel owner, EnemyStateMachine.States initialState)
            : base(owner)
        {           
        }

        public void Update()
        {
            //ENEMY behaviour logic
        }
    }
}

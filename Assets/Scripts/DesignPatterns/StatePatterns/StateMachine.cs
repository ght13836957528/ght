using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.StatePatterns
{
    public class StateMachine
    {
        private List<IState> _stateList;

        private IState _currentState;

        private int _curIndex;
        
        public StateMachine()
        {
            _stateList = new List<IState>();
        }
        
        public void Init()
        {
            if (_stateList == null || _stateList.Count <= 0)
            {
                Debug.LogError("_stateList init fail");
                return;
            }
            _curIndex = 0;
            _currentState = _stateList[_curIndex];
        }

        public void Dispose()
        {
            _currentState = null;
            _stateList.Clear();
        }
        
        public void AddState(IState state)
        {
            _stateList.Add(state);
        }

        public void MoveNext()
        {
            _currentState.OnExit();
            _curIndex ++ ;
            _currentState = _stateList[_curIndex];
            _currentState.OnEnter();
        }
        
        public void JumpIntoState()
        {
            _currentState.OnExit();
            _currentState = _stateList[_curIndex + 1];
            _currentState.OnEnter();
        }

       

    }
}
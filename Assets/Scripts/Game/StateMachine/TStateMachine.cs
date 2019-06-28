using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public sealed class StateMachine<T>
    {
        private T _owner;
        private State<T> _stateCur;
        public State<T> CurState
        {
            get { return _stateCur; }
        }
        private State<T> _statePrev;
        private int _nCurStateEnum;

        private Dictionary<int, State<T>> _dicStates = new Dictionary<int, State<T>>();
        //private bool _bIsStateChanging;

        public StateMachine(T owner)
        {
            _owner = owner;
        }

        public void AddState(State<T> state)
        {
            _dicStates.Add(state.StateEnum, state);
            state.SetOwner(_owner);
        }

        public void ChangeState(int stateEnum, object param = null, bool isForce = false)
        {
            if (_dicStates.ContainsKey(stateEnum))
            {
                if (!isForce && _stateCur != null && _stateCur.HandleNextState(stateEnum) == false)
                    return;

                if (_stateCur != null)
                {
                    _statePrev = _stateCur;
                    _stateCur.Exit();
                }
                _stateCur = _dicStates[stateEnum];
                _stateCur.Enter(param);
            }
        }

        //! 状态机更新
        public string TickState(float deltaTime)
        {
            if (_stateCur != null)
            {
                return _stateCur.Execute(deltaTime);
            }
            return null;
        }

        public void CleanState()
        {
            _stateCur = null;
        }

        public void CleanStateStatus()
        {
            _stateCur.StrStateStatus = null;
        }
    }

    public class State<T>
    {
        protected T _owner;
        protected string _strStateStatus;
        public string StrStateStatus
        {
            set {
                if (value != null)
                    GuideManager.Instance.StopGuide();
                _strStateStatus = value;
            }
            get { return _strStateStatus; }
        }
        private readonly int _nStateEnum;
        public int StateEnum {
            get { return _nStateEnum; }
        }

        public State(int stateEnum)
        {
            _nStateEnum = stateEnum;
        }

        public void SetOwner(T owner)
        {
            _owner = owner;
        }

        public virtual void Enter(object param)
        {
            _strStateStatus = null;
            Lean.Touch.LeanTouch.OnFingerDown += OnFingerDown;
            Lean.Touch.LeanTouch.OnFingerSet += OnFingerSet;
            Lean.Touch.LeanTouch.OnFingerUp += OnFingerUp;
        }
        public virtual string Execute(float deltaTime)
        { return _strStateStatus; }
        public virtual void Exit()
        {
            Lean.Touch.LeanTouch.OnFingerDown -= OnFingerDown;
            Lean.Touch.LeanTouch.OnFingerSet -= OnFingerSet;
            Lean.Touch.LeanTouch.OnFingerUp -= OnFingerUp;
        }

        public virtual bool HandleNextState(int stateEnum)
        {
            return true;
        }

        protected virtual void OnFingerDown(Lean.Touch.LeanFinger finger)
        { }
        protected virtual void OnFingerSet(Lean.Touch.LeanFinger finger)
        { }
        protected virtual void OnFingerUp(Lean.Touch.LeanFinger finger)
        { }
    }

}

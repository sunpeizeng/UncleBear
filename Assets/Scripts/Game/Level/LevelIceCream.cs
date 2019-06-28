using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public class LevelIceCream : LevelGame
    {
        private StateMachine<LevelIceCream> _fsmLevel;

        private List<GameObject> _lstBalls = new List<GameObject>();
        public List<GameObject> IceCreamBalls
        {
            get { return _lstBalls; }
        }

        private List<GameObject> _lstBallDecors = new List<GameObject>();
        public List<GameObject> BallDecors
        {
            get { return _lstBallDecors; }
        }

        public enum PhaseEnum
        {
            Milk,
            Egg,
            Mix,
            MixPre,
            Ball,
            Sauce,
            DecorBar,
            DecorCookies,
            Place,
        }

        //创建一个菜的所有子状态
        protected override void CreateLevelFsm()
        {
            _fsmLevel = new StateMachine<LevelIceCream>(this);

            //可以加参数设置每个状态的一些常量,或者在每个状态构造里自己处理
            _fsmLevel.AddState(new IceCreamStateMilk((int)PhaseEnum.Milk));
            _fsmLevel.AddState(new IceCreamStateEgg((int)PhaseEnum.Egg));
            _fsmLevel.AddState(new IceCreamStateMixPre((int)PhaseEnum.MixPre));
            _fsmLevel.AddState(new IceCreamStateMix((int)PhaseEnum.Mix));
            _fsmLevel.AddState(new IceCreamStateSauce((int)PhaseEnum.Sauce));
            _fsmLevel.AddState(new IceCreamStateBall((int)PhaseEnum.Ball));
            _fsmLevel.AddState(new IceCreamStateDecorBar((int)PhaseEnum.DecorBar));
            _fsmLevel.AddState(new IceCreamStateDecorCookies((int)PhaseEnum.DecorCookies));
            _fsmLevel.AddState(new IceCreamStatePlace((int)PhaseEnum.Place));
        }

        public override void LoadLevel()
        {
            base.LoadLevel();
            GenLevelObjects("Data/Levels/LevelObjects_IceCream");
            EnterKitchen.Instance.ObjHearth.SetActive(false);
            EnterKitchen.Instance.ObjHearthLight.SetActive(false);

            _fsmLevel.ChangeState((int)PhaseEnum.Milk, null, true);
        }

        //TODO::如果推出场景,已经加载的资源怎么处理
        public override void CleanLevel()
        {
            if (_fsmLevel.CurState != null)
                _fsmLevel.CurState.Exit();
            _fsmLevel.CleanState();

            _lstBalls.Clear();
            _lstBallDecors.Clear();

            base.CleanLevel();
            //Debug.Log("clean");
        }


        //TODO::目前先用一个字符串作为状态标识,后面看能不能改的好一点
        public override void TickStateExecuting(float deltaTime)
        {
            if (_bLevelEnded) return;

            string status = _fsmLevel.TickState(deltaTime);
            if (status != null)
            {
                if (status == "MilkReady")
                {
                    _fsmLevel.ChangeState((int)PhaseEnum.Egg);
                }
                else if (status == "EggBreakOver")
                {
                    _fsmLevel.ChangeState((int)PhaseEnum.MixPre);
                }
                else if (status == "MixPreFinished") 
                {
                    _fsmLevel.ChangeState((int)PhaseEnum.Mix);
                }
                else if (status == "MixOver") 
                {
                    _fsmLevel.ChangeState((int)PhaseEnum.Ball);
                }
                else if (status == "BallOver")
                {
                    _fsmLevel.ChangeState((int)PhaseEnum.Sauce);
                }
                else if (!_bPausedAtUI)
                {
                    _bPausedAtUI = true;
                    UIPanelManager.Instance.GetPanel("UIPanelDishLevel").ShowSubElements("UIContinueButton");
                }
                else
                {
                    bool forceChange = true;
                    switch (status)
                    {
                        case "DecorCookiesFinished":
                            _fsmLevel.ChangeState((int)PhaseEnum.Place);
                            break;
                        default:
                            forceChange = false;
                            break;
                    }
                    if (forceChange)
                        OnLevelStateContinued();
                }
            }
        }

        public override void OnContinueLevelPhase()
        {
            base.OnContinueLevelPhase();
            bool isChanged = true;
            switch (_fsmLevel.CurState.StrStateStatus)
            {
                case "SauceOver":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.DecorBar);
                    }
                    break;
                case "DecorBarReady":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.DecorCookies);
                    }
                    break;
                case "DecorCookiesOk":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.Place);
                    }
                    break;
                default:
                    isChanged = false;
                    break;
            }
            if (isChanged == true)
                OnLevelStateContinued();
        }
    }
}


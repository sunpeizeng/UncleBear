using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public class LevelSushi : LevelGame
    {
        private StateMachine<LevelSushi> _fsmLevel;
        private bool _bNorioutside;
        public bool IsNoriOutside
        {
            set { _bNorioutside = value; }
            get { return _bNorioutside; }
        }


        #region 临时资源

        private List<GameObject> _lstStuffs = new List<GameObject>();
        public List<GameObject> Stuffs
        {
            get { return _lstStuffs; }
        }

        #endregion

        public enum PhaseEnum
        {
            Ingredient,//选材
            Scroll,//卷
            Cut,//切
            Move,//摆盘
            Place,//上菜
        }

        //创建一个菜的所有子状态
        protected override void CreateLevelFsm()
        {
            _fsmLevel = new StateMachine<LevelSushi>(this);

            //AddStateHere
            _fsmLevel.AddState(new SushiStateIngredient((int)PhaseEnum.Ingredient));
            _fsmLevel.AddState(new SushiStateScroll((int)PhaseEnum.Scroll));
            _fsmLevel.AddState(new SushiStateCut((int)PhaseEnum.Cut));
            _fsmLevel.AddState(new SushiStateMove((int)PhaseEnum.Move));
            _fsmLevel.AddState(new SushiStatePlace((int)PhaseEnum.Place));
        }

        public override void LoadLevel()
        {
            base.LoadLevel();
            GenLevelObjects("Data/Levels/LevelObjects_Sushi");

            EnterKitchen.Instance.TrsTableCloth.position = new Vector3(-8, 22.4f, -19.5f);
            //可以把GameObject作为参数传入,但是目前是get属性
            _fsmLevel.ChangeState((int)PhaseEnum.Ingredient, null, true);
        }

        //TODO::如果推出场景,已经加载的资源怎么处理
        public override void CleanLevel()
        {
            if (_fsmLevel.CurState != null)
                _fsmLevel.CurState.Exit();
            _fsmLevel.CleanState();

            _lstStuffs.Clear();
            base.CleanLevel();
            //Debug.Log("clean");
        }

        public override void TickStateExecuting(float deltaTime)
        {
            if (_bLevelEnded) return;

            if(Input.GetKeyDown(KeyCode.M))
                LevelManager.Instance.ChangeLevel(LevelEnum.Main);

            string status = _fsmLevel.TickState(deltaTime);

            if (status != null)
            {
                if (status == "ScrollOver")
                {
                    _fsmLevel.ChangeState((int)PhaseEnum.Cut);
                }
                else if (!_bPausedAtUI)
                {
                    _bPausedAtUI = true;
                    UIPanelManager.Instance.GetPanel("UIPanelDishLevel").ShowSubElements("UIContinueButton");
                }
                else {
                    bool forceChange = true;
                    switch (status)
                    {
                        case "IngredientOver":
                            _fsmLevel.ChangeState((int)PhaseEnum.Scroll);
                            break;
                        case "CutOver":
                            _fsmLevel.ChangeState((int)PhaseEnum.Move);
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
                case "IngredientOK":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.Scroll);
                    }
                    break;
                case "ScrollOver":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.Cut);
                    }
                    break;
                case "SushiCuttedOk":
                    {
                        DoozyUI.UIManager.PlaySound("8成功");
                        _fsmLevel.ChangeState((int)PhaseEnum.Move);
                    }
                    break;
                case "MovedOk":
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

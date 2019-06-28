using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UncleBear
{
    public class LevelBurger : LevelGame
    {
        private StateMachine<LevelBurger> _fsmLevel;

        private List<GameObject> _lstPieces = new List<GameObject>();
        public List<GameObject> BurgerPieces
        {
            get { return _lstPieces; }
        }
        private GameObject _objChipsRoot;
        public GameObject ObjChipsRoot
        {
            set { _objChipsRoot = value; }
            get { return _objChipsRoot; }
        }

        private GameObject _objChipsPlate;
        public GameObject ObjChipsPlate
        {
            set { _objChipsPlate = value; }
            get { return _objChipsPlate; }
        }

        public enum PhaseEnum
        {
            PlanePotato,//刨
            FryChips,//煎
            ShakeChips,//摇
            Ingredient,//选料
            CutBread,//切
            Place,//摆
        }

        //创建一个菜的所有子状态
        protected override void CreateLevelFsm()
        {
            _fsmLevel = new StateMachine<LevelBurger>(this);

            //可以加参数设置每个状态的一些常量,或者在每个状态构造里自己处理
            _fsmLevel.AddState(new BurgerStatePlanePotato((int)PhaseEnum.PlanePotato));
            _fsmLevel.AddState(new BurgerStateFryPotato((int)PhaseEnum.FryChips));
            _fsmLevel.AddState(new BurgerStateShakePotato((int)PhaseEnum.ShakeChips));
            _fsmLevel.AddState(new BurgerStateIngredient((int)PhaseEnum.Ingredient));
            _fsmLevel.AddState(new BurgerStateBread((int)PhaseEnum.CutBread));
            _fsmLevel.AddState(new BurgerStatePlace((int)PhaseEnum.Place));
        }

        public override void LoadLevel()
        {
            base.LoadLevel();
            GenLevelObjects("Data/Levels/LevelObjects_Burger");
            EnterKitchen.Instance.ObjHearth.SetActive(false);
            EnterKitchen.Instance.ObjHearthLight.SetActive(false);

            _fsmLevel.ChangeState((int)PhaseEnum.PlanePotato, null, true);
        }

        //TODO::如果推出场景,已经加载的资源怎么处理
        public override void CleanLevel()
        {
            if (_fsmLevel.CurState != null)
                _fsmLevel.CurState.Exit();
            _fsmLevel.CleanState();
            _lstPieces.Clear();
            _objChipsRoot = null;
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
                if(status =="GraterOver")
                {
                    _fsmLevel.ChangeState((int)PhaseEnum.FryChips);
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
                        case "BreadCutOver":
                            _fsmLevel.ChangeState((int)PhaseEnum.Ingredient);
                            break;
                        case "IngredientOver":
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
                case "GraterOver":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.FryChips);
                    }
                    break;
                case "ChipsFriedOver":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.ShakeChips);
                    }
                    break;
                case "IngredientOk":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.Place);
                    }
                    break;
                case "PotatoShakeOver":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.CutBread);
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

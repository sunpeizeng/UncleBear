using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public class LevelPasta : LevelGame
    {
        private float _fScaleChef;
        private StateMachine<LevelPasta> _fsmLevel;

        private List<GameObject> _lstPastas = new List<GameObject>();
        public List<GameObject> PastaPieces 
        {
            get { return _lstPastas; }
        }

        public enum PhaseEnum
        {
            Cut,//切片
            Press,//按压
            Pot,//下锅
            PourOut,//倒出
            Fridge,//冰箱
            FreeCook,//自由
            Place,//摆
        }

        //创建一个菜的所有子状态
        protected override void CreateLevelFsm()
        {
            _fsmLevel = new StateMachine<LevelPasta>(this);

            //可以加参数设置每个状态的一些常量,或者在每个状态构造里自己处理
            _fsmLevel.AddState(new PastaStateCut((int)PhaseEnum.Cut));
            _fsmLevel.AddState(new PastaStatePress((int)PhaseEnum.Press));
            _fsmLevel.AddState(new PastaStatePot((int)PhaseEnum.Pot));
            _fsmLevel.AddState(new PastaStatePourOut((int)PhaseEnum.PourOut));
            _fsmLevel.AddState(new PastaStateFridge((int)PhaseEnum.Fridge));
            _fsmLevel.AddState(new PastaStateFreeCook((int)PhaseEnum.FreeCook));
            _fsmLevel.AddState(new PastaStatePlace((int)PhaseEnum.Place));
        }

        public override void LoadLevel()
        {
            base.LoadLevel();
            _fScaleChef = CharaCreator.Chef.transform.localScale.x;
            EnterKitchen.Instance.ObjHearth.SetActive(false);
            EnterKitchen.Instance.ObjHearthLight.SetActive(false);
            EnterKitchen.Instance.TrsTableCloth.position = new Vector3(-27.4f, 22.4f, -20.7f);

            #region 加载场景用到的资源
            GenLevelObjects("Data/Levels/LevelObjects_Pasta");
            #endregion

            _fsmLevel.ChangeState((int)PhaseEnum.Cut, null, true);
        }

        //TODO::如果推出场景,已经加载的资源怎么处理
        public override void CleanLevel()
        {
            CharaCreator.Chef.transform.localScale = Vector3.one * _fScaleChef;
            GameObject.Destroy(LevelObjs[Consts.ITEM_FRIDGEPLATE]);
            if (_fsmLevel.CurState != null)
                _fsmLevel.CurState.Exit();
            _fsmLevel.CleanState();
            _lstPastas.Clear();
            base.CleanLevel();
            //Debug.Log("clean");
        }

        public override void TickStateExecuting(float deltaTime)
        {
            if (_bLevelEnded) return;

            string status = _fsmLevel.TickState(deltaTime);
            if (status != null)
            {
                if (status == "IngredientDealLimited")
                    OnLevelStateContinued();
                else if (status == "CutOver")
                {
                    _fsmLevel.ChangeState((int)PhaseEnum.Press);
                }
                else if (status == "PressOver")
                {
                    _fsmLevel.ChangeState((int)PhaseEnum.Pot);
                }
                else if (!_bPausedAtUI)
                {
                    _bPausedAtUI = true;
                    UIPanelManager.Instance.GetPanel("UIPanelDishLevel").ShowSubElements("UIContinueButton");
                }
            }
        }

        public override void OnContinueLevelPhase()
        {
            base.OnContinueLevelPhase();
            bool isChanged = true;
            switch (_fsmLevel.CurState.StrStateStatus)
            {
                case "PastaBoiled":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.PourOut);
                    }
                    break;
                case "PourOutAll":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.Fridge);
                    }
                    break;
                case "FridgeSelectOver":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.FreeCook, LevelObjs);
                    }
                    break;
                case "FreeCookedOver":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.Place, LevelObjs);
                    }
                    break;
                case "IngredientDealt":
                    {
                        (_fsmLevel.CurState as PastaStateFreeCook).StopCookMachine();
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
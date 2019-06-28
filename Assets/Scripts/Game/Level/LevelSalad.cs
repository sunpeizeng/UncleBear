using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public class LevelSalad : LevelGame
    {
        private float _fScaleChef;
        private StateMachine<LevelSalad> _fsmLevel;

        public enum PhaseEnum
        {
            Lettuce,//生菜
            Wash,//洗菜
            FridgeSelect,//冰箱选材
            Free,//自由发挥
            Place,//摆放
        }

        //创建一个菜的所有子状态
        protected override void CreateLevelFsm()
        {
            _fsmLevel = new StateMachine<LevelSalad>(this);

            //AddStateHere
            _fsmLevel.AddState(new SaladStateLettuce((int)PhaseEnum.Lettuce));
            _fsmLevel.AddState(new SaladStateWash((int)PhaseEnum.Wash));
            _fsmLevel.AddState(new SaladFridgeSelect((int)PhaseEnum.FridgeSelect));
            _fsmLevel.AddState(new SaladFree((int)PhaseEnum.Free));
            _fsmLevel.AddState(new SaladPlace((int)PhaseEnum.Place));
        }

        public override void LoadLevel()
        {
            _fScaleChef = CharaCreator.Chef.transform.localScale.x;
            EnterKitchen.Instance.ObjHearth.SetActive(false);
            EnterKitchen.Instance.ObjHearthLight.SetActive(false);
            EnterKitchen.Instance.TrsTableCloth.position = new Vector3(-27.4f, 22.4f, -20.7f);

            base.LoadLevel();
            GenLevelObjects("Data/Levels/LevelObjects_Salad");

            //可以把GameObject作为参数传入,但是目前是get属性
            _fsmLevel.ChangeState((int)PhaseEnum.Lettuce);
        }

        //TODO::如果推出场景,已经加载的资源怎么处理
        public override void CleanLevel()
        {
            CharaCreator.Chef.transform.localScale = Vector3.one * _fScaleChef;
            GameObject.Destroy(LevelObjs[Consts.ITEM_FRIDGEPLATE]);

            if (_fsmLevel.CurState != null)
                _fsmLevel.CurState.Exit();
            _fsmLevel.CleanState();
            base.CleanLevel();
            //Debug.Log("clean");
        }

        public override void TickStateExecuting(float deltaTime)
        {
            if (_bLevelEnded) return;

            string status = _fsmLevel.TickState(deltaTime);
            if (status != null)
            {
                if (status == "LettuceOver")
                {
                    _fsmLevel.ChangeState((int)PhaseEnum.Wash);
                }
                else if (status == "IngredientDealLimited")
                {
                    OnLevelStateContinued();
                    _fsmLevel.CleanStateStatus();
                }
                else if (status == "FridgePlateEmpty")
                {
                    OnLevelStateContinued();
                    _fsmLevel.CleanStateStatus();
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
                case "LettuceOver":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.Wash);
                    }
                    break;
                case "WashOver":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.FridgeSelect);
                    }
                    break;
                case "FridgeSelectOver":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.Free, LevelObjs);
                    }
                    break;
                case "FreeCookedOver":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.Place);
                    }
                    break;
                case "IngredientDealt":
                    {
                        (_fsmLevel.CurState as SaladFree).StopCookMachine();
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
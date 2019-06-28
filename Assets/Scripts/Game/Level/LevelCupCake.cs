using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public class LevelCupCake : LevelGame
    {
        private StateMachine<LevelCupCake> _fsmLevel;
        List<GameObject> _objCupcakes = new List<GameObject>();
        public List<GameObject> Cupcakes {
            get { return _objCupcakes; }
        }

        public enum PhaseEnum
        {
            Egg,//打蛋
            MixEgg,//搅拌鸡蛋
            Injection,//注射模型
            Bake,//烘烤
            MixCream,//搅拌奶油
            PaintCream,//涂奶油
            Ingredient,//选料
            Place,//摆
        }

        //创建一个菜的所有子状态
        protected override void CreateLevelFsm()
        {
            _fsmLevel = new StateMachine<LevelCupCake>(this);

            //AddStateHere
            _fsmLevel.AddState(new CupCakeStateEgg((int)PhaseEnum.Egg));
            _fsmLevel.AddState(new CupCakeStateMixEgg((int)PhaseEnum.MixEgg));
            _fsmLevel.AddState(new CupCakeStateInjection((int)PhaseEnum.Injection));
            _fsmLevel.AddState(new CupCakeStateBake((int)PhaseEnum.Bake));
            _fsmLevel.AddState(new CupCakeStateMixCream((int)PhaseEnum.MixCream));
            _fsmLevel.AddState(new CupCakeStatePaintCream((int)PhaseEnum.PaintCream));
            _fsmLevel.AddState(new CupCakeStateIngredient((int)PhaseEnum.Ingredient));
            _fsmLevel.AddState(new CupCakeStatePlace((int)PhaseEnum.Place));
        }

        public override void LoadLevel()
        {
            base.LoadLevel();
            GenLevelObjects("Data/Levels/LevelObjects_Cupcake");

            EnterKitchen.Instance.ObjHearth.SetActive(false);
            EnterKitchen.Instance.ObjHearthLight.SetActive(false);

            //可以把GameObject作为参数传入,但是目前是get属性
            _fsmLevel.ChangeState((int)PhaseEnum.Egg, null, true);
        }

        //TODO::如果推出场景,已经加载的资源怎么处理
        public override void CleanLevel()
        {
            if (_fsmLevel.CurState != null)
                _fsmLevel.CurState.Exit();
            _objCupcakes.Clear();
            _fsmLevel.CleanState();

            base.CleanLevel();
            //Debug.Log("clean cupcake");
        }

        public override void TickStateExecuting(float deltaTime)
        {
            if (_bLevelEnded) return;

            string status = _fsmLevel.TickState(deltaTime);
            if (status != null )
            {
                if (status == "MixEggOver")
                {
                    _fsmLevel.ChangeState((int)PhaseEnum.Injection);
                }
                else if (status == "EggBreakOver")
                {
                    _fsmLevel.ChangeState((int)PhaseEnum.MixEgg);
                }
                else if(status == "CakeInjectionOver")
                {
                    _fsmLevel.ChangeState((int)PhaseEnum.Bake);
                }
                else if (status == "IngredientOver")
                {
                    _fsmLevel.ChangeState((int)PhaseEnum.Place);
                }
                else if (status == "BakeOver")
                {
                    _fsmLevel.ChangeState((int)PhaseEnum.PaintCream);
                }
                else if (status == "PaintCreamOver")
                {
                    _fsmLevel.ChangeState((int)PhaseEnum.Ingredient);
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
                case "EggBreakOver":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.MixEgg);
                    }
                    break;
                case "MixEggOver":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.Injection);
                    }
                    break;
                case "BakeOver":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.PaintCream);
                    }
                    break;
                case "CakeInjectionOver":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.Bake);
                    }
                    break;
                case "PaintCreamOver":
                    _fsmLevel.ChangeState((int)PhaseEnum.Ingredient);
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

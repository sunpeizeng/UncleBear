using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public class LevelPizza : LevelGame
    {
        private StateMachine<LevelPizza> _fsmLevel;
        #region 披萨的一些公用object
        //饼
        private GameObject _objPizzaBody;
        public GameObject ObjPizzaBody { get { return _objPizzaBody; } }
     
        private List<GameObject> _lstAdditives = new List<GameObject>();
        public List<GameObject> PizzaAdditives
        {
            get { return _lstAdditives; }
        }
        private float _fPizzaRadius = 6.5f;
        public float PizzaRadius { get { return _fPizzaRadius; } }


        #endregion

        public enum PhaseEnum
        {
            Dough,//面团
            Ketchup,//番茄酱
            Grater,//刨奶酪
            SprinkleCheese,//撒奶酪
            Ingredient,//选料
            Bake,//烘烤
            SliceUp,//切割
            Place,//摆
        }

        //创建一个菜的所有子状态
        protected override void CreateLevelFsm()
        {
            _fsmLevel = new StateMachine<LevelPizza>(this);

            //可以加参数设置每个状态的一些常量,或者在每个状态构造里自己处理
            _fsmLevel.AddState(new PizzaStateDough((int)PhaseEnum.Dough));
            _fsmLevel.AddState(new PizzaStateKetchup((int)PhaseEnum.Ketchup));
            _fsmLevel.AddState(new PizzaStateGrater((int)PhaseEnum.Grater));
            _fsmLevel.AddState(new PizzaStateSprinkle((int)PhaseEnum.SprinkleCheese));
            _fsmLevel.AddState(new PizzaStateIngredient((int)PhaseEnum.Ingredient));
            _fsmLevel.AddState(new PizzaStateBake((int)PhaseEnum.Bake));
            _fsmLevel.AddState(new PizzaStateSliceUp((int)PhaseEnum.SliceUp));
            _fsmLevel.AddState(new PizzaStatePlace((int)PhaseEnum.Place));
        }

        public override void LoadLevel()
        {
            base.LoadLevel();
            //TODO::根据表现决定这里一开始统一加载还是每一步加载自己的
            #region 测试
            GenLevelObjects("Data/Levels/LevelObjects_Pizza");
            _objPizzaBody = LevelObjs[Consts.ITEM_PIZZA].transform.FindChild("Body").gameObject;
            #endregion

            _fsmLevel.ChangeState((int)PhaseEnum.Grater, null, true);
        }

        //TODO::如果推出场景,已经加载的资源怎么处理
        public override void CleanLevel()
        {
            if (_fsmLevel.CurState != null)
                _fsmLevel.CurState.Exit();
            _fsmLevel.CleanState();

            _lstAdditives.Clear();
            base.CleanLevel();
            //Debug.Log("clean pizza");
        }

        public void AdditivesToPizza(GameObject obj)
        {
            obj.transform.SetParent(LevelObjs[Consts.ITEM_PIZZA].transform, true);
            _lstAdditives.Add(obj);
        }

        //TODO::目前先用一个字符串作为状态标识,后面看能不能改的好一点
        public override void TickStateExecuting(float deltaTime)
        {
            if (_bLevelEnded) return;

            string status = _fsmLevel.TickState(deltaTime);

            if (status != null)
            {
                if (status == "GraterOver")
                {
                    _fsmLevel.ChangeState((int)PhaseEnum.Dough);
                }
                else if (status == "DoughOver")
                {
                    _fsmLevel.ChangeState((int)PhaseEnum.Ketchup);
                }
                else if (status == "BakeOver")
                {
                    _fsmLevel.ChangeState((int)PhaseEnum.SliceUp);
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
                        case "KetchupOver":
                            _fsmLevel.ChangeState((int)PhaseEnum.SprinkleCheese);
                            break;
                        case "SprinkleOver":
                            _fsmLevel.ChangeState((int)PhaseEnum.Ingredient);
                            break;
                        case "PizzaCuttedOver":
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
            LimitAdditivesInPizza();
        }

        void LimitAdditivesInPizza()
        {
            switch (_fsmLevel.CurState.StateEnum)
            {
                case (int)PhaseEnum.Ketchup:
                case (int)PhaseEnum.Ingredient:
                case (int)PhaseEnum.SprinkleCheese:
                case (int)PhaseEnum.Bake:
                    _lstAdditives.ForEach(p =>
                    {
                        if (p != null)
                        {
                            var disVec = p.transform.position - _objPizzaBody.transform.position;
                            disVec.y = 0;
                            if (disVec.sqrMagnitude > _fPizzaRadius * _fPizzaRadius)
                            {
                                p.transform.position = new Vector3(_objPizzaBody.transform.position.x, p.transform.position.y, _objPizzaBody.transform.position.z)
                                + disVec.normalized * _fPizzaRadius;
                            }
                        }
                        else _lstAdditives.Remove(p);
                    });
                    break;
            }
        }

        public override void OnContinueLevelPhase()
        {
            base.OnContinueLevelPhase();
            bool isChanged = true;
            switch (_fsmLevel.CurState.StrStateStatus)
            {
                case "DoughOver":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.Ketchup);
                    }
                    break;
                case "KetchupOk":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.SprinkleCheese);
                    }
                    break;
                case "GraterOver":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.Dough);
                    }
                    break;
                case "SprinkleOk":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.Ingredient);
                    }
                    break;
                case "IngredientOver":
                    {
                        DoozyUI.UIManager.PlaySound("8成功");
                        _fsmLevel.ChangeState((int)PhaseEnum.Bake);
                    }
                    break;
                case "BakeOver":
                    {
                        _fsmLevel.ChangeState((int)PhaseEnum.SliceUp);
                    }
                    break;
                case "PizzaCuttedOk":
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

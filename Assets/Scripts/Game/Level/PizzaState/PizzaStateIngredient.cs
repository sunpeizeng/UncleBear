using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Touch;

namespace UncleBear
{
    public class PizzaStateIngredient : IngredientState<LevelPizza>
    {
        float _fBowlDelta = 6.9f;
        int _nIngredLimit = 3;
        ConveyorCtrl _conveyorCtrl;
        Vector3 _v3BowlPos = new Vector3(-36.8f, 25.2f, -110.4f);
        bool _bIngredGenerated;
        Vector3 _v3SourceBowlPos;
        bool _bConveying;

        int _nIngredCount;

        List<GameObject> _lstBowl = new List<GameObject>();

        public PizzaStateIngredient(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("ingredient");
            _conveyorCtrl = _owner.LevelObjs[Consts.ITEM_CONVEYOR].AddMissingComponent<ConveyorCtrl>();
            _conveyorCtrl.enabled = true;

            GenIngredients();
            base.Enter(param);
        }

        void OnPickFingerUp(GameObject objPicking)
        {
            if (objPicking != null)
            {
                RaycastHit hit = GameUtilities.GetRaycastHitInfo(objPicking.transform.position, Vector3.down, 1000, 1 << LayerMask.NameToLayer("Cuttable"));
                //这里加了一个层,用来标识菜的主体
                float vecDis = Vector2.Distance(new Vector2(hit.point.x, hit.point.z), new Vector2(_owner.ObjPizzaBody.transform.position.x, _owner.ObjPizzaBody.transform.position.z));
                if (hit.collider != null && vecDis < _owner.PizzaRadius + 1.5f)
                {
                    objPicking.GetComponent<Rigidbody>().isKinematic = false;
                    objPicking.layer = LayerMask.NameToLayer("Cuttable");
                    _owner.AdditivesToPizza(objPicking);
                    DoozyUI.UIManager.PlaySound("14撒配料", objPicking.transform.position, false, 1f, 0.3f);
                    _conveyorCtrl.ClearPicking();
                    DishManager.Instance.IngredsInDish.Add(objPicking.name);
                    _nIngredCount += 1;
                    if (_nIngredCount >= _nIngredLimit)
                        StrStateStatus = "IngredientOver";
                }
                else
                {
                    _conveyorCtrl.DestroyPicking();
                }
            }
        }

        void GenIngredients()
        {
            _lstBowl = _conveyorCtrl.InitConveyorList(_owner.LevelObjs[Consts.ITEM_BOWLSMALL], "Configs/PizzaIngredItems", OnPickFingerUp);
            _bIngredGenerated = false;
            //具体配菜有多少走配置
            for (int i = 0; i < _lstBowl.Count; i++)
            {
                _lstBowl[i].transform.position = _v3BowlPos + new Vector3(-100, 0, 0);
                _lstBowl[i].transform.DOMove(_v3BowlPos + new Vector3(-i * _fBowlDelta, 0, 0), 1f).SetEase(Ease.OutElastic).OnComplete(() => { _bIngredGenerated = true; });
            }
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            base.OnFingerDown(finger);
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null)
            {
                if (hit.collider.gameObject == _owner.LevelObjs[Consts.ITEM_CONVEYOR] ||
                    (!hit.collider.name.Contains("item") && hit.collider.transform.parent != null && hit.collider.transform.parent.name.Contains("ConveyorPlate")))//点到传送带或者碗
                {
                    _bConveying = true;
                }
            }
        }

        //拖动传送带的逻辑,先写在这,不确定是否通用
        protected override void OnFingerSet(LeanFinger finger)
        {
            base.OnFingerSet(finger);
            if (_bConveying)
            {
                _lstBowl.ForEach(p =>
                {
                    p.transform.position += new Vector3(-finger.ScreenDelta.x * 0.05f, 0, 0);
                });
                CheckBowlPos(finger);
            }
        }

        //刷新碗的位置
        void CheckBowlPos(LeanFinger finger)
        {
            _lstBowl.ForEach(p =>
            {
                if (finger.ScreenDelta.x < 0 && p.transform.position.x > -29.6f)
                {
                    p.transform.position = _lstBowl[_lstBowl.Count - 1].transform.position - new Vector3(_fBowlDelta, 0, 0);
                    _lstBowl.Remove(p);
                    _lstBowl.Add(p);
                }
                else if (finger.ScreenDelta.x > 0 && p.transform.position.x < -83.3f)
                {
                    p.transform.position = _lstBowl[0].transform.position + new Vector3(_fBowlDelta, 0, 0);
                    _lstBowl.Remove(p);
                    _lstBowl.Insert(0, p);
                }
            });
        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            base.OnFingerUp(finger);
            _bConveying = false;
        }


        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();

            if (_conveyorCtrl != null)
                _conveyorCtrl.enabled = false;

            for (int i = 0; i < _lstBowl.Count; i++)
            {
                GameObject.Destroy(_lstBowl[i]);
            }
            _lstBowl.Clear();
        }

        //private void LimitIngredientPositionOnPizza()
        //{
        //    if (Vector3.Distance(base.transform.position, this.pizzaCenter) > 0.11f)
        //    {
        //        base.transform.position = this.pizzaCenter + (base.transform.position - this.pizzaCenter).normalized * 0.11f;
        //    }
        //}
    }
}

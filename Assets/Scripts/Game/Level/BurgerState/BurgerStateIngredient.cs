using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    public class BurgerStateIngredient : IngredientState<LevelBurger>
    {
        int _nBurgerLayerLimit = 10;
        float _fBowlDelta = 10;
        ConveyorCtrl _conveyorCtrl;
        Vector3 _v3BowlPos = new Vector3(-36.8f, 25.5f, -110.4f);
        bool _bConveying;
        bool _bBurgerReady;

        float _fWaitTime = 1f;
        float _fWaitTimer;

        List<GameObject> _lstBowl = new List<GameObject>();
        List<GameObject> _lstIngredients = new List<GameObject>();

        public BurgerStateIngredient(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("Ingredient");

            _owner.LevelObjs[Consts.ITEM_CHOPBOARD].name = "TableSurface";

            var tableCol = _owner.LevelObjs[Consts.ITEM_CHOPBOARD].AddComponent<BoxCollider>();
            tableCol.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            tableCol.size = new Vector3(100, 3, 100);
            tableCol.isTrigger = true;


            _conveyorCtrl = _owner.LevelObjs[Consts.ITEM_CONVEYOR].AddMissingComponent<ConveyorCtrl>();
            _conveyorCtrl.enabled = true;
            GenIngredients();
            _bBurgerReady = _bConveying = false;
            base.Enter(param);
            _fWaitTimer = _fWaitTime;
        }

        public override string Execute(float deltaTime)
        {
            if (_fWaitTimer > 0)
                _fWaitTimer -= deltaTime;

            _owner.BurgerPieces.ForEach(p =>
            {
                if (p.transform.position.z < -100)
                    p.transform.position = new Vector3(p.transform.position.x, p.transform.position.y, -100);
            });

            if (!_bBurgerReady && _owner.BurgerPieces.Count >= _nBurgerLayerLimit && _fWaitTimer <= 0)
            {
                if (GameUtilities.IsAllRigidBodyQuiet(_owner.BurgerPieces))
                {
                    _bBurgerReady = true;
                    StrStateStatus = "IngredientOver";
                    //LevelManager.Instance.StartCoroutine(ReturnBreadTopBack());
                }
            }
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

        void OnPickFingerSet(GameObject objPicking, LeanFinger finger)
        {
            if (finger == null)
            {
                //Debug.Log("figer lost");
                return;
            }
            var height = _owner.BurgerPieces.Count > 0 ? GameUtilities.GetMeshMaxHeight(_owner.BurgerPieces) : _v3BowlPos.y;
            var pos = GameUtilities.GetFingerTargetWolrdPos(finger, objPicking, height + 4);
            objPicking.transform.position = Vector3.Lerp(objPicking.transform.position, pos, 20 * Time.deltaTime);
        }

        void OnPickFingerUp(GameObject objPicking)
        {
            if (objPicking != null)
            {
                objPicking.AddMissingComponent<FallOnTableChecker>().SetCollisionCallback(ClearFailedIngredients);
                RaycastHit hit = GameUtilities.GetRaycastHitInfo(objPicking.transform.position, Vector3.down);
                //这里加了一个层,用来标识菜的主体
                float vecDis = Vector2.Distance(new Vector2(hit.point.x, hit.point.z), new Vector2(_owner.LevelObjs[Consts.ITEM_BREAD].transform.position.x, _owner.LevelObjs[Consts.ITEM_BREAD].transform.position.z));
                if (hit.collider != null && hit.collider.transform.parent != null && hit.collider.transform.IsChildOf(_owner.LevelObjs[Consts.ITEM_BREAD].transform))// && vecDis < _owner.BurgerRadius)
                {
                    var colSize = objPicking.GetComponent<BoxCollider>().size;
                    objPicking.GetComponent<BoxCollider>().size = new Vector3(colSize.x * 0.7f, colSize.y * 0.7f, colSize.z);
                    objPicking.GetComponent<Rigidbody>().isKinematic = false;
                    DoozyUI.UIManager.PlaySound("14撒配料", objPicking.transform.position, false, 1f, 0.3f);
                    _owner.BurgerPieces.Add(objPicking);
                    objPicking.layer = LayerMask.NameToLayer("Cuttable");
                    objPicking.transform.SetParent(_owner.LevelObjs[Consts.ITEM_BREAD].transform);
                    _conveyorCtrl.ClearPicking();
                    objPicking.transform.DOScale(Vector3.one, 0.2f);
                    DishManager.Instance.IngredsInDish.Add(objPicking.name);

                    StrStateStatus = "IngredientOk";

                    if (_owner.BurgerPieces.Count >= _nBurgerLayerLimit && _conveyorCtrl != null)
                    {
                        _fWaitTimer = _fWaitTime;
                        _bConveying = _conveyorCtrl.enabled = false;
                    }
                }
                else
                {
                    _conveyorCtrl.DestroyPicking(0.7f);
                }
            }
        }

        //清空已经叠的层数,比如堆叠倒塌
        void ClearFailedIngredients(GameObject obj)
        {
            DishManager.Instance.IngredsInDish.Remove(obj.name);
            _owner.BurgerPieces.Remove(obj);// ForEach(p => GameObject.Destroy(p));
            GameObject.Destroy(obj, 0.5f);
            //_lstIngredients.Clear();
            _conveyorCtrl.enabled = true;
        }

        void GenIngredients()
        {
            _lstBowl = _conveyorCtrl.InitConveyorList(_owner.LevelObjs[Consts.ITEM_PLATESTUFF], "Configs/BurgerIngredItems", OnPickFingerUp, OnPickFingerSet, false);
            //具体配菜有多少走配置
            for (int i = 0; i < _lstBowl.Count; i++)
            {
                var childs = _lstBowl[i].transform.GetChildTrsList();
                for (int j = 0; j < childs.Count; j++)
                {
                    if (childs[j].name == "Mesh")
                        continue;
                    childs[j].localScale = Vector3.one * 0.7f;//不然穿墙
                    childs[j].localEulerAngles = new Vector3(-90, Random.Range(0, 360), 0);
                    childs[j].localPosition = new Vector3(0, 1.2f * j, 0);
                }

                _lstBowl[i].transform.position = _v3BowlPos + new Vector3(-100, 0, 0);
                _lstBowl[i].transform.DOMove(_v3BowlPos + new Vector3(-i * _fBowlDelta, 0, 0), 1f).SetEase(Ease.OutElastic);
            }
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            if (!_conveyorCtrl.enabled)
                return;
            base.OnFingerDown(finger);
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null)
            {
                if (hit.collider.gameObject == _owner.LevelObjs[Consts.ITEM_CONVEYOR] || hit.collider.gameObject.name.Contains("ConveyorPlate"))//点到碗
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
                if (finger.ScreenDelta.x < 0 && p.transform.position.x > -29f)
                {
                    p.transform.position = _lstBowl[_lstBowl.Count - 1].transform.position - new Vector3(_fBowlDelta, 0, 0);
                    _lstBowl.Remove(p);
                    _lstBowl.Add(p);
                }
                else if (finger.ScreenDelta.x > 0 && p.transform.position.x < -84f)
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
    }
}

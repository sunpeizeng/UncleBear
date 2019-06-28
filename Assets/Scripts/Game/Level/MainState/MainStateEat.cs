using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Touch;

namespace UncleBear
{

    public class MainStateEat : State<LevelMain>
    {
        bool _bEatOver;
        bool _bReadyToEat;
        bool _bHitting;
        List<Collider> _dishCols;
        GameObject _objEating;
        float _fDishDis;
        Vector3 _v3Origin;

        public MainStateEat(int stateEnum) : base(stateEnum)
        {
        }

        public override void Enter(object param)
        {
            //Input.multiTouchEnabled = false;
            base.Enter(param);
            //_bReadyToEat = false;
            //特写吃菜
            _bEatOver = false;
            _bReadyToEat = true;
            _dishCols = new List<Collider>(DishManager.Instance.ObjFinishedDish.GetComponentsInChildren<Collider>());
            _fDishDis = Vector3.Distance(DishManager.Instance.ObjFinishedDish.transform.position, CameraManager.Instance.MainCamera.transform.position) - 8;
            GuideManager.Instance.SetGuideClick(DishManager.Instance.ObjFinishedDish.transform.position - Vector3.right * 5);

        }

        public override string Execute(float deltaTime)
        {
            if (_bEatOver && DishManager.Instance.CurCustomer.GetCurState() != CharaStateEnum.Eat)
            {
                StrStateStatus = "EatOver";
            }

            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            //Input.multiTouchEnabled = true;
            _dishCols.Clear();
            base.Exit();
            _bReadyToEat = false;
        }

        //用手指拖动进食,需要在每个菜品完成时确保菜品除了要被吃的部分之外,不要有碰撞
        protected override void OnFingerDown(LeanFinger finger)
        {
            if (_bEatOver)
                return;

            if (_bReadyToEat && !_bHitting && _objEating == null)
            {
                var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
                if (hit.collider != null && hit.collider.transform.IsChildOf(DishManager.Instance.ObjFinishedDish.transform))
                {
                    _bHitting = true;
                    _objEating = hit.collider.gameObject;
                    _v3Origin = _objEating.transform.position;
                    DoozyUI.UIManager.PlaySound("12物品拿起");
                    GuideManager.Instance.StopGuide();
                }
            }
        }
        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_bReadyToEat && _bHitting && _objEating != null && !_bEatOver)
            {
                _objEating.SetPos(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition).GetPoint(_fDishDis));
            }
        }
        protected override void OnFingerUp(LeanFinger finger)
        {
            if (_bReadyToEat && _bHitting && _objEating != null && !_bEatOver)
            {
                _bHitting = false;
                var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition), 1000, 1 << LayerMask.NameToLayer("Character"));
                if (hit.collider != null)
                {
                    var customer = hit.collider.transform.parent.GetComponent<CharaCustomer>();
                    if (customer != null && customer.GetCurState() == CharaStateEnum.Wait)
                    {
                        //TODO::加入吃菜动画和角色状态判断,如果小动物在嚼,喂食无效,需要获取角色嘴巴的点飞入
                        customer.ChangeCharaState(CharaStateEnum.Eat);
                        var originScale = _objEating.transform.localScale;
                        _objEating.transform.DOMove(DishManager.Instance.GetCustomerMouthPoint(customer) + new Vector3(0, 0.5f, 8), 0.2f);
                        _objEating.transform.DOScale(originScale * 1.5f, 0.3f).OnComplete(() =>
                        {
                            _objEating.transform.DOScale(Vector3.zero, 0.5f);
                            DoozyUI.UIManager.PlaySound("53咀嚼", customer.transform.position, false, 1, 0.2f);

                            _objEating.transform.DOMove(DishManager.Instance.GetCustomerMouthPoint(customer), 0.5f).OnComplete(() =>
                            {
                                DoozyUI.UIManager.PlaySound("54吞", customer.transform.position, false, 1, 0.2f);
                                if (_dishCols.Contains(_objEating.GetComponent<Collider>()))
                                    _dishCols.Remove(_objEating.GetComponent<Collider>());
                                GameObject.Destroy(_objEating);
                                _objEating = null;

                                if (_dishCols.Count <= 0)
                                {
                                    _bEatOver = true;
                                    //GameObject.Destroy(DishManager.Instance.ObjFinishedDish);
                                    //DishManager.Instance.ObjFinishedDish = null;
                                }

                            });
                        });
                        //_objEating.SetPos();
                        return;
                    }
                }
                _objEating.SetPos(_v3Origin);
                _objEating.transform.localScale = Vector3.zero;
                _objEating.transform.DOScale(Vector3.one, 0.5f);
                _objEating = null;
            }
        }


    }
}

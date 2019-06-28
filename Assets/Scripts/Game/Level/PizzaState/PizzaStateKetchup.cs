using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    public class PizzaStateKetchup : State<LevelPizza>
    {
        GameObject _objBottle;

        //Vector3 _v3OriginPos;
        Vector3 _v3BottlePos = new Vector3(-71.2f, 22.5f, -102.7f);
        Vector3 _v3BottleAngle = new Vector3(0, 0, 359);
        Vector3 _v3TarAngle = new Vector3(0, 0, 270);
        int _nLimitCount = 13;

        bool _bBottleSelected;
        int _nCurCount;
        float _fDropCd;
        bool _bDropOver;

        public PizzaStateKetchup(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("ketchup");

            _nCurCount = 0;
            _fDropCd = 0;
            _bDropOver = _bBottleSelected = false;
            _objBottle = _owner.LevelObjs[Consts.ITEM_KETCHUPBOTTLE];
            _objBottle.SetPos(_v3BottlePos + Vector3.left * 50);
            _objBottle.transform.DOMove(_v3BottlePos, 0.5f);

            base.Enter(param);

            GuideManager.Instance.SetGuideSingleDir(_v3BottlePos, _owner.ObjPizzaBody.transform.position, true, true, 1.5f);
        }

        public override string Execute(float deltaTime)
        {
            if (_fDropCd > 0)
                _fDropCd -= deltaTime;
            if (_bBottleSelected && _objBottle.transform.eulerAngles.z > 270)
            {
                _objBottle.transform.DOLocalRotate(_v3TarAngle, 0.25f);
            }
            else if (!_bBottleSelected && _objBottle.transform.eulerAngles.z < 359)
            {
                _objBottle.transform.DOLocalRotate(_v3BottleAngle, 0.25f);
            }
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            _objBottle.transform.DOMove(_v3BottlePos + Vector3.left * 50, 0.5f).OnComplete(()=> {
                _objBottle.SetPos(Vector3.one * 500);
            });
            base.Exit();
        }


        protected override void OnFingerDown(LeanFinger finger)
        {
            if (_nCurCount >= _nLimitCount)
                return;
            RaycastHit hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.gameObject == _objBottle)
            {
                _bBottleSelected = true;
            }
        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            _bBottleSelected = false;
        }

        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_bBottleSelected)
            {
                //位置跟随指针
                var pos = GameUtilities.GetFingerTargetWolrdPos(finger, _objBottle, _v3BottlePos.y + 7.5f);
                if (pos.z < -110)//墙
                    pos.z = -110;
                _objBottle.transform.position = Vector3.Slerp(_objBottle.transform.position, pos, 20 * Time.deltaTime);

                if (_fDropCd <= 0)
                {
                    var swipeThreshold = LeanTouch.Instance.SwipeThreshold;
                    var tapThreshold = LeanTouch.Instance.TapThreshold;
                    var recentDelta = finger.GetSnapshotScreenDelta(tapThreshold);
                    if (recentDelta.magnitude > swipeThreshold / 2)
                    {
                        RaycastHit hit = GameUtilities.GetRaycastHitInfo(_objBottle.transform.position + new Vector3(5, 0, 0), Vector3.down);
                        //摄像机有一定角度,加上瓶口有偏移,做向下射线,如果第一个射中的是饼,就创,如果射中的是酱,不创
                        if (hit.collider != null && hit.collider.gameObject == _owner.ObjPizzaBody &&
                            Vector2.Distance(new Vector2(hit.collider.transform.position.x, hit.collider.transform.position.z), new Vector2(hit.point.x, hit.point.z)) < _owner.PizzaRadius)
                        {
                            var newDrop = GameObject.Instantiate(_owner.LevelObjs[Consts.ITEM_KETCHUPPIECE]) as GameObject;
                            newDrop.name = "PizzaSauce";
                            newDrop.transform.position = hit.point;// + new Vector3(0, 0.05f, 0);
                            newDrop.transform.localScale = Vector3.zero;
                            newDrop.transform.localEulerAngles = new Vector3(-90, Random.Range(0, 360), 0);
                            DoozyUI.UIManager.PlaySound("13撒番茄酱", hit.point);
                            newDrop.transform.DOScale(1, 0.2f).OnComplete(() => { _owner.AdditivesToPizza(newDrop); });
                            _fDropCd = 0.3f;
                            StrStateStatus = "KetchupOk";
                            if (++_nCurCount >= _nLimitCount)
                            {
                                StrStateStatus = "KetchupOver";
                                _bBottleSelected = false;
                            }
                        }
                    }
                }

            }
        }
    }
}

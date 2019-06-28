using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    public class PizzaStateDough : State<LevelPizza>
    {
        bool _bRollerReady;
        bool _bHittingRoller;

        GameObject _objRoller;

        Vector3 _v3Conveyor = new Vector3(-55, 22.5f, -117);
        Vector3 _v3BoardPos = new Vector3(-56.5f, 22.5f, -98f);
        Vector3 _v3PizzaPos = new Vector3(-56.5f, 24.5f, -98);
        Vector3 _v3Roller = new Vector3(-56.5f, 26.5f, -98);
        Vector3 _v3OriginScale = new Vector3(0.55f, 0.55f, 10f);
        Vector3 _v3TargetScale;
        int _nDoughCount;
        float _fRollDelta;

        public PizzaStateDough(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            _bHittingRoller = _bRollerReady = false;
            CameraManager.Instance.DoCamTween(new Vector3(-56.5f, 80, -57f), new Vector3(50, 180, 0), 0.1f);

            //Debug.Log("enter dough");
            base.Enter(param);

            _fRollDelta = 0f;
            _nDoughCount = 9;
            _objRoller = _owner.LevelObjs[Consts.ITEM_ROLLPIN];
            _objRoller.SetPos(_v3Roller + Vector3.up * 50);
            _objRoller.transform.DOMoveY(_v3Roller.y, 0.5f).OnComplete(()=> {
                _bRollerReady = true;
            });

            _owner.LevelObjs[Consts.ITEM_CONVEYOR].SetPos(_v3Conveyor);
            _owner.LevelObjs[Consts.ITEM_FLOURPIECE].transform.SetParent(_owner.LevelObjs[Consts.ITEM_CHOPBOARD].transform.FindChild("Mesh"));
            _owner.LevelObjs[Consts.ITEM_FLOURPIECE].SetLocalPos(new Vector3(0, 0, 1.98f));
            _owner.LevelObjs[Consts.ITEM_FLOURPIECE].SetAngle(Vector3.zero);

            _owner.LevelObjs[Consts.ITEM_CHOPBOARD].SetPos(_v3BoardPos);
            _owner.LevelObjs[Consts.ITEM_CHOPBOARD].transform.localScale = new Vector3(0.8f, 1, 0.8f);
            _owner.LevelObjs[Consts.ITEM_PIZZA].SetPos(_v3PizzaPos);
            _owner.LevelObjs[Consts.ITEM_PIZZA].transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            _owner.ObjPizzaBody.transform.localScale = _v3OriginScale;
            _v3TargetScale = _v3OriginScale;

            GuideManager.Instance.SetGuideDoubleDir(_v3PizzaPos + Vector3.forward * 5, _v3PizzaPos - Vector3.forward * 5, 0.5f);
        }

        public override string Execute(float deltaTime)
        {
            if (_fRollDelta > 0)
                _fRollDelta -= Time.deltaTime;
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            base.OnFingerDown(finger);
            if (!_bRollerReady || _bHittingRoller)
                return;
            //点到原材料才可以刨
            RaycastHit hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.gameObject == _objRoller)
                _bHittingRoller = true;
        }

        protected override void OnFingerSet(LeanFinger finger)
        {
            base.OnFingerSet(finger);
            if (!_bHittingRoller)
                return;
            if (finger.ScreenDelta.y != 0)
            {
                //拖动起司,1和-1是测试值
                float newZ = Mathf.Clamp(_objRoller.transform.position.z - finger.ScreenDelta.y * 0.02f, _v3Roller.z -5, _v3Roller.z + 2);
                _objRoller.transform.position = new Vector3(_objRoller.transform.position.x, _objRoller.transform.position.y, newZ);

                if (_nDoughCount > 0 && finger.GetSnapshotScreenDelta(LeanTouch.Instance.TapThreshold).magnitude > LeanTouch.Instance.SwipeThreshold)
                {
                    if (_fRollDelta <= 0)
                    {
                        _nDoughCount -= 1;
                        _fRollDelta = 0.5f;
                        _v3TargetScale += new Vector3(0.05f, 0.05f, -1f);
                        DoozyUI.UIManager.PlaySound("11面团饭团", _v3BoardPos);
                        _owner.ObjPizzaBody.transform.DOScale(_v3TargetScale, 0.4f).OnComplete(()=> {
                            if (_nDoughCount <= 0)
                            {
                                _bHittingRoller = false;
                                DoozyUI.UIManager.PlaySound("8成功");
                                _objRoller.transform.DOMove(_v3Roller + Vector3.up * 50, 0.5f).OnComplete(() => {
                                    _owner.LevelObjs[Consts.ITEM_ROLLPIN].transform.DOMove(Vector3.one * 500, 1f).OnComplete(() => {
                                        StrStateStatus = "DoughOver";
                                    });
                                });
                                _owner.LevelObjs[Consts.ITEM_PIZZA].transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Cuttable");
                            }
                        });

                        //if (_nDoughCount <= 0)
                           
                    }
                }
            }
        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            base.OnFingerUp(finger);
            _bHittingRoller = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class BurgerStateBread : State<LevelBurger>
    {
        Transform _trsBreadTop;
        bool _bCamReady;

        Vector3 _v3CamPos = new Vector3(-56.5f, 80, -57f);
        Vector3 _v3CamAngle = new Vector3(50, 180, 0);
        //Vector3 _v3KnifePos = new Vector3(-37, 23, -30);
        //Vector3 _v3KnifeAngle = new Vector3(0, 90, 0);
        Vector3 _v3BoardPos = new Vector3(-56.5f, 22.5f, -98f);

        public BurgerStateBread(int stateEnum) : base(stateEnum)
        {

        }
        public override void Enter(object param)
        {
            //Debug.Log("Bread");

            _bCamReady = false;

            _owner.LevelObjs[Consts.ITEM_CHOPBOARD].SetPos(_v3BoardPos);
            _owner.LevelObjs[Consts.ITEM_CHOPBOARD].transform.localScale = new Vector3(0.8f, 1, 0.8f);

            //_owner.LevelObjs[Consts.ITEM_KNIFE].SetPos(_v3KnifePos);
            //_owner.LevelObjs[Consts.ITEM_KNIFE].SetAngle(_v3KnifeAngle);
            _owner.LevelObjs[Consts.ITEM_BREAD].SetPos(_v3BoardPos + Vector3.up * 2);
            _trsBreadTop = _owner.LevelObjs[Consts.ITEM_BREAD].transform.FindChild("Top");
            CameraManager.Instance.DoCamTween(_v3CamPos, _v3CamAngle, 0.5f, ()=> { _bCamReady = true; });

            base.Enter(param);

            GuideManager.Instance.SetGuideClick(_trsBreadTop.position);
        }

        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
        }


        protected override void OnFingerDown(Lean.Touch.LeanFinger finger)
        {
            if (!_bCamReady)
                return;
            //_owner.LevelObjs[Consts.ITEM_KNIFE].transform.DORotate(Vector3.zero, 0.5f).OnComplete(() =>
            //{
            //    _owner.LevelObjs[Consts.ITEM_KNIFE].transform.DOMove(Vector3.one * 500, 1).OnComplete(() => { _strStateStatus = "BreadCutOver"; });
            //});

            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.transform.IsChildOf(_owner.LevelObjs[Consts.ITEM_BREAD].transform))
            {
                _bCamReady = false;
                _trsBreadTop.DOLocalMoveX(-40, 1f).OnComplete(()=> {
                    StrStateStatus = "BreadCutOver";
                });
            }

        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    public class CupCakeStateMixEgg : State<LevelCupCake>
    {
        float _fMixPerc = 0;
        GameObject _objChopstick;
        LeanGestureCircle _circleGesCtrl;
        Material _matFluid;

        Vector3 _v3ChopstickPos = new Vector3(-83, 29.5f, -72.5f);
        float _fAroundRadius = 1.5f;
        bool _bMixOk;
        float _fRotSpeed;
        float _fRotAngle;
        bool _bHitChopstick;

        List<Transform> _lstTrsPieces = new List<Transform>();

        float _fShakeX = 1;
        //float _fShakeFixX = 0;

        public CupCakeStateMixEgg(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("mix egg");

            //Debug.Log(_v3ChopstickPos);
            _bHitChopstick = false;
            _fRotSpeed = _fRotAngle = _fMixPerc = 0;
            _matFluid = _owner.LevelObjs[Consts.ITEM_FLUID].transform.FindChild("Mesh").GetComponent<MeshRenderer>().material;
            _objChopstick = _owner.LevelObjs[Consts.ITEM_EGGBEATER];
            _objChopstick.transform.position = _v3ChopstickPos + new Vector3(0, 10, 0);
            _objChopstick.transform.DOMove(_v3ChopstickPos + new Vector3(0, 0, -_fAroundRadius), 0.5f);
            //引入公用的旋转手势判断,至于具体是什么旋转表现在这个类里面写
            _circleGesCtrl = _objChopstick.AddMissingComponent<LeanGestureCircle>();
            _circleGesCtrl.enabled = true;
            _bMixOk = false;
            
            _circleGesCtrl.SetParams(_objChopstick, _v3ChopstickPos, _fAroundRadius, false, true, true);
            //目前是360度回调一次
            _circleGesCtrl.OnRotateFinish = ChangeFluidColor;
            //把蛋黄归在蛋液下一期旋转
            _lstTrsPieces = _owner.LevelObjs[Consts.ITEM_FLUID].transform.GetChildTrsList();
            _lstTrsPieces.Remove(_lstTrsPieces.Find(p => p.name == "Mesh"));
            base.Enter(param);

            GuideManager.Instance.SetGuideRotate(_owner.LevelObjs[Consts.ITEM_BOWL].transform.position + Vector3.up * 3);
        }

        public override string Execute(float deltaTime)
        {
            var curDelta = Mathf.Abs(_fRotSpeed - _fRotAngle) > 0f ? 1 : 0;
            //if (_fShakeFixX != 0)
            //    curDelta = Mathf.Clamp01(Mathf.Abs(_fRotSpeed - _fRotAngle) / _fShakeFixX);

            if (!_bMixOk)
            {
                if (_matFluid.HasProperty("_Slider_Val"))
                {
                    _matFluid.SetFloat("_Slider_Val", _fMixPerc);
                }
            }


            DOTween.To(() => _fRotAngle, p => _fRotAngle = p, _fRotSpeed, 1);
            _owner.LevelObjs[Consts.ITEM_FLUID].transform.localEulerAngles = new Vector3(_fShakeX * curDelta, -_fRotAngle, 0);

            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            _lstTrsPieces.Clear();
            base.Exit();
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.gameObject == _objChopstick)
                _bHitChopstick = true;
        }


        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_bHitChopstick)
            {
                var newPos = GameUtilities.GetFingerTargetWolrdPos(finger, _objChopstick, _v3ChopstickPos.y);
                if (Vector3.Distance(newPos, _v3ChopstickPos) > _fAroundRadius)
                    newPos = _v3ChopstickPos + (newPos - _v3ChopstickPos).normalized * _fAroundRadius;
                _fRotSpeed += finger.GetDeltaDegrees(CameraManager.Instance.MainCamera.WorldToScreenPoint(_v3ChopstickPos));
                _objChopstick.transform.position = Vector3.Lerp(_objChopstick.transform.position, newPos, 20 * Time.deltaTime);

                //var disVec = new Vector3(-Mathf.Sin(Mathf.Deg2Rad * _circleGesCtrl.fCurAngle), 0, -Mathf.Cos(Mathf.Deg2Rad * _circleGesCtrl.fCurAngle));
                ////if(_circleGesCtrl.fDeltaAngle > 0)
                ////_objChopstick.transform.position = Vector3.Lerp(_objChopstick.transform.position, _v3ChopstickPos + disVec.normalized * _fAroundRadius, 20 * Time.deltaTime);
            }
        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            _bHitChopstick = false;
        }

        void ChangeFluidColor()
        {
            if (!_bMixOk)
            {
                DoozyUI.UIManager.PlaySound("25搅拌", _v3ChopstickPos, false);
                DOTween.To(() => _fMixPerc, p => _fMixPerc = p, _fMixPerc + 0.1f, 0.5f);
               

                //float alpha = _mrFluid.material.color.a;
                //_mrFluid.material.DOFade(alpha + 0.1f, 0.5f);
                _lstTrsPieces.ForEach(p => p.DOScale(p.localScale * 0.8f, 0.5f));

                if (_fMixPerc >= 1)
                {
                    _lstTrsPieces.ForEach(p => GameObject.Destroy(p.gameObject));
                    _bMixOk = true;
                    GuideManager.Instance.StopGuide();

                    _bHitChopstick = false;
                    DoozyUI.UIManager.PlaySound("8成功");
                    _circleGesCtrl.enabled = false;
                    _objChopstick.transform.DOMoveY(60, 1f).OnComplete(() => {
                        _objChopstick.transform.DOMove(Vector3.one * 500, 0.5f);
                    });
                    _owner.LevelObjs[Consts.ITEM_BOWL].transform.DOMoveX(0, 1f).SetDelay(2f).OnComplete(() =>
                    {
                        StrStateStatus = "MixEggOver";
                    });
                   
                }
            }
        }
    }
}

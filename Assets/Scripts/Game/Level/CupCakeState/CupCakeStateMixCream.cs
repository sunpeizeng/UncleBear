using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Touch;

namespace UncleBear
{
    public class CupCakeStateMixCream : State<LevelCupCake>
    {
        bool _bCanMix;
        GameObject _objMixer;
        Vector3 _v3BowlPos = new Vector3(-31, 22.5f, -30);
        Vector3 _v3CamPos = new Vector3(-32, 83, 23);
        Vector3 _v3MixerPos = new Vector3(-31, 31, -30);
        float _fAroundRadius = 1.5f;
        bool _bMixOk;
        float _fRotSpeed;
        float _fRotAngle;
        bool _bHitChopstick;

        //转汤的一些参数
        float _fShakeX = 1;
        float _fShakeFixX = 0;
        float _fMixCd = 0.5f;
        float _fMixColorCounter;

        public CupCakeStateMixCream(int stateEnum) : base(stateEnum)
        {

        }
        public override void Enter(object param)
        {
            _bCanMix = false;
            //Debug.Log("mix cream");
            _objMixer = _owner.LevelObjs[Consts.ITEM_MIXER];
            CameraManager.Instance.DoCamTween(_v3CamPos, 1);

            _owner.Cupcakes.ForEach(p => p.transform.DOMove(p.transform.position + Vector3.left * 20, 0.5f));
            _owner.LevelObjs[Consts.ITEM_FLUID].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.3f);
            _owner.LevelObjs[Consts.ITEM_BOWL].transform.DOMove(_v3BowlPos, 1).OnComplete(()=> {
                _objMixer.SetPos(_v3MixerPos + Vector3.up * 10);
                _objMixer.transform.DOMove(_v3MixerPos + new Vector3(0, 0, -_fAroundRadius), 0.5f).OnComplete(()=> { _bCanMix = true; });
            });
         
            _bMixOk = false;
            base.Enter(param);
        }

        public override string Execute(float deltaTime)
        {
            //if (_fRotSpeed != _fRotAngle)
            //{
            //    DOTween.To(() => _fRotAngle, p => _fRotAngle = p, _fRotSpeed, 1);
            //    _owner.LevelObjs[Consts.ITEM_FLUID].transform.localEulerAngles = new Vector3(0, -_fRotAngle, 0);
            //}

            if (_fRotSpeed != _fRotAngle)
            {
                var curDelta = 1f;
                if (_fShakeFixX != 0)
                    curDelta = Mathf.Clamp01(Mathf.Abs(_fRotSpeed - _fRotAngle) / _fShakeFixX);

                DOTween.To(() => _fRotAngle, p => _fRotAngle = p, _fRotSpeed, 1);
                _owner.LevelObjs[Consts.ITEM_FLUID].transform.localEulerAngles = new Vector3(_fShakeX * curDelta, -_fRotAngle, 0);
            }


            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            _owner.LevelObjs[Consts.ITEM_MIXER].transform.DOMove(Vector3.one * 500, 2f);
            _owner.LevelObjs[Consts.ITEM_BOWL].transform.DOMove(Vector3.one * 500, 2f);
            _owner.Cupcakes.ForEach(p => p.transform.DOMove(p.transform.position + Vector3.right * 20, 0.5f));
            base.Exit();
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            if (!_bCanMix || _bMixOk)
                return;
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.gameObject == _objMixer)
            {
                _bHitChopstick = true;
                _fMixColorCounter = _fMixCd;
            }
        }


        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_bHitChopstick)
            {
                var newPos = GameUtilities.GetFingerTargetWolrdPos(finger, _objMixer, _v3MixerPos.y);
                if (Vector3.Distance(newPos, _v3MixerPos) > _fAroundRadius)
                    newPos = _v3MixerPos + (newPos - _v3MixerPos).normalized * _fAroundRadius;
                _fRotSpeed += -50;

                _fMixColorCounter -= Time.deltaTime;
                if (_fMixColorCounter < 0)
                {
                    _fMixColorCounter = _fMixCd;
                    ChangeFluidColor();
                }

                _objMixer.transform.position = Vector3.Lerp(_objMixer.transform.position, newPos, 20 * Time.deltaTime);
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
                float alpha = _owner.LevelObjs[Consts.ITEM_FLUID].GetComponent<MeshRenderer>().material.color.a;
                _owner.LevelObjs[Consts.ITEM_FLUID].GetComponent<MeshRenderer>().material.DOFade(alpha + 0.1f, 0.5f);

                if (_owner.LevelObjs[Consts.ITEM_FLUID].GetComponent<MeshRenderer>().material.color.a >= 1)
                {
                    _bMixOk = true;
                    StrStateStatus = "MixCreamOver";
                }
            }
        }
    }
}

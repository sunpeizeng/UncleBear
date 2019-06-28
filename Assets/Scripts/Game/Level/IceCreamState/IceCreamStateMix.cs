using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class IceCreamStateMix : State<LevelIceCream> {

        AudioSource _asMix;
        enum PhaseEnum
        {
            Waiting,
            Mix,
            Over,
        }
        PhaseEnum _mixPhase;

        GameObject _objMixer;
        ElecMixerCtrller _mixer;

        Vector3 _v3MixerPos;
        float _fRotSpeed;
        float _fRotAngle;
        float _fMixPerc;

        float _fMixColorCounter;
        float _fMixCD;

        bool _bHitBody = false;
        bool _bHitMixer = false;
        float _fAroundRadius = 1.5f;
        MeshRenderer _meshMilk;

        List<Transform> _lstTrsPieces = new List<Transform>();

        public IceCreamStateMix(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);
            _mixPhase = PhaseEnum.Waiting;
            _objMixer = _owner.LevelObjs[Consts.ITEM_ICMIXER];
            _mixer = _objMixer.GetComponent<ElecMixerCtrller>();
            _v3MixerPos = _owner.LevelObjs[Consts.ITEM_ICBOWLBIG].transform.position + new Vector3(0, 0.5f, -1.35f);
            _objMixer.SetPos(_v3MixerPos + Vector3.up * 50);

            _objMixer.transform.DOMoveY(_v3MixerPos.y, 1f).OnComplete(()=> {
                _mixPhase = PhaseEnum.Mix;
                GuideManager.Instance.SetGuideClick(_mixer.objBTRed.transform.position - Vector3.up);
            });
            _meshMilk = _owner.LevelObjs[Consts.ITEM_ICBOWLBIG].transform.Find("Milk").GetComponentInChildren<MeshRenderer>();

            for (int i = 0; i < _owner.LevelObjs[Consts.ITEM_ICBOWLBIG].transform.childCount; i++)
            {
                Transform trs = _owner.LevelObjs[Consts.ITEM_ICBOWLBIG].transform.GetChild(i);
                if (trs.name == "Mesh" || trs.name == "Milk")
                    continue;
                else
                    _lstTrsPieces.Add(trs);
            }
            _fMixPerc = _fRotAngle = _fRotSpeed = _fMixColorCounter = 0;


        }

        public override string Execute(float deltaTime)
        {
            if (_mixPhase == PhaseEnum.Mix)
            {
                if (_mixer.eState != ElecMixerCtrller.MixerState.Closed)
                {
                    _fRotSpeed += _mixer.fCurSpeed;

                    _fMixColorCounter -= Time.deltaTime;
                    if (_fMixColorCounter < 0)
                    {
                        _fMixColorCounter = _fMixCD;
                        ChangeFluidColor();
                    }

                    if (_meshMilk.material.HasProperty("_Slider_Val"))
                    {
                        _meshMilk.material.SetFloat("_Slider_Val", _fMixPerc);
                    }
                }
            }
            var curDelta = Mathf.Sqrt(_mixer.fCurSpeed / 10);
            DOTween.To(() => _fRotAngle, p => _fRotAngle = p, _fRotSpeed, 1);
            _meshMilk.transform.localEulerAngles = new Vector3(curDelta, -_fRotAngle, 0);

            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            _objMixer = null;
            base.Exit();
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            if (_mixPhase == PhaseEnum.Mix)
            {
                RaycastHit hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
                if (hit.collider != null)
                {
                    if (_bHitMixer && hit.collider.transform == _mixer.trsBody)
                    {
                        _bHitBody = true;
                    }
                    else if (hit.collider.gameObject == _mixer.objBTBlue)
                    {
                        _bHitMixer = true;
                        DoozyUI.UIManager.PlaySound("67搅拌机按键", _v3MixerPos);
                        if (_asMix == null)
                            _asMix = DoozyUI.UIManager.PlaySound("66搅拌机工作8位wav", _v3MixerPos, true, 0.5f, 0.3f);
                        GuideManager.Instance.StopGuide();
                        _mixer.OnPressBlue();
                        _fMixColorCounter = _fMixCD = 1f;
                    }
                    else if (hit.collider.gameObject == _mixer.objBTRed)
                    {
                        _bHitMixer = true;
                        DoozyUI.UIManager.PlaySound("67搅拌机按键", _v3MixerPos);
                        if (_asMix == null)
                            _asMix = DoozyUI.UIManager.PlaySound("66搅拌机工作8位wav", _v3MixerPos, true, 0.5f, 0.3f);
                        GuideManager.Instance.StopGuide();
                        _mixer.OnPressRed();
                        _fMixColorCounter = _fMixCD = 0.5f;
                    }
                }
            }
        }

      
        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_bHitBody)
            {
                var newPos = GameUtilities.GetFingerTargetWolrdPos(finger, _objMixer, _v3MixerPos.y);
                if (Vector3.Distance(newPos, _v3MixerPos) > _fAroundRadius)
                    newPos = _v3MixerPos + (newPos - _v3MixerPos).normalized * _fAroundRadius;
                _objMixer.transform.position = Vector3.Lerp(_objMixer.transform.position, newPos, 20 * Time.deltaTime);

                //var disVec = new Vector3(-Mathf.Sin(Mathf.Deg2Rad * _circleGesCtrl.fCurAngle), 0, -Mathf.Cos(Mathf.Deg2Rad * _circleGesCtrl.fCurAngle));
                ////if(_circleGesCtrl.fDeltaAngle > 0)
                ////_objChopstick.transform.position = Vector3.Lerp(_objChopstick.transform.position, _v3ChopstickPos + disVec.normalized * _fAroundRadius, 20 * Time.deltaTime);
            }
        }


        protected override void OnFingerUp(LeanFinger finger)
        {
            _bHitBody = false;
        }

        void ChangeFluidColor()
        {
            if (_mixPhase == PhaseEnum.Mix)
            {
                if(_mixer.eState == ElecMixerCtrller.MixerState.High)
                    DoozyUI.UIManager.PlaySound("69搅拌碗高频", _v3MixerPos);
                else if (_mixer.eState == ElecMixerCtrller.MixerState.Low)
                    DoozyUI.UIManager.PlaySound("68搅拌碗低频", _v3MixerPos);
                DOTween.To(() => _fMixPerc, p => _fMixPerc = p, _fMixPerc + 0.1f, 0.5f);
                _lstTrsPieces.ForEach(p => p.DOScale(p.localScale * 0.8f, 0.5f));

                if (_fMixPerc >= 1)
                {
                    AudioSourcePool.Instance.Free(_asMix);
                    _lstTrsPieces.ForEach(p => GameObject.Destroy(p.gameObject));
                    _mixPhase = PhaseEnum.Over;
                    _mixer.Close();

                    DoozyUI.UIManager.PlaySound("8成功");

                    
                    _objMixer.transform.DOMoveY(_v3MixerPos.y + 50, 1f).SetDelay(0.8f).OnComplete(() =>
                    {
                        _owner.LevelObjs[Consts.ITEM_ICBOWLBIG].transform.DOMove(_v3MixerPos + Vector3.left * 40, 0.5f);
                        _objMixer.transform.DOMove(Vector3.one * 500, 1f).OnComplete(()=>{
                            StrStateStatus = "MixOver";
                        });
                    });
                }
            }
        }

    }
}
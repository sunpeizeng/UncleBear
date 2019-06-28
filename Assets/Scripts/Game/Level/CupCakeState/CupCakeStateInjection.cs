using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    public class CupCakeStateInjection : State<LevelCupCake>
    {
        AudioSource _asInjection;

        string _strInjectionAnim = "anim_cakeInjection";
        Vector3 _v3CupPos = new Vector3(-89.2f, 22.8f, -90f);//new Vector3(-28, 22.8f, -35);
        Vector3 _v3PlatePos = new Vector3(-86.2f, 22.7f, -80);
        //Vector3 _v3FluidScale = new Vector3(2, 0.5f, 2);

        bool _bInjection;
        bool _bCupCakeOk;

        int _nCakeCount = 6;
        int _nCakeIndex;

        float _fHoldTime;
        bool _bFailed;

        public CupCakeStateInjection(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("injection");
            base.Enter(param);

            CameraManager.Instance.DoCamTween(new Vector3(-34.8f, 80.4f, -80), 0.5f);

            _bFailed = _bCupCakeOk = false;
            _nCakeIndex = 0;
            _fHoldTime = 0;
 
            _owner.LevelObjs[Consts.ITEM_OVENPLATE].SetAngle(new Vector3(0, 90, 0));
            _owner.LevelObjs[Consts.ITEM_BOWL].transform.DOMove(Vector3.one * 500, 0.5f).OnComplete(() => {
                _owner.LevelObjs[Consts.ITEM_OVENPLATE].transform.DOMove(_v3PlatePos, 0.5f).OnComplete(() => { FormCupsInLine(); });
            });
      
            LeanTouch.OnFingerHeldDown += StartHoldCakeCup;
            LeanTouch.OnFingerHeldUp += ReleaseHoldCakeCup;

        }

        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
            LeanTouch.OnFingerHeldDown -= StartHoldCakeCup;
            LeanTouch.OnFingerHeldUp -= ReleaseHoldCakeCup;
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            base.OnFingerDown(finger);
            if (_bInjection || _bCupCakeOk)
                return;
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.gameObject == _owner.LevelObjs[Consts.ITEM_SYRINGE])
            {
                _bInjection = true;
                //var animCake = _owner.Cupcakes[_nCakeIndex].GetComponentInChildren<Animation>();
                _animCurCup.Play(_strInjectionAnim);
                _animCurCup[_strInjectionAnim].speed = 1;
                _asInjection = DoozyUI.UIManager.PlaySound("27注射蛋液", _owner.LevelObjs[Consts.ITEM_SYRINGE].transform.position);
            }
        }

        protected override void OnFingerSet(LeanFinger finger)
        {
            base.OnFingerSet(finger);
            if (_bInjection)
            {
                var animLen = _animCurCup[_strInjectionAnim].length;

                _fHoldTime += Time.deltaTime;
                _fHoldTime = Mathf.Clamp(_fHoldTime, 0, 0.9f);//animLen);
                _animCurCup.SampleAnim(_strInjectionAnim, _fHoldTime / animLen);

                if (_fHoldTime > animLen * 0.9f && !_bFailed)
                {
                    _bFailed = true;
                    DoozyUI.UIManager.PlaySound("28蛋液漫出", _owner.LevelObjs[Consts.ITEM_SYRINGE].transform.position);
                    _asInjection.Stop();
                }
            }
        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            base.OnFingerUp(finger);
            if (_bInjection)
            {
                _asInjection.Stop();
                _bInjection = false;
                _animCurCup[_strInjectionAnim].speed = 0;
                var normalizedTime = _animCurCup[_strInjectionAnim].normalizedTime;
                if (normalizedTime >= 0.7f && normalizedTime <= 0.9f)
                {
                    _fHoldTime = 0;
                    DoozyUI.UIManager.PlaySound("8成功");
                    _nCakeIndex++;
                    if (_nCakeIndex < _nCakeCount)
                        MovePipingBag();
                    else
                    {
                        _bCupCakeOk = true;
                        _owner.LevelObjs[Consts.ITEM_SYRINGE].transform.DOMoveY(60, 1f).OnComplete(() => {
                            _owner.LevelObjs[Consts.ITEM_SYRINGE].transform.DOMove(Vector3.one * 500, 0.5f).OnComplete(() =>
                            {
                                StrStateStatus = "CakeInjectionOver";
                            });
                        });
                    }
                }
                else if (normalizedTime < 0.6f)
                {
                    //Debug.Log("More.");
                    _animCurCup.SampleAnim(_strInjectionAnim, normalizedTime);
                }
                else if (normalizedTime > 0.9f)
                {
                    Debug.Log("Please try again.");
                    _fHoldTime = 0;
                    _bFailed = false;
                    _animCurCup.SampleAnim(_strInjectionAnim, 0);
                }
            }

        }


        void StartHoldCakeCup(LeanFinger finger)
        {
         
        }

        void ReleaseHoldCakeCup(LeanFinger finger)
        {
           
        }

        protected void FormCupsInLine(float disX = 10, float disY = 6)
        {
            _owner.LevelObjs[Consts.ITEM_CUPCAKE].transform.SetParent(_owner.LevelObjs[Consts.ITEM_PAPERCUP].transform);
            _owner.LevelObjs[Consts.ITEM_CUPCAKE].SetAngle(new Vector3(-90, 0, 0));
            _owner.LevelObjs[Consts.ITEM_CUPCAKE].SetLocalPos(Vector3.zero);
            _owner.LevelObjs[Consts.ITEM_CUPCAKE].transform.localScale = Vector3.zero;

            for (int i = 0; i < _nCakeCount; i++)
            {
                var objCup = GameUtilities.InstantiateT<GameObject>(_owner.LevelObjs[Consts.ITEM_PAPERCUP]);
                objCup.GetComponentInChildren<MeshRenderer>().material = objCup.GetComponent<CupcakeMatsCtrller>().RandomCupMat();
                _owner.Cupcakes.Add(objCup);
                var cupPos = _v3CupPos + Vector3.right * disY * (i % 2) + Vector3.forward * disX * (i / 2);
                if (i == _nCakeCount - 1)
                    objCup.transform.DOMove(cupPos, 0.5f).OnComplete(MovePipingBag);
                else objCup.transform.DOMove(cupPos, 0.5f);
            }
        }


        Animation _animCurCup;
        void MovePipingBag()
        {
            _animCurCup = _owner.Cupcakes[_nCakeIndex].transform.FindChild("Cupcake").GetComponent<Animation>();
            _owner.LevelObjs[Consts.ITEM_SYRINGE].transform.DOMove(_owner.Cupcakes[_nCakeIndex].transform.position + Vector3.up * 3, 0.5f).OnComplete(()=> {
                GuideManager.Instance.SetGuideClick(_owner.LevelObjs[Consts.ITEM_SYRINGE].transform.position + Vector3.forward * 2, 0.5f);
            });
        }
    }
}

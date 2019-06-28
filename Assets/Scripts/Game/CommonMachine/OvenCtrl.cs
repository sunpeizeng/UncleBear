using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    public class OvenCtrl :  MachineCtrl
    {
        AudioSource _asBaking;
        float _fAnimTime;
        Animation _animOven;

        bool _bBaking;
        bool _bOvenOpened;
        bool _bBakedOver;
        bool _bBakedOpened;

        Vector3 _v3PlatePos;

        float _fBakeTime = 5;
        float _fBakeTimer;
    
        GameObject _objBaking;
        bool _bStartBake;
        System.Action<bool> _callbackCookOk;

        Renderer[] _renders;

        void Awake()
        {
            _animOven = EnterKitchen.Instance.ObjOvenDoor.GetComponent<Animation>();
            _v3PlatePos = EnterKitchen.Instance.ObjOvenPlate.transform.position;
        }

        public void RegisterObject(GameObject obj, System.Action<GameObject> finishCallback, System.Action<bool> cookOkCallback)
        {
            _callbackCookOk = cookOkCallback;
            OnMachineFinish = finishCallback;
            _objBaking = obj;
            _renders = _objBaking.GetComponentsInChildren<Renderer>();
            _bBakedOpened = _bStartBake = _bRigidLimitUp = false;

            _fBakeTimer = _fAnimTime = 0;
            _bBaking = _bBakedOver = _bOvenOpened = false;

            _animOven.Play("anim_OpenOven");
            _fAnimTime = _animOven["anim_OpenOven"].length;
            DoozyUI.UIManager.PlaySound("15开烤箱门");

#if !UNITY_EDITOR
            AdHelper.HideBanner();
#endif
        }

        new void OnDisable()
        {
#if !UNITY_EDITOR
			if (!GameData.AdsRemoved && UncleBear.GameUtilities.GetParam("isBannerOpened", "close") == "open")
			{
				AdHelper.ShowBanner();
			}
#endif

            LeanTouch.OnFingerSwipe -= OnFingerSwipe;
            base.OnDisable();
        }

        void Update()
        {
            TickBakeTime(Time.deltaTime);
            TickAnimTime(Time.deltaTime);
        }

        void TickBakeTime(float deltaTime)
        {
            if (_fBakeTimer > 0 && _callbackCookOk != null)
            {
                _fBakeTimer -= Time.deltaTime;
                EnterKitchen.Instance.SetOvenTime(_fBakeTimer);
                SetRenderLerp((_fBakeTime - _fBakeTimer) / _fBakeTime);
                if (_fBakeTimer <= 0)
                {
                    _bBaking = false;
                    EnterKitchen.Instance.SetOvenButtonLight(EnterKitchen.ButtonStateEnum.Finish);
                    _bBakedOver = true;
                    var eff = EffectCenter.Instance.SpawnEffect("Steam", _objBaking.transform.position, Vector3.zero);
                    if (eff != null)
                    {
                        eff.transform.SetParent(_objBaking.transform);
                        eff.transform.GetChild(0).localScale = new Vector3(20, 20, 30);
                    }
                    AudioSourcePool.Instance.Free(_asBaking);
                    DoozyUI.UIManager.PlaySound("18烤箱时间到");
                    GuideManager.Instance.SetGuideSingleDir(EnterKitchen.Instance.ObjOvenDoor.transform.position + Vector3.up * 5, EnterKitchen.Instance.ObjOvenDoor.transform.position - Vector3.up * 5, true, true, 1f);
                }
            }
        }
        void TickAnimTime(float deltaTime)
        {
            if (_fAnimTime > 0)
            {
                _fAnimTime -= deltaTime;
                if (_fAnimTime <= 0)
                {
                    if (!_bOvenOpened)
                    {
                        _bOvenOpened = true;
                        EnterKitchen.Instance.ObjOvenPlate.transform.DOMoveZ(_v3PlatePos.z + 10, 0.5f).OnComplete(() =>
                        {
                            _objBaking.SetPos(new Vector3(13.5f, 15.6f, -57));
                            _objBaking.transform.DOMove(EnterKitchen.Instance.ObjOvenPlate.transform.position + new Vector3(0, 0.5f, 0), 0.5f).OnComplete(() =>
                            {
                                GuideManager.Instance.SetGuideSingleDir(EnterKitchen.Instance.ObjOvenPlate.transform.position, EnterKitchen.Instance.ObjOvenPlate.transform.position + Vector3.up * 5, true, true, 1f);
                                LeanTouch.OnFingerSwipe += OnFingerSwipe;
                                _objBaking.transform.SetParent(EnterKitchen.Instance.ObjOvenPlate.transform);
                                _items = EnterKitchen.Instance.ObjOvenPlate.GetComponentsInChildren<FridgeItemCtrller>();
                            });
                        });
                    }
                    else if (_bBakedOver)
                    {
                        GuideManager.Instance.StopGuide();
                        EnterKitchen.Instance.ObjOvenPlate.transform.DOMoveZ(_v3PlatePos.z + 10, 0.5f).OnComplete(() =>
                        {
                            _callbackCookOk.Invoke(false);
                            _callbackCookOk = null;
                        });
                    }
                }

            }
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition), 1000, 1 << LayerMask.NameToLayer("CookMachine"));
            if (hit.collider != null && hit.collider.transform.IsChildOf(transform))
            {
                //_bTouchingSoup = true;
            }
        }

        void OnFingerSwipe(LeanFinger finger)
        {
            var swipe = finger.SwipeScreenDelta;
            if (swipe.y < -Mathf.Abs(swipe.x))
            {
                //向下
                if (_bBakedOver && !_bBakedOpened)
                {
                    _bBakedOpened = true;
                    DoozyUI.UIManager.PlaySound("15开烤箱门");
                    _animOven["anim_OpenOven"].speed = 1;
                    _animOven["anim_OpenOven"].normalizedTime = 0;
                    _animOven.Play("anim_OpenOven");
                    _fAnimTime = _animOven["anim_OpenOven"].length;
                }
            }

            if (!_bBakedOver)
            {
                if (swipe.y > Mathf.Abs(swipe.x))
                {
                    //向上
                    if (!_bBaking && _fBakeTimer <= 0 && _bOvenOpened && _objBaking.transform.parent == EnterKitchen.Instance.ObjOvenPlate.transform)
                    {
                        DoozyUI.UIManager.PlaySound("16关烤箱门");
                        _animOven["anim_OpenOven"].speed = -1;
                        _animOven["anim_OpenOven"].normalizedTime = 1;
                        _animOven.Play("anim_OpenOven");
                        EnterKitchen.Instance.ObjOvenPlate.transform.DOMoveZ(_v3PlatePos.z, _animOven["anim_OpenOven"].length).OnComplete(() =>
                        {
                            EnterKitchen.Instance.SetOvenButtonLight(EnterKitchen.ButtonStateEnum.Cooking);
                            _asBaking = DoozyUI.UIManager.PlaySound("17烤箱风声_1", EnterKitchen.Instance.ObjOvenDoor.transform.position, true, 0.7f, 0.5f);
                            _fBakeTimer = _fBakeTime;
                        });
                        _bBaking = true;
                        GuideManager.Instance.StopGuide();
                    }
                }
            }
        }

        public override void Stop()
        {
            EnterKitchen.Instance.ObjOvenPlate.SetPos(_v3PlatePos);
            EnterKitchen.Instance.SetOvenButtonLight(EnterKitchen.ButtonStateEnum.Close);
            if (OnMachineFinish != null)
                OnMachineFinish(_objBaking);
        }
    }
}

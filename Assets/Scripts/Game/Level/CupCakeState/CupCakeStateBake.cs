using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    public class CupCakeStateBake : State<LevelCupCake> {

        AudioSource _asBaking;
        GameObject _objCakePlate;

        float _fAnimTime;
        bool _bBaking;
        bool _bOvenReady;
        bool _bOvenOpened;
        bool _bBakedOver;
        bool _bBakedOpened;
        Animation _animOven;
        float _fBakeTime = 5f;
        float _fBakeTimer;
        List<GameObject> _lstObsoleteObjs = new List<GameObject>();
        Vector3 _v3PlatePos;

        Material[] _mats;
        public CupCakeStateBake(int stateEnum) : base(stateEnum)
        {
            
        }

        //可以仿照Drpanda写一个Oven类,传参数过去,交给那边处理
        public override void Enter(object param)
        {
            EnterKitchen.Instance.ShowOvenTime(true);
            //Debug.Log("bake");
            _fBakeTimer = _fAnimTime = 0;
            _bBakedOpened = _bBaking = _bBakedOver = _bOvenOpened = _bOvenReady = false;
            _animOven = EnterKitchen.Instance.ObjOvenDoor.GetComponent<Animation>();
            EnterKitchen.Instance.ObjOvenPlate.SetActive(false);
            _v3PlatePos = EnterKitchen.Instance.ObjOvenPlate.transform.position;

            _objCakePlate = _owner.LevelObjs[Consts.ITEM_OVENPLATE];
            _mats = new Material[_owner.Cupcakes.Count];
            //准备烤盘一起进去
            for(int i= 0;i<_owner.Cupcakes.Count;i++)
            {
                _owner.Cupcakes[i].transform.SetParent(_objCakePlate.transform);
                _mats[i] = _owner.Cupcakes[i].transform.FindChild("Cupcake").GetComponent<MeshRenderer>().material;
            }

            //TODO::烤箱专用视角,可通用
            CameraManager.Instance.DoCamTween(new Vector3(-4.5f, 34.4f, -13.7f), new Vector3(15, 180, 0), 0.5f, () =>
            {

                LeanTouch.OnFingerSwipe += OnFingerSwipe;
                _animOven.Play("anim_toOven");
                _fAnimTime = _animOven["anim_toOven"].length;
            });

            base.Enter(param);
            GuideManager.Instance.SetGuideSingleDir(EnterKitchen.Instance.ObjOvenDoor.transform.position + Vector3.up * 5, EnterKitchen.Instance.ObjOvenDoor.transform.position - Vector3.up * 5, true, true, 1f);

#if !UNITY_EDITOR
            AdHelper.HideBanner();
#endif
        }

        public override string Execute(float deltaTime)
        {
            TickBakeTime(deltaTime);
            TickAnimTime(deltaTime);
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
#if !UNITY_EDITOR
			if (!GameData.AdsRemoved && UncleBear.GameUtilities.GetParam("isBannerOpened", "close") == "open")
			{
				AdHelper.ShowBanner();
			}
#endif

            EnterKitchen.Instance.ShowOvenTime(false);
            _animOven.SampleAnim("anim_OpenOven", 0);
            _animOven = null;
            _mats = null;
            LeanTouch.OnFingerSwipe -= OnFingerSwipe;
            base.Exit();
            EnterKitchen.Instance.SetOvenButtonLight(EnterKitchen.ButtonStateEnum.Close);
        }

        void SetrenderLerp(float val)
        {
            for (int i = 0; i < _mats.Length; i++)
            {
                _mats[i].SetFloat("_Slider_Val", val);
            }
        }

        void TickBakeTime(float deltaTime)
        {
            if (_fBakeTimer > 0)
            {
                _fBakeTimer -= deltaTime;
                EnterKitchen.Instance.SetOvenTime(_fBakeTimer);
                SetrenderLerp((_fBakeTime - _fBakeTimer) / _fBakeTime);
                if (_fBakeTimer <= 0)
                {
                    _bBaking = false;
                    _bBakedOver = true;
                    AudioSourcePool.Instance.Free(_asBaking);
                    _owner.Cupcakes.ForEach(p =>
                    {
                        var eff = EffectCenter.Instance.SpawnEffect("Steam", p.transform.position + Vector3.up * 3.5f, Vector3.zero);
                        if (eff != null)
                        {
                            eff.transform.SetParent(p.transform);
                            eff.transform.GetChild(0).localScale = new Vector3(15, 15, 10);
                        }
                    });

                    DoozyUI.UIManager.PlaySound("18烤箱时间到");
                    GuideManager.Instance.SetGuideSingleDir(EnterKitchen.Instance.ObjOvenDoor.transform.position + Vector3.up * 5, EnterKitchen.Instance.ObjOvenDoor.transform.position - Vector3.up * 5, true, true, 1f);
                    EnterKitchen.Instance.SetOvenButtonLight(EnterKitchen.ButtonStateEnum.Finish);
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
                    if (_bOvenReady && !_bOvenOpened)
                    {
                        _objCakePlate.SetPos(new Vector3(-4.3f, 15.6f, -38));
                        _objCakePlate.SetAngle(Vector3.zero);
                        _objCakePlate.transform.DOMove(_v3PlatePos + Vector3.forward * 10, 0.5f).OnComplete(() => { _bOvenOpened = true; });
                        GuideManager.Instance.SetGuideSingleDir(EnterKitchen.Instance.ObjOvenPlate.transform.position, EnterKitchen.Instance.ObjOvenPlate.transform.position + Vector3.up * 5, true, true, 1f);
                    }
                    else if (_bBakedOver)
                    {
                        GuideManager.Instance.StopGuide();
                        _objCakePlate.transform.DOMoveZ(_v3PlatePos.z + 10, 0.5f).OnComplete(() =>
                        {
                            _owner.LevelObjs[Consts.ITEM_BOWL].transform.DOMove(501 * Vector3.one, 1f).OnComplete(() =>
                            {
                                StrStateStatus = "BakeOver";
                            });
                        });
                    }
                }

            }
        }

        void OnFingerSwipe(LeanFinger finger)
        {
            var swipe = finger.SwipeScreenDelta;
            if (swipe.y < -Mathf.Abs(swipe.x))
            {
                //向下
                if (!_bOvenReady && _fAnimTime <= 0)
                {
                    _bOvenReady = true;
                    _animOven.CrossFade("anim_OpenOven", 0.2f);
                    _fAnimTime = _animOven["anim_OpenOven"].length;
                    DoozyUI.UIManager.PlaySound("15开烤箱门");
                }

                //向下
                if (_bBakedOver && !_bBakedOpened)
                {
                    _bBakedOpened = true;
                    _animOven["anim_OpenOven"].speed = 1;
                    _animOven["anim_OpenOven"].normalizedTime = 0;
                    _animOven.Play("anim_OpenOven");
                    _fAnimTime = _animOven["anim_OpenOven"].length;
                    DoozyUI.UIManager.PlaySound("15开烤箱门");
                }
            }

            if (!_bBakedOver)
            {
                if (swipe.y > Mathf.Abs(swipe.x))
                {
                    //向上
                    if (!_bBaking && _fBakeTimer <= 0 && _bOvenOpened)
                    {
                        DoozyUI.UIManager.PlaySound("16关烤箱门");
                        _animOven["anim_OpenOven"].speed = -1;
                        _animOven["anim_OpenOven"].normalizedTime = 1;
                        _animOven.Play("anim_OpenOven");
                        _objCakePlate.transform.DOMoveZ(_v3PlatePos.z, _animOven["anim_OpenOven"].length).OnComplete(() =>
                        {
                            EnterKitchen.Instance.SetOvenButtonLight(EnterKitchen.ButtonStateEnum.Cooking);
                            _fBakeTimer = _fBakeTime;
                            _asBaking = DoozyUI.UIManager.PlaySound("17烤箱风声_1", _objCakePlate.transform.position, true, 0.7f, 0.5f);
                        });
                        _bBaking = true;
                        GuideManager.Instance.StopGuide();
                        HandleCupcakeBake();
                    }
                }
            }


        }


        void HandleCupcakeBake()
        {
            //Debug.Log(_fBakeTimer);
        }
    }
}

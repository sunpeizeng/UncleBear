using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Touch;

namespace UncleBear
{
    public class PizzaStateBake : State<LevelPizza>
    {
        ParticleCtrller _bakeEff;
        AudioSource _asBaking;

        float _fAnimTime;
        bool _bBakedOpened;
        bool _bBaking;
        bool _bOvenReady;
        bool _bOvenOpened;
        bool _bBakedOver;
        Animation _animOven;
        float _fBakeTime = 5f;
        float _fBakeTimer;
        List<GameObject> _lstObsoleteObjs = new List<GameObject>();
        Vector3 _v3PlatePos;

        Material[] _mats;

        public PizzaStateBake(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("bake");
            _owner.LevelObjs[Consts.ITEM_CONVEYOR].SetPos(Vector3.one * 500);
            _fBakeTimer = _fAnimTime = 0;
            _bBakedOpened = _bBaking = _bBakedOver = _bOvenOpened = _bOvenReady = false;
            _animOven = EnterKitchen.Instance.ObjOvenDoor.GetComponent<Animation>();
            _v3PlatePos = EnterKitchen.Instance.ObjOvenPlate.transform.position;

            var renderers = _owner.LevelObjs[Consts.ITEM_PIZZA].GetComponentsInChildren<Renderer>();
            _mats = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                _mats[i] = renderers[i].material;
            }

            CameraManager.Instance.DoCamTween(new Vector3(-4.5f, 44.8f, -16.7f), new Vector3(20, 180, 0), 0.5f, ()=> {

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

        //Bake功能一部分是通用的,比如drpanda中有一个Oven类,把物体传入就会有一系列操作
        //目前没有烤箱元素,没有贴图置换,所以先不写通用的逻辑
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

            if (_bakeEff != null)
            {
                _bakeEff.ResetMaxTimeUseful();
                _bakeEff = null;
            }
            LeanTouch.OnFingerSwipe -= OnFingerSwipe;
            base.Exit();
            EnterKitchen.Instance.SetOvenButtonLight(EnterKitchen.ButtonStateEnum.Close);
        }

        void SetrenderLerp(float val)
        {
            for (int i = 0; i < _mats.Length; i++)
            {
                //TODO::想一个办法,尽可能还是用sharedmat减少new消耗
                if (_mats[i].HasProperty("_Slider_Val"))
                {
                    _mats[i].SetFloat("_Slider_Val", val);
                }
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
                    //考好特效
                    _bakeEff = EffectCenter.Instance.SpawnEffect("Steam", _owner.LevelObjs[Consts.ITEM_PIZZA].transform.position, Vector3.zero);
                    if (_bakeEff != null)
                    {
                        _bakeEff.SetMaxTimeUseless();
                        _bakeEff.transform.SetParent(_owner.LevelObjs[Consts.ITEM_PIZZA].transform);
                        _bakeEff.transform.GetChild(0).localScale = new Vector3(50, 50, 30);
                    }

                    AudioSourcePool.Instance.Free(_asBaking);
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
                        _bOvenOpened = true;
                        EnterKitchen.Instance.ObjOvenPlate.transform.DOMoveZ(_v3PlatePos.z + 10, 0.5f).OnComplete(() =>
                        {
                            GuideManager.Instance.SetGuideSingleDir(EnterKitchen.Instance.ObjOvenPlate.transform.position, EnterKitchen.Instance.ObjOvenPlate.transform.position + Vector3.up * 5, true, true, 1f);
                            _owner.LevelObjs[Consts.ITEM_PIZZA].SetPos(new Vector3(-3.5f, 15.6f, -57));
                            _owner.LevelObjs[Consts.ITEM_PIZZA].transform.DOMove(EnterKitchen.Instance.ObjOvenPlate.transform.position + new Vector3(0, 0.5f, -1), 0.5f).OnComplete(() =>
                            {
                                _owner.LevelObjs[Consts.ITEM_PIZZA].transform.SetParent(EnterKitchen.Instance.ObjOvenPlate.transform);
                            });
                        });
                    }
                    else if(_bBakedOver)
                    {
                        GuideManager.Instance.StopGuide();
                        EnterKitchen.Instance.ObjOvenPlate.transform.DOMoveZ(_v3PlatePos.z + 10, 0.5f).OnComplete(() => {
                            _owner.LevelObjs[Consts.ITEM_PIZZA].transform.SetParent(null);
                            _owner.LevelObjs[Consts.ITEM_CONVEYOR].transform.DOMove(501 * Vector3.one, 1f).OnComplete(() =>
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
                    DoozyUI.UIManager.PlaySound("15开烤箱门");
                    _bOvenReady = true;
                    _animOven.CrossFade("anim_OpenOven", 0.2f);
                    _fAnimTime = _animOven["anim_OpenOven"].length;
                }

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
                    if (!_bBaking && _fBakeTimer <= 0 && _bOvenOpened && _owner.LevelObjs[Consts.ITEM_PIZZA].transform.parent != null)
                    {
                        DoozyUI.UIManager.PlaySound("16关烤箱门");
                        _animOven["anim_OpenOven"].speed = -1;
                        _animOven["anim_OpenOven"].normalizedTime = 1;
                        _animOven.Play("anim_OpenOven");
                        EnterKitchen.Instance.ObjOvenPlate.transform.DOMoveZ(_v3PlatePos.z, _animOven["anim_OpenOven"].length).OnComplete(() =>
                        {
                            EnterKitchen.Instance.SetOvenButtonLight(EnterKitchen.ButtonStateEnum.Cooking);
                            _fBakeTimer = _fBakeTime;
                            _asBaking = DoozyUI.UIManager.PlaySound("17烤箱风声_1", EnterKitchen.Instance.ObjOvenDoor.transform.position, true, 0.7f, 0.5f);
                        });
                        _bBaking = true;
                        GuideManager.Instance.StopGuide();
                        HandlePizzaBake();
                    }
                }
            }
        }


        void HandlePizzaBake()
        {
            _owner.ObjPizzaBody.transform.DOScaleZ(1.5f, _fBakeTime).OnComplete(() =>
            {
                //_bBakedOver = true;
                //EnterKitchen.Instance.SetOvenButtonLight(EnterKitchen.ButtonStateEnum.Finish);
                //_strStateStatus = "BakeOver";
                _lstObsoleteObjs.ForEach(p =>
                {
                    GameObject.Destroy(p);
                });
                _lstObsoleteObjs.Clear();
            });
            _owner.PizzaAdditives.ForEach(p =>
            {
                if (p.name == "PizzaSauce")
                {
                    p.transform.DOLocalMoveY(0.6f, _fBakeTime);
                    p.layer = LayerMask.NameToLayer("Cuttable");
                }
                else if (p.name == "PizzaCheese")
                {
                    if (p.GetComponent<Rigidbody>() != null)
                        GameObject.Destroy(p.GetComponent<Rigidbody>());
                    var bakeCheese = p.transform.GetChild(0);
                    bakeCheese.name = "PizzaCheesePiece";
                    bakeCheese.localScale = Vector3.zero;
                    _owner.AdditivesToPizza(bakeCheese.gameObject);
                    p.transform.DOScale(0, _fBakeTime);
                    _lstObsoleteObjs.Add(p);
                }
                else if (p.name == "PizzaCheesePiece")
                {
                    p.SetActive(true);
                    p.layer = LayerMask.NameToLayer("Cuttable");
                    p.transform.DOLocalMoveY(0.7f, _fBakeTime);
                    p.transform.localEulerAngles = new Vector3(-90, p.transform.localEulerAngles.y, p.transform.localEulerAngles.z);
                    p.transform.DOScale(1, _fBakeTime);
                }
                else
                {
                    if (p.transform.localPosition.y < 0.8f)
                        p.transform.DOLocalMoveY(0.8f, _fBakeTime);
                    float tarX = p.transform.localEulerAngles.x > 0 && p.transform.localEulerAngles.x < 180 ? 90 : -90;
                    //float tarZ = p.transform.localEulerAngles.z > 90 && p.transform.localEulerAngles.z < 270 ? 180 : 0;
                    p.transform.DOLocalRotate(new Vector3(tarX, p.transform.localEulerAngles.y, p.transform.localEulerAngles.z), _fBakeTime);
                    if (p.GetComponent<Rigidbody>() != null)
                        GameObject.Destroy(p.GetComponent<Rigidbody>());
                }
            });
        }
    }
}

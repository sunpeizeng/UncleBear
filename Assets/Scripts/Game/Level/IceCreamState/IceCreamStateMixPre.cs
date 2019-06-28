using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Touch;

namespace UncleBear
{
    public class IceCreamStateMixPre : State<LevelIceCream>
    {
        AudioSource _asPotWater;
        enum PhaseEnum
        {
            Sugar,
            Cream,
            Milk,
            Waiting,
        }

        PhaseEnum _prePhase;

        Vector3 _v3PotOriginPos;
        Vector3 _v3BowlPos;
        Vector3 _v3SugarBottle = new Vector3(-60, 22.5f, -105);
        Vector3 _v3SugarMiddle = new Vector3(-60, 23.69f, -105);

        Vector3 _v3PipingBag = new Vector3(-50, 24.6f, -96.8f);
        Vector3 _v3PourMilkPos = new Vector3(-37.4f, 31, -93.8f);
        Vector3 _v3PourAngle = new Vector3(-20, 90, -13);

        Vector3 _v3PotMilkPos = new Vector3(0.41f, -0.47f, -2.26f);
        Vector3 _v3PotMilkAngle = new Vector3(50, 0, 0);

        float _fSugarY = -0.4f;

        GameObject _objSugarBottle;
        GameObject _objSugar;
        GameObject _objPipingBag;
        GameObject _objCream;
        GameObject _objPot;
        GameObject _objMilkPiece;

        Transform _trsPotMilk;

        bool _bHolding;
        bool _bPouring;

        ParticleCtrller _milkEff;
        Vector3 _v3MilkEff = new Vector3(0, 9.35f, -5.5f);

        Vector3 _v3MilkPieceOriginScale = new Vector3(0.5f, 1, 0.5f);
        Vector3 _v3MilkPieceTarPos = new Vector3(0, 2.5f, 0);

        float _fPourSpeed = 0.003f;

        public IceCreamStateMixPre(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);
            _v3BowlPos = _owner.LevelObjs[Consts.ITEM_ICBOWLBIG].transform.position;

            _prePhase = PhaseEnum.Sugar;
            _objSugarBottle = _owner.LevelObjs[Consts.ITEM_ICSUGARBOTTLE];
            _objSugarBottle.SetPos(_v3SugarBottle + Vector3.left * 50);
            _objSugarBottle.transform.DOMove(_v3SugarBottle, 0.5f).OnComplete(()=> {
                GuideManager.Instance.SetGuideSingleDir(_v3SugarMiddle, _v3BowlPos + Vector3.up * 5, true ,true, 1.5f);
            });

            _objPipingBag = _owner.LevelObjs[Consts.ITEM_PIPINGBAG];
            _objCream = _owner.LevelObjs[Consts.ITEM_ICCREAM];
            _objCream.transform.localScale = Vector3.zero;
            _objPot = _owner.LevelObjs[Consts.ITEM_POT];
            _v3PotOriginPos = _objPot.transform.position;
            _trsPotMilk = _objPot.transform.Find("Soup/SoupMesh");
            _trsPotMilk.parent.localEulerAngles = Vector3.zero;
            _objMilkPiece = _owner.LevelObjs[Consts.ITEM_ICMILK];
            _objMilkPiece.transform.localScale = _v3MilkPieceOriginScale;

            _objSugar = _owner.LevelObjs[Consts.ITEM_FLUID];
            _objSugar.transform.SetParent(_owner.LevelObjs[Consts.ITEM_ICBOWLBIG].transform);
            _objSugar.SetLocalPos(new Vector3(0, _fSugarY, 0));
            _objSugar.transform.localScale = Vector3.one * 0.9f;
            _objSugar.GetComponentInChildren<MeshRenderer>().material.color = new Color(1, 1, 1, 0);
            _bPouring = _bHolding = false;

            _milkEff = EffectCenter.Instance.SpawnEffect("Milk_Low", Vector3.zero, Vector3.zero);
            _milkEff.SetMaxTimeUseless();
            _milkEff.gameObject.SetActive(false);
            _milkEff.transform.SetParent(_owner.LevelObjs[Consts.ITEM_POT].transform);
            _milkEff.gameObject.SetLocalPos(_v3MilkEff);
            _milkEff.gameObject.SetAngle(new Vector3(0, 180, 0));
        }

        public override string Execute(float deltaTime)
        {
            HandleMilkEff(deltaTime);
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            _objSugarBottle = _objCream = _objMilkPiece = _objPipingBag = _objPot = _objSugar = null;
            _milkEff = null;
            base.Exit();
            AudioSourcePool.Instance.Free(_asPotWater);
        }

        void HandleMilkEff(float deltaTime)
        {
            if (_prePhase == PhaseEnum.Milk)
            {
                if (_objMilkPiece.transform.localPosition.y > 1 && _objSugar.gameObject.activeSelf)
                {
                    _objSugar.gameObject.SetActive(false);
                }


                float angle = _objPot.transform.eulerAngles.x;
                if (angle > 180)
                    angle -= 360;
                if (angle < -55)
                {
                    if (!_milkEff.gameObject.activeSelf)
                    {
                        _asPotWater = DoozyUI.UIManager.PlaySound("73倒牛奶修改", _milkEff.transform.position);
                        _milkEff.gameObject.SetActive(true);
                        _milkEff.SetMaxTimeUseless();
                        _trsPotMilk.DOKill();
                        _trsPotMilk.DOLocalMove(_v3PotMilkPos, 0.3f).OnComplete(()=> { _bPouring = true; });
                        _trsPotMilk.DOLocalRotate(_v3PotMilkAngle, 0.3f);
                    }
                    if (_bPouring)
                    {
                        float scaleVal = Mathf.Clamp(_objMilkPiece.transform.localScale.x + _fPourSpeed, 0.5f, 1);
                        float heightVal = Mathf.Clamp(_objMilkPiece.transform.localPosition.y + _fPourSpeed * 3, 0.5f, _v3MilkPieceTarPos.y);
                        _objMilkPiece.transform.localPosition = new Vector3(0, heightVal, 0);
                        _objMilkPiece.transform.localScale = new Vector3(scaleVal, 1, scaleVal);

                        if (heightVal == _v3MilkPieceTarPos.y && scaleVal == 1)
                        {
                            _prePhase = PhaseEnum.Waiting;
                            HandlePreFinish();
                        }
                    }
                }
                else
                {
                    if (_milkEff.gameObject.activeSelf)
                    {
                        _milkEff.gameObject.SetActive(false);
                        _trsPotMilk.DOKill();
                        _bPouring = false;
                        _trsPotMilk.DOLocalMove(Vector3.zero, 0.3f);
                        _trsPotMilk.DOLocalRotate(Vector3.zero, 0.3f);
                    }
                }
            }
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            switch (_prePhase)
            {
                case PhaseEnum.Sugar:
                    {
                        RaycastHit hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
                        if (hit.collider != null && hit.collider.transform.IsChildOf(_objSugarBottle.transform))
                        {
                            _bHolding = true;
                        }
                    }
                    break;
                case PhaseEnum.Cream:
                    {
                        RaycastHit hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
                        if (hit.collider != null && hit.collider.gameObject == _objPipingBag)
                        {
                            GuideManager.Instance.StopGuide();
                            _prePhase = PhaseEnum.Waiting;
                            DoozyUI.UIManager.PlaySound("13撒番茄酱", hit.point);
                            _objCream.SetPos(_v3PipingBag + Vector3.up * 0.4f);
                            _objCream.transform.SetParent(_owner.LevelObjs[Consts.ITEM_ICBOWLBIG].transform);
                            //
                            _objPipingBag.transform.DOMove(_v3PipingBag + Vector3.up * 1.5f, 1.5f);
                            //DOSpiral(1.5f, new Vector3(0, 1, 0), SpiralMode.ExpandThenContract, 0.5f, 2)
                            _objCream.transform.DOScale(new Vector3(1f, 1, 1f), 1.5f).OnComplete(() =>
                              {
                                  _objPipingBag.transform.DOMove(_v3PipingBag + Vector3.up * 50, 0.5f).OnComplete(() =>
                                  {
                                      HandleMilkPot();
                                      _objPipingBag.SetPos(Vector3.one * 500);
                                  });
                              });
                        }
                    }
                    break;
                case PhaseEnum.Milk:
                    {
                        var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
                        if (hit.collider != null && hit.collider.transform.IsChildOf(_owner.LevelObjs[Consts.ITEM_POT].transform))
                        {
                            _owner.LevelObjs[Consts.ITEM_POT].transform.DOKill();
                            _bHolding = true;
                        }
                    }
                    break;
            }
        }

        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_bHolding)
            {
                switch (_prePhase)
                {
                    case PhaseEnum.Sugar:
                        {
                            //位置跟随指针
                            var pos = GameUtilities.GetFingerTargetWolrdPos(finger, _objSugarBottle, _v3SugarBottle.y + 10f);
                            if (pos.z < -110)//墙
                                pos.z = -110;
                            _objSugarBottle.transform.position = Vector3.Slerp(_objSugarBottle.transform.position, pos, 25 * Time.deltaTime);
                        }
                        break;
                    case PhaseEnum.Milk:
                        {
                            if (finger.ScreenDelta.y < 0)
                            {
                                var newX = _owner.LevelObjs[Consts.ITEM_POT].transform.eulerAngles.x + finger.ScreenDelta.y * 0.5f - 360; 
                                newX = Mathf.Clamp(newX, -60, -20);
                                _owner.LevelObjs[Consts.ITEM_POT].transform.eulerAngles = new Vector3(newX, _owner.LevelObjs[Consts.ITEM_POT].transform.eulerAngles.y, _owner.LevelObjs[Consts.ITEM_POT].transform.eulerAngles.z);
                            }
                        }
                        break;

                }
            }
        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            switch (_prePhase)
            {
                case PhaseEnum.Sugar:
                    {
                        if (_bHolding)
                        {
                            _bHolding = false;
                            if (Mathf.Abs(_objSugarBottle.transform.position.x - _v3BowlPos.x) < 5 &&
                                _objSugarBottle.transform.position.z > _v3BowlPos.z - 5)
                            {
                                GuideManager.Instance.StopGuide();
                                _prePhase = PhaseEnum.Waiting;
                                _objSugarBottle.transform.DORotate(new Vector3(0, 13, -170), 0.5f);
                                _objSugarBottle.transform.DOMove(_v3BowlPos + new Vector3(-1.3f, 12, 0), 0.5f).OnComplete(HandleSugar);
                            }
                            else
                            {
                                _objSugarBottle.transform.DOMove(_v3SugarBottle, 0.5f);
                            }
                        }
                    }
                    break;
                case PhaseEnum.Milk:
                    {
                        if (_bHolding)
                        {
                            _bHolding = false;
                            _owner.LevelObjs[Consts.ITEM_POT].transform.DORotate(_v3PourAngle, 0.5f);
                            if (_asPotWater != null)  
                            {          
                                _asPotWater.Pause();
                            }
                            //if (_nPastaIndex < _owner.PastaPieces.Count)
                            //    GuideManager.Instance.SetGuideSingleDir(_v3PotPos + new Vector3(0, 7, 4), _v3PotPos + new Vector3(0, 0, 4));
                        }

                    }
                    break;
            }

        }


        void HandleSugar()
        {
            _objSugar.GetComponentInChildren<MeshRenderer>().material.DOColor(new Color(1, 1, 1, 0.5f), 0.5f);
            var finishEff = EffectCenter.Instance.SpawnEffect("Sugar1", Vector3.zero, Vector3.zero);
            DoozyUI.UIManager.PlaySound("65撒糖", _v3SugarBottle);
            finishEff.transform.SetParent(_owner.LevelObjs[Consts.ITEM_ICBOWLBIG].transform);
            finishEff.transform.localPosition = new Vector3(-0.45f, 7, 0);
            _objSugarBottle.transform.DOShakePosition(0.5f, Vector3.up, 5, 30).OnComplete(() =>
            {    
                _objSugar.GetComponentInChildren<MeshRenderer>().material.DOColor(new Color(1, 1, 1, 1), 0.5f);
                _objSugarBottle.transform.DOShakePosition(0.5f, Vector3.up, 5, 30).OnComplete(() =>
                {
                    _objSugarBottle.transform.DORotate(Vector3.zero, 0.5f);
                    _objSugarBottle.transform.DOMove(_v3SugarBottle + Vector3.left * 50, 0.5f).OnComplete(HandleCreamPainter);
                });
                finishEff = EffectCenter.Instance.SpawnEffect("Sugar1", Vector3.zero, Vector3.zero);
                DoozyUI.UIManager.PlaySound("65撒糖", _v3SugarBottle);
                finishEff.transform.SetParent(_owner.LevelObjs[Consts.ITEM_ICBOWLBIG].transform);
                finishEff.transform.localPosition = new Vector3(-0.45f, 7, 0);
            });

        }

        void HandleCreamPainter()
        {
            _objSugarBottle.SetPos(Vector3.one * 500);
            _objPipingBag.SetPos(_v3PipingBag + Vector3.up * 50);
            _objPipingBag.transform.DOMove(_v3PipingBag, 0.5f).OnComplete(() =>
            {
                _prePhase = PhaseEnum.Cream;
                GuideManager.Instance.SetGuideClick(_v3PipingBag + new Vector3(-1.5f, 2f, 0));
            });
        }

        void HandleMilkPot()
        {
            _bHolding = false;
            _objPot.SetAngle(_v3PourAngle);
            _objPot.transform.DOMove(_v3PourMilkPos, 1f).OnComplete(()=> {
                GuideManager.Instance.SetGuideSingleDir(_v3PourMilkPos + new Vector3(-7, 7, 4), _v3PourMilkPos + new Vector3(-7, 0, 4));
                _prePhase = PhaseEnum.Milk;
                _objMilkPiece.transform.SetParent(_owner.LevelObjs[Consts.ITEM_ICBOWLBIG].transform);
                _objMilkPiece.SetLocalPos(Vector3.up * 0.5f);
            });
        }

        void HandlePreFinish()
        {
            GuideManager.Instance.StopGuide();
            _milkEff.transform.SetParent(null);
            _milkEff.ResetMaxTimeUseful();
            _bPouring = _bHolding = false;
            _trsPotMilk.DOLocalMove(Vector3.zero, 0.3f);
            _trsPotMilk.DOLocalRotate(Vector3.zero, 0.3f);
            _owner.LevelObjs[Consts.ITEM_POT].transform.DORotate(_v3PourAngle, 0.5f).OnComplete(() =>
            {
                _owner.LevelObjs[Consts.ITEM_POT].transform.DOMove(_v3PotOriginPos, 0.5f).OnComplete(() =>
                {
                    _owner.LevelObjs[Consts.ITEM_POT].SetPos(Vector3.one * 500);
                    _owner.LevelObjs[Consts.ITEM_POT].SetAngle(Vector3.zero);
                    StrStateStatus = "MixPreFinished";
                });
            });
        }
    }
}
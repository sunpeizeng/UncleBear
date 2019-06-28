using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    public class CupCakeStateIngredient : IngredientState<LevelCupCake>
    {
        GameObject _objConvRoot;
        GameObject _objHitting;
        GameObject _objCenterDecor;

        Vector3 _v3LastCakePos;
        Vector3 _v3CakePos = new Vector3(-14, 22.8f, -21.5f);
        Vector3 _v3CamPos = new Vector3(-14, 74.4f, 57);
        Vector3 _v3CamRot = new Vector3(30, 180, 0);
        Vector3 _v3Conveyor = new Vector3(-14, 46f, -21.5f);
        Vector3 _v3DecorLocalPos = new Vector3(27, -25.2f, 0);

        float _fPickDeltaX;
        float _fLastFingerDeltaX;
        bool _bMoving;
        bool _bGened;
        bool _bPicking;
        bool _bCanPick;
        float _fDeltaX = 9;
        int _nCakeIndex;
        int _nPickingId;
        List<GameObject> _lstDecorators;
        List<ConveyorItem> _lstItems;

        public CupCakeStateIngredient(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            _bGened = false;
            _nCakeIndex = 0;
            _nPickingId = -1;
            _bCanPick = _bPicking = _bMoving = _bGened = false;
            base.Enter(param);

            _owner.LevelObjs[Consts.ITEM_CAKECONVEYOR].SetPos(_v3Conveyor + Vector3.up * 20);
            GenIngredients();
            CameraManager.Instance.DoCamTween(_v3CamPos, _v3CamRot, 0.5f, () => {
                _owner.LevelObjs[Consts.ITEM_CAKECONVEYOR].transform.DOMoveY(_v3Conveyor.y, 0.5f).SetEase(Ease.OutBack).OnComplete(() => {
                    _owner.LevelObjs[Consts.ITEM_OVENPLATE].transform.DOMoveX(-78, 0.5f).SetDelay(0.5f).OnComplete(() =>
                    {
                        SetCakeToDecor();
                    });
                });
            });
        }

        public override string Execute(float deltaTime)
        {
            //if (!_bGened && _lstDecorators.Count == _lstItems.Count * 2)
            //    _bGened = true;
            CheckDecorPos();
            CheckDecorFocus();
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            GameObject.Destroy(_objConvRoot);
            _objConvRoot = null;
            base.Exit();
        }

        //蛋糕进场
        void SetCakeToDecor()
        {
            _bGened = false;
            if (_nCakeIndex < _owner.Cupcakes.Count)
            {
                _v3LastCakePos = _owner.Cupcakes[_nCakeIndex].transform.position;
                _owner.Cupcakes[_nCakeIndex].transform.DOMove(_v3CakePos, 0.5f).OnComplete(() => { _bGened = true; });
            }
            else
            {
                GuideManager.Instance.StopGuide();
                _owner.LevelObjs[Consts.ITEM_CAKECONVEYOR].transform.DOMoveY(200, 1.5f).SetDelay(0.5f).OnComplete(() =>
                {
                    _bGened = false;
                    StrStateStatus = "IngredientOver";
                });
            }
        }

        Material GetMatFromCtrller(Material[] mats, string id)
        {
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].name.Contains(id))
                    return mats[i];
            }
            return null;
        }

        //生成整个机器
        void GenIngredients()
        {
            _objConvRoot = new GameObject("Root");
            _objConvRoot.transform.SetParent(_owner.LevelObjs[Consts.ITEM_CAKECONVEYOR].transform);
            _objConvRoot.SetLocalPos(Vector3.zero);

            _lstDecorators = new List<GameObject>();
            _lstItems = SerializationManager.LoadFromCSV<ConveyorItem>("Configs/CupcakeIngredItems");
            List<string> itemIDs = new List<string>();
            for (int i = 0; i < _lstItems.Count * 2; i++)
            {
                int doubleId = 0;
                if (i >= _lstItems.Count)
                {
                    doubleId = i - _lstItems.Count;
                }
                else
                {
                    doubleId = i;
                    itemIDs.Add(_lstItems[i].ID);
                }

                var newDecor = GameObject.Instantiate(_owner.LevelObjs[Consts.ITEM_CAKEDECOR]) as GameObject;
                newDecor.transform.FindChild("Icon").GetComponent<Renderer>().material = GetMatFromCtrller(newDecor.GetComponent<CupcakeMatsCtrller>().matIngreds, _lstItems[doubleId].ID);
                newDecor.name = i.ToString();
                newDecor.transform.SetParent(_objConvRoot.transform);
                //newDecor.transform.localPosition = _v3DecorLocalPos + Vector3.right * 100;
                newDecor.transform.localPosition = _v3DecorLocalPos + Vector3.left * _fDeltaX * i;
                //, 1f).OnComplete(() => {
                _lstDecorators.Add(newDecor);

                //if (Mathf.Abs(newDecor.transform.position.x - _v3CakePos.x) < 0.1f)
                //{
                //    _objCenterDecor = newDecor;
                //    _objCenterDecor.transform.FindChild("Stick").DOScaleZ(1.2f, 0.3f).SetEase(Ease.InSine);
                //    _objCenterDecor.transform.FindChild("Icon").DOLocalMoveY(-12.7f, 0.3f).SetEase(Ease.InSine);
                //}
                //});

            }

            DishManager.Instance.PickRandomFavorItem(itemIDs);
        }


        protected override void OnFingerDown(LeanFinger finger)
        {
            if (!_bGened || _bPicking)
                return;
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.name == "Icon" && hit.collider.transform.parent != null)
                {
                    //选中一个了
                    _objHitting = hit.collider.transform.gameObject;
                }
            }
        }

        //拖动传送带的逻辑,先写在这,不确定是否通用
        protected override void OnFingerSet(LeanFinger finger)
        {
            if (!_bGened)
                return;
            if (!_bPicking && !_bMoving)
            {
                if (Mathf.Abs(finger.ScreenDelta.x) > 2)
                {
                    MoveConveyor();

                    _fLastFingerDeltaX = finger.ScreenDelta.x;
                    Vector3 desirePos = _objConvRoot.transform.localPosition + new Vector3(-_fLastFingerDeltaX * 0.2f, 0, 0);
                    //Debug.Log(desirePos.x + " " + desirePos.x % _fDeltaX);
                    if (Mathf.Abs(desirePos.x % _fDeltaX) != 0)
                    {
                        if (_fLastFingerDeltaX < 0)
                        {
                            if (desirePos.x > 0)
                                desirePos.x += _fDeltaX - desirePos.x % _fDeltaX;
                            else
                                desirePos.x -= desirePos.x % _fDeltaX;

                        }
                        else if (_fLastFingerDeltaX > 0)
                        {
                            if (desirePos.x < 0)
                                desirePos.x -= desirePos.x % _fDeltaX + _fDeltaX;
                            else
                                desirePos.x -= desirePos.x % _fDeltaX;
                        }
                    }

                    _objConvRoot.transform.DOKill(false);
                    _objConvRoot.transform.DOLocalMove(desirePos, 0.5f).OnComplete(() => {
                        _bMoving = false;
                    });
                }
            }
        }

        //超出屏幕的放到另一头
        void CheckDecorPos()
        {
            if (!_bGened || !_bMoving)
                return;
            _lstDecorators.ForEach(p =>
            {
                if (_fLastFingerDeltaX < 0 && p.transform.position.x > 18f)
                {
                    p.transform.position = _lstDecorators[_lstDecorators.Count - 1].transform.position - new Vector3(_fDeltaX, 0, 0);
                    _lstDecorators.Remove(p);
                    _lstDecorators.Add(p);
                }
                else if (_fLastFingerDeltaX > 0 && p.transform.position.x < -47f)
                {
                    p.transform.position = _lstDecorators[0].transform.position + new Vector3(_fDeltaX, 0, 0);
                    _lstDecorators.Remove(p);
                    _lstDecorators.Insert(0, p);
                }
            });
        }

        Animation _animCenter;
        //检查在蛋糕正上方的
        void CheckDecorFocus()
        { 
            if (_bPicking || !_bGened)
                return;
            if (!_bMoving && _objCenterDecor == null)
            {
                _objCenterDecor = _lstDecorators.Find(p => Mathf.Abs(p.transform.position.x - _v3CakePos.x) < 0.1f);
                if (_objCenterDecor != null)
                {
                    GuideManager.Instance.SetGuideClick(_objCenterDecor.transform.position + new Vector3(-1.5f, 9.2f, 0f));

                    var decorId = int.Parse(_objCenterDecor.name);
                    DishManager.Instance.JudgeIngredInFavor(_lstItems[decorId >= _lstItems.Count ? decorId - _lstItems.Count : decorId].ID);

                    _animCenter = _objCenterDecor.GetComponent<Animation>();
                    PlayDecorAnimation(true);
                    //_objCenterDecor.transform.FindChild("Stick").DOScaleZ(1.2f, 0.3f).SetEase(Ease.InSine);
                    //_objCenterDecor.transform.FindChild("Icon").DOLocalMoveY(-12.7f, 0.3f).SetEase(Ease.InSine).OnComplete(() => { _bCanPick = true; });
                }
            }

            if (_animCenter["anim_machineDown"].normalizedTime > 1 && _objCenterDecor != null && _bCanPick == false)
                _bCanPick = true;
        }

        void PlayDecorAnimation(bool state)
        {
            if (state)
            {
                _animCenter["anim_machineDown"].speed = 1;
                _animCenter["anim_machineDown"].normalizedTime = 0;
                _animCenter.Play("anim_machineDown");
            }
            else
            {
                _animCenter["anim_machineDown"].speed = -1;
                if (_animCenter["anim_machineDown"].normalizedTime > 1)
                    _animCenter["anim_machineDown"].normalizedTime = 1;
                _animCenter.Play("anim_machineDown");
            }
        }

        void SetDecorPickCake(Transform trsDecor)
        {
            
            _owner.Cupcakes[_nCakeIndex].GetComponent<Animation>().Play("anim_cakeShake");
            _animCenter.CrossFade("anim_machineHit", 0.2f);
            DoozyUI.UIManager.PlaySound("59杯蛋糕-模具下压", _objCenterDecor.transform.position, false, 1, 0.5f);

            //trsDecor.FindChild("Stick").DOScaleZ(1.9f, 0.3f).SetEase(Ease.OutSine);
            //trsDecor.FindChild("Icon").DOLocalMoveY(-17f, 0.3f).SetEase(Ease.OutSine).OnComplete(() => {

            var id = _nPickingId >= _lstItems.Count ? _nPickingId - _lstItems.Count : _nPickingId;
                //Debug.Log(id + " " + _lstItems.Count);
                var newStuff = GameObject.Instantiate(_lstItems[id].Prefab) as GameObject;
                newStuff.name = _lstItems[id].ID;
                newStuff.transform.SetParent(_owner.Cupcakes[_nCakeIndex].transform);
                newStuff.SetLocalPos(Vector3.zero);
                newStuff.SetAngle(new Vector3(-90, 90, 0));
                newStuff.transform.localScale = Vector3.zero;
                DishManager.Instance.IngredsInDish.Add(newStuff.name);
                DoozyUI.UIManager.PlaySound("30杯蛋糕加配料", newStuff.transform.position);
                newStuff.transform.DOScale(Vector3.one, 0.3f).SetDelay(1.3f).OnComplete(() => {
                    DoozyUI.UIManager.PlaySound("58蝴蝶面杯蛋糕-模具抬起", _objCenterDecor.transform.position);
                    LevelManager.Instance.CallWithDelay(() =>
                    {
                        CameraManager.Instance.BackToLastPos(1f, () =>
                        {
                            _bPicking = false;
                            _owner.Cupcakes[_nCakeIndex].transform.DOMoveX(_v3CakePos.x + 60, 0.5f);
                            _nCakeIndex++;
                            SetCakeToDecor();

                            _objCenterDecor = null;
                            CheckDecorFocus();
                        });
                    }, 0.5f);
                });

                //trsDecor.FindChild("Stick").DOScaleZ(1f, 0.3f).SetEase(Ease.InSine).SetDelay(0.5f);
                //trsDecor.FindChild("Icon").DOLocalMoveY(-11.5f, 0.3f).SetEase(Ease.InSine).SetDelay(0.5f).OnComplete(()=> {
                   
                //});
            //});
        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            if (!_bGened || _bPicking)
                return;

            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.name == "Icon" && hit.collider.transform.parent != null)
                {
                    //确认是选中一个了
                    if (_objHitting == hit.collider.gameObject)
                    {
                        if (_bCanPick && _objCenterDecor != null && _objHitting.transform.IsChildOf(_objCenterDecor.transform))
                        {
							GuideManager.Instance.StopGuide ();
                            _nPickingId = int.Parse(hit.collider.transform.parent.name);
                            _bPicking = true;
                            CameraManager.Instance.DoCamTween(new Vector3(-14, 58, 27.7f), 0.5f, ()=> {
                                SetDecorPickCake(hit.collider.transform.parent);
                            });
                        }
                        else
                        {
                            _fLastFingerDeltaX = hit.collider.transform.parent.position.x - _v3CakePos.x;
                            if (_fLastFingerDeltaX != 0)
                            {
                                MoveConveyor();
                                Vector3 desirePos = _objConvRoot.transform.localPosition + new Vector3(-_fLastFingerDeltaX, 0, 0);
                                _objConvRoot.transform.DOKill(false);
                                _objConvRoot.transform.DOLocalMove(desirePos, 0.5f).OnComplete(() =>
                                {
                                    _bMoving = false;
                                });
                            }
                        }
                    }
                }
            }

            _objHitting = null;
        }

        void MoveConveyor()
        {
            if (_objCenterDecor != null)
            {
                _objCenterDecor.transform.DOKill();
                PlayDecorAnimation(false);
                //_objCenterDecor.transform.FindChild("Stick").DOScaleZ(1f, 0.3f).SetEase(Ease.InSine);
                //_objCenterDecor.transform.FindChild("Icon").DOLocalMoveY(-11.5f, 0.3f).SetEase(Ease.InSine);
                _objCenterDecor = null;
            }
            _bCanPick = false;
            _bMoving = true;
        }

    }
}

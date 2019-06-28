using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Touch;

namespace UncleBear
{
    public class BurgerStateShakePotato : State<LevelBurger>
    {
        Animation _animBag;
        Animation _animRoll;
        Vector3 _v3Conveyor = new Vector3(-55, 22.5f, -117);

        Vector3 _v3CamPos = new Vector3(-14, 105, 70);
        Vector3 _v3CamRot = new Vector3(40, 180, 0);

        Vector3 _v3PanPos = new Vector3(-21, 45, -16);
        Vector3 _v3BagPos = new Vector3(-14, 25, -17);
        Vector3 _v3BagPourPos = new Vector3(-23.5f, 40, -17);
        Vector3 _v3PlatePos = new Vector3(-14, 22.5f, -17);

        bool _bReadyShake;
        bool _bShaking;
        List<Transform> _lstTrsChips = new List<Transform>();

        int _nShakeCount;
        int _nShakeLimit = 100;

        public BurgerStateShakePotato(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("Shake");
            base.Enter(param);

            _owner.ObjChipsPlate = GameObject.Instantiate(_owner.LevelObjs[Consts.ITEM_PLATE]) as GameObject;
            _owner.ObjChipsPlate.transform.FindChild("Mesh").localScale = new Vector3(1.6f, 1.6f, 1);//Vector3.one * 1.2f;
            _owner.ObjChipsPlate.transform.FindChild("Mesh").GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector2(0, 0.5f);

            _nShakeCount = 0;
            _bShaking = _bReadyShake = false;
            CameraManager.Instance.DoCamTween(_v3CamPos, _v3CamRot, 1);
            _animBag = _owner.LevelObjs[Consts.ITEM_SHAKEBAG].GetComponent<Animation>();
            _animRoll = _owner.LevelObjs[Consts.ITEM_SHAKEBAG].transform.FindChild("Mesh").GetComponent<Animation>();
            _owner.LevelObjs[Consts.ITEM_SHAKEBAG].transform.DOMove(_v3BagPos, 0.5f).OnComplete(() =>
            {
                _owner.LevelObjs[Consts.ITEM_PAN].transform.DOMove(_v3PanPos, 0.5f);
                _owner.LevelObjs[Consts.ITEM_PAN].transform.DORotate(new Vector3(0, 0, -80), 0.5f).OnComplete(()=> {
                    _lstTrsChips = _owner.ObjChipsRoot.transform.GetChildTrsList();
                    for (int i = 0; i < _lstTrsChips.Count; i++)
                    {
                        _lstTrsChips[i].SetParent(_owner.LevelObjs[Consts.ITEM_SHAKEBAG].transform);
                        _lstTrsChips[i].transform.DORotate(new Vector3(0, 90, 0), 0.3f).SetDelay(0.05f * i);
                        _lstTrsChips[i].DOLocalPath(new Vector3[] { new Vector3(0, 12, 0), new Vector3(0, 3, 0) }, 0.5f).SetDelay(0.05f * i);
                    }
                });
                //此处才算正式开始
                _owner.LevelObjs[Consts.ITEM_PAN].transform.DOMove(_v3PanPos + Vector3.up * 20, 0.5f).SetDelay(1.5f).OnComplete(() => {
                    _owner.LevelObjs[Consts.ITEM_PAN].SetPos(Vector3.one * 500);
                    _bReadyShake = true;
                    _animRoll.SampleAnim("close&open", 0);
                    _animRoll["close&open"].speed = 1;

                    GuideManager.Instance.SetGuideFree(_v3BagPos);
                });
            });
        }

        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            _lstTrsChips.Clear();
            base.Exit();
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            if (!_bReadyShake || _bShaking ||  _nShakeCount >= _nShakeLimit)
                return;
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.gameObject == _owner.LevelObjs[Consts.ITEM_SHAKEBAG])
            {
                _bShaking = true;
                //_owner.LevelObjs[Consts.ITEM_SHAKEBAG].transform.DOShakeRotation(1).OnComplete(() =>
                //{
                //    // (2f, null, SpiralMode.ExpandThenContract, 2, 15)
                //    _bShaking = false;
                //    _nShakeCount += 1;
                //    if (_nShakeCount >= _nShakeLimit)
                //    {
                //        _owner.ObjChipsPlate.transform.DOMove(_v3PlatePos, 0.5f).OnComplete(() => {
                //            _owner.LevelObjs[Consts.ITEM_SHAKEBAG].transform.DOMove(_v3BagPourPos, 1);
                //            _owner.LevelObjs[Consts.ITEM_SHAKEBAG].transform.DORotate(new Vector3(0, 0, -120), 1).OnComplete(()=> {
                //                LevelManager.Instance.StartCoroutine(PourOutChips());
                //            });
                //        });

                //    }
                //});
            }
        }

        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_bShaking)
            {
                if(finger.ScreenDelta != Vector2.zero)
                {
                    if (!_animBag.isPlaying && finger.GetSnapshotScreenDelta(0.5f).magnitude > LeanTouch.Instance.SwipeThreshold)
                    {
                        _animBag.Play("anim_shakeBag");
                        _nShakeCount += 10;
                        DoozyUI.UIManager.PlaySound("32摇摇乐", _owner.LevelObjs[Consts.ITEM_SHAKEBAG].transform.position);

                        if (_nShakeCount >= _nShakeLimit)
                        {
                            GuideManager.Instance.StopGuide();
                            _owner.ObjChipsPlate.transform.DOMove(_v3PlatePos, 0.5f).OnComplete(() =>
                            {
                                _bShaking = false;
                                _animRoll.SampleAnim("close&open", 1);
                                _animRoll["close&open"].speed = -1;
                                _owner.LevelObjs[Consts.ITEM_SHAKEBAG].transform.DOMove(_v3BagPourPos, 1);
                                _owner.LevelObjs[Consts.ITEM_SHAKEBAG].transform.DORotate(new Vector3(0, 0, -120), 1).OnComplete(() =>
                                {
                                    LevelManager.Instance.StartCoroutine(PourOutChips());
                                });

                                return;
                            });
                        }
                    }

                    var pos = GameUtilities.GetFingerTargetWolrdPos(finger, _owner.LevelObjs[Consts.ITEM_SHAKEBAG]);
                    pos.z = _v3BagPos.z;

                    _owner.LevelObjs[Consts.ITEM_SHAKEBAG].SetPos(pos);

                    if (_owner.LevelObjs[Consts.ITEM_SHAKEBAG].transform.position.y < 24f)
                        _owner.LevelObjs[Consts.ITEM_SHAKEBAG].SetPos(new Vector3(_owner.LevelObjs[Consts.ITEM_SHAKEBAG].transform.position.x, 24, _owner.LevelObjs[Consts.ITEM_SHAKEBAG].transform.position.z));
                }
            }
        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            if (_nShakeCount < _nShakeLimit)
                GuideManager.Instance.SetGuideFree(_owner.LevelObjs[Consts.ITEM_SHAKEBAG].transform.position);
            _bShaking = false;
        }


        IEnumerator PourOutChips()
        {
            for (int i = 0; i < _lstTrsChips.Count; i++)
            {
                _lstTrsChips[i].GetComponent<MeshRenderer>().material.SetFloat("_Slider_Val", 1);
                _lstTrsChips[i].SetParent(_owner.ObjChipsPlate.transform);
                _lstTrsChips[i].localPosition = new Vector3(0, 9, 0);
                _lstTrsChips[i].GetComponent<Rigidbody>().isKinematic = false;
                yield return new WaitForSeconds(0.15f);
            }
            _owner.LevelObjs[Consts.ITEM_CONVEYOR].SetPos(_v3Conveyor);
            DoozyUI.UIManager.PlaySound("8成功");
            _owner.LevelObjs[Consts.ITEM_SHAKEBAG].transform.DOMoveX(-78, 1.5f).OnComplete(() =>
            {
                StrStateStatus = "PotatoShakeOver";
            });
        }
    }
}

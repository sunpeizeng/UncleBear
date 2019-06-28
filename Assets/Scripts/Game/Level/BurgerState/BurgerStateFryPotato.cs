using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class BurgerStateFryPotato : State<LevelBurger>
    {
        bool _bChipsInPan;
        bool _bChipsReady;

        PanCtrl _panCtrl;
        Vector3 _v3CamPos = new Vector3(2f, 72.5f, -53);
        Vector3 _v3PanPos = new Vector3(4f, 23.9f, -99.5f);
        Vector3 _v3BowlPos = new Vector3(-7.4f, 24.8f, -92f);

        List<Transform> _lstTrsChips = new List<Transform>();

        public BurgerStateFryPotato(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("Fry");

            _bChipsInPan = _bChipsReady = false;
            CameraManager.Instance.DoCamTween(_v3CamPos, 1f);

            _owner.LevelObjs[Consts.ITEM_PAN].SetPos(_v3PanPos);
            _owner.LevelObjs[Consts.ITEM_BOWL].transform.DOMove(_v3BowlPos, 1f).OnComplete(()=> {
                GuideManager.Instance.SetGuideSingleDir(_v3BowlPos, _v3PanPos);
                Lean.Touch.LeanTouch.OnFingerSwipe += OnFingerSwipe;
            });

            _panCtrl = _owner.LevelObjs[Consts.ITEM_PAN].AddMissingComponent<PanCtrl>();
            _panCtrl.enabled = true;
            //_panCtrl.RegisterObject(_owner.ObjPotato, OnPanFinish);

            base.Enter(param);
           
        }

        public override string Execute(float deltaTime)
        {
            if (_lstTrsChips.Count > 0 && _panCtrl.enabled)
            {
                for (int i = 0; i < _lstTrsChips.Count; i++)
                {
                    if (_lstTrsChips[i] != null)
                    {
                        if (Vector3.Distance(new Vector3(_lstTrsChips[i].position.x, _owner.LevelObjs[Consts.ITEM_PAN].transform.position.y, _lstTrsChips[i].position.z), _owner.LevelObjs[Consts.ITEM_PAN].transform.position) > 4)
                        {
                            var limit = (_lstTrsChips[i].position - _owner.LevelObjs[Consts.ITEM_PAN].transform.position).normalized * 4;
                            limit.y = 0;
                            _lstTrsChips[i].position = new Vector3(_owner.LevelObjs[Consts.ITEM_PAN].transform.position.x + limit.x, _lstTrsChips[i].position.y, _owner.LevelObjs[Consts.ITEM_PAN].transform.position.z + limit.z);
                        }
                    }
                    else
                        _lstTrsChips.RemoveAt(i);
                }
            }
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            _lstTrsChips.Clear();
            if (_panCtrl != null)
                _panCtrl.Stop();
            base.Exit();
            Lean.Touch.LeanTouch.OnFingerSwipe -= OnFingerSwipe;
        }

        void OnPanFinish(GameObject objSrc)
        {
            if (_panCtrl != null)
                _panCtrl.enabled = false;
        }

        void OnFingerSwipe(Lean.Touch.LeanFinger finger)
        {
            if (_bChipsInPan)
                return;
            _bChipsInPan = true;
            GuideManager.Instance.StopGuide();
            _owner.LevelObjs[Consts.ITEM_BOWL].SetRigidBodiesKinematic(true);
            _owner.LevelObjs[Consts.ITEM_BOWL].transform.DOMove(_v3PanPos + new Vector3(2, 12, 0), 1f).OnComplete(()=> {
                _owner.LevelObjs[Consts.ITEM_BOWL].transform.DORotate( new Vector3(8, 24, 145), 0.5f).OnComplete(() =>
                {
                    _owner.LevelObjs[Consts.ITEM_BOWL].SetRigidBodiesKinematic(false);
                    if (_owner.ObjChipsRoot == null)
                        _owner.ObjChipsRoot = new GameObject("PotatoChips");
                    _owner.ObjChipsRoot.SetPos(_v3PanPos + new Vector3(0, 1, 0));
                    _owner.LevelObjs[Consts.ITEM_BOWL].transform.GetChildTrsList().ForEach(p =>
                    {
                        if (p.name.Contains("Chips"))
                        {
                            p.SetParent(_owner.ObjChipsRoot.transform);
                            _lstTrsChips.Add(p);
                        }
                    });
                    _owner.LevelObjs[Consts.ITEM_BOWL].transform.DOMove(_owner.LevelObjs[Consts.ITEM_BOWL].transform.position + Vector3.up * 50, 0.5f).OnComplete(()=> {
                        _owner.LevelObjs[Consts.ITEM_BOWL].SetPos(Vector3.one * 500);
                    });

                    _panCtrl.RegisterObject(_owner.ObjChipsRoot, OnPanFinish, OnChipsFriedOk, false);
                });
            });

        }

        void OnChipsFriedOk(bool isLimit)
        {
            StrStateStatus = "ChipsFriedOver";
        }
    }
}

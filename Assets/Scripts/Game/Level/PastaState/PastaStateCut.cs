using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class PastaStateCut : State<LevelPasta>
    {
        enum CutPhase
        {
            Ready,
            Cutting,
            Waiting,
            Finished,
            Over,
        }

        Vector3 _v3CommmonAngle = new Vector3(0, 90, 0);
        Vector3 _v3BoardPos = new Vector3(-84.5f, 22.5f, -74.4f);
        Vector3 _v3MakerPos = new Vector3(-85, 24.5f, -74.4f);
        Vector3 _v3PastaBasePos = new Vector3(-85, 24.5f, -74.4f);

        CutPhase _ePhase;

        int _nPastaIndex;
        int _nPastaCount = 6;
        GameObject[] _objPastaOrigins;
        Animation _animMaker;

        public PastaStateCut(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            CameraManager.Instance.DoCamTween(new Vector3(-52f, 60f, -74.4f), new Vector3(45, 270, 0), 0.3f);


            //Debug.Log("Sausage");
            base.Enter(param);
            _objPastaOrigins = new GameObject[_nPastaCount];
            //_objPastaOrigin = _owner.LevelObjs[Consts.ITEM_PASTAORIGIN];
            

            _ePhase = CutPhase.Waiting;
            _nPastaIndex = 0;
            _owner.LevelObjs[Consts.ITEM_FLOURPIECE].transform.SetParent(_owner.LevelObjs[Consts.ITEM_CHOPBOARD].transform.FindChild("Mesh"));
            _owner.LevelObjs[Consts.ITEM_FLOURPIECE].SetLocalPos(new Vector3(0, 0, 1.98f));
            _owner.LevelObjs[Consts.ITEM_FLOURPIECE].SetAngle(Vector3.zero);
            _owner.LevelObjs[Consts.ITEM_CHOPBOARD].SetPos(_v3BoardPos);
            _owner.LevelObjs[Consts.ITEM_CHOPBOARD].SetAngle(_v3CommmonAngle);
            _owner.LevelObjs[Consts.ITEM_PASTAMAKER].SetPos(_v3MakerPos + Vector3.up * 50);
            _owner.LevelObjs[Consts.ITEM_PASTAMAKER].SetAngle(_v3CommmonAngle);
            _animMaker = _owner.LevelObjs[Consts.ITEM_PASTAMAKER].GetComponent<Animation>();

            for (int i = 0; i < _nPastaCount; i++)
            {
                _objPastaOrigins[i] = GameObject.Instantiate(_owner.LevelObjs[Consts.ITEM_PASTAORIGIN]) as GameObject;
                _objPastaOrigins[i].transform.localEulerAngles = new Vector3(-90, 0, 0);
                var newPastaBase = GameObject.Instantiate(_owner.LevelObjs[Consts.ITEM_PASTABASE]) as GameObject;
                _owner.PastaPieces.Add(newPastaBase);
            }

            //首次进入摆好位置
            _owner.LevelObjs[Consts.ITEM_PASTAMAKER].transform.DOMove(_v3MakerPos, 1f);
            EnterPasta();
        }

        public override string Execute(float deltaTime)
        {
            if (_ePhase == CutPhase.Cutting && !_animMaker.isPlaying)
            {
                _ePhase = CutPhase.Waiting;
                CameraManager.Instance.BackToLastPos(0.5f);
                _objPastaOrigins[_nPastaIndex].transform.DOMoveY(_v3PastaBasePos.y + 2, 0.2f).SetDelay(0.2f).OnComplete(() =>
                {
                    _objPastaOrigins[_nPastaIndex].transform.DOMove(_v3PastaBasePos + new Vector3(50, 3, 0), 0.3f).OnComplete(() =>
                    {
                        _objPastaOrigins[_nPastaIndex].SetPos(Vector3.one * 500);
                    });
                    _owner.PastaPieces[_nPastaIndex].transform.DOMove(_v3PastaBasePos - new Vector3(0, 0, 30), 0.7f).SetDelay(0.2f).OnComplete(() =>
                    {
                        _nPastaIndex += 1;
                        EnterPasta();
                    });

                    if (_nPastaIndex == _nPastaCount - 1)
                    {
                        DoozyUI.UIManager.PlaySound("8成功",Vector3.zero, false, 1, 0.2f);
                    }
                });
            }

            if (_ePhase == CutPhase.Finished)
            {
                _ePhase = CutPhase.Over;
                _animMaker.transform.DOMoveY(_v3MakerPos.y + 50, 1f).OnComplete(() =>
                {
                    StrStateStatus = "CutOver";
                });
            }

            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
        }

        void StartCutting()
        {
            DoozyUI.UIManager.PlaySound("57蝴蝶面-模具下压", _owner.PastaPieces[_nPastaIndex].transform.position, false, 1, 0.3f);
            _animMaker.Play("anim_tool");
            _owner.PastaPieces[_nPastaIndex].GetComponentInChildren<Animation>().Play("anim_pasta");
            CameraManager.Instance.DoCamTween(new Vector3(-60, 51, -74.4f), 0.5f, ()=> {
                DoozyUI.UIManager.PlaySound("58蝴蝶面杯蛋糕-模具抬起", _animMaker.transform.position, false, 1, 1f);
            });
        }

        void EnterPasta()
        {
            if (_nPastaIndex < _nPastaCount)
            {
                if (_nPastaIndex < _nPastaCount - 1)
                {
                    _owner.PastaPieces[_nPastaIndex + 1].transform.DOMove(_v3PastaBasePos + Vector3.forward * 7, 1f);
                    _objPastaOrigins[_nPastaIndex + 1].transform.DOMove(_v3PastaBasePos + Vector3.forward * 7, 1f);
                }

                _owner.PastaPieces[_nPastaIndex].transform.DOMove(_v3PastaBasePos, 0.5f);
                _objPastaOrigins[_nPastaIndex].transform.DOMove(_v3PastaBasePos, 0.5f).OnComplete(() =>
                {
                    GuideManager.Instance.SetGuideClick(_v3MakerPos + new Vector3(0, 3, 1));
                    _ePhase = CutPhase.Ready;
                });

            }
            else
                _ePhase = CutPhase.Finished;
        }

        protected override void OnFingerDown(Lean.Touch.LeanFinger finger)
        {
            if (_ePhase != CutPhase.Ready || _nPastaIndex >= _nPastaCount)
                return;
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.gameObject == _animMaker.gameObject)
            {
                StartCutting();
                _ePhase = CutPhase.Cutting;
                GuideManager.Instance.StopGuide();
            }
        }
    }
}
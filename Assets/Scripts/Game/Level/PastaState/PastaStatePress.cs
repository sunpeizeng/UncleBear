using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class PastaStatePress : State<LevelPasta>
    {
        enum PressPhase
        {
            Ready,
            Waiting,
            Finished,
            Over,
        }

        float _fAnimLen;

        Vector3 _v3CommmonAngle = new Vector3(0, 90, 0);
        Vector3 _v3MakerPos = new Vector3(-85, 24.5f, -74.4f);
        Vector3 _v3PastaBasePos = new Vector3(-85, 24.5f, -74.4f);

        PressPhase _ePhase;

        int _nPastaIndex;
        int _nPastaCount = 6;

        public PastaStatePress(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("Sausage");
            base.Enter(param);

            _ePhase = PressPhase.Waiting;
            _nPastaIndex = 0;
            _fAnimLen = 0;

            _owner.PastaPieces.ForEach(p => p.SetPos(_v3PastaBasePos + Vector3.forward * 50));
            CameraManager.Instance.DoCamTween(new Vector3(-60, 51, -74.4f), 1, EnterPasta);
        }

        public override string Execute(float deltaTime)
        {
            if (_ePhase == PressPhase.Waiting && _fAnimLen > 0)
            {
                _fAnimLen -= deltaTime;
                if (_fAnimLen <= 0)
                {
                    _owner.PastaPieces[_nPastaIndex].transform.DOMove(_v3PastaBasePos - new Vector3(0, 0, 50), 0.5f).SetDelay(0.3f).OnComplete(() =>
                    {
                        _nPastaIndex += 1;
                        EnterPasta();
                    });
                    if (_nPastaIndex == _nPastaCount - 1)
                    {
                        DoozyUI.UIManager.PlaySound("8成功", Vector3.zero, false, 1, 0.2f);
                    }
                }
            }

            if (_ePhase == PressPhase.Finished)
            {
                _ePhase = PressPhase.Over;
                StrStateStatus = "PressOver";
            }

            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
        }

        void EnterPasta()
        {
            if (_nPastaIndex < _nPastaCount)
            {
                if (_nPastaIndex < _nPastaCount - 1)
                {
                    _owner.PastaPieces[_nPastaIndex + 1].SetAngle(_v3CommmonAngle);
                    _owner.PastaPieces[_nPastaIndex + 1].transform.DOMove(_v3PastaBasePos + Vector3.forward * 7, 0.5f).SetDelay(0.3f);
                }
               
                _owner.PastaPieces[_nPastaIndex].SetAngle(_v3CommmonAngle);
                _owner.PastaPieces[_nPastaIndex].transform.DOMove(_v3PastaBasePos, 0.5f).OnComplete(() =>
                {
                    GuideManager.Instance.SetGuideClick(_v3PastaBasePos + new Vector3(0, -1, 1));
                    _ePhase = PressPhase.Ready;
                });

            }
            else
                _ePhase = PressPhase.Finished;
        }

        protected override void OnFingerDown(Lean.Touch.LeanFinger finger)
        {
            if (_ePhase != PressPhase.Ready || _nPastaIndex >= _nPastaCount)
                return;
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.gameObject == _owner.PastaPieces[_nPastaIndex])
            {
                GuideManager.Instance.StopGuide();

                DoozyUI.UIManager.PlaySound("63蝴蝶面-面片手指点击", _owner.PastaPieces[_nPastaIndex].transform.position);
                Animation anim = _owner.PastaPieces[_nPastaIndex].GetComponentInChildren<Animation>();
                anim.Play("anim_farfalle");
                anim["anim_farfalle"].speed = 1.5f;
                _fAnimLen = anim["anim_farfalle"].length;

                _ePhase = PressPhase.Waiting;
            }
        }
    }
}
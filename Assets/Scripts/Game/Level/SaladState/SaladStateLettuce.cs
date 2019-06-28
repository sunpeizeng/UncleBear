using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    public class SaladStateLettuce : State<LevelSalad>
    {
        LeanFinger _fingerSwipe;

        float _fLeafProgress = 0;
        int _nLeafCount = 4;
        int _nLeafIndex;

        Vector3[] _v3LeafOffsets = { new Vector3(-3.8f, 2.3f, 0), new Vector3(-4f, 2.6f, -0.5f), new Vector3(-4.2f, 2.8f, 1f), new Vector3(-4.5f, 3f, 0) };
        Vector3 _v3LeafRotates = new Vector3(0, 0, -30);
        Vector3 _v3BowlPos = new Vector3(-42, 22.5f, -100);
        bool _bHitting;
        
        public SaladStateLettuce(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("lettuce");
            _fLeafProgress = 0;
            _nLeafIndex = 0;
            _owner.LevelObjs[Consts.ITEM_WASHER].transform.position = _v3BowlPos;
            _owner.LevelObjs[Consts.ITEM_LETTUCE].transform.position = _v3BowlPos + new Vector3(0, 10, 0);
            _bHitting = false;
            CameraManager.Instance.DoCamTween(new Vector3(-42.5f, 90, 3.5f), new Vector3(30, 180, 0));

            base.Enter(param);

            GuideManager.Instance.SetGuideSingleDir(_v3BowlPos + new Vector3(-2, 20, 0), _v3BowlPos + new Vector3(-2, 5, 0));
        }

        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            if (_bHitting || _nLeafIndex >= _nLeafCount)
                return;
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.gameObject == _owner.LevelObjs[Consts.ITEM_LETTUCE])
            {
                _bHitting = true;
                DoozyUI.UIManager.PlaySound("55撕菜", hit.point);
                if (_fingerSwipe == null)
                    _fingerSwipe = finger;
            }
        }

        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_bHitting)
            {
                var curLeaf = _owner.LevelObjs[Consts.ITEM_LETTUCE].transform.FindChild("Leaf_" + (_nLeafIndex + 1));
                if (finger.ScreenDelta.y < 0)
                    _fLeafProgress -= finger.ScreenDelta.y * 0.01f;
                curLeaf.GetComponent<Animation>().SampleAnim("Take 001", _fLeafProgress);
                if (_fLeafProgress >= 1)
                {
                    _fLeafProgress = 0;
                    curLeaf.GetComponent<Animation>().SampleAnim("Take 001", 0);
                    TweenLeaf(curLeaf);
                    _bHitting = false;
                }
            }
        }

        void TweenLeaf(Transform leafTrs)
        {
            leafTrs.transform.SetParent(_owner.LevelObjs[Consts.ITEM_WASHER].transform);
            leafTrs.DOMove(_owner.LevelObjs[Consts.ITEM_WASHER].transform.position + _v3LeafOffsets[_nLeafIndex], 0.5f);
            leafTrs.DORotate(_v3LeafRotates, 0.5f);
            //leafTrs.DOScale(0.8f, 0.5f);
            _owner.LevelObjs[Consts.ITEM_LETTUCE].transform.DORotate(_owner.LevelObjs[Consts.ITEM_LETTUCE].transform.localEulerAngles + new Vector3(0, 90, 0), 0.3f);
            _nLeafIndex += 1;
        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            _bHitting = false;
            if (_nLeafIndex >= _nLeafCount)
            {
                _owner.LevelObjs[Consts.ITEM_LETTUCE].transform.DOMove(_v3BowlPos + Vector3.up * 50, 1).OnComplete(() => {
                    _owner.LevelObjs[Consts.ITEM_LETTUCE].SetPos(Vector3.one * 500);
                    StrStateStatus = "LettuceOver";
                });
            }
        }

    }
}

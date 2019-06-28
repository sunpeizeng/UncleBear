using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class SaladStateWash : State<LevelSalad>
    {
        GameObject _objWasher;
        GameObject _objFluidTop;
        GameObject _objFluidBottom;
        GameObject _objPicking;

        int _nCurLeafId;
        bool _bTapOpened;
        bool _bLettuceReady;
        bool _bLettuceWashed;
        Vector3 _v3CamPos = new Vector3(11.5f, 68.5f, -18f);
        Vector3 _v3CamAngle = new Vector3(25, 255.5f, 0);
        Vector3 _v3WasherPos = new Vector3(-86.3f, 17.5f, -41.5f);
        Vector3 _v3BowlPos = new Vector3(-85.5f, 22.5f, -63f);

        float _fWashTime = 3;
        float _fWashTimer;

        Vector3[] _v3LeafOffsets = { new Vector3(0, 0.5f, -1.1f), new Vector3(-1.1f, 0.5f, 0), new Vector3(0, 0.5f, 1.1f), new Vector3(1.1f, 0.5f, 0) };
        float[] _fLeafRotates = { 90, 180, 270, 0 };
        List<Transform> _lstLeafs = new List<Transform>();

        Transform _trsTop;
        float _fDistance;
        Vector3 _v3OriginLeaf;

        public SaladStateWash(int stateEnum) : base(stateEnum)
        {

        }
        public override void Enter(object param)
        {
            base.Enter(param);
            _nCurLeafId = 0;
            _fWashTimer = 0;
            _bLettuceWashed = _bLettuceReady = _bTapOpened = false;
            CameraManager.Instance.DoCamTween(_v3CamPos, _v3CamAngle);
            _objWasher = _owner.LevelObjs[Consts.ITEM_WASHER];
            _objFluidTop = _owner.LevelObjs[Consts.ITEM_FLUIDCUSTOM];
 
            _objFluidTop.GetComponent<MeshRenderer>().material.color = new Color(0.8f, 1, 0.9f, 0.4f);
            _objFluidBottom = GameObject.Instantiate(_objFluidTop, _objFluidTop.transform.position, Quaternion.identity);
            _objFluidTop.name = "FluidTop";
            _objWasher.transform.DOScale(Vector3.one * 0.7f, 0.5f);
            _objWasher.transform.DOMove(_v3WasherPos + Vector3.up * 5, 0.7f).OnComplete(()=> {
                _objWasher.transform.DOMoveY(_v3WasherPos.y, 0.3f).OnComplete(() => {
                    _bLettuceReady = true;
                    SetWaterTransform(_objFluidTop);
                    SetWaterTransform(_objFluidBottom);

                    GuideManager.Instance.SetGuideClick(EnterKitchen.Instance.ObjTap.transform.position + Vector3.forward * 2);
                });
            });
        }

        void SetWaterTransform(GameObject objFluid)
        {
            objFluid.transform.SetParent(_owner.LevelObjs[Consts.ITEM_WASHER].transform);
            objFluid.SetLocalPos(Vector3.zero);
            objFluid.transform.localScale = Vector3.zero;
        }

        public override string Execute(float deltaTime)
        {
            if (_bTapOpened)
            {
                _fWashTimer += deltaTime;
                //模拟水升起
                _objFluidTop.transform.localPosition = Vector3.up * _fWashTimer * 2;
                _objFluidTop.transform.localScale = new Vector3(4 * _fWashTimer, 0.5f, 4 * _fWashTimer);
                if (_fWashTimer > _fWashTime)
                {
                    _bTapOpened = false;
                    EnterKitchen.Instance.OpenTap(false);
                    _owner.LevelObjs[Consts.ITEM_SALADBOWL].transform.DOMove(_v3BowlPos, 1);

                    //整理父级关系
                    _trsTop = _objWasher.transform.FindChild("Top");
                    var contents = _objWasher.transform.GetChildTrsList();
                    contents.ForEach(p => {
                        if (p.name.Contains("Leaf"))
                        {
                            _lstLeafs.Add(p);
                            p.SetParent(_trsTop);
                        }
                        else if (p.name.Contains("FluidTop"))
                            p.SetParent(_trsTop);
                    });
                    //升起滤网,让水漏出去,漏完移动滤网到碗边,移动菜叶
                    _objFluidBottom.transform.DOScale(new Vector3(4 * _fWashTime, 0.5f, 4 * _fWashTime), 1.5f);
                    _objFluidBottom.transform.DOLocalMoveY(_fWashTime * 2 - 1, 1.5f);
                    //TODO::加漏水特效
                    _trsTop.DOMoveY(_v3WasherPos.y + 5, 1f);
                    _objFluidTop.transform.DOLocalMoveY(0, 2);
                    _objFluidTop.transform.DOScale(Vector3.zero, 2).OnComplete(()=> {
                        _bLettuceWashed = true;
                        GuideManager.Instance.SetGuideSingleDir(_trsTop.position, _v3BowlPos);
                        _fDistance = Vector3.Distance(CameraManager.Instance.MainCamera.transform.position, _trsTop.position) - 15;
                        //LevelManager.Instance.StartCoroutine(TweenLeaf());
                    });
                }
            }

            return base.Execute(deltaTime);                                        
        }

        public override void Exit()
        {
            GameObject.Destroy(_objFluidBottom);
            _objFluidBottom = _objFluidTop = _objWasher = null;
            _lstLeafs.Clear();
            base.Exit();
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            if (!_bLettuceReady || _objPicking != null)
                return;

            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null)
            {
                if (_fWashTimer < _fWashTime && hit.collider.gameObject == EnterKitchen.Instance.ObjTap)
                {
                    GuideManager.Instance.StopGuide();
                    _bTapOpened = !_bTapOpened;
                    EnterKitchen.Instance.OpenTap(_bTapOpened);
                }
                else if (_bLettuceWashed && _nCurLeafId < _lstLeafs.Count && hit.collider.transform == _trsTop)
                {
                    _objPicking = _lstLeafs[_lstLeafs.Count - _nCurLeafId - 1].gameObject;
                    _v3OriginLeaf = _lstLeafs[_lstLeafs.Count - _nCurLeafId - 1].transform.position;
                }
            }
        }

        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_objPicking != null)
            {
                var holdPos = CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition).GetPoint(_fDistance);//GameUtilities.GetFingerTargetWolrdPos(finger, _objPicking, _v3ChopBoardPos.y + 5);
                _objPicking.transform.position = Vector3.Lerp(_objPicking.transform.position, holdPos, 30 * Time.deltaTime);
            }
        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            if (_objPicking != null)
            {
                var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
                if (hit.collider != null && hit.collider.transform.IsChildOf(_owner.LevelObjs[Consts.ITEM_SALADBOWL].transform))
                {
                    int index = _nCurLeafId;
                    var tempTrs = _objPicking.transform;
                    tempTrs.SetParent(_owner.LevelObjs[Consts.ITEM_SALADBOWL].transform);
                    tempTrs.DOLocalMove(Vector3.up * 10, 0.5f).OnComplete(() =>
                    {
                        DoozyUI.UIManager.PlaySound("29上奶油", tempTrs.transform.position, false, 1, 0.4f);
                        tempTrs.DOLocalMove(_v3LeafOffsets[index], 0.5f);
                    });
                    tempTrs.DORotate(new Vector3(0, _fLeafRotates[index], 0), 0.5f);
                    _nCurLeafId += 1;
                    if (_nCurLeafId > _lstLeafs.Count - 1)
                    {
                        CameraManager.Instance.DoCamTween(new Vector3(16.5f, 72, -37.2f), 0.5f, () =>
                        {
                            StrStateStatus = "WashOver";
                        });
                        _owner.LevelObjs[Consts.ITEM_WASHER].transform.DOMove(Vector3.one * 500, 2);
                    }
                }
                else {
                    ReturnLeaf(_objPicking);
                }
                _objPicking = null;
            }
        }

        void ReturnLeaf(GameObject objLeaf)
        {
            objLeaf.SetPos(_v3OriginLeaf);
            objLeaf.transform.localScale = Vector3.zero;
            objLeaf.transform.DOScale(Vector3.one, 0.5f);
        }
    }
}
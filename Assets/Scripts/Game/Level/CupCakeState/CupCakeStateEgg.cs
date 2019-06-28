using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    public class CupCakeStateEgg : State<LevelCupCake>
    {
        Vector3 _v3CamPos = new Vector3(-45.2f, 67.4f, -74.4f);
        Vector3 _v3BowlPos = new Vector3(-83, 22.5f, -74.4f);
        Vector3 _v3EggPos = new Vector3(-83f, 33.3f, -64.5f);
        Vector3 _v3EggHitPos = new Vector3(-83f, 27f, -67.5f);
        Vector3 _v3EggEndPos = new Vector3(-83, 31.5f, -72f);
        Vector3 _v3EggAngle = new Vector3(270, 0, 0);

        Transform _trsEggTop;
        Transform _trsEggBottom;

        float[] _fHeightEggFluid = { 2.8f, 2.7f, 2.6f };
        float[] _fScaleEggFluid = { 7.2f, 6.5f, 4 };
        Vector3 _v3BrokenEggScale = new Vector3(2, 0.5f, 2);

        int _nEggCount;
        int _nBreakCount;
        bool _bSwipeLock;

        List<GameObject> _listEggRawPieces = new List<GameObject>();

        public CupCakeStateEgg(int stateEnum) : base(stateEnum)
        {

        }


        public override void Enter(object param)
        {
            CameraManager.Instance.DoCamTween(_v3CamPos, new Vector3(45, 270, 0), 0.5f);

            //Debug.Log("egg");
            base.Enter(param);
            _trsEggTop = _owner.LevelObjs[Consts.ITEM_EGG].transform.FindChild("Top");
            _trsEggBottom = _owner.LevelObjs[Consts.ITEM_EGG].transform.FindChild("Bottom");
            _owner.LevelObjs[Consts.ITEM_EGG].transform.localEulerAngles = _v3EggAngle;
            _owner.LevelObjs[Consts.ITEM_EGG].transform.position = _v3EggPos;
            _owner.LevelObjs[Consts.ITEM_BOWL].transform.position = _v3BowlPos;
            _owner.LevelObjs[Consts.ITEM_FLUID].transform.SetParent(_owner.LevelObjs[Consts.ITEM_BOWL].transform);
            _owner.LevelObjs[Consts.ITEM_FLUID].transform.localPosition = Vector3.zero;
            //_owner.LevelObjs[Consts.ITEM_FLUID].transform.localScale = _v3BrokenEggScale;

            _bSwipeLock = false;
            _nEggCount = 3;//有几个蛋
            _nBreakCount = 1;//要敲几下
            LeanTouch.OnFingerSwipe += SwipeToBreak;

            _listEggRawPieces.Clear();
            for (int i = _nEggCount; i > 0; i--)
            {
                var eggRaw = GameUtilities.InstantiateT<GameObject>(_owner.LevelObjs[Consts.ITEM_EGGRAW]);
                eggRaw.transform.localScale = Vector3.zero;
                _listEggRawPieces.Add(eggRaw);
            }

            GuideManager.Instance.SetGuideSingleDir(_v3EggPos, _v3BowlPos, true, true, 1f);
        }

        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();

            LeanTouch.OnFingerSwipe -= SwipeToBreak;
        }


        void SwipeToBreak(LeanFinger finger)
        {
            if (_bSwipeLock)
                return;
            if (_nBreakCount > 0)
            {
                _nBreakCount -= 1;
                //模拟鸡蛋敲碎
                _owner.LevelObjs[Consts.ITEM_EGG].transform.DORotate(_v3EggAngle + new Vector3(30, 0, 0), 0.2f);
                _owner.LevelObjs[Consts.ITEM_EGG].transform.DOMove(_v3EggHitPos, 0.2f).OnComplete(() => {
                    DoozyUI.UIManager.PlaySound("22打鸡蛋砸碗上", _v3BowlPos, false);
                    _owner.LevelObjs[Consts.ITEM_EGG].transform.DORotate(_v3EggAngle, 0.4f);
                    if (_nBreakCount > 0)
                        _owner.LevelObjs[Consts.ITEM_EGG].transform.DOMove(_v3EggPos, 0.4f);
                    else
                        _owner.LevelObjs[Consts.ITEM_EGG].transform.DOMove(_v3EggEndPos, 0.3f).OnComplete(EggBreakToFluid);

                });
            }   
        }

        //模拟鸡蛋敲碎以后,流出蛋液,蛋液和生鸡蛋是两个物体一个是缩放的半透球体,一个是面片用来显示蛋黄
        void EggBreakToFluid()
        {
            _bSwipeLock = true;
            DoozyUI.UIManager.PlaySound("23打鸡蛋的蛋壳", _v3BowlPos, false);
            _trsEggTop.DOLocalMoveY(5, 0.3f).OnComplete(() => { _trsEggTop.DOLocalMoveX(20, 0.3f); });
            _trsEggBottom.DOLocalMoveY(-5, 0.3f).OnComplete(() => { _trsEggBottom.DOLocalMoveX(20, 0.3f); });
            _listEggRawPieces[_nEggCount - 1].SetPos(_v3EggEndPos);
            _listEggRawPieces[_nEggCount - 1].transform.SetParent(_owner.LevelObjs[Consts.ITEM_FLUID].transform);
            _listEggRawPieces[_nEggCount - 1].SetAngle(new Vector3(-90, Random.Range(0, 360), 0));
            _listEggRawPieces[_nEggCount - 1].transform.DOScale(Vector3.one, 0.5f);
            _listEggRawPieces[_nEggCount - 1].transform.DOLocalMove(new Vector3(Random.Range(-1f, 1f), _fHeightEggFluid[_nEggCount - 1], Random.Range(-1f, 1f)), 0.7f).OnComplete(JudgeEggOver);                
            //    () =>
            //{
            //    _owner.LevelObjs[Consts.ITEM_FLUID].transform.DOScale(new Vector3(_fScaleEggFluid[_nEggCount - 1], 0.5f, _fScaleEggFluid[_nEggCount - 1]), 0.3f);
            //    _listEggRawPieces[_nEggCount - 1].transform.DOLocalMoveY(_fHeightEggFluid[_nEggCount - 1], 0.3f);
            //    _owner.LevelObjs[Consts.ITEM_FLUID].transform.DOLocalMoveY(_fHeightEggFluid[_nEggCount - 1], 0.3f).OnComplete(JudgeEggOver);
            //});
        
        }

        void JudgeEggOver()
        {
            DoozyUI.UIManager.PlaySound("24鸡蛋入水", _v3BowlPos, false);
            _nEggCount -= 1;
            _trsEggTop.localPosition = _trsEggBottom.localPosition = Vector3.zero;
            //到此一个蛋处理完了
            if (_nEggCount <= 0)
            {
                _owner.LevelObjs[Consts.ITEM_EGG].transform.position = Vector3.one * 500;
                StrStateStatus = "EggBreakOver";
            }
            else
            {
                _bSwipeLock = false;
                _nBreakCount = 1;
                _owner.LevelObjs[Consts.ITEM_EGG].transform.position = _v3EggPos + new Vector3(0, 10, 0);
                _owner.LevelObjs[Consts.ITEM_EGG].transform.DOMove(_v3EggPos, 0.5f);
            }
        }
    }
}

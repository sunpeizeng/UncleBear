using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class IceCreamStateEgg : State<LevelIceCream>
    {
        bool _bDropping;

        Vector3 _v3CamPos = new Vector3(-50f, 58.4f, -47f);

        Vector3 _v3BowlSmall = new Vector3(-52.5f, 22.5f, -99);
        Vector3 _v3Spoon = new Vector3(-54.2f, 27, -99);
        Vector3 _v3BowlBig = new Vector3(-41.6f, 22.5f, -98.4f);
        Vector3 _v3Egg = new Vector3(-54.3f, 35.5f, -99);
        Vector3 _v3EggAngle = new Vector3(0, 0, -90);

        GameObject _objEgg;
        Transform _trsEggTop;
        Transform _trsEggBottom;
        Transform _trsBowlFluid;
        GameObject _objEggSun;

        int _nCurEggClick;

        int _nEggLeft;
        Vector3[] _v3FluidScales = new Vector3[] {
            new Vector3(4.5f, 0.5f, 4.5f),
            new Vector3(4.1f, 0.5f, 4.1f),
            new Vector3(4, 0.5f, 4),
            new Vector3(2.5f, 0.5f, 2.5f)
        };
        float[] _fFluidLocalHeights = new float[] { 1.05f, 0.9f, 0.7f, 0 };

        GameObject _objCurEggSeperate;
        Animation _animCurEggSep;
        Vector3 _v3EggDropPos = new Vector3(-48.5f, 22.5f, -99);
        float _fDropTime;

        public IceCreamStateEgg(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);

            CameraManager.Instance.DoCamTween(_v3CamPos, new Vector3(30, 180, 0), 0.8f);
            _owner.LevelObjs[Consts.ITEM_ICSPOON].SetPos(new Vector3(-54.646f, 26.666f, -99));
            _owner.LevelObjs[Consts.ITEM_ICBOWLBIG].SetPos(_v3BowlBig);
            _owner.LevelObjs[Consts.ITEM_ICBOWLSMALL].SetPos(_v3BowlSmall);
            _trsBowlFluid = _owner.LevelObjs[Consts.ITEM_ICBOWLSMALL].transform.Find("Fluid");
            _objEgg = _owner.LevelObjs[Consts.ITEM_EGG];
            _trsEggTop = _objEgg.transform.FindChild("Top");
            _trsEggBottom = _objEgg.transform.FindChild("Bottom");
            _objEgg.SetPos(_v3Egg);
            _objEgg.SetAngle(_v3EggAngle);
            _nCurEggClick = 0;
            //本来他有三个蛋
            _nEggLeft = 3;
            _trsBowlFluid.SetLocalY(_fFluidLocalHeights[_nEggLeft]);
            _trsBowlFluid.localScale = _v3FluidScales[_nEggLeft];
            _bDropping = false;

            GuideManager.Instance.SetGuideClick(_v3Egg - Vector3.up);
        }

        public override string Execute(float deltaTime)
        {
            if (!_bDropping)
            {
                switch (_nCurEggClick)
                {
                    case 1:
                        _trsEggTop.transform.localPosition = new Vector3(0, 0.05f, 0);
                        break;
                    case 2:
                        _trsEggTop.transform.localPosition = new Vector3(0, 0.15f, 0);
                        break;
                    case 3:
                        {
                            OnEggBreak();
                        }
                        break;
                }
            }
            else
            {
                if (_fDropTime >= 0)
                {
                    _fDropTime -= deltaTime;
                    if (_fDropTime < 0)
                        SeperateEggSun();
                }
            }

            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            _objEgg = _objEggSun = null;
            base.Exit();
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            if (_bDropping || _nEggLeft <= 0)
                return;

            RaycastHit hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.gameObject == _objEgg)
            {
                DoozyUI.UIManager.PlaySound("72手指点击鸡蛋", _v3Egg);
                _nCurEggClick += 1;
            }
        }
        protected override void OnFingerSet(LeanFinger finger)
        {
        }
        protected override void OnFingerUp(LeanFinger finger)
        {
        }

        void OnEggBreak()
        {
            GuideManager.Instance.StopGuide();
            DoozyUI.UIManager.PlaySound("23打鸡蛋的蛋壳");

            _trsEggTop.DOLocalMoveY(5, 0.3f).OnComplete(() => { _trsEggTop.DOLocalMoveZ(40, 0.5f); });
            _trsEggBottom.DOLocalMoveY(-5, 0.3f).OnComplete(() => { _trsEggBottom.DOLocalMoveZ(40, 0.5f); });

            if (_objCurEggSeperate != null)
                GameObject.Destroy(_objCurEggSeperate, 0.05f);
            _objCurEggSeperate = GameObject.Instantiate(_owner.LevelObjs[Consts.ITEM_ICEGGSEPARATE], _v3EggDropPos, Quaternion.identity);
            _owner.LevelObjs[Consts.ITEM_ICSPOON].SetPos(Vector3.one * 500);
            string animName = "anim_egg" + _nEggLeft;
            _animCurEggSep = _objCurEggSeperate.GetComponent<Animation>();
            _fDropTime =  _animCurEggSep[animName].length;
            _animCurEggSep.Play(animName);
            _bDropping = true;
            _nEggLeft -= 1;
            _nCurEggClick = 0;

            DoozyUI.UIManager.PlaySound("24鸡蛋入水", _v3BowlBig, false, 1, 3.3f);
            LevelManager.Instance.CallWithDelay(OnEggWhiteTouchBowl, 1.1f);
        }

        void SeperateEggSun()
        {
            _animCurEggSep.Stop();
            var eggTrs = _objCurEggSeperate.transform.FindChild("Dummy001/Dummy002");
            eggTrs.SetParent(_owner.LevelObjs[Consts.ITEM_ICBOWLBIG].transform);

            _trsEggTop.localPosition = _trsEggBottom.localPosition = Vector3.zero;
            if (_nEggLeft <= 0)
            {
                _owner.LevelObjs[Consts.ITEM_EGG].transform.position = Vector3.one * 500;

                _owner.LevelObjs[Consts.ITEM_ICBOWLSMALL].transform.DOMoveX(_v3BowlSmall.x - 30, 0.5f).SetDelay(0.2f);
                _objCurEggSeperate.transform.DOMoveY(50, 0.8f).OnComplete(() =>
                {
                    GameObject.Destroy(_objCurEggSeperate);
                    _objCurEggSeperate = null;
                    _owner.LevelObjs[Consts.ITEM_ICBOWLBIG].transform.DOMove(
                        new Vector3(_v3CamPos.x, _v3BowlBig.y, _v3BowlBig.z), 0.8f).OnComplete(() =>
                        {
                            StrStateStatus = "EggBreakOver";
                        });
                });

            }
            else
            {
                _owner.LevelObjs[Consts.ITEM_EGG].transform.position = _v3Egg + new Vector3(0, 10, 0);
                _owner.LevelObjs[Consts.ITEM_EGG].transform.DOMove(_v3Egg, 0.8f).OnComplete(() =>
                {
                    GuideManager.Instance.SetGuideClick(_v3Egg - Vector3.up);
                });
                _bDropping = false;
            }
        }

        void OnEggWhiteTouchBowl()
        {
            DoozyUI.UIManager.PlaySound("24鸡蛋入水");

            _trsBowlFluid.DOScale(_v3FluidScales[_nEggLeft], 0.5f);
            _trsBowlFluid.DOLocalMoveY(_fFluidLocalHeights[_nEggLeft], 0.5f);
        }
    }
}

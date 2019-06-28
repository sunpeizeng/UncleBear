using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Touch;

namespace UncleBear
{
    public class IceCreamStateDecorBar : State<LevelIceCream>
    {
        enum PhaseEnum
        {
            Prepare,
            Waiting,
            Dragging,
            Placing,
        }
        PhaseEnum _ePhase;

        Vector3 _v3LocalUmbrellas = new Vector3(5, 0, 1);
        Vector3 _v3LocalFlags = new Vector3(0, 0, -1);
        Vector3 _v3LocalCandies = new Vector3(-5, 0, 0.5f);

        GameObject _objTray;
        Vector3 _v3TrayPos = new Vector3(-25, 24.2f, -19.5f);
        GameObject _objHolding;
        Vector3 _v3SrcLocalPos;
        Vector3 _v3SrcLocalAngle;

        Vector3[] _v3OnBallAngle = new Vector3[] {
            new Vector3(-4.7f, -1.9f, -15.2f),
            new Vector3(2.5f, -17.3f, 11.8f),
            new Vector3(-5,-1.5f,-5.5f)
        };

        Vector3[] _v3OnBallPos = new Vector3[] {
            new Vector3(1.82f, 4.24f, 1.14f),
            new Vector3(-2.36f,4.9f,1.05f),
            new Vector3(0.3f, 5.5f, -2f)
        };

        List<int> _decoredBallIndexes = new List<int>();

        public IceCreamStateDecorBar(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);

            _ePhase = PhaseEnum.Prepare;
            _objHolding = null;

            _objTray = _owner.LevelObjs[Consts.ITEM_ICTRAY];
            _objTray.transform.DOMove(_v3TrayPos + Vector3.left * 50, 0.5f).OnComplete(CleanBottlesForNewDecors);
        }

        void CleanBottlesForNewDecors()
        {
            var children = _objTray.transform.GetChildTrsList();
            children.ForEach(p =>
            {
                if (p.name.Contains("Bottle"))
                    GameObject.Destroy(p.gameObject);
            });

            _owner.LevelObjs[Consts.ITEM_ICCANDIES].transform.SetParent(_objTray.transform);
            _owner.LevelObjs[Consts.ITEM_ICCANDIES].SetLocalPos(_v3LocalCandies);
            _owner.LevelObjs[Consts.ITEM_ICUMBRELLAS].transform.SetParent(_objTray.transform);
            _owner.LevelObjs[Consts.ITEM_ICUMBRELLAS].SetLocalPos(_v3LocalUmbrellas);
            _owner.LevelObjs[Consts.ITEM_ICFLAGS].transform.SetParent(_objTray.transform);
            _owner.LevelObjs[Consts.ITEM_ICFLAGS].SetLocalPos(_v3LocalFlags);

            _objTray.transform.DOMove(_v3TrayPos, 1f).OnComplete(() => {
                _ePhase = PhaseEnum.Waiting;
                GuideManager.Instance.SetGuideSingleDir(_v3TrayPos, _owner.LevelObjs[Consts.ITEM_ICBALLBOWL].transform.position + Vector3.up * 3);
            });
        }

        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            _decoredBallIndexes.Clear();
            base.Exit();
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            switch (_ePhase)
            {
                case PhaseEnum.Waiting:
                    {
                        RaycastHit hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
                        if (hit.collider != null && hit.collider.transform.IsChildOf(_objTray.transform))
                        {
                            _ePhase = PhaseEnum.Dragging;
                            _objHolding = hit.collider.gameObject;
                            _v3SrcLocalPos = _objHolding.transform.localPosition;
                            _v3SrcLocalAngle = _objHolding.transform.localEulerAngles;
                            _objHolding.transform.DORotate(Vector3.zero, 0.3f);
                            _objHolding.GetComponent<BoxCollider>().enabled = false;
                            DoozyUI.UIManager.PlaySound("12物品拿起", hit.point);
                        }
                    }
                    break;

            }
        }

        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_ePhase == PhaseEnum.Dragging && _objHolding != null)
            {
                //位置跟随指针
                var pos = GameUtilities.GetFingerTargetWolrdPos(finger, _objHolding);
                if (pos.y < _v3TrayPos.y + 7)
                    pos.y = _v3TrayPos.y + 7;
                _objHolding.transform.position = Vector3.Slerp(_objHolding.transform.position, pos, 20 * Time.deltaTime);
            }
        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            if (_ePhase == PhaseEnum.Dragging && _objHolding != null)
            {
                _ePhase = PhaseEnum.Placing;
                RaycastHit hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
                if (hit.collider != null && 
                    hit.collider.transform.IsChildOf(_owner.LevelObjs[Consts.ITEM_ICBALLBOWL].transform))
                {
                    int index = Random.Range(0, _owner.IceCreamBalls.Count);
                    if (int.TryParse(hit.collider.gameObject.name, out index))
                    {
                        if (!_decoredBallIndexes.Contains(index))
                        {
                            GuideManager.Instance.StopGuide();
                            _decoredBallIndexes.Add(index);
                            _objHolding.transform.DOKill();
                            _objHolding.transform.DOLocalRotate(_v3OnBallAngle[index], 0.3f).OnComplete(() =>
                            {
                                _objHolding.transform.SetParent(_owner.LevelObjs[Consts.ITEM_ICBALLBOWL].transform);
                                _objHolding.transform.DOLocalMove(_v3OnBallPos[index] + Vector3.up * 2, 0.5f).OnComplete(() =>
                                {
                                    _objHolding.transform.DOLocalMove(_v3OnBallPos[index], 0.3f).SetEase(Ease.InQuad).OnComplete(() =>
                                    {
                                        DoozyUI.UIManager.PlaySound("28蛋液漫出", hit.point);
                                        StrStateStatus = "DecorBarReady";
                                        _objHolding = null;
                                        _ePhase = PhaseEnum.Waiting;
                                    });
                                });
                            });
                            return;
                        }
                    }
                }
                _objHolding.GetComponent<BoxCollider>().enabled = true;
                _objHolding.transform.DOLocalRotate(_v3SrcLocalAngle, 0.5f);
                _objHolding.transform.DOLocalMove(_v3SrcLocalPos, 0.8f).OnComplete(() =>
                {
                    _ePhase = PhaseEnum.Waiting;
                    _objHolding = null;
                });
            }
        }
    }
}

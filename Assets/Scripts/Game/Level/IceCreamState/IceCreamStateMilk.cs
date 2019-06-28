using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;
using Lean;

namespace UncleBear
{
    public class IceCreamStateMilk : State<LevelIceCream>
    {
        AudioSource _asPotWater;
        enum PhaseEnum
        {
            Ready,
            Returning,
            Moving,
            EnterPouring,
            Pouring,
            Enough,
            Cooked,
        }

        PhaseEnum _milkPhase;

        Vector3 _v3CamPos = new Vector3(-12.4f, 68.4f, -63);
        Vector3 _v3CamAngle = new Vector3(45, 180, 0);
        Vector3 _v3PotPos = new Vector3(-12.4f, 24, -99);
        PotCtrl _potCtrl;


        GameObject _objBottle;
        Vector3 _v3BottlePos = new Vector3(-26f, 22.55f, -104.8f);
        Vector3 _v3BottleAngle = new Vector3(0, 0, 359);

        Vector3 _v3PourPos = new Vector3(-22, 40, -99);
        Vector3 _v3PourAngle = new Vector3(0, 0, 263);

        Material _matMilk;
        Transform _trsSoupMesh;
        ParticleCtrller _milkEff;
        Vector3 _v3MilkEffPos = new Vector3(3.25f, 8.58f, -0.2F);
        Vector3 _v3MilkEffAngle = new Vector3(-5, 90, 0);

        float _fExitTimer;
        float _fExitTime = 1f;

        public IceCreamStateMilk(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);

            _milkPhase = PhaseEnum.Ready;
            _fExitTimer = _fExitTime;

            _objBottle = _owner.LevelObjs[Consts.ITEM_ICMILKBOX];
            _objBottle.SetPos(_v3BottlePos);
            _objBottle.SetAngle(_v3BottleAngle);

            _milkEff = EffectCenter.Instance.SpawnEffect("Milk_High", Vector3.zero, Vector3.zero);
            _milkEff.SetMaxTimeUseless();
            _milkEff.gameObject.SetActive(false);
            _milkEff.transform.SetParent(_objBottle.transform);
            _milkEff.gameObject.SetLocalPos(_v3MilkEffPos);
            _milkEff.gameObject.SetAngle(_v3MilkEffAngle);

            _owner.LevelObjs[Consts.ITEM_POT].SetPos(_v3PotPos);
            _potCtrl = _owner.LevelObjs[Consts.ITEM_POT].AddMissingComponent<PotCtrl>();

            //牛奶颜色设置，先隐藏在锅最下方
            _trsSoupMesh = _potCtrl.transform.Find("Soup/SoupMesh");
            if (_trsSoupMesh != null)
            {
                _trsSoupMesh.SetLocalY(-3.5f);
                _matMilk = _trsSoupMesh.GetComponent<MeshRenderer>().material;
                _matMilk.color = new Color(1, 1, 0.95f, 0f);
            }

            CameraManager.Instance.DoCamTween(_v3CamPos, _v3CamAngle, 1f);
            _asPotWater = DoozyUI.UIManager.PlaySound("73倒牛奶修改", _milkEff.transform.position, true);
            _asPotWater.Pause();

            GuideManager.Instance.SetGuideSingleDir(_v3BottlePos, _v3PotPos + Vector3.back * 5);
        }

        public override string Execute(float deltaTime)
        {
            if (_milkPhase == PhaseEnum.EnterPouring)
            {
                _milkPhase = PhaseEnum.Pouring;
                _objBottle.transform.DOMove(_v3PourPos, 0.5f);
                _objBottle.transform.DORotate(_v3PourAngle, 0.5f).OnComplete(()=> {
                    _milkEff.gameObject.SetActive(true);
                    _matMilk.DOColor(new Color(1, 1, 0.95f, 0.9f), 1.5f).SetDelay(0.5f).OnStart(()=> {
                        if (_asPotWater != null)
                            _asPotWater.UnPause();
                    });
                    _trsSoupMesh.DOLocalMoveY(0, 3).SetDelay(0.5f).OnComplete(()=> {
                        _milkEff.DestroyEffectGradually();
                        AudioSourcePool.Instance.Free(_asPotWater);
                        LevelManager.Instance.CallWithDelay(() =>
                        {
                            _milkPhase = PhaseEnum.Enough;

                            _potCtrl.RegisterObject(null, null, MilkCooked, false, false);
                            _potCtrl.StartBoiling();

                            _objBottle.transform.DORotate(_v3BottleAngle, 0.5f);
                            _objBottle.transform.DOMove(_v3BottlePos + Vector3.up * 20, 1f).OnComplete(() =>
                            {
                                _objBottle.SetPos(Vector3.one * 500);
                            });
                        }, 1f);
                    });
                });
            }

            if (_milkPhase == PhaseEnum.Cooked)
            {
                if (_fExitTimer > 0)
                {
                    _fExitTimer -= deltaTime;
                    if (_fExitTimer <= 0)
                    {
                        StrStateStatus = "MilkReady";
                        _potCtrl.enabled = false;
                    }
                }
            }

            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            _objBottle = null;
            base.Exit();
        }


        protected override void OnFingerDown(LeanFinger finger)
        {
            if (_trsSoupMesh.localPosition.y >= 0 || _milkPhase != PhaseEnum.Ready)
                return;

            RaycastHit hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.gameObject == _objBottle)
            {
                _milkPhase = PhaseEnum.Moving;
            }
        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            if (_milkPhase == PhaseEnum.Moving)
            {
                RaycastHit hit = GameUtilities.GetRaycastHitInfo(_objBottle.transform.position, Vector3.down);
                if (hit.collider != null && hit.collider.transform.IsChildOf(_potCtrl.transform))
                {
                    _milkPhase = PhaseEnum.EnterPouring;
                    GuideManager.Instance.StopGuide();
                }
                else
                {
                    _milkPhase = PhaseEnum.Returning;
                    _objBottle.transform.DOMove(_v3BottlePos + new Vector3(0, 9.5f, 0), 0.5f).OnComplete(() =>
                    {
                        _objBottle.transform.DOMoveY(_v3BottlePos.y, 0.5f).OnComplete(()=> { _milkPhase = PhaseEnum.Ready; });
                    });
                }
            }
        }

        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_milkPhase == PhaseEnum.Moving)
            {
                //位置跟随指针
                var pos = GameUtilities.GetFingerTargetWolrdPos(finger, _objBottle, _v3PotPos.y + 9.5f);
                if (pos.z < -110)//墙
                    pos.z = -110;
                _objBottle.transform.position = Vector3.Slerp(_objBottle.transform.position, pos, 20 * Time.deltaTime);
            }
        }


        void CheckMilkEff()
        {
            if (_milkPhase == PhaseEnum.Pouring)
            {
                //根据角度控制特效和香肠出锅
                if (_objBottle.transform.eulerAngles.z < 264)
                {
                    _milkEff.gameObject.SetActive(true);
                }
                else
                {
                    _milkEff.gameObject.SetActive(false);
                    //if (_asPotWater != null)
                    //    _asPotWater.Pause();
                }
            }

        }

        void MilkCooked(bool state)
        {
            _milkPhase = PhaseEnum.Cooked;
            DoozyUI.UIManager.PlaySound("9完成");
        }
    }
}

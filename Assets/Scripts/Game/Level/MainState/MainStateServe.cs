using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Touch;

namespace UncleBear
{
    public class MainStateServe : State<LevelMain>
    {
        Vector3 _v3FinishedDish = new Vector3(-578, 530, 17.6f);
        bool _bHitCover;
        bool _bDishOnTable;
        bool _bHaveCover;

        public MainStateServe(int stateEnum) : base(stateEnum)
        { }

        public override void Enter(object param)
        {
            base.Enter(param);
            _bHitCover = _bDishOnTable = false;
            CameraManager.Instance.SetCamTransform(LevelMain.v3CamDefaultPos, LevelMain.v3CamDefaultAngle);
            //上菜,归位
            CharaCreator.Chef.SetTransform(EnterDinning.Instance.ChefPath.wps[0], Vector3.zero);
            CharaCreator.Waiter.SetTransform(EnterDinning.Instance.ServeDishPath.wps[0], Vector3.zero);

            _bHaveCover = DishManager.Instance.AddCoverToDish();
            DishManager.Instance.ObjFinishedDish.transform.SetParent(CharaCreator.Waiter.GetCharaModel().trsHandR);//CharaCreator.Waiter.transform);
            DishManager.Instance.ObjFinishedDish.SetLocalPos(new Vector3(-6.4f, 1.3f, -2.9f));
            DishManager.Instance.ObjFinishedDish.transform.localEulerAngles = new Vector3(-5, -86, -8.8f);

            //熊走回
            CharaCreator.Chef.ChangeCharaState(CharaStateEnum.Move);
            CharaCreator.Chef.PathMove(EnterDinning.Instance.ChefPath.wps, true, () =>
            {
                CharaCreator.Chef.ChangeCharaState(CharaStateEnum.Wait, false);
                CharaCreator.Chef.transform.DORotate(new Vector3(0, 60, 0), 0.5f);
            });
            //小弟上菜
            CharaCreator.Waiter.ChangeCharaState(CharaStateEnum.Serve, true);
            CharaCreator.Waiter.PathMove(EnterDinning.Instance.ServeDishPath.wps, true, () =>
            {
                CharaCreator.Waiter.transform.DORotate(new Vector3(0, 120, 0), 0.3f).OnComplete(() =>
                {
                    DishManager.Instance.CurCustomer.ChangeCharaState(CharaStateEnum.Wait, true);
                    CharaCreator.Waiter.ChangeCharaState(CharaStateEnum.Wait, false);
                    DishManager.Instance.ObjFinishedDish.transform.SetParent(null);
                    DishManager.Instance.ObjFinishedDish.transform.localEulerAngles = Vector3.zero;
                    DishManager.Instance.ObjFinishedDish.transform.DOMove(EnterDinning.Instance.trsDishPoint.position, 0.5f).OnComplete(() =>
                    {
                        CharaCreator.Waiter.transform.DORotate(new Vector3(0, 60, 0), 0.3f).OnComplete(() =>
                        {
                            CameraManager.Instance.DoCamTween(new Vector3(-527.8f, 545.5f, 33.6f), new Vector3(15f, 180, 0), 0.5f, () =>
                            {
                                if (_bHaveCover)
                                {
                                    _bDishOnTable = true;
                                    GuideManager.Instance.SetGuideClick(DishManager.Instance.ObjFinishedDish.transform.position - Vector3.right * 5);
                                }
                                else
                                    StrStateStatus = "DishReady";
                            });
                        });
                    });
                });
            });

            //DishManager.Instance.ObjFinishedDish.transform.localScale = Vector3.zero;
            //DishManager.Instance.ObjFinishedDish.transform.DOScale(Vector3.one, 0.5f).
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
            if (_bHitCover || !_bDishOnTable || !_bHaveCover)
                return;
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.gameObject == DishManager.Instance.ObjDishCover)
            {
                _bHitCover = true;
                GuideManager.Instance.StopGuide();
                DishManager.Instance.ObjDishCover.transform.SetParent(null);
                var desY = DishManager.Instance.ObjDishCover.transform.position.y + 50;
                DoozyUI.UIManager.PlaySound("9完成");
                EffectCenter.Instance.SpawnEffect("StarsFinish", new Vector3(-527.8f, 524.5f, -22.8f), Vector3.zero);
                DishManager.Instance.CurCustomer.ChangeCharaState(CharaStateEnum.JustHappy);
                DishManager.Instance.BackDishToOriginScale();
                DishManager.Instance.ObjDishCover.transform.DOMoveY(desY, 1.5f).OnComplete(() => {
                    DishManager.Instance.CurCustomer.GetCharaModel().ChangeCharaMood(CharaMoodEnum.Normal);
                    DishManager.Instance.ObjDishCover.SetPos(Vector3.one * 500);
                    StrStateStatus = "DishReady";
                });
            }
        }

    }
}

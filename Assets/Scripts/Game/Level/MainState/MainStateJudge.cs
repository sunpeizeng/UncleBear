using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class MainStateJudge : State<LevelMain>
    {
        bool _bJudgeOver;
        public MainStateJudge(int stateEnum) : base(stateEnum)
        {
        }

        public override void Enter(object param)
        {
            _bJudgeOver = false;
            base.Enter(param);
            //特写吃菜
            CameraManager.Instance.DoCamTween(new Vector3(-532.5f, 563, 101.8f), new Vector3(15f, 180, 0), 0.5f, () =>
            {
                //吃完弹出评价UI
                UIPanelManager.Instance.ShowPanel("UIPanelDishJudgement").DoOnShowCompleted((panel) =>
                {
                    int points = DishManager.Instance.GetDishPoint();
                    (panel as UIPanelDishJudgement).ShowDishStar(DishManager.Instance.DishType, points, ()=>
                    {
                        CharaCreator.Waiter.ChangeCharaState(CharaStateEnum.Settle, points > 0);
                        CharaCreator.Chef.ChangeCharaState(CharaStateEnum.Settle, points > 0);
                        DishManager.Instance.CurCustomer.ChangeCharaState(CharaStateEnum.Settle, points > 0);
                    }, ()=> {
                        UIPanelManager.Instance.HidePanel("UIPanelDishJudgement").DoOnHideCompleted((panelHide) => { _bJudgeOver = true; });
                    });
                });
            });
        }

        public override string Execute(float deltaTime)
        {
            if (_bJudgeOver && DishManager.Instance.CurCustomer.GetCurState() == CharaStateEnum.Move)
            {
                _bJudgeOver = false;
                //评价UI退去,恢复镜头,申请回到IDLE状态
                CameraManager.Instance.DoCamTween(LevelMain.v3CamDefaultPos, LevelMain.v3CamDefaultAngle, 0.5f);
                var leavePath = new List<Vector3>();
                leavePath.AddRange(EnterDinning.Instance.EnterPathes[0].wps);
                leavePath.AddRange(EnterDinning.Instance.EnterPathes[1].wps);
                leavePath.RemoveAt(leavePath.Count - 1);

                DishManager.Instance.CurCustomer.transform.DORotate(new Vector3(0,120,0), 0.5f).OnComplete(()=> {
                    //顾客离去
                    DishManager.Instance.CurCustomer.bIsLeaved = true;
                    //DishManager.Instance.CurCustomer.transform.localEulerAngles = Vector3.zero;
                    DishManager.Instance.CurCustomer.PathMove(leavePath, false, () =>
                    {

                        DishManager.Instance.CurCustomer.SetTransform(Vector3.one * 500, Vector3.zero);
                        DishManager.Instance.CurCustomer = null;

                    });
                    StrStateStatus = "CustomerLeaved";
                    if (DishManager.Instance.ObjCoins != null)
                    {
                        DishManager.Instance.ObjCoins.transform.DOScale(Vector3.zero, 0.5f).SetDelay(0.5f).OnComplete(() =>
                        {
                            Lean.LeanPool.Despawn(DishManager.Instance.ObjCoins, 0.1f);
                        });
                    }
                    DishManager.Instance.ObjFinishedDish.transform.DOScale(Vector3.zero, 0.5f).SetDelay(0.5f).OnComplete(()=> {
                        GameObject.Destroy(DishManager.Instance.ObjFinishedDish);
                        DishManager.Instance.ObjFinishedDish = null;
                    });
                });
            }


            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
        }

    }
}
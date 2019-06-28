using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    public class MainStateWait : State<LevelMain>
    {
        bool _bWaitInPos;
        bool _bGreeting;
        bool _bServing;
        Vector3 _v3HelloAngle = new Vector3(0, 90, 0);
        Vector3 _v3ServeAngle = new Vector3(0, 60, 0);

        public MainStateWait(int stateEnum) : base(stateEnum)
        {
        }

        public override void Enter(object param)
        {
            _bWaitInPos = _bServing = false;
            base.Enter(param);

            _bGreeting = true;

            //DishManager.Instance.CurCustomer.transform.position
            CharaCreator.Waiter.ChangeCharaState(CharaStateEnum.Move);
            CharaCreator.Waiter.transform.DORotate(_v3HelloAngle, 0.5f).OnComplete(() =>
            {
                CharaCreator.Waiter.ChangeCharaState(CharaStateEnum.Greet);
                //Debug.Log("waiter say hello");
                _bServing = true;
                CharaCreator.Chef.ChangeCharaState(CharaStateEnum.Move);
                CharaCreator.Chef.transform.DORotate(_v3ServeAngle, 0.3f).OnComplete(()=> {
                    CharaCreator.Chef.ChangeCharaState(CharaStateEnum.Greet);
                });
              
            });

        }

        public override string Execute(float deltaTime)
        {
            if (_bGreeting)
            {
                if (DishManager.Instance.CurCustomer.GetCurState() == CharaStateEnum.Move)
                {
                    _bGreeting = false;
                    //DishManager.Instance.CurCustomer.ChangeCharaState(CharaStateEnum.Move);
                    DishManager.Instance.CurCustomer.transform.DOLookAt(EnterDinning.Instance.EnterPathes[1].wps[0], 0.3f).OnComplete(() => {
                        DishManager.Instance.CurCustomer.PathMove(EnterDinning.Instance.EnterPathes[1].wps, true, () =>
                        {
                            
                            ////转身上座
                            //Vector3 cusPos = DishManager.Instance.CurCustomer.transform.position;
                            //DishManager.Instance.CurCustomer.transform.DOMove(cusPos + EnterDinning.Instance.OffsetsToSeat[DishManager.Instance.CurCustomer.name], 0.5f);
                            DishManager.Instance.CurCustomer.transform.DORotate(Vector3.zero, 0.5f).OnComplete(() =>
                            {
                                DishManager.Instance.CurCustomer.ChangeCharaState(CharaStateEnum.Sit, true);
                            });
                            CharaCreator.Waiter.transform.DORotate(_v3ServeAngle, 0.5f).OnComplete(()=> {
                                CameraManager.Instance.DoCamTween(new Vector3(-528, 559, 90), 0.5f, ()=> {
                                    //坐好了拿菜单出来
                                    EnterDinning.Instance.ShowMenuObj(true);
                                });
                                CharaCreator.Waiter.ChangeCharaState(CharaStateEnum.Serve, false);
                            });
                        });
                    });
                   
                }
            }

            if (!_bWaitInPos && CharaCreator.Waiter.GetCurState() == CharaStateEnum.Wait)
            {
                _bWaitInPos = true;
                CharaCreator.Waiter.ChangeCharaState(CharaStateEnum.Move);
                int pathLen = EnterDinning.Instance.ServeDishPath.wps.Count;
                CharaCreator.Waiter.transform.DOLookAt(EnterDinning.Instance.ServeDishPath.wps[pathLen - 1], 0.2f).OnComplete(() =>
                {
                    CharaCreator.Waiter.PathMove(new List<Vector3> { EnterDinning.Instance.ServeDishPath.wps[pathLen - 1] }, true, () =>
                    {
                        CharaCreator.Waiter.transform.DORotate(_v3HelloAngle, 0.3f).OnComplete(() =>
                        {
                            CharaCreator.Waiter.ChangeCharaState(CharaStateEnum.Wait, false);
                        });
                    });
                });
            }

            //if (_bServing)
            //    CharaCreator.Chef.transform.LookAt(DishManager.Instance.CurCustomer.transform);


            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
            CharaCreator.Waiter.ChangeCharaState(CharaStateEnum.Wait, false);
            CharaCreator.Waiter.AnimCtrller.AnimStop();
            EnterDinning.Instance.ShowMenuObj(false);
            _bServing = false;
        }
    }
}

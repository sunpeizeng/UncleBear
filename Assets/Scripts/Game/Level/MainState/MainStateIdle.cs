using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class MainStateIdle : State<LevelMain>
    {
        private int _nLastCustomerId = -1;

        private float _fGenCustomerTime = 2;
        private float _fGenCustomerTimer;

        Vector3 _v3UnclePos = new Vector3(-616, 500, -80);
        Vector3 _v3AndyPos = new Vector3(-547.6f, 500, -17.5f);
        Vector3 _v3AndyResetPos = new Vector3(-552, 500, -21f);//-554.5  500  -20.7
        Vector3 _v3WaiterAngle = new Vector3(0, 122, 0);
        Vector3 _v3UBAngle = new Vector3(0, 80, 0);

        public MainStateIdle(int stateEnum) : base(stateEnum)
        {
        }

        public override void Enter(object param)
        {
            base.Enter(param);
            _fGenCustomerTimer = 0;
            CameraManager.Instance.SetCamTransform(LevelMain.v3CamDefaultPos, LevelMain.v3CamDefaultAngle);

            LevelManager.Instance.StartCoroutine(ResetUBState());
            LevelManager.Instance.StartCoroutine(ResetAndyState());
        }

        IEnumerator ResetAndyState()
        {
            yield return new WaitForSeconds(0.1f);
            CharaCreator.Waiter.transform.position = _v3AndyResetPos;
            CharaCreator.Waiter.ChangeCharaState(CharaStateEnum.Move, false);
            CharaCreator.Waiter.PathMove(new List<Vector3> { _v3AndyPos }, true, () =>
            {
                CharaCreator.Waiter.transform.DORotate(_v3WaiterAngle, 0.5f).OnComplete(() =>
                {
                    CharaCreator.Waiter.ChangeCharaState(CharaStateEnum.Idle);
                });
            });
        }
        IEnumerator ResetUBState()
        {
            yield return new WaitForSeconds(Random.Range(0.3f, 0.8f));
            CharaCreator.Chef.transform.position = _v3UnclePos;
            CharaCreator.Chef.ChangeCharaState(CharaStateEnum.Move, false);
            CharaCreator.Chef.transform.DORotate(_v3UBAngle, 0.5f).OnComplete(() =>
            {
                CharaCreator.Chef.ChangeCharaState(CharaStateEnum.Idle);
            });
        }

        public override string Execute(float deltaTime)
        {
            if (UIPanelManager.Instance.GetPanel("UIGameHUD") != null && UIPanelManager.Instance.GetPanel("UIGameHUD").IsActive)
            {
                _fGenCustomerTimer += deltaTime;
                if (_fGenCustomerTimer > _fGenCustomerTime)
                {  //!把客人生成写在这会导致如果做完菜,不会刷新客人,之后还是挪出去
                    GenCustomer();
                    _fGenCustomerTimer = 0;
                }
            }

            //if (_bGreeted)
            //{
            //    if(DishManager.Instance.CurCustomer != null && DishManager.Instance.CurCustomer.GetCurState()== CharaStateEnum.Move)
            //        _strStateStatus = "CustomerReady";
            //}


            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
        }

        //根据空位生成一个客人
        void GenCustomer()
        {
            if (DishManager.Instance.CurCustomer == null)
            {
                var newCustomer = CharaCreator.Customers[RanPickCustomer()];
                newCustomer.SetTransform(EnterDinning.Instance.EnterPathes[0].wps[0], Vector3.zero);
                //newCustomer.SetFinishDishCallback(OnCustomerFinished);
                newCustomer.ChangeCharaState(CharaStateEnum.Move);
                newCustomer.PathMove(EnterDinning.Instance.EnterPathes[0].wps, true, () =>
                {
                    //打招呼
                    newCustomer.transform.DOLookAt(CharaCreator.Waiter.transform.position, 0.3f).OnComplete(()=> {
                        newCustomer.ChangeCharaState(CharaStateEnum.Greet);
                        StrStateStatus = "CustomerReady";
                    });
                
                    //_bGreeted = true;
                });
                newCustomer.bIsLeaved = false;
                DishManager.Instance.CurCustomer = newCustomer;
            }
        }

        //随机挑选客人
        int RanPickCustomer()
        {
            int index = Random.Range(0, CharaCreator.Customers.Count);
            if (index == _nLastCustomerId)
                return RanPickCustomer();
            else
            {
                _nLastCustomerId = index;
                return index;
            }
        }

    }
}

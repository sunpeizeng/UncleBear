using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


namespace UncleBear
{

    public class CharaStateBase : State<CharaBase>
    {
        public CharaStateBase(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);
            //Debug.Log((CharaStateEnum)StateEnum);
        }
    }
       

    public class CharaStateIdle : CharaStateBase
    {
        public CharaStateIdle(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);
            //if (!(bool)param)
            _owner.AnimCtrller.AnimPlay(CharaAnimEnum.IdleStanding.ToString(), true);


            SetWaiterItem(true);
            //else
            //    _owner.AnimCtrller.AnimPlay(CharaAnimEnum.IdleSitting.ToString(), true);
        }

        public override void Exit()
        {
            base.Exit();
            SetWaiterItem(false);
        }

        void SetWaiterItem(bool state)
        {
            if (_owner.GetClassType() == CharaData.CharaClassType.Waiter)
            {
                if ((_owner as CharaWaiter).trsRag != null)
                {
                    (_owner as CharaWaiter).trsRag.gameObject.SetActive(state);
                }
            }
        }
    }
    public class CharaStateMove : CharaStateBase
    {
        //AudioSource _walkSource;

        public CharaStateMove(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);
            _owner.AnimCtrller.AnimPlay(CharaAnimEnum.Move.ToString(), true);
            //_walkSource = SoundCenter.Instance.PlaySound("3走路", _owner.transform, true);
        }

        public override void Exit()
        {
            base.Exit();
            //AudioSourcePool.Instance.Free(_walkSource);
        }
    }
    public class CharaStateGreet : CharaStateBase
    {
        float _fGreetTime;
        public CharaStateGreet(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);
            _owner.GetCharaModel().ChangeCharaMood(CharaMoodEnum.Happy);
            _fGreetTime = _owner.AnimCtrller.AnimPlay(CharaAnimEnum.Greet.ToString());
            switch (_owner.name)
            {
                case "Giraffe":
                case "Elephant":
                    { DoozyUI.UIManager.PlaySound("43小象（长颈鹿）打招呼", _owner.transform.position); }
                    break;
                case "Rabbit":
                    { DoozyUI.UIManager.PlaySound("46小兔子打招呼", _owner.transform.position); }
                    break;
                case "UncleBear":
                    { DoozyUI.UIManager.PlaySound("49熊大叔打招呼", _owner.transform.position); }
                    break;
            }

        }

        public override string Execute(float deltaTime)
        {
            if (_fGreetTime >= 0)
            {
                _fGreetTime -= deltaTime;
                if (_fGreetTime < 0)
                {
                    AfterGreet();
                }
            }

            return base.Execute(deltaTime);
        }

        void AfterGreet()
        {
            _owner.GetCharaModel().ChangeCharaMood(CharaMoodEnum.Normal);
            switch (_owner.GetClassType())
            {
                case CharaData.CharaClassType.Chef:
                case CharaData.CharaClassType.Waiter:
                    //case CharaData.CharaClassType.Customer:
                    _owner.ChangeCharaState(CharaStateEnum.Wait, false);
                    break;
                case CharaData.CharaClassType.Customer:
                    _owner.ChangeCharaState(CharaStateEnum.Move);
                    break;
            }
        }

        public override void Exit()
        {
            _owner.GetCharaModel().ResetCharaMood();
            base.Exit();
        }
    }

    public class CharaStateSit : CharaStateBase
    {
        bool _bSitdown;
        float _fSitTime;
        public CharaStateSit(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);
            _bSitdown = (bool)param;
            if (_bSitdown)
                _fSitTime = _owner.AnimCtrller.AnimPlay(CharaAnimEnum.SitDown.ToString(), false);
            else
                _fSitTime = _owner.AnimCtrller.AnimPlay(CharaAnimEnum.StandUp.ToString(), false);
        }

        public override string Execute(float deltaTime)
        {
            if (_fSitTime >= 0)
            {
                _fSitTime -= deltaTime;
                if (_fSitTime < 0)
                {
                    AfterSit();
                }
            }
            return base.Execute(deltaTime);
        }

        void AfterSit()
        {
            switch (_owner.GetClassType())
            {
                case CharaData.CharaClassType.Customer:
                    if (_bSitdown)
                        _owner.ChangeCharaState(CharaStateEnum.Wait, false);
                    else
                        _owner.ChangeCharaState(CharaStateEnum.Move, false);
                    break;
            }
        }
    }

    public class CharaStateWait : CharaStateBase
    {
        public CharaStateWait(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);
            _owner.GetCharaModel().ResetCharaMood();
            bool haveFood = (bool)param;
            if (haveFood)
                _owner.AnimCtrller.AnimPlay(CharaAnimEnum.WaitFeed.ToString(), true);
            else
                _owner.AnimCtrller.AnimPlay(CharaAnimEnum.Wait.ToString(), true);
        }
    }

    public class CharaStateServe : CharaStateBase
    {
        bool _bHaveDish;
        float _fNoteTime;
        float _fWaitTime;
        float _fTimer;

        public CharaStateServe(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);
            _bHaveDish = (bool)param;
            switch (_owner.GetClassType())
            {
                case CharaData.CharaClassType.Waiter:
                    {
                        if (!_bHaveDish)
                        {
                            _fNoteTime = _owner.AnimCtrller.AnimPlay(CharaAnimEnum.TakeNotes.ToString());
                            DoozyUI.UIManager.PlaySound("7记笔记", _owner.transform.position);
                            CharaCreator.Waiter.GetCharaModel().trsHandL.FindChild("Notebook").gameObject.SetActive(true);
                            CharaCreator.Waiter.GetCharaModel().trsHandR.FindChild("Pencil").gameObject.SetActive(true);
                            ResetWaitTime();
                        }
                        else
                            _owner.AnimCtrller.AnimPlay(CharaAnimEnum.TransDish.ToString(), true);
                    }
                    break;
                case CharaData.CharaClassType.Chef:
                    {
                        if (_owner == null)
                            return;

                        if (!_bHaveDish)
                            _owner.AnimCtrller.AnimPlay(CharaAnimEnum.TakeNotes.ToString(), true);
                        else
                            _owner.AnimCtrller.AnimPlay(CharaAnimEnum.TransDish.ToString(), true);
                    }
                    break;
            }
          
        }

        public override string Execute(float deltaTime)
        {
            if (!_bHaveDish && _owner.GetClassType() == CharaData.CharaClassType.Waiter)
            {
                if (_fNoteTime > 0)
                {
                    _fNoteTime -= deltaTime;
                    if (_fNoteTime <= 0)
                    {
                        _owner.AnimCtrller.AnimPlay(CharaAnimEnum.Wait.ToString(), true);
                    }
                }
                else
                {
                    _fTimer += deltaTime;
                    if (_fTimer > _fWaitTime)
                    {
                        ResetWaitTime();
                        _fNoteTime = _owner.AnimCtrller.AnimPlay(CharaAnimEnum.TakeNotes.ToString());
                        DoozyUI.UIManager.PlaySound("7记笔记", _owner.transform.position);
                    }
                }
            }

            return base.Execute(deltaTime);
        }

        void ResetWaitTime()
        {
            _fTimer = 0;
            _fWaitTime = Random.Range(1.5f, 3f);
        }

        public override void Exit()
        {
            base.Exit();
            if (_owner.GetClassType() == CharaData.CharaClassType.Waiter)
            {
                CharaCreator.Waiter.GetCharaModel().trsHandL.FindChild("Notebook").gameObject.SetActive(false);
                CharaCreator.Waiter.GetCharaModel().trsHandR.FindChild("Pencil").gameObject.SetActive(false);
            }
        }
    }

    public class CharaStateJustHappy : CharaStateBase
    {
        float _fSettleTime;

        public CharaStateJustHappy(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);

            _owner.GetCharaModel().ChangeCharaMood(CharaMoodEnum.Happy);
            _fSettleTime = _owner.AnimCtrller.AnimPlay(CharaAnimEnum.Happy.ToString());
        }

        public override string Execute(float deltaTime)
        {
            if (_fSettleTime >= 0)
            {
                _fSettleTime -= deltaTime;
                if (_fSettleTime < 0)
                    AfterSettlement();
            }
            return base.Execute(deltaTime);
        }

        void AfterSettlement()
        {
            _owner.GetCharaModel().ChangeCharaMood(CharaMoodEnum.Normal);
            switch (_owner.GetClassType())
            {
                case CharaData.CharaClassType.Chef:
                case CharaData.CharaClassType.Waiter:
                    _owner.ChangeCharaState(CharaStateEnum.Wait, false);
                    break;
                case CharaData.CharaClassType.Customer:
                    _owner.ChangeCharaState(CharaStateEnum.Wait, true);
                    break;
            }
        }

        public override void Exit()
        {
            _owner.GetCharaModel().ResetCharaMood();
            base.Exit();
        }
    }

    public class CharaStateSettle : CharaStateBase
    {
        float _fSettleTime;
        float _fPaymentTime;
        public CharaStateSettle(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);
            _fPaymentTime = -1;
            bool isHappy = (bool)param;
            if (isHappy)
            {
                _owner.GetCharaModel().ChangeCharaMood(CharaMoodEnum.Happy);
                _fSettleTime = _owner.AnimCtrller.AnimPlay(CharaAnimEnum.Happy.ToString());
            }
            else
            {
                _owner.GetCharaModel().ChangeCharaMood(CharaMoodEnum.Sad);
                _fSettleTime = _owner.AnimCtrller.AnimPlay(CharaAnimEnum.Sad.ToString());
            }

            switch (_owner.name)
            {
                case "Giraffe":
                case "Elephant":
                    {
                        if (isHappy)
                            DoozyUI.UIManager.PlaySound("44小象（长颈鹿）高兴", _owner.transform.position);
                        else
                            DoozyUI.UIManager.PlaySound("45小象（长颈鹿）不高兴", _owner.transform.position);
                    }
                    break;
                case "Rabbit":
                    {
                        if (isHappy)
                            DoozyUI.UIManager.PlaySound("47小兔子高兴", _owner.transform.position);
                        else
                            DoozyUI.UIManager.PlaySound("48小兔子高兴", _owner.transform.position);
                    }
                    break;
                case "UncleBear":
                    {
                        if (isHappy)
                            DoozyUI.UIManager.PlaySound("50熊大叔高兴", _owner.transform.position);
                        else
                            DoozyUI.UIManager.PlaySound("51熊大叔不高兴", _owner.transform.position);
                    }
                    break;
            }
        }

        public override string Execute(float deltaTime)
        {
            if (_fSettleTime >= 0)
            {
                _fSettleTime -= deltaTime;
                if (_fSettleTime < 0)
                    AfterSettlement();
            }

            if (_fPaymentTime >= 0)
            {
                _fPaymentTime -= deltaTime;
                if(_fPaymentTime < 0)
                    _owner.ChangeCharaState(CharaStateEnum.Sit, false);
            }
            return base.Execute(deltaTime);
        }

        void AfterSettlement()
        {
            _owner.GetCharaModel().ChangeCharaMood(CharaMoodEnum.Normal);
            switch (_owner.GetClassType())
            {
                case CharaData.CharaClassType.Chef:
                case CharaData.CharaClassType.Waiter:
                    _owner.ChangeCharaState(CharaStateEnum.Wait, false);
                    break;
                case CharaData.CharaClassType.Customer:
                    {
                        if (DishManager.Instance.GetDishPoint() > 0)
                        {
                            _fPaymentTime = _owner.AnimCtrller.AnimPlay(CharaAnimEnum.Pay.ToString(), false, 0.8f) + 1;//完事后等一等
                            DishManager.Instance.ShowMoney(DishManager.Instance.GetDishPoint());
                        }
                        else
                            _owner.ChangeCharaState(CharaStateEnum.Sit, false);
                    }
                    break;
            }
        }

        public override void Exit()
        {
            _owner.GetCharaModel().ResetCharaMood();
            base.Exit();
        }
    }

    public class CharaStateEat : CharaStateBase
    {
        float _fEatTime;
        public CharaStateEat(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);
            //此处如果用crossfade,需要判断是否在动画过渡中,否则可能导致动画播放失败,效果还可以接受的情况下还是用强切
            _fEatTime = _owner.AnimCtrller.AnimPlay(CharaAnimEnum.Eat.ToString(), false, 1, CharaAnimation.AnimPlayType.Normal);
        }

        public override string Execute(float deltaTime)
        {
            if (_fEatTime >= 0)
            {
                _fEatTime -= deltaTime;
                if (_fEatTime < 0)
                {
                    AfterEat();
                }
            }
            return base.Execute(deltaTime);
        }

        void AfterEat()
        {
            switch (_owner.GetClassType())
            {
                case CharaData.CharaClassType.Customer:
                    _owner.ChangeCharaState(CharaStateEnum.Wait, true);
                    break;
            }
        }
    }

    public class CharaStatePick : CharaStateBase
    {
        float _fFaceTime = 0;
        CharaAnimEnum _lastAnim;

        public CharaStatePick(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);
            bool isAgree = (bool)param;
            var type = _owner.AnimCtrller.IsAnimFading()? CharaAnimation.AnimPlayType.Normal : CharaAnimation.AnimPlayType.Crossfade;
            if (isAgree)
            {
                if (_fFaceTime > 0 && _lastAnim == CharaAnimEnum.Agree)
                    return;//type = CharaAnimation.AnimPlayType.Normal;
                _owner.GetCharaModel().ChangeCharaMood(CharaMoodEnum.Happy);
                _fFaceTime = _owner.AnimCtrller.AnimPlay(CharaAnimEnum.Agree.ToString(), false, 1, type);
                _lastAnim = CharaAnimEnum.Agree;
            }
            else
            {
                if (_fFaceTime > 0 && _lastAnim == CharaAnimEnum.Sad)
                    return;//type = CharaAnimation.AnimPlayType.Normal;
                _owner.GetCharaModel().ChangeCharaMood(CharaMoodEnum.Sad);
                _fFaceTime = _owner.AnimCtrller.AnimPlay(CharaAnimEnum.Sad.ToString(), false, 1, type);
                _lastAnim = CharaAnimEnum.Sad;
            }

            switch (_owner.name)
            {
                case "Giraffe":
                case "Elephant":
                    {
                        if (isAgree)
                            DoozyUI.UIManager.PlaySound("44小象（长颈鹿）高兴", _owner.transform.position);
                        else
                            DoozyUI.UIManager.PlaySound("45小象（长颈鹿）不高兴", _owner.transform.position);
                    }
                    break;
                case "Rabbit":
                    {
                        if (isAgree)
                            DoozyUI.UIManager.PlaySound("47小兔子高兴", _owner.transform.position);
                        else
                            DoozyUI.UIManager.PlaySound("48小兔子高兴", _owner.transform.position);
                    }
                    break;
                case "UncleBear":
                    {
                        if (isAgree)
                            DoozyUI.UIManager.PlaySound("50熊大叔高兴", _owner.transform.position);
                        else
                            DoozyUI.UIManager.PlaySound("51熊大叔不高兴", _owner.transform.position);
                    }
                    break;
            }
        }

        public override string Execute(float deltaTime)
        {
            if (_fFaceTime >= 0)
            {
                _fFaceTime -= deltaTime;
                if (_fFaceTime < 0)
                    AfterFaceTime();
            }
            return base.Execute(deltaTime);
        }
        void AfterFaceTime()
        {
            _owner.GetCharaModel().ChangeCharaMood(CharaMoodEnum.Normal);
            switch (_owner.GetClassType())
            {
                case CharaData.CharaClassType.Customer:
                    _owner.ChangeCharaState(CharaStateEnum.Wait, false);
                    break;
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class CharaCustomer : CharaBase
    {
        public bool bIsLeaved;

        private System.Action<CharaBase> _cbOnFinishDish;
        public System.Action<CharaBase> OnFinishDish
        {
            get { return _cbOnFinishDish; }
        }

        public override void Initialize(CharaEnum type, string animId)
        {
            base.Initialize(type, animId);

            _fsmChara.AddState(new CharaStateIdle((int)CharaStateEnum.Idle));
            _fsmChara.AddState(new CharaStateMove((int)CharaStateEnum.Move));
            _fsmChara.AddState(new CharaStateWait((int)CharaStateEnum.Wait));
            _fsmChara.AddState(new CharaStateEat((int)CharaStateEnum.Eat));
            _fsmChara.AddState(new CharaStateGreet((int)CharaStateEnum.Greet));
            _fsmChara.AddState(new CharaStateSettle((int)CharaStateEnum.Settle));
            _fsmChara.AddState(new CharaStatePick((int)CharaStateEnum.Pick));
            _fsmChara.AddState(new CharaStateSit((int)CharaStateEnum.Sit));
            _fsmChara.AddState(new CharaStateJustHappy((int)CharaStateEnum.JustHappy));
        }

        public void SetFinishDishCallback(System.Action<CharaBase> callback)
        {
            _cbOnFinishDish = callback;
        }

        public void ShowLike()
        {
            ChangeCharaState(CharaStateEnum.Pick, true);
            //transform.DOKill(true);
            //transform.DOShakePosition(0.5f);
        }

        public void ShowDislike()
        {
            ChangeCharaState(CharaStateEnum.Pick, false);
            //transform.DOKill(true);
            //transform.DOShakeRotation(0.5f);
        }
    }
}

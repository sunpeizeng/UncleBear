using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public class CharaWaiter :  CharaBase
    {
        public Transform trsRag;

        public override void Initialize(CharaEnum type, string animId)
        {
            base.Initialize(type, animId);

            trsRag = _model.trsHandR.FindChild("rag");

            _fsmChara.AddState(new CharaStateIdle((int)CharaStateEnum.Idle));
            _fsmChara.AddState(new CharaStateMove((int)CharaStateEnum.Move));
            _fsmChara.AddState(new CharaStateGreet((int)CharaStateEnum.Greet));
            _fsmChara.AddState(new CharaStateWait((int)CharaStateEnum.Wait));
            _fsmChara.AddState(new CharaStateServe((int)CharaStateEnum.Serve));
            _fsmChara.AddState(new CharaStateSettle((int)CharaStateEnum.Settle));
        }
    }
}

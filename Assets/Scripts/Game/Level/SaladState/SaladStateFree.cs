using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public class SaladFree : FreeCookingState<LevelSalad>
    {
        //GameObject _objBowl;
        Vector3 _v3BowlPos = new Vector3(-27.5f, 22.5f, -20.5f);

        public SaladFree(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            var tools = param as Dictionary<string, GameObject>;
            _objFinalPlace = tools[Consts.ITEM_SALADBOWL];
            _objFinalPlace.SetPos(_v3BowlPos);

            base.Enter(param);
        }

        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();

        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class SushiStatePlace : State<LevelSushi>
    {

        public SushiStatePlace(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Input.multiTouchEnabled = false;
            //Debug.Log("place");
            base.Enter(param);
            DishManager.Instance.ObjFinishedDish = _owner.LevelObjs[Consts.ITEM_SUSHIBOARD];
            DoozyUI.UIManager.PlaySound("9完成");
        }
    }
}

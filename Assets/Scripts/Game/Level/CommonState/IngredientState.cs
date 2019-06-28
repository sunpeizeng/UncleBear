using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;

namespace UncleBear
{
    public class IngredientState<T> : State<T>
    {
        protected bool _bShowCustomer = true;

        public IngredientState(int stateEnum) : base(stateEnum)
        {

        }
        public override void Enter(object param)
        {
            base.Enter(param);
            DishManager.Instance.IngredsInDish.Clear();

            if (_bShowCustomer)
            {
                DishManager.Instance.FixRenderCamHeightByCustomer();
                UIPanelManager.Instance.GetPanel("UIPanelDishLevel").ShowSubElements("UIRenderChara");
            }
        }

        public override void Exit()
        {
            base.Exit();
            UIPanelManager.Instance.GetPanel("UIPanelDishLevel").HideSubElements("UIRenderChara");
        }
    }
}

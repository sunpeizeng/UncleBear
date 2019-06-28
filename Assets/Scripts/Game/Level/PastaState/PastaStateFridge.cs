using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    //TODO::目前看来这个冰箱和沙拉的冰箱逻辑一样,可以考虑多整合一下
    public class PastaStateFridge : IngredientState<LevelPasta>
    {
        Vector3 _v3CamPos = new Vector3(514, 30, 125);
        Vector3 _v3CamRot = new Vector3(0, 180, 0);
        Vector3 _v3FridgePlatePos = new Vector3(527, 12, 15f);

        FridgeCtrl _ctrllerFridge;

        public PastaStateFridge(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("Fridge");
            base.Enter(param);
            _owner.LevelObjs[Consts.ITEM_FRIDGEPLATE].transform.position = _v3FridgePlatePos;
            //TODO::让UI出来遮一下
            CameraManager.Instance.SetCamTransform(_v3CamPos, _v3CamRot);

            _ctrllerFridge = _owner.LevelObjs[Consts.ITEM_FRIDGE].AddMissingComponent<FridgeCtrl>();
            _ctrllerFridge.OnFridgePickOver = OnIngredPicked;
            _ctrllerFridge.InitializeFridge(_owner.LevelObjs[Consts.ITEM_FRIDGELAYER], _owner.LevelObjs[Consts.ITEM_FRIDGEPLATE]);
            _ctrllerFridge.enabled = true;
        }


        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
            if (_ctrllerFridge != null)
            {
                _ctrllerFridge.enabled = false;
                //结束以后,从plate取子物体就可以了
                for (int i = 0; i < _ctrllerFridge.ObjsInPlate.Length; i++)
                {
                    if (_ctrllerFridge.ObjsInPlate[i] != null)
                        DishManager.Instance.IngredsInDish.Add(_ctrllerFridge.ObjsInPlate[i].name);
                    //_lstSaladIngredients.Add(objs[i]);
                }
            }
        }


        void OnIngredPicked(bool empty)
        {
            if (empty)
                StrStateStatus = "FridgePlateEmpty";
            else
                StrStateStatus = "FridgeSelectOver";
            _ctrllerFridge.ShowFridgeGuide(true);
        }
    }
}

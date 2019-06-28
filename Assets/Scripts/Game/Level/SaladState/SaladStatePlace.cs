using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class SaladPlace : State<LevelSalad>
    {
        public SaladPlace(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("place");
            base.Enter(param);

            CameraManager.Instance.DoCamTween(new Vector3(-26.5f, 91.9f, 45), new Vector3(45, 180, 0), 1f, SetSaladReady);
        }

        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();

        }

        void SetSaladReady()
        {
            DoozyUI.UIManager.PlaySound("9完成");
            var trsSaladStuffs = _owner.LevelObjs[Consts.ITEM_SALADBOWL].transform.GetChildTrsList();
            var saladCols = _owner.LevelObjs[Consts.ITEM_SALADBOWL].GetComponentsInChildren<Collider>();
            for (int i = 0; i < saladCols.Length; i++)
                GameObject.Destroy(saladCols[i]);
            var saladBodies = _owner.LevelObjs[Consts.ITEM_SALADBOWL].GetComponentsInChildren<Rigidbody>();
            for (int i = 0; i < saladBodies.Length; i++)
                GameObject.Destroy(saladBodies[i]);

            GameObject salad = new GameObject("Salad");
            salad.transform.SetParent(_owner.LevelObjs[Consts.ITEM_SALADBOWL].transform);
            salad.SetLocalPos(Vector3.zero);
            trsSaladStuffs.ForEach(p =>
            {
                if (p.name != "Mesh")
                    p.SetParent(salad.transform);
            });
            var saladCol = salad.AddComponent<BoxCollider>();
            saladCol.size = Vector3.one * 7;
            saladCol.center = Vector3.up * 3.5f;
            DishManager.Instance.ObjFinishedDish = _owner.LevelObjs[Consts.ITEM_SALADBOWL];
        }
    }
}

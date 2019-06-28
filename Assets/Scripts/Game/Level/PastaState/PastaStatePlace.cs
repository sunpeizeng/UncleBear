using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class PastaStatePlace : State<LevelPasta> {

        public PastaStatePlace(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("Show");
            base.Enter(param);

            CameraManager.Instance.DoCamTween(new Vector3(-26.5f, 91.9f, 45), new Vector3(45, 180, 0), 1f, SetPastaReady);
        }

        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
        }

        void SetPastaReady()
        {
            DoozyUI.UIManager.PlaySound("9完成");
            var trsStuffs = _owner.LevelObjs[Consts.ITEM_PASTAPLATE].transform.GetChildTrsList();
            var cols = _owner.LevelObjs[Consts.ITEM_PASTAPLATE].GetComponentsInChildren<Collider>();
            for (int i = 0; i < cols.Length; i++)
                GameObject.Destroy(cols[i]);
            var bodies = _owner.LevelObjs[Consts.ITEM_PASTAPLATE].GetComponentsInChildren<Rigidbody>();
            for (int i = 0; i < bodies.Length; i++)
                GameObject.Destroy(bodies[i]);

            GameObject pasta = new GameObject("Pasta");
            pasta.transform.SetParent(_owner.LevelObjs[Consts.ITEM_PASTAPLATE].transform);
            pasta.SetLocalPos(Vector3.zero);
            trsStuffs.ForEach(p =>
            {
                if (p.name != "Mesh")
                    p.SetParent(pasta.transform);
            });
            var pastaCol = pasta.AddComponent<BoxCollider>();
            pastaCol.size = Vector3.one * 7;
            pastaCol.center = Vector3.up * 3.5f;
            DishManager.Instance.ObjFinishedDish = _owner.LevelObjs[Consts.ITEM_PASTAPLATE];
        }
    }
}

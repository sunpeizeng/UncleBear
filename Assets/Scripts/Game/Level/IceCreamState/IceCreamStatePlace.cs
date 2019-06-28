using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class IceCreamStatePlace : State<LevelIceCream>
    {
        bool _bPlaced;
        public IceCreamStatePlace(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            Debug.Log("place");
            base.Enter(param);

            _bPlaced = false;
                
            _owner.LevelObjs[Consts.ITEM_ICTRAY].transform.DOMove(new Vector3(-45, 24.2f, -19.5f), 0.7f).OnComplete(SetPastaReady);
            CameraManager.Instance.DoCamTween(new Vector3(-14, 46.5f, 22), new Vector3(25, 180, 0), 1f, () =>
            {
                DishManager.Instance.ObjFinishedDish = _owner.LevelObjs[Consts.ITEM_ICBALLBOWL];
            });
        }

        public override string Execute(float deltaTime)
        {
            if (!_bPlaced)
                GameUtilities.LimitListPosition(_owner.BallDecors, 
                    _owner.LevelObjs[Consts.ITEM_ICBALLBOWL].transform.position, 3.5f);
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
        }

        void SetPastaReady()
        {
            DoozyUI.UIManager.PlaySound("9完成");
            var trsStuffs = _owner.LevelObjs[Consts.ITEM_ICBALLBOWL].transform.GetChildTrsList();
            var cols = _owner.LevelObjs[Consts.ITEM_ICBALLBOWL].GetComponentsInChildren<Collider>();
            for (int i = 0; i < cols.Length; i++)
                GameObject.Destroy(cols[i]);
            var bodies = _owner.LevelObjs[Consts.ITEM_ICBALLBOWL].GetComponentsInChildren<Rigidbody>();
            for (int i = 0; i < bodies.Length; i++)
                GameObject.Destroy(bodies[i]);

            _bPlaced = true;

            GameObject icecream = new GameObject("IceCream");
            icecream.transform.SetParent(_owner.LevelObjs[Consts.ITEM_ICBALLBOWL].transform);
            icecream.SetLocalPos(Vector3.zero);
            trsStuffs.ForEach(p =>
            {
                if (p.name != "Mesh")
                    p.SetParent(icecream.transform);
            });
            var icecreamCol = icecream.AddComponent<BoxCollider>();
            icecreamCol.isTrigger = true;
            icecreamCol.size = Vector3.one * 7;
            icecreamCol.center = Vector3.up * 3.5f;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class BurgerStatePlace : State<LevelBurger>
    {
        Vector3 _v3Center = new Vector3(-56.5f, 24.5f, -98);

        public BurgerStatePlace(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("Show");
            base.Enter(param);

            _owner.ObjChipsPlate.SetPos(_owner.LevelObjs[Consts.ITEM_BREAD].transform.position + Vector3.left * 100);

            //去掉薯条碰撞,归到带碰撞的父物体
            _owner.ObjChipsRoot.transform.SetParent(_owner.ObjChipsPlate.transform);
            _owner.ObjChipsRoot.SetLocalPos(Vector3.up);
            _owner.ObjChipsRoot.SetAngle(Vector3.zero);
            GameObject.Destroy(_owner.ObjChipsPlate.GetComponent<Collider>());
            var trsChips = _owner.ObjChipsPlate.transform.GetChildTrsList();
            trsChips.ForEach(p =>
            {
                GameObject.Destroy(p.GetComponent<Collider>());
                GameObject.Destroy(p.GetComponent<Rigidbody>());
                if (p.name != "Mesh")
                {
                    p.SetParent(_owner.ObjChipsRoot.transform);
                }
            });
            _owner.ObjChipsRoot.AddComponent<BoxCollider>().size = new Vector3(5, 2, 5);
            _owner.ObjChipsRoot.transform.localPosition = new Vector3(-5, 1, 0);

            GameObject.Destroy(_owner.ObjChipsPlate.transform.FindChild("Mesh/Dummy").GetComponent<Collider>());
            ReturnBreadTopBack();
        }

        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
        }

        void ReturnBreadTopBack()
        {
            //汉堡顶归位
            _owner.LevelObjs[Consts.ITEM_BREAD].SetRigidBodiesKinematic(true);
            //Debug.Log(GameUtilities.GetMeshMaxHeight(_owner.BurgerPieces));
            var breadTop = _owner.LevelObjs[Consts.ITEM_BREAD].transform.FindChild("Top");
            breadTop.DOLocalMove(new Vector3(0, 15, 0), 0.5f).OnComplete(() => {
                if (_owner.BurgerPieces.Count > 0)
                    breadTop.DOMoveY(GameUtilities.GetMeshMaxHeight(_owner.BurgerPieces) - 1.6f, 0.5f).OnComplete(SetBurgerReady);
                else
                    breadTop.DOLocalMoveY(0, 1f).OnComplete(SetBurgerReady);
            });

    
        }

        void SetBurgerReady()
        {
            //去掉汉堡碰撞,归到带碰撞的父物体
            var burgerCols = _owner.LevelObjs[Consts.ITEM_BREAD].GetComponentsInChildren<Collider>();
            for (int i = 0; i < burgerCols.Length; i++)
                GameObject.Destroy(burgerCols[i]);
            var burgerBodies = _owner.LevelObjs[Consts.ITEM_BREAD].GetComponentsInChildren<Rigidbody>();
            for (int i = 0; i < burgerBodies.Length; i++)
                GameObject.Destroy(burgerBodies[i]);
            var newBurgerCol = _owner.LevelObjs[Consts.ITEM_BREAD].AddComponent<BoxCollider>();
            newBurgerCol.size = Vector3.one * 7;
            newBurgerCol.center = Vector3.up * 3.5f;

            _owner.LevelObjs[Consts.ITEM_BREAD].transform.DOMove(_v3Center + new Vector3(3.5f, 4, 0), 0.5f).OnComplete(() => {
                _owner.ObjChipsPlate.transform.DOMove(_v3Center,0.5f).OnComplete(()=> {
                    _owner.LevelObjs[Consts.ITEM_BREAD].transform.SetParent(_owner.ObjChipsPlate.transform);
                    _owner.LevelObjs[Consts.ITEM_BREAD].transform.DOLocalMove(new Vector3(3.5f, 0, 0), 0.5f).OnComplete(()=> {
                        DishManager.Instance.ObjFinishedDish = _owner.ObjChipsPlate;
                        DoozyUI.UIManager.PlaySound("9完成");
                    });
                });
            });
        }
    }
}
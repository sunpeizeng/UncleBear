using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class CupCakeStatePlace : State<LevelCupCake> {

        GameObject _objPlate;

        public CupCakeStatePlace(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);

            var originPos = _owner.LevelObjs[Consts.ITEM_OVENPLATE].transform.position;
            _owner.LevelObjs[Consts.ITEM_OVENPLATE].SetPos(new Vector3(78, originPos.y, originPos.z));
            _objPlate = _owner.LevelObjs[Consts.ITEM_PLATE];
            _objPlate.SetPos(_owner.LevelObjs[Consts.ITEM_OVENPLATE].transform.position);
            _objPlate.transform.FindChild("Mesh").localScale = new Vector3(2, 2, 1);
            _objPlate.transform.FindChild("Mesh").GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector3(0.5f, 0);

            var cols = _objPlate.GetComponentsInChildren<Collider>();
            for (int i = 0; i < cols.Length; i++)
            {
                GameObject.Destroy(cols[i]);
            }

            _owner.Cupcakes.ForEach(p => p.transform.SetParent(_objPlate.transform));
            _owner.LevelObjs[Consts.ITEM_OVENPLATE].SetPos(Vector3.one * 500);

            FormPartsInCircle();
            _objPlate.SetRigidBodiesKinematic(true);
            CameraManager.Instance.DoCamTween(new Vector3(-14, 72.5f, 27.6f), new Vector3(45, 180, 0), 1f, ()=> {
                _objPlate.transform.DOMoveX(-14, 0.5f).OnComplete(() =>
                {
                    DishManager.Instance.ObjFinishedDish = _objPlate;
                    DoozyUI.UIManager.PlaySound("9完成");
                });

            });
            //CameraManager.Instance.DoCamTween(new Vector3(-55, 75, -52.3f), new Vector3(45, 180, 0), 0.5f, () =>
            //     {
                    
            //     });
            //GameObject cakeParent = new GameObject("Cakes");
            //Vector3 cakePos = Vector3.zero;
            //_owner.Cupcakes.ForEach(p => { cakePos += p.transform.position; });
            //cakeParent.SetPos(cakePos / _owner.Cupcakes.Count);
            //_owner.Cupcakes.ForEach(p => p.transform.SetParent(cakeParent.transform));

        }

        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
        }

        protected void FormPartsInCircle(float radius = 5f)
        {
            float perRad = 360f / _owner.Cupcakes.Count * Mathf.Deg2Rad;
            for (int i = 0; i < _owner.Cupcakes.Count; i++)
            {
                _owner.Cupcakes[i].transform.localPosition = new Vector3(radius * Mathf.Sin(perRad * i), 0, radius * Mathf.Cos(perRad * i));
                _owner.Cupcakes[i].transform.localEulerAngles = new Vector3(0, -90, 0); // new Vector3(0, perRad * Mathf.Rad2Deg * i, 0);
            }

        }
    }

}
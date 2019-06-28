using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public class PizzaStatePlace : State<LevelPizza>
    {
        Vector3 _v3PlatePos = new Vector3(-56.5f, 23.2f, -101);
        Vector3 _v3PlateScale = new Vector3(1.75f, 1.75f, 1);

        public PizzaStatePlace(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);
            _owner.LevelObjs[Consts.ITEM_PLATE].SetPos(_v3PlatePos);
            _owner.LevelObjs[Consts.ITEM_PLATE].transform.FindChild("Mesh").localScale = _v3PlateScale;

            var cols = _owner.LevelObjs[Consts.ITEM_PLATE].GetComponentsInChildren<Collider>();
            for (int i = 0; i < cols.Length; i++)
            {
                GameObject.Destroy(cols[i]);
            }

            _owner.LevelObjs[Consts.ITEM_PIZZA].SetRigidBodiesKinematic(true);
            _owner.LevelObjs[Consts.ITEM_PIZZA].transform.SetParent(_owner.LevelObjs[Consts.ITEM_PLATE].transform);
            DishManager.Instance.ObjFinishedDish = _owner.LevelObjs[Consts.ITEM_PLATE];

            cols = _owner.LevelObjs[Consts.ITEM_PIZZA].GetComponentsInChildren<MeshCollider>();
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i].gameObject.name.Contains("Body"))
                {
                    var boxSize = cols[i].GetComponent<MeshRenderer>().bounds.size;
                    var part = cols[i].transform.parent;
                    GameObject.Destroy(cols[i]);
                    if (boxSize.x < 4 && boxSize.z < 4)
                        continue;
                    var boxCol = part.gameObject.AddComponent<BoxCollider>();
                    boxCol.size = boxSize;
                    
                }
            }

            DoozyUI.UIManager.PlaySound("9完成");
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

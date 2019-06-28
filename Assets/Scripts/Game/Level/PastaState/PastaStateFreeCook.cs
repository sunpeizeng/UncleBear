using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public class PastaStateFreeCook : FreeCookingState<LevelPasta>
    {
        Vector3 _v3PlatePos = new Vector3(-27.5f, 22.5f, -20.5f);

        public PastaStateFreeCook(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("Free");
            base.Enter(param);

            _objFinalPlace = _owner.LevelObjs[Consts.ITEM_PASTAPLATE];
            _objFinalPlace.SetPos(_v3PlatePos);
            EnableDummy(true);
        }

        public override string Execute(float deltaTime)
        {
            List<Transform> trsList = new List<Transform>(_owner.LevelObjs[Consts.ITEM_PASTAPLATE].GetComponentsInChildren<Transform>());
            GameUtilities.LimitListPosition(trsList, _owner.LevelObjs[Consts.ITEM_PASTAPLATE].transform.position, 4);
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            EnableDummy(false);
            base.Exit();
        }

        void EnableDummy(bool state)
        {
            var trsChildren = _owner.LevelObjs[Consts.ITEM_PASTAPLATE].transform.FindChild("Mesh").GetChildTrsList();
            trsChildren.ForEach(p =>
            {
                if (p.gameObject != null && p.name == "Dummy")
                {
                    p.GetComponent<BoxCollider>().enabled = state;
                }
            });
        }
    }
}

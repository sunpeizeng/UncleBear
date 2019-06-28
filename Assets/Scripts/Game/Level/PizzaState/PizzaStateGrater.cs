using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    public class PizzaStateGrater : State<LevelPizza>
    {
        GraterCtrl _grater;

        Vector3 _v3CamPos = new Vector3(-49, 62, -71.5f);

        Vector3 _v3GraterPos = new Vector3(-83, 30, -71);
        Vector3 _v3GraterAngle = new Vector3(0, 120, 20);
        Vector3 _v3BowlPos = new Vector3(-83, 22.5f, -71);

        public PizzaStateGrater(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("Grater");

            _owner.LevelObjs[Consts.ITEM_GRATER].SetPos(_v3GraterPos);
            _owner.LevelObjs[Consts.ITEM_GRATER].SetAngle(_v3GraterAngle);
            _owner.LevelObjs[Consts.ITEM_BOWL].SetPos(_v3BowlPos);
            _grater = _owner.LevelObjs[Consts.ITEM_GRATER].AddMissingComponent<GraterCtrl>();
            _grater.RegisterObject(_owner.LevelObjs[Consts.ITEM_CHEESE], OnGraterFinish, _owner.LevelObjs[Consts.ITEM_CHEESESTICK], 24);

            CameraManager.Instance.DoCamTween(_v3CamPos, new Vector3(45, 270, 0), 0.5f, () =>
            {
                _grater.enabled = true;
            });
            base.Enter(param);
        }

        void OnGraterFinish(GameObject objSrc)
        {
            if (_grater.GenedDesObjs.Count > 0)
            {
                _grater.GenedDesObjs.ForEach(p =>
                {
                    p.transform.SetParent(_owner.LevelObjs[Consts.ITEM_BOWL].transform);
                    p.name = "PizzaCheese";
                });
            }
            DoozyUI.UIManager.PlaySound("8成功");
            _owner.LevelObjs[Consts.ITEM_GRATER].transform.DOMove(_v3GraterPos + new Vector3(0, 50, 0), 1f).OnComplete(() => {
                _owner.LevelObjs[Consts.ITEM_GRATER].transform.DOMove(Vector3.one * 500, 0.5f).OnComplete(() => {
                    StrStateStatus = "GraterOver";
                });
            });
        }

        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
            if (_grater != null)
                _grater.enabled = false;
            //CameraManager.Instance.PlayCamAnim("CP_Pizza2Cheese", true);
        }
    }
}

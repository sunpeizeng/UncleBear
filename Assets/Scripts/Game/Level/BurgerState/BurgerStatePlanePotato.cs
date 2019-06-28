using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    public class BurgerStatePlanePotato : State<LevelBurger>
    {
        //刨子控制
        GraterCtrl _grater;

    
        Vector3 _v3GraterPos = new Vector3(-56.5f, 29, -99);
        Vector3 _v3GraterAngle = new Vector3(0, 120, 20);
        Vector3 _v3BowlPos = new Vector3(-56.5f, 22.5f, -99);

        bool _bGraterReady;

        public BurgerStatePlanePotato(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            CameraManager.Instance.DoCamTween(new Vector3(-56.5f, 72.5f, -57f), new Vector3(45, 180, 0), 0.5f);
            //Debug.Log("Grater");

            _bGraterReady = false;

            _owner.LevelObjs[Consts.ITEM_GRATER].SetPos(_v3GraterPos);
            _owner.LevelObjs[Consts.ITEM_GRATER].SetAngle(_v3GraterAngle);
            _owner.LevelObjs[Consts.ITEM_BOWL].SetPos(_v3BowlPos);

            _grater = _owner.LevelObjs[Consts.ITEM_GRATER].AddMissingComponent<GraterCtrl>();
            _grater.enabled = true;
            _grater.RegisterObject(_owner.LevelObjs[Consts.ITEM_POTATO4CHIP], OnGraterFinish, _owner.LevelObjs[Consts.ITEM_POTATOCHIP], 15);

            base.Enter(param);
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
        }

        void OnGraterFinish(GameObject objSrc)
        {
            if (_grater.GenedDesObjs.Count > 0)
            {
                _grater.GenedDesObjs.ForEach(p =>
                {
                    p.transform.SetParent(_owner.LevelObjs[Consts.ITEM_BOWL].transform);
                    p.name = "Chips";
                });
            }
            DoozyUI.UIManager.PlaySound("8成功");
            _owner.LevelObjs[Consts.ITEM_GRATER].transform.DOMove(_v3GraterPos + new Vector3(0, 50, 0), 1.5f).OnComplete(() => { StrStateStatus = "GraterOver"; });
        }
    }
}

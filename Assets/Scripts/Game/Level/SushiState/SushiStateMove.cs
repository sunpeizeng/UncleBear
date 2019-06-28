using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Touch;

namespace UncleBear
{
    public class SushiStateMove : State<LevelSushi> {
        Vector3 _v3ShowPos = new Vector3(-8f, 22.5f, -18f);
        bool _bSushiReady;
        Transform _trsHolding;
        List<Transform> _lstSushiBodies = new List<Transform>();

        public SushiStateMove(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Input.multiTouchEnabled = false;
            //Debug.Log("place");
            base.Enter(param);
            _bSushiReady = false;

            _owner.LevelObjs[Consts.ITEM_BAMBOO].transform.DOMoveZ(500, 1f);
            _owner.LevelObjs[Consts.ITEM_CHOPBOARD].transform.DOMoveZ(500, 1f);
            _owner.LevelObjs[Consts.ITEM_SUSHISCROLL].transform.DOMoveY(27, 0.5f);
            _owner.LevelObjs[Consts.ITEM_SUSHIBOARD].transform.DOMove(_v3ShowPos, 1f).OnComplete(() =>
            {
                _bSushiReady = true;
                _lstSushiBodies = _owner.LevelObjs[Consts.ITEM_SUSHISCROLL].transform.GetChildTrsList();
                _lstSushiBodies.ForEach(p => p.GetComponent<Rigidbody>().isKinematic = false);

                _owner.LevelObjs[Consts.ITEM_SUSHISCROLL].transform.SetParent(_owner.LevelObjs[Consts.ITEM_SUSHIBOARD].transform);

                GuideManager.Instance.SetGuideClick(_owner.LevelObjs[Consts.ITEM_SUSHISCROLL].transform.position);

            });
        }
        public override string Execute(float deltaTime)
        {
            //或者用碰撞,效果差不多
            _lstSushiBodies.ForEach(p =>
            {
                var newPos = p.position;

                if (newPos.x > _v3ShowPos.x + 9)
                    newPos.x = _v3ShowPos.x + 9;
                else if (newPos.x < _v3ShowPos.x - 9)
                    newPos.x = _v3ShowPos.x - 9;
                if (newPos.y > _v3ShowPos.y + 15)
                    newPos.y = _v3ShowPos.y + 15;
                if (newPos.z > _v3ShowPos.z + 5)
                    newPos.z = _v3ShowPos.z + 5;
                else if (newPos.z < _v3ShowPos.z - 5)
                    newPos.z = _v3ShowPos.z - 5;

                p.position = newPos;
            });

            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            GameObject.Destroy(_owner.LevelObjs[Consts.ITEM_SUSHIBOARD].GetComponent<Collider>());
            _owner.LevelObjs[Consts.ITEM_SUSHISCROLL].SetRigidBodiesKinematic(true);
            //Input.multiTouchEnabled = true;
            _lstSushiBodies.Clear();
            base.Exit();

        }


        protected override void OnFingerDown(LeanFinger finger)
        {
            base.OnFingerDown(finger);

            if (_trsHolding != null || !_bSushiReady)
                return;
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.name.Contains("Body"))
            {
                _trsHolding = hit.collider.transform;
                _trsHolding.GetComponent<Rigidbody>().isKinematic = true;

                //if (_trsHolding.GetComponent<MeshRenderer>().bounds.size.x < 8)
                //    _trsHolding.DORotate(new Vector3(0, 0, 90), 0.3f);
                //else
                _trsHolding.DORotate(new Vector3(0, 0, 90), 0.3f);
                //_trsHolding.DORotate(new Vector3(0, 50, 0), 0.3f);
            }
        }
        protected override void OnFingerSet(LeanFinger finger)
        {
            base.OnFingerSet(finger);
            if (_trsHolding != null)
            {
                //位置跟随指针
                //_trsHolding.position = Vector3.Slerp(_trsHolding.position, GameUtilities.GetFingerTargetWolrdPos(finger, _trsHolding.position, 26), 20 * Time.deltaTime);
                var fingerWorldPos = GameUtilities.GetFingerTargetWolrdPos(finger, _trsHolding.position + new Vector3(0, -5, 0), _v3ShowPos.y + 14);
                _trsHolding.position = Vector3.Lerp(_trsHolding.position, fingerWorldPos, 20 * Time.deltaTime);

                //if (newPos.x > _v3ShowPos.x + 9)
                //    newPos.x = _v3ShowPos.x + 9;
                //else if (newPos.x < _v3ShowPos.x - 9)
                //    newPos.x = _v3ShowPos.x - 9;
                //if (newPos.z > _v3ShowPos.z + 5)
                //    newPos.z = _v3ShowPos.z + 5;
                //else if (newPos.z < _v3ShowPos.z - 5)
                //    newPos.z = _v3ShowPos.z - 5;

                // = newPos;
            }
        }
        protected override void OnFingerUp(LeanFinger finger)
        {
            base.OnFingerUp(finger);

            if (_trsHolding != null)
            {
                GuideManager.Instance.StopGuide();
                _trsHolding.GetComponent<Rigidbody>().isKinematic = false;
                _trsHolding = null;
                StrStateStatus = "MovedOk";
            }


        }
    }
}

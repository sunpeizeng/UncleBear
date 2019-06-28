using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Touch;

namespace UncleBear
{
    public class PizzaStateSprinkle : State<LevelPizza>
    {
        Vector3 _v3BowlPos = new Vector3(-69f, 25, -90.7f);

        GameObject _objBowl;
        List<GameObject> _lstSrc = new List<GameObject>();
        List<GameObject> _lstPicking = new List<GameObject>();
        bool _bPicking;
        float _fStartDis;
        int _nEachPickCount = 5;
        int _nTotalCheese;

        public PizzaStateSprinkle(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("sprinkle");
            _objBowl = _owner.LevelObjs[Consts.ITEM_BOWL];
            _objBowl.transform.DOMove(_v3BowlPos, 0.5f);

            for (int i = 0; i < _objBowl.transform.childCount; i++)
            {
                if (_objBowl.transform.GetChild(i).name.Contains("Cheese"))
                {
                    _lstSrc.Add(_objBowl.transform.GetChild(i).gameObject);
                }
            }
            _nTotalCheese = _lstSrc.Count;

            //Input.multiTouchEnabled = false;
            base.Enter(param);

            GuideManager.Instance.SetGuideSingleDir(_v3BowlPos, _owner.ObjPizzaBody.transform.position, true, true, 1.5f);
        }

        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            _lstSrc.Clear();
            base.Exit();

            //Input.multiTouchEnabled = true;
            _objBowl.transform.DOMove(Vector3.one * 500, 3f);
        }
        protected override void OnFingerDown(LeanFinger finger)
        {
            if (_nTotalCheese <= 0)
                return;
            if (!_bPicking)
            {
                RaycastHit hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
                if (hit.collider != null && (hit.collider.gameObject == _objBowl || hit.collider.transform.IsChildOf(_objBowl.transform)))
                {
                    _bPicking = true;
                    _lstPicking.Clear();
                    _fStartDis = Vector3.Distance(CameraManager.Instance.MainCamera.transform.position, _owner.ObjPizzaBody.transform.position);
                    int left = _lstSrc.Count > _nEachPickCount ? _nEachPickCount : _lstSrc.Count;
                    for (int i = 0; i < left; i++)
                    {
                        int ranIndex = Random.Range(0, _lstSrc.Count);
                        _lstSrc[ranIndex].GetComponent<Rigidbody>().isKinematic = true;
                        _lstPicking.Add(_lstSrc[ranIndex]);
                        _lstSrc.RemoveAt(ranIndex);
                    }
                }
            }
        }

        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_bPicking && _lstPicking.Count > 0)
            {
                var fingerWorldPos = GameUtilities.GetFingerTargetWolrdPos(finger, _lstPicking[0].transform.position + new Vector3(0, -5, 0), _owner.LevelObjs[Consts.ITEM_PIZZA].transform.position.y + 5);
                for (int i = 0; i < _lstPicking.Count; i++)
                {
                    _lstPicking[i].transform.position = Vector3.Lerp(_lstPicking[i].transform.position, fingerWorldPos, 20 * Time.deltaTime);
                }
            }
        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            var fingerWorldPos = GameUtilities.GetFingerTargetWolrdPos(finger, _owner.LevelObjs[Consts.ITEM_PIZZA], _owner.LevelObjs[Consts.ITEM_PIZZA].transform.position.y + 5);
            RaycastHit hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition), 1000, 1<<LayerMask.NameToLayer("Cuttable"));
            //这里加了一个层,用来标识菜的主体
            float vecDis = Vector2.Distance(new Vector2(hit.point.x, hit.point.z), new Vector2(_owner.ObjPizzaBody.transform.position.x, _owner.ObjPizzaBody.transform.position.z));
            if (hit.collider != null && vecDis < _owner.PizzaRadius + 1.5f && hit.collider.gameObject == _owner.ObjPizzaBody)
            {
                if (_lstPicking.Count > 0)
                    StrStateStatus = "SprinkleOk";
                DoozyUI.UIManager.PlaySound("14撒配料", _owner.ObjPizzaBody.transform.position, false, 1f, 0.5f);
                for (int i = 0; i < _lstPicking.Count; i++)
                {
                    _lstPicking[i].GetComponent<Rigidbody>().isKinematic = false;
                    _lstPicking[i].transform.position += new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                    _owner.AdditivesToPizza(_lstPicking[i]);
                    _nTotalCheese -= 1;
                    //测试全部撒完结束
                    if (_nTotalCheese <= 0)
                        StrStateStatus = "SprinkleOver";
                }
            }
            else
            {
                for (int i = 0; i < _lstPicking.Count; i++)
                {
                    _lstPicking[i].GetComponent<Rigidbody>().isKinematic = false;
                    _lstPicking[i].transform.DOMove(_objBowl.transform.position + new Vector3(Random.Range(-1f, 1f), 5, Random.Range(-1f, 1f)), 0.2f);
                    _lstSrc.Add(_lstPicking[i]);
                }
            }
            _bPicking = false;
            _lstPicking.Clear();
        }
    }
}

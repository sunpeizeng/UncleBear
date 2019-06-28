using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    public class PizzaStateSliceUp : State<LevelPizza>
    {
        LeanCutter _cutter;
        //能切几刀
        int _nCutLimit = 4;
        int _nCutCount;
        GameObject _objBlade;

        Vector3 _v3BoardPos = new Vector3(-56.5f, 22.5f, -101);


        public PizzaStateSliceUp(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("slice");
            base.Enter(param);
            _owner.LevelObjs[Consts.ITEM_CHOPBOARD].name = "TableSurface";
            _owner.LevelObjs[Consts.ITEM_CHOPBOARD].SetPos(_v3BoardPos);

            var tableCol = _owner.LevelObjs[Consts.ITEM_CHOPBOARD].AddComponent<BoxCollider>();
            tableCol.size = new Vector3(100, 3, 100);
            tableCol.isTrigger = true;

            _objBlade = _owner.LevelObjs[Consts.ITEM_BLADE];
            _nCutCount = 0;
            _cutter = _owner.LevelObjs[Consts.ITEM_PIZZA].AddMissingComponent<LeanCutter>();
            _cutter.SetParams(_owner.LevelObjs[Consts.ITEM_PIZZA].transform, _owner.LevelObjs[Consts.ITEM_PIZZA].transform.position, "Cuttable", false, OnPizzaCutted, OnMoveCutter);

            _owner.LevelObjs[Consts.ITEM_PIZZA].SetPos(new Vector3(-14, 26, -73));//一个随便看不到的高位置
            _owner.LevelObjs[Consts.ITEM_PIZZA].transform.DOMove(_v3BoardPos + Vector3.up * 2, 0.5f);
            CameraManager.Instance.DoCamTween(new Vector3(-56.5f, 70, -63), new Vector3(50, 180, 0), 0.5f, ()=> {
                _cutter.enabled = true;
            });

            GuideManager.Instance.SetGuideSingleDir(_v3BoardPos + Vector3.right * 10, _v3BoardPos - Vector3.right * 10, true, true, 2f);
        }

        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            HandlePizzaPieces();
            base.Exit();
            _cutter.enabled = false;

        }

        //根据披萨的半径计算刀子的移动轨迹
        void OnMoveCutter(Vector3 disVec, Plane splitPlane)
        {
            Vector3 startPos, endPos;
            float disToCenter = splitPlane.GetDistanceToPoint(_owner.LevelObjs[Consts.ITEM_PIZZA].transform.position);

            if (Mathf.Abs(disToCenter) < _owner.PizzaRadius)
            {
                float sqrDisLine = Mathf.Sqrt(_owner.PizzaRadius * _owner.PizzaRadius - disToCenter * disToCenter);
                Vector3 line2Center = _owner.LevelObjs[Consts.ITEM_PIZZA].transform.position - splitPlane.normal * disToCenter;
                startPos = line2Center - disVec * sqrDisLine / disVec.magnitude + Vector3.up * 4.5f;
                endPos = line2Center + disVec * sqrDisLine / disVec.magnitude + Vector3.up * 4.5f;
                _objBlade.transform.forward = disVec.normalized * -1;
                _objBlade.transform.DOMove(startPos, 0.2f).OnComplete(() => {
                    _objBlade.transform.DOMove(endPos, 0.4f).OnComplete(() =>
                    {
                        _objBlade.transform.position = Vector3.one * 500;
                    });
                });
            }
        }

        //!!!这里需要注意pizza饼的碰撞框,目前用的是meshCollider,否则切出来的边角料向下打射线时会打到错误的饼,相应的性能可能有影响
        void OnPizzaCutted(bool state)
        {
            if (state)
            {
                _nCutCount += 1;
                //Debug.Log(_nCutCount);
                if (_nCutCount >= _nCutLimit)
                {
                    _cutter.enabled = false;
                    StrStateStatus = "PizzaCuttedOver";
                }
                else StrStateStatus = "PizzaCuttedOk";
            }
            else
                StrStateStatus = null;
        }

        void HandlePizzaPieces()
        {
            var trsChildren = _owner.LevelObjs[Consts.ITEM_PIZZA].transform.GetChildTrsList();
            for (int i = 0; i < trsChildren.Count; i++)
            {
                if (trsChildren[i].name.Contains("Body"))
                {
                    if (trsChildren[i].gameObject.GetMeshSize().sqrMagnitude < 2)
                        GameObject.Destroy(trsChildren[i].gameObject);
                    else
                    {
                        GameObject partObj = new GameObject("Part");
                        partObj.transform.SetParent(_owner.LevelObjs[Consts.ITEM_PIZZA].transform);
                        partObj.SetPos(trsChildren[i].gameObject.GetMeshCenter());
                        trsChildren[i].SetParent(partObj.transform);
                    }
                }
                else
                {
                    var hits = GameUtilities.GetRaycastAllHitInfo(trsChildren[i].gameObject.GetMeshCenter(), Vector3.down, 10, 1 << LayerMask.NameToLayer("Cuttable"));
                    var hittedPizzaBody = false;
                    if (hits != null && hits.Length > 0)
                    {
                        for (int j = 0; j < hits.Length; j++)
                        {
                            if (hits[j].collider != null && hits[j].collider.gameObject.name.Contains("Body"))
                            {
                                hittedPizzaBody = true;
                                trsChildren[i].SetParent(hits[j].collider.transform);
                                GameObject.Destroy(trsChildren[i].GetComponent<BoxCollider>());
                            }
                        }
                    }
                    if (!hittedPizzaBody)
                        GameObject.Destroy(trsChildren[i].gameObject);
                }
            }
        }
    }
}

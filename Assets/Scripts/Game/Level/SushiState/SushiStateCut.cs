using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class SushiStateCut : State<LevelSushi>
    {
        int _nCutLimit = 5;
        int _nCutCount;
        LeanCutter _cutter;
        bool _bCutOver;

        public SushiStateCut(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);
            _nCutCount = 0;
            var scrollPos = _owner.LevelObjs[Consts.ITEM_SUSHISCROLL].transform.position;

            _cutter = _owner.LevelObjs[Consts.ITEM_SUSHISCROLL].AddMissingComponent<LeanCutter>();

            _cutter.SetParams(_owner.LevelObjs[Consts.ITEM_SUSHISCROLL].transform,
                scrollPos, "Cuttable", true, RecordCut);
            _cutter.enabled = true;
            _bCutOver = false;
            GuideManager.Instance.SetGuideSingleDir(scrollPos - Vector3.forward * 8, scrollPos + Vector3.forward * 5);
        }
        public override string Execute(float deltaTime)
        {
            if (_bCutOver)
                return "CutOver";

            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            var trsChildren = _owner.LevelObjs[Consts.ITEM_SUSHISCROLL].transform.GetChildTrsList();
            for (int i = 0; i < trsChildren.Count; i++)
            {
                if (trsChildren[i].name.Contains("Body"))
                {
                    //重新定位
                    MeshFilter filter = trsChildren[i].GetComponent<MeshFilter>();
                    var centerPos = filter.mesh.bounds.center;
                    var deltaPos = filter.mesh.bounds.center - filter.transform.localPosition;
                    var rensVertices = filter.mesh.vertices;
                    for (int j = 0; j < rensVertices.Length; j++)
                        rensVertices[j] += Vector3.left * deltaPos.x;
                    filter.mesh.vertices = rensVertices;
                    filter.mesh.RecalculateBounds();
                    filter.transform.localPosition = new Vector3(centerPos.x, filter.transform.localPosition.y, filter.transform.localPosition.z);
                    var col = filter.GetComponent<BoxCollider>();
                    if (col != null)
                    {
                        GameObject.DestroyImmediate(col);
                        filter.gameObject.AddComponent<BoxCollider>();
                    }

                    trsChildren[i].localScale -= Vector3.right * 0.02f;
                    trsChildren[i].gameObject.AddMissingComponent<Rigidbody>().isKinematic = true;
                    //GameObject partObj = new GameObject("Part");
                    //partObj.transform.SetParent(_owner.LevelObjs[Consts.ITEM_SUSHISCROLL].transform);
                    //partObj.SetPos(trsChildren[i].gameObject.GetMeshCenter());
                    //trsChildren[i].SetParent(partObj.transform);
                }
                else
                {
                    var hits = GameUtilities.GetRaycastAllHitInfo(trsChildren[i].gameObject.GetMeshCenter() + Vector3.up * 5, Vector3.down, 10, 1 << LayerMask.NameToLayer("Cuttable"));
                    var hittedBody = false;
                    if (hits != null && hits.Length > 0)
                    {
                        for (int j = 0; j < hits.Length; j++)
                        {
                            if (hits[j].collider != null && hits[j].collider.gameObject.name.Contains("Body"))
                            {
                                hittedBody = true;
                                trsChildren[i].SetParent(hits[j].collider.transform);
                                GameObject.Destroy(trsChildren[i].GetComponent<BoxCollider>());
                            }
                        }
                    }
                    if (!hittedBody)
                        GameObject.Destroy(trsChildren[i].gameObject);
                }
            }

            base.Exit();
            _cutter.enabled = false;
        }


        void RecordCut(bool state)
        {
            if (state)
            {
                GuideManager.Instance.StopGuide();
                _nCutCount += 1;
                StrStateStatus = "SushiCuttedOk";
                if (_nCutCount >= _nCutLimit)
                {
                    _cutter.enabled = false;
                    DoozyUI.UIManager.PlaySound("8成功");

                    StrStateStatus = "CutOver";
                }
            }
            else
                StrStateStatus = null;
          
        }

    }
}

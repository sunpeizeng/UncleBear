using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;
using ShatterToolkit;

namespace UncleBear
{
    public class LeanCutterFree : MonoBehaviour
    {
        Camera _mainCam;
        TrailRenderer _trail;


        Transform _trsRoot;
        string _strTarLayer;

        bool _bLimitDir;
        bool _bBeginCut;

        public System.Action<Vector3> OnCut;

        public void SetParams(Transform root, string layerName, bool limitDir = true)
        {
            _trsRoot = root;
            _strTarLayer = layerName;

            for (int i = 0; i < root.childCount; i++)
            {
                if (root.GetChild(i).gameObject.layer == LayerMask.NameToLayer(layerName))
                {
                    if (root.GetChild(i).gameObject.GetComponent<TargetUvMapper>() == null)
                        root.GetChild(i).gameObject.AddMissingComponent<WorldUvMapper>();
                    root.GetChild(i).gameObject.AddMissingComponent<ShatterTool>();
                    root.GetChild(i).gameObject.AddMissingComponent<CutterTimer>();
                }
            }
            //_objTarget.AddComponent<WorldUvMapper>();
            //_objTarget.AddComponent<ShatterTool>();
            _bLimitDir = limitDir;
        }

        void Update()
        {
        }

        void OnEnable()
        {
            _mainCam = CameraManager.Instance.MainCamera;
            _trail = GameObject.Find("CutLineFree").GetComponent<TrailRenderer>();

            LeanTouch.OnFingerDown += SetCutBegin;
            LeanTouch.OnFingerSet += TickCutPos;
            LeanTouch.OnFingerUp += SetCutEnd;
        }

        void OnDisable()
        {
            SetCutEnd(null);

            LeanTouch.OnFingerDown -= SetCutBegin;
            LeanTouch.OnFingerSet -= TickCutPos;
            LeanTouch.OnFingerUp -= SetCutEnd;
        }

        void SetCutBegin(LeanFinger finger)
        {
            //Input.multiTouchEnabled = false;
            //拖尾粒子
            _bBeginCut = true;

        }
        void TickCutPos(LeanFinger finger)
        {
            if (!_bBeginCut) return;

            if (finger.ScreenDelta.sqrMagnitude < 25)
                return;

            _trail.transform.position = GetFingerWorldPos(finger);
            float near = _mainCam.nearClipPlane;
            var lastPos = finger.LastScreenPosition;
            Vector3 line = _mainCam.ScreenToWorldPoint(new Vector3(finger.ScreenPosition.x, finger.ScreenPosition.y, near))
                - _mainCam.ScreenToWorldPoint(new Vector3(lastPos.x, lastPos.y, near));

            // Find game objects to split by raycasting at points along the line

            Ray ray = _mainCam.ScreenPointToRay(Vector3.Lerp(lastPos, finger.ScreenPosition, 0.5f));
            RaycastHit hit = GameUtilities.GetRaycastHitInfo(ray, 1000, 1 << LayerMask.NameToLayer(_strTarLayer));
            if (hit.collider != null)
            {
                bool haveCut = false;
                Plane splitPlane = _bLimitDir ? new Plane(hit.point + Vector3.up, hit.point - Vector3.back, hit.point) : new Plane(Vector3.Normalize(Vector3.Cross(line, ray.direction)), hit.point);
                //切割物为一个整体,碰撞到一个就切所有
                ShatterTool tool = hit.collider.GetComponent<ShatterTool>();

                if (tool != null && !tool.GetComponent<CutterTimer>().bCutLimiting)
                {
                    var originParent = tool.transform.parent;
                    var originPos = tool.transform.position;
                    var splitedObjs = tool.Split(new Plane[] { splitPlane });
                    if (splitedObjs != null)
                    {
                        if(splitedObjs.Count > 1)
                            haveCut = true;
                        splitedObjs.ForEach(p =>
                        {
                            p.name = "Part";
                            p.GetComponent<CutterTimer>().ActiveTimer();
                            p.transform.localPosition = originPos;
                            p.transform.SetParent(originParent);
                        });
                    }
                }

                if (haveCut && OnCut != null) OnCut.Invoke(hit.point);
            }

        }

        void CleanRedundantPart(GameObject partObj)
        {
            if (partObj != null)
            {
                if (partObj.transform.childCount == 0)
                    GameObject.Destroy(partObj);
            }
        }

        void SetCutEnd(LeanFinger finger)
        {
            //Input.multiTouchEnabled = true;
            //隐藏拖尾粒子
            _bBeginCut = false;
        }

        Vector3 GetFingerWorldPos(LeanFinger finger)
        {
            //需要另外一个正交相机
            return finger.GetWorldPosition(1);
        }
    }

    //记录一下,如果有切割类似披萨的东西,用回来,但是翻炒会有位置问题
    /*
     *   if (hit.collider != null)
            {
                bool haveCut = false;
                Plane splitPlane = _bLimitDir ? new Plane(hit.point + Vector3.up, hit.point - Vector3.back, hit.point) : new Plane(Vector3.Normalize(Vector3.Cross(line, ray.direction)), hit.point);
                GameObject newPartObj = null;
                GameObject originPartObj = null;
                //切割物为一个整体,碰撞到一个就切所有
                ShatterTool[] tools = hit.collider.transform.parent.gameObject.GetComponentsInChildren<ShatterTool>();
                if (tools.Length > 0)
                {
                    newPartObj = new GameObject("Part");
                    newPartObj.transform.SetParent(_trsRoot);
                    newPartObj.transform.localPosition = Vector3.zero;
                }
                for (int j = 0; j < tools.Length; j++)
                {
                    if (tools[j].GetComponent<CutterTimer>().bCutLimiting)
                        continue;
                    var originParent = tools[j].transform.parent;
                    originPartObj = originParent.gameObject;
                    var originPos = tools[j].transform.position;
                    var splitedObjs = tools[j].Split(new Plane[] { splitPlane });
                    if (splitedObjs == null) continue;
                    haveCut = true;
                    splitedObjs.ForEach(p =>
                    {
                        p.GetComponent<CutterTimer>().ActiveTimer();
                        p.transform.localPosition = originPos;
                        if (!splitPlane.GetSide(GameUtilities.GetMeshCenter(p)))//IfOnTheNewSide(hit.point, p))
                                p.transform.SetParent(newPartObj.transform);
                        else
                            p.transform.SetParent(originParent);
                    });
                }

                //把剩下的归在一起
                var partObj = new GameObject("Part");
                partObj.transform.SetParent(_trsRoot);
                partObj.transform.localPosition = Vector3.zero;
                var trsList = new List<Transform>();
                for (int m = 0; m < _trsRoot.childCount; m++)
                {
                    if (_trsRoot.GetChild(m).name != "Part" && _trsRoot.GetChild(m).gameObject.layer == LayerMask.NameToLayer(_strTarLayer))
                    {
                        trsList.Add(_trsRoot.GetChild(m));
                    }
                }
                if (trsList.Count > 0)
                    trsList.ForEach(p => p.SetParent(partObj.transform));
                else
                {
                    GameObject.Destroy(partObj);
                    partObj = null;
                }

                CleanRedundantPart(newPartObj);
                CleanRedundantPart(partObj);
                CleanRedundantPart(originPartObj);

                if (haveCut && OnCut != null) OnCut.Invoke();
            }
            */


}

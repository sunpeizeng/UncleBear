using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;
using ShatterToolkit;

namespace UncleBear
{
    public class LeanCutter : MonoBehaviour
    {
        Camera _mainCam;

        Transform _trsRoot;
        string _strTarLayer;

        //切割线
        LineRenderer _commonLine;
        //侦查点数量
        int _nCheckTarRayCount = 10;
        //手指起始点
        Vector2 _v2StartPos;
        Vector3 _v3targetPos;
        bool _bStartCutter;
        bool _bLimitDir;
        //每切一刀锁住一些动画时间
        bool _bLockCutter;

        float _fCutterDelay;

        System.Action<bool> OnCutCallback;
        System.Action<Vector3, Plane> OnMoveCutterObj;

        public void SetParams(Transform root, Vector3 middlePos, string layerName, bool limitDir = true, System.Action<bool> cutCallback = null, System.Action<Vector3, Plane> cutterMover = null)
        {
            _trsRoot = root;
            _strTarLayer = layerName;

            _v3targetPos = middlePos;
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

            _bLockCutter = false;
            _bLimitDir = limitDir;
            OnCutCallback = cutCallback;
            OnMoveCutterObj = cutterMover;
            _fCutterDelay = OnMoveCutterObj == null ? 0 : 0.3f;
        }

        void OnEnable()
        {
            _mainCam = CameraManager.Instance.MainCamera;
            _commonLine = GameObject.Find("CutLine").GetComponent<LineRenderer>();

            LeanTouch.OnFingerDown += SetCutBegin;
            LeanTouch.OnFingerSet += TickCutPos;
            LeanTouch.OnFingerUp += SetCutEnd;
        }

        void OnDisable()
        {
            LeanTouch.OnFingerDown -= SetCutBegin;
            LeanTouch.OnFingerSet -= TickCutPos;
            LeanTouch.OnFingerUp -= SetCutEnd;
        }

        void SetCutBegin(LeanFinger finger)
        {
            if(_bLockCutter)
                return;

            //Input.multiTouchEnabled = false;
            _v2StartPos = finger.ScreenPosition;
            _commonLine.SetPosition(0, GetFingerWorldPos(finger));
            _commonLine.SetPosition(1, GetFingerWorldPos(finger));
            _bStartCutter = true;
        }
        void TickCutPos(LeanFinger finger)
        {
            if (_bStartCutter)
                _commonLine.SetPosition(1, GetFingerWorldPos(finger));
        }
        void SetCutEnd(LeanFinger finger)
        {
            if (!_bStartCutter)
                return;
            else
                _bStartCutter = false;
            //Input.multiTouchEnabled = true;
            if (Vector2.Distance(_v2StartPos, finger.ScreenPosition) < 0.1f)
                return;
            float near = _mainCam.nearClipPlane;
            Vector3 line = GetPosOnTable(finger.ScreenPosition) - GetPosOnTable(_v2StartPos);

            // Find game objects to split by raycasting at points along the line
            for (int i = 0; i < _nCheckTarRayCount; i++)
            {
                Ray ray = _mainCam.ScreenPointToRay(Vector3.Lerp(_v2StartPos, finger.ScreenPosition, (float)i / _nCheckTarRayCount));
                RaycastHit hit = GameUtilities.GetRaycastHitInfo(ray, 1000, 1 << LayerMask.NameToLayer(_strTarLayer));
                if (hit.collider != null)
                {
                    Plane splitPlane = _bLimitDir ? new Plane(hit.point + Vector3.up, hit.point - Vector3.back, hit.point) : new Plane(hit.point + line.normalized, hit.point + Vector3.up, hit.point);
                    //切割物为一个整体,碰撞到一个就切所有
                    ShatterTool[] tools = _trsRoot.GetComponentsInChildren<ShatterTool>();

                    if (tools.Length > 0)
                    {
                        bool allCd = true;
                        for (int j = 0; j < tools.Length; j++)
                        {
                            if (!tools[j].GetComponent<CutterTimer>().bCutLimiting)
                            {
                                allCd = false;
                                break;
                            }
                        }

                        if (!allCd)
                        {
                            for (int j = 0; j < tools.Length; j++)
                            {
                                if (tools[j].GetComponent<CutterTimer>().bCutLimiting)
                                    continue;
                                var originPos = tools[j].transform.position;
                                var splitedObjs = tools[j].Split(new Plane[] { splitPlane });
                                if (splitedObjs != null)
                                {
                                    splitedObjs.ForEach(p =>
                                    {
                                        p.transform.SetParent(_trsRoot);
                                        p.transform.position = originPos;
                                        p.GetComponent<CutterTimer>().ActiveTimer();

                                        if (splitPlane.GetSide(p.GetMeshCenter()))
                                            p.transform.DOLocalMove(p.transform.localPosition + splitPlane.normal * 0.1f, 0.2f).SetDelay(_fCutterDelay);
                                        else
                                            p.transform.DOLocalMove(p.transform.localPosition - splitPlane.normal * 0.1f, 0.2f).SetDelay(_fCutterDelay);
                                    });
                                }

                            }

                            _bLockCutter = true;
                            if (OnMoveCutterObj != null)
                                OnMoveCutterObj.Invoke(line, splitPlane);
                            DoozyUI.UIManager.PlaySound("19切披萨、寿司", transform.position);
                            StartCoroutine(InvokeCutCallBack());
                            break;
                        }
                    }
                }
            }
            _commonLine.SetPosition(0, Vector3.zero);
            _commonLine.SetPosition(1, Vector3.zero);
        }

        IEnumerator InvokeCutCallBack()
        {
            if (OnCutCallback != null)
            {
                OnCutCallback.Invoke(false);
            }
            yield return new WaitForSeconds(0.4f + _fCutterDelay);
            if (OnCutCallback != null)
            {
                OnCutCallback.Invoke(true);
            }
            _bLockCutter = false;
        }

        Vector3 GetFingerWorldPos(LeanFinger finger)
        {
            //需要另外一个正交相机
            return finger.GetWorldPosition(1);
        }

        Vector3 GetPosOnTable(Vector2 screenPos)
        {
            var hits = GameUtilities.GetRaycastAllHitInfo(_mainCam.ScreenPointToRay(screenPos));
            if (hits != null)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].collider != null && hits[i].collider.gameObject.name.Contains("TableSurface"))
                        return hits[i].point;
                }
            }
            return _mainCam.ScreenToWorldPoint(screenPos);
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;

namespace UncleBear
{
    public class LeanGestureCircle : MonoBehaviour
    {
        private Camera _camMain;

        private GameObject _objCircleTarget;
        //方向
        private bool _bIgnoreClockWise;
        private bool _bIsClockWise;
        //半径
        private float _fWorldRaius;
        private float _fScreenRadius;
        private float _fScreenRadiusHollow;
        private float _fScreenRadiusFix = 10;
        private bool _bCircleInRadius;

        private bool _bTargetOnRadius = true;
        //旋转系数,60帧的话一次转8度多
        private float _fRotateFactor = 500f;
        //屏幕中心
        private Vector3 _v3WorldCenter;
        private Vector3 _v3ScreenCenter;

        //算法参数
        private float _fSampleDisThreshold;
        private List<Vector3> _inputGesturePhases = new List<Vector3>();
        private float _fLastZ;

        private bool _bHitting;
        private float _fTotalRotate;
        private float _fCurAngle;
        private float _fDeltaAngle;
        public float fDeltaAngle { get { return _fDeltaAngle; } }
        public float fCurAngle { get { return _fCurAngle; } }
        public float fTotalAngle { get { return _fTotalRotate; } }
        private Vector3 _v3CurWorldPosOnCircle;

        public System.Action OnOutOfRadius;
        public System.Action OnRotateReverse;
        public System.Action OnRotateBreak;
        public System.Action OnRotateFinish;
        public float fRoundPerc
        {
            get
            {
                return Mathf.Clamp01(Mathf.Abs(_fTotalRotate) / 360f);
            }
        }

        // Use this for initialization
        public void SetParams(GameObject targetObj, Vector3 wolrdCenter, float worldRadius, bool fingerRadiusLimit = true, 
            bool ignoreDir = true, bool clockWise = true)
        {
            _camMain = CameraManager.Instance.MainCamera;

            _objCircleTarget = targetObj;

            _bIgnoreClockWise = ignoreDir;
            _bIsClockWise = clockWise;

            _fWorldRaius = worldRadius;
            _fScreenRadius = Vector3.Distance(_camMain.WorldToScreenPoint(_v3WorldCenter + new Vector3(worldRadius, 0, 0)), _v3ScreenCenter);
            _bCircleInRadius = fingerRadiusLimit;

            _v3WorldCenter = wolrdCenter;
            _v3ScreenCenter = _camMain.WorldToScreenPoint(_v3WorldCenter);

            _bHitting = false;
            _fCurAngle = _fDeltaAngle =  _fTotalRotate = 0;
            _fLastZ = 0;
            _inputGesturePhases.Clear();
        }

        void OnEnable()
        {
            LeanTouch.OnFingerDown += OnFingerDown;
            LeanTouch.OnFingerSet += OnFingerSet;
            LeanTouch.OnFingerUp += OnFingerUp;
        }

        void OnDisable()
        {
            LeanTouch.OnFingerDown -= OnFingerDown;
            LeanTouch.OnFingerSet -= OnFingerSet;
            LeanTouch.OnFingerUp -= OnFingerUp;
        }

        void OnFingerDown(LeanFinger finger)
        {
            //Input.multiTouchEnabled = false;
            if (_objCircleTarget != null)
            {
                    RaycastHit hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
                    if (hit.collider != null && hit.collider.gameObject == _objCircleTarget)
                        _bHitting = true;
            }
        }

        void OnFingerSet(LeanFinger finger)
        {
            if (_bHitting)
            {
                _fDeltaAngle = 0;
                float disFinger = Vector3.Distance(_v3ScreenCenter, finger.ScreenPosition);
                if (disFinger > _fScreenRadiusHollow)
                {
                    if (_bCircleInRadius && disFinger > _fScreenRadius + _fScreenRadiusFix)
                    {
                        _inputGesturePhases.Clear();
                        if (OnOutOfRadius != null)
                        {
                            Debug.Log("OutRadius");
                            OnOutOfRadius.Invoke();
                        }
                    }
                    else
                    {
                        //获得0.5秒内手指一动距离
                        var recentDelta = finger.GetSnapshotScreenDelta(0.5f);
                        if (recentDelta.sqrMagnitude > LeanTouch.Instance.RecordThreshold * LeanTouch.Instance.RecordThreshold)//超过阈值,记录一下
                        {
                            _inputGesturePhases.Add(recentDelta);
                            if (_inputGesturePhases.Count > 1)
                            {
                                int curCount = _inputGesturePhases.Count;
                                float multiDot = Vector3.Dot(_inputGesturePhases[curCount - 1], _inputGesturePhases[curCount - 2]);
                                Vector3 multiCross = Vector3.Cross(_inputGesturePhases[curCount - 1], _inputGesturePhases[curCount - 2]);
                                if (multiDot <= 0)//画圆只能是锐角
                                {
                                    _inputGesturePhases.Clear();
                                }
                                else if (!_bIgnoreClockWise && (multiCross.z == 0 || (multiCross.z > 0 && !_bIsClockWise) || (multiCross.z < 0 && _bIsClockWise)))//叉积右手法则,顺时针后一条叉前一条,z应该是正,z是0表示平行
                                {
                                    _inputGesturePhases.Clear();
                                }
                                else if (_bIgnoreClockWise && (multiCross.z == 0 || multiCross.z * _fLastZ < 0))
                                {
                                    _inputGesturePhases.Clear();
                                    if (OnRotateReverse != null)
                                        OnRotateReverse.Invoke();
                                    _fTotalRotate = 0;
                                }
                                else
                                {
                                    _fDeltaAngle = Vector3.Angle(_inputGesturePhases[curCount - 1], _inputGesturePhases[curCount - 2]);
                                    _fTotalRotate += _fDeltaAngle;
                                    if (_fTotalRotate > 360)
                                    {
                                        if (OnRotateFinish != null)
                                            OnRotateFinish.Invoke();
                                        _fTotalRotate = 0;
                                    }
                                    if (multiCross.z < 0)
                                        _fDeltaAngle *= -1;

                                    _fCurAngle += _fDeltaAngle;
                                }

                                _fLastZ = multiCross.z;
                            }
                        }
                    }
                }
                else
                {
                    _inputGesturePhases.Clear();//乱画就清除路径,重新来过
                }
            }
        }

        void OnFingerUp(LeanFinger finger)
        {
            //Input.multiTouchEnabled = true;
            _bHitting = false;
            if (OnRotateBreak != null)
                OnRotateBreak.Invoke();
        }
    }
}
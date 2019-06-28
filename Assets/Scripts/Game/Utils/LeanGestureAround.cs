using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;

namespace UncleBear
{

    [System.Obsolete("这个类有问题,留着做参考,请用LeanGesturCircle代替")]
    public class LeanGestureAround : MonoBehaviour
    {
        //如果忽略顺时针逆时针限制,那么反向旋转会重置值而不是无视
        private bool _bIgnoreClockWise;
        private bool _bIsClockWise;
        private GameObject _objRotateTarget;
        private float _fGestureRadius = 50;//做的好一点要拿屏幕的大小做一个比例
        private float _fGestureRadiusFix = 10;
        private float _fRotateFactor = 500f;//旋转系数,60帧的话一次转8度多

        private Vector3 _v3AroundCenterPoint;
        private bool _bNeedHit;
        private bool _bLimitRadius;
        private bool _bGesturing;
        private float _fSampleDisThreshold;
        private List<Vector3> _inputGesturePhases = new List<Vector3>();
        private float _fLastZ;

        private float _fTotalRotate;

        public System.Action OnRotateFinish;
        public float fRoundPerc
        {
            get {
                return Mathf.Clamp01(Mathf.Abs(_fTotalRotate) / 360f);
            }
        }
        // Use this for initialization
        public void SetParams(Vector3 centerScreenPoint, float aroundRadius, bool upLimit = true, GameObject targetObj = null, bool needHit = false,
            bool ignoreDir = true, bool isClockWise = true, float radiusFix = 10, float rotateSpeed = 600)
        {
            _v3AroundCenterPoint = centerScreenPoint;
            _fGestureRadius = aroundRadius;
            _objRotateTarget = targetObj;
            _bIgnoreClockWise = ignoreDir;
            _bNeedHit = needHit;
            _bIsClockWise = isClockWise;
            _bLimitRadius = upLimit;
            _fGestureRadiusFix = radiusFix;
            _fRotateFactor = rotateSpeed;
            _fSampleDisThreshold = _fGestureRadius / 4f;//(2*PI/24),PI~=3

            _fTotalRotate = 0;
            _fLastZ = 0;
        }

        void OnEnable()
        {
            LeanTouch.OnFingerDown += StartGestureAround;
            LeanTouch.OnFingerSet += TickGestureAround;
            LeanTouch.OnFingerUp += OverGestureAround;
        }

        void OnDisable()
        {
            LeanTouch.OnFingerDown -= StartGestureAround;
            LeanTouch.OnFingerSet -= TickGestureAround;
            LeanTouch.OnFingerUp -= OverGestureAround;
        }

        void StartGestureAround(LeanFinger finger)
        {
            if (_objRotateTarget != null)
            {
                if (_bNeedHit)
                {
                    RaycastHit hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
                    if (hit.collider != null && hit.collider.gameObject == _objRotateTarget)
                        _bGesturing = true;
                }
            }
            else
                _bGesturing = true;
        }

        void TickGestureAround(LeanFinger finger)
        {
            if (_bGesturing)
            {
                //支持一个大概的圆形区,空心处理的小一点,就用修正值了
                //TODO::如何能把这个圆直接用renderer显示出来,不然只能猜个大概
                if (Vector3.Distance(_v3AroundCenterPoint, finger.ScreenPosition) > _fGestureRadiusFix)
                {
                    if (_bLimitRadius && Vector3.Distance(_v3AroundCenterPoint, finger.ScreenPosition) > _fGestureRadius + _fGestureRadiusFix)
                        _inputGesturePhases.Clear();
                    else 
                    {
                        var deltaVec = finger.ScreenDelta;
                        if (deltaVec.sqrMagnitude > LeanTouch.Instance.RecordThreshold * LeanTouch.Instance.RecordThreshold)//超过阈值,记录一下
                        {
                            _inputGesturePhases.Add(deltaVec);
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
                                    //_fTotalRotate = 0;
                                    _inputGesturePhases.Clear(); 
                                }
                                else
                                {
                                    //通过上面几个条件测试表示是正在画一个圆,可以转动物体了
                                    float rotateZ = _bIsClockWise ? -1 * _fRotateFactor * Time.deltaTime : 1 * _fRotateFactor * Time.deltaTime;
                                    //ReuseFuncs.GetDeltaDegrees(new Vector2(_objRotateTarget.transform.position.x, _objRotateTarget.transform.position.z, ))
                                    _fTotalRotate += rotateZ;// finger.GetDeltaDegrees(_v3AroundCenterPoint);
                                    //Debug.Log(_fTotalRotate + " " + finger.GetDeltaDegrees(_v3AroundCenterPoint));
                                    //_objRotateTarget.transform.Rotate(new Vector3(0, 0, rotateZ));

                                    if (Mathf.Abs(_fTotalRotate) > 360)
                                    {
                                        if (OnRotateFinish != null) OnRotateFinish.Invoke();
                                        _fTotalRotate = 0;
                                    }
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

        void OverGestureAround(LeanFinger finger)
        {
            _bGesturing = false;
        }




        public Vector3 gizmoscenter;
        public float gizmosradius;
        void OnDrawGizmos()
        {
            return;
            Gizmos.DrawCube(gizmoscenter, Vector3.zero);
            Gizmos.DrawWireSphere(gizmoscenter, 1);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    //加工原材料的工具,不包括冰箱和传送带.
    public class MachineCtrl : MonoBehaviour
    {
        protected bool _bRigidSpeedLimit = true;
        protected float _fRigidSpeedLimit = 4;
        protected bool _bRigidLimitUp = true;

        protected System.Action<GameObject> OnMachineFinish;
        ////进入一个工具时注册初始化
        //public virtual void RegisterObject(GameObject obj, System.Action<GameObject> finishCallback)
        //{
        //    OnMachineFinish = finishCallback;
        //}

        protected FridgeItemCtrller[] _items;

        protected void OnEnable()
        {
            Lean.Touch.LeanTouch.OnFingerDown += OnFingerDown;
            Lean.Touch.LeanTouch.OnFingerSet += OnFingerSet;
            Lean.Touch.LeanTouch.OnFingerUp += OnFingerUp;
        }
        protected void OnDisable()
        {
            Lean.Touch.LeanTouch.OnFingerDown -= OnFingerDown;
            Lean.Touch.LeanTouch.OnFingerSet -= OnFingerSet;
            Lean.Touch.LeanTouch.OnFingerUp -= OnFingerUp;
        }

        protected void FixedUpdate()
        {
            if (_bRigidSpeedLimit)
                ControlRigidSpeed();
        }

        protected void FormPartsInCircle(Transform trsRoot, float radius = 1.5f)
        {
            var trsChildren = trsRoot.GetChildTrsList();
            if (trsChildren.Count > 1)
            {
                float perRad = 360f / trsChildren.Count * Mathf.Deg2Rad;
                for (int i = 0; i < trsChildren.Count; i++)
                {
                    if (trsChildren[i].gameObject.layer == LayerMask.NameToLayer("CookMachine"))
                        continue;
                    trsChildren[i].localPosition = new Vector3(radius * Mathf.Sin(perRad * i), 0, radius * Mathf.Cos(perRad * i));
                    trsChildren[i].localEulerAngles = new Vector3(0, perRad * Mathf.Rad2Deg * i, 0);
                }
            }
            else if (trsChildren.Count == 1)
            {
                if (trsChildren[0].gameObject.layer == LayerMask.NameToLayer("CookMachine"))
                    return;
                trsChildren[0].localPosition = Vector3.zero;
                trsChildren[0].localEulerAngles = Vector3.zero;
            }
        }

        protected void FormPartsInRow(Transform trsRoot, float deltaZ = 1.5f)
        {
            var trsChildren = trsRoot.GetChildTrsList();
            if (trsChildren.Count > 1)
            {
                float fixVal = trsChildren.Count % 2 == 0 ? deltaZ / 2f : 0;
                for (int i = 0; i < trsChildren.Count; i++)
                {
                    if (trsChildren[i].gameObject.layer == LayerMask.NameToLayer("CookMachine"))
                        continue;

                    int flag = i % 2 == 0 ? -1 : 1;

                    float newZ = deltaZ * flag * Mathf.CeilToInt(i / 2f) - fixVal;
                    trsChildren[i].localPosition = new Vector3(0, 0, newZ);
                    trsChildren[i].localEulerAngles = new Vector3(0, 0, 45);
                }
            }
            else if (trsChildren.Count == 1)
            {
                if (trsChildren[0].gameObject.layer == LayerMask.NameToLayer("CookMachine"))
                    return;
                trsChildren[0].localPosition = Vector3.zero;
                trsChildren[0].localEulerAngles = new Vector3(0, 0, 45);
            }
        }

        //限制被处理物体的刚体速度,减少飞出情况
        protected void ControlRigidSpeed()
        {
            Rigidbody[] bodies = gameObject.GetComponentsInChildren<Rigidbody>();
            for (int i = 0; i < bodies.Length; i++)
            {
                var newX = Mathf.Clamp(bodies[i].velocity.x, -_fRigidSpeedLimit, _fRigidSpeedLimit);
                var newY = bodies[i].velocity.y;
                if (_bRigidLimitUp && newY > 0)
                    newY = 0;
                else if (!_bRigidLimitUp)
                    newY = Mathf.Clamp(bodies[i].velocity.y, -_fRigidSpeedLimit, _fRigidSpeedLimit);
                var newZ = Mathf.Clamp(bodies[i].velocity.z, -_fRigidSpeedLimit, _fRigidSpeedLimit);
                bodies[i].velocity = new Vector3(newX, newY, newZ);
            }
        }


        protected virtual void OnFingerDown(Lean.Touch.LeanFinger finger)
        { }
        protected virtual void OnFingerSet(Lean.Touch.LeanFinger finger)
        { }
        protected virtual void OnFingerUp(Lean.Touch.LeanFinger finger)
        { }

        //被外部调用强制结束
        public virtual void Stop()
        { }

        public virtual void SetRenderLerp(float val)
        {
            if (_items != null)
            {
                val = Mathf.Clamp01(val);
                for (int i = 0; i < _items.Length; i++)
                {
                    _items[i].SetMatValue(val);
                }
            }
        }
    }


    public class ObjectFryingCtrl : MonoBehaviour
    {
        public float fFryForce = 50;
        public bool _bDetecting;
        private Rigidbody _body;

        void OnEnable()
        {
            _bDetecting = true;
        }

        void OnDisable()
        {
            _bDetecting = false;
        }

        void Start()
        {
            _body = gameObject.GetComponent<Rigidbody>();
        }

        void OnCollisionEnter(Collision col)
        {
            RaiseObjFried(col);
        }

        void OnCollisionStay(Collision col)
        {
            RaiseObjFried(col);
        }

        void RaiseObjFried(Collision col)
        {
            if (_bDetecting && _body != null)
            {
                if (col.gameObject.layer == LayerMask.NameToLayer("CookMachine"))
                {
                    //Vector3 upDir = transform.position - col.transform.position;
                    _body.AddForce(fFryForce * _body.mass * Vector3.up);
                }
            }
        }
    }
}
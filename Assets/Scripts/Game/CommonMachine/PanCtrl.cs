using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    public class PanCtrl : MachineCtrl
    {
        AudioSource _asPan;

        public ForceMode modeOfForce = ForceMode.VelocityChange;
        public float fBaseForce = 3;
        public float fHitForce = 15;
        public float fRollForce = 40;
        public float _fFryingTimeLimit = 5;

        //顶部碰撞,防止东西飞出去
        //BoxCollider _colTop;
        GameObject _objFried;
        SpriteRenderer _spOil;

        float _fFryingCounter;
        bool _bMovingPan;

        System.Action<bool> _callbackFriedOk;

        void Awake()
        {
            _spOil = transform.FindChild("Mesh/Oil").gameObject.GetComponent<SpriteRenderer>();
        }

        //各种料理工具应该可以共用一些方法
        public void RegisterObject(GameObject obj, System.Action<GameObject> finishCallback, System.Action<bool> friedOkCallback, bool formParts = true)
        {
            _callbackFriedOk = friedOkCallback;
            OnMachineFinish = finishCallback;
            //_colTop = transform.FindChild("DummyTop").GetComponent<BoxCollider>();

            obj.transform.SetParent(transform);
            //if (formParts)
            //    FormPartsInCircle(obj.transform);
            obj.transform.DOLocalMove(Vector3.up * 4, 0.5f).OnComplete(() =>
            {
                //_colTop.enabled = true;
                _objFried = obj;
                _objFried.SetRigidBodiesKinematic(false);
                DoozyUI.UIManager.PlaySound("40物体入锅", transform.position, false, 1, 0.3f);
                _asPan = DoozyUI.UIManager.PlaySound("41物体煎烤声低质", transform.position, true, 1, 0.6f);

                InvokeRepeating("RaisePanObj", 0, 0.2f);
            });
            _bMovingPan = false;

            GuideManager.Instance.SetGuideDoubleDir(transform.position + Vector3.forward * 4, transform.position + Vector3.forward * 12);
            _items = transform.GetComponentsInChildren<FridgeItemCtrller>();
            //_fHittingDelta = -1;
        }

        void Update()
        {
            TickFryingTime(Time.deltaTime);
        }

        new void OnEnable()
        {
            _spOil.color = new Color(1, 1, 1, 0);
            _spOil.gameObject.SetActive(true);
            _spOil.DOKill();
            _spOil.DOColor(Color.white, 1f);
            base.OnEnable();
            LeanTouch.OnFingerSwipe += MovePan;
            _fFryingCounter = 0;
        }

        new void OnDisable()
        {
            if (_asPan != null)
                AudioSourcePool.Instance.Free(_asPan);
            _spOil.DOKill();
            _spOil.DOColor(new Color(1, 1, 1, 0), 1f).OnComplete(()=> { _spOil.gameObject.SetActive(false); });
            base.OnDisable();
            _objFried = null;
            LeanTouch.OnFingerSwipe -= MovePan;
            CancelInvoke("RaisePanObj");
            //if (_colTop != null)
            //    _colTop.enabled = false;
        }

        void TickFryingTime(float deltaTime)
        {
            if (_objFried != null && _callbackFriedOk != null)
            {
                if (_fFryingCounter < _fFryingTimeLimit)
                {
                    _fFryingCounter += deltaTime;
                    SetRenderLerp(_fFryingCounter / _fFryingTimeLimit);
                    //Debug.Log(_fFryingCounter);
                    if (_fFryingCounter >= _fFryingTimeLimit)
                    {
                        _callbackFriedOk.Invoke(false);
                        _callbackFriedOk = null;
                    }
                }
            }

            if (_fHittingDelta > 0)
            {
                _fHittingDelta -= deltaTime;
            }
        }

        //float _fHittingY;
        //Rigidbody _bodyHitting;
        //protected override void OnFingerDown(LeanFinger finger)
        //{
        //    if (_objFried == null || _bMovingPan)
        //        return;
        //    //点击食材加力
        //    var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
        //    if (hit.collider != null)
        //    {
        //        var body = hit.collider.GetComponent<Rigidbody>();
        //        if (body != null)
        //        {
        //            _bodyHitting = body;
        //            _fHittingY = _bodyHitting.transform.position.y;
        //            ////加力要根据质量来,默认质量是1
        //            //hit.collider.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-fHitForce, fHitForce) * body.mass, fHitForce / 2, Random.Range(-fHitForce, fHitForce) * body.mass), modeOfForce);
        //        }
        //    }
        //}


        float _fHittingDelta;
        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_objFried == null || _bMovingPan || _fHittingDelta > 0)
                return;
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null)
            {
                var body = hit.collider.GetComponent<Rigidbody>();
                if (body != null)
                {
                    ////加力要根据质量来,默认质量是1
                    hit.collider.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-fHitForce, fHitForce) * body.mass, 0, Random.Range(-fHitForce, fHitForce) * body.mass), modeOfForce);
                    _fHittingDelta = 0.1f;
                }
            }

            //if (_bodyHitting != null && !_bMovingPan)
            //{
            //    var pos = GameUtilities.GetFingerTargetWolrdPos(finger, _bodyHitting.gameObject, _fHittingY);
            //    var disVec = pos - new Vector3(transform.position.x, _fHittingY, transform.position.z);
            //    if (disVec.magnitude > 2.5f)
            //    {
            //        pos = transform.position + disVec.normalized * 2.5f;
            //        pos.y = _fHittingY;
            //    }
            //    //效率很低
            //    _bodyHitting.transform.DOKill();
            //    _bodyHitting.transform.DOMove(pos, 0.3f);;
            //}
        }

        //protected override void OnFingerUp(LeanFinger finger)
        //{
            
        //    //_bodyHitting = null;
        //}

        bool _bHittingPan;
        protected override void OnFingerDown(LeanFinger finger)
        {
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
               _bHittingPan = true;
            }
        }

        //临时甩锅
        void MovePan(LeanFinger finger)
        {
            if (_bMovingPan || !_bHittingPan)
                return;

            if (finger.SwipeScreenDelta.y > Mathf.Abs(finger.SwipeScreenDelta.x))
            {
                _bHittingPan = false;
                _bMovingPan = true;
                DoozyUI.UIManager.PlaySound("31炒锅颠锅", transform.position);
                Vector3 start = transform.position;
                var bodies = transform.GetComponentsInChildren<Rigidbody>();
                if (bodies != null)
                {
                    for (int i = 0; i < bodies.Length; i++)
                    {
                        bodies[i].AddForce(Vector3.back * 2 * fRollForce, modeOfForce);
                    }
                }
                transform.DORotate(new Vector3(-15, 0, 0), 0.3f);
                transform.DOMove(start + Vector3.up * 3, 0.3f).OnComplete(() =>
                {
                    if (bodies != null)
                    {
                        for (int i = 0; i < bodies.Length; i++)
                        {
                            bodies[i].AddForce(new Vector3(0, 4, 0.1f) * fRollForce, modeOfForce);
                        }
                    }

                    transform.DORotate(new Vector3(15, 0, 0), 0.2f).OnComplete(() =>
                    {
                        transform.DOMove(start, 0.3f);
                        transform.DORotate(Vector3.zero, 0.3f).OnComplete(() =>
                        {
                            _bMovingPan = false;
                        });
                    });
                });
               
            }
        }

        void RaisePanObj()
        {
            var bodies = transform.GetComponentsInChildren<Rigidbody>();
            if (bodies != null)
            {
                for (int i = 0; i < bodies.Length; i++)
                {
                    bodies[i].AddForce(fBaseForce * bodies[i].mass * Vector3.up, modeOfForce);
                }
            }
        }

        public override void Stop()
        {
            _objFried.SetRigidBodiesKinematic(true);
            if (OnMachineFinish != null)
                OnMachineFinish(_objFried);
        }
    }
}

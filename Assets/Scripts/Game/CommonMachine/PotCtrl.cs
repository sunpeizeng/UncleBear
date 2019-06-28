using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Touch;

namespace UncleBear
{
    //TODO::煮过这有个思考,因为物体以后都继承自item类,配置表中加入一个密度属性,锅内受力恒定,如果是意面,进入锅内是浮起来,但是土豆是沉下去
    public class PotCtrl : MachineCtrl {

        AudioSource _asBoil;
        LeanGestureCircle _circleGesCtrl;
        Transform trsSoup;
        List<Transform> trsSoupContents;
        ParticleSystem psBubble;
        ParticleSystem psSteam;

        float _fBoilTimeLimit = 5;
        float _fBoilTime;
        public float _fBuoyancyLimit = 3.5f;
        public float _fForce = 1f;//测试值
        float _fBuoyancy = 0;
        GameObject _objBoiled;
        bool _bStartBoiling;
        bool _bTouchingSoup;
        bool _bLockFloat;
        bool _bNeedScale;

        float _fBoilDelta = 0.5f;
        float _fBoilCounter;

        //转汤的一些参数
        float _fRotSpeed;
        float _fRotAngle;
        float _fShakeX = 1;
        float _fShakeFixX = 0;

        System.Action<bool> _callbackCookOk;

        void Awake()
        {
            trsSoup = transform.FindChild("Soup");
            psBubble = transform.FindChild("BubblePops").GetComponent<ParticleSystem>();
            psSteam = transform.FindChild("Soap").GetComponent<ParticleSystem>();
        }

        public void RegisterObject(GameObject obj, System.Action<GameObject> finishCallback, System.Action<bool> cookOkCallback, bool formCircle = true, bool needScale = true)
        {
            _bRigidSpeedLimit = false;
            _callbackCookOk = cookOkCallback;
            OnMachineFinish = finishCallback;
            _objBoiled = obj;
            _bLockFloat = _bStartBoiling = _bRigidLimitUp = false;
            _fBoilCounter = _fBoilDelta = 1f;
            _fBoilTime = _fBuoyancy = 0;

            if (_objBoiled != null)
            {
                _objBoiled.transform.SetParent(trsSoup);
                _objBoiled.SetLocalPos(new Vector3(0, -2, 0));
                _bNeedScale = needScale;
                if (_bNeedScale)
                    _objBoiled.transform.localScale = _objBoiled.transform.localScale * 0.7f;
                StartCoroutine(SetBoiledPartsToPot());
            }
            ////支持旋转
            //_circleGesCtrl = gameObject.AddMissingComponent<LeanGestureCircle>();
            //_circleGesCtrl.enabled = true;
            //_circleGesCtrl.SetParams(gameObject, trsSoup.position, 2);
            _items = transform.GetComponentsInChildren<FridgeItemCtrller>();
        }

        IEnumerator SetBoiledPartsToPot()
        {
            bool allFinished = false;
            trsSoupContents = _objBoiled.transform.GetChildTrsList();
            if (trsSoupContents.Count > 0)
            {
                _objBoiled.SetColliderEnable(false);
                if (trsSoupContents.Count > 1)
                {
                    for (int i = 0; i < trsSoupContents.Count; i++)
                    {
                        trsSoupContents[i].position = _objBoiled.transform.position + new Vector3(Random.Range(-3f, 3f), Random.Range(8, 12), Random.Range(-3f, 3f));
                        trsSoupContents[i].transform.localEulerAngles = new Vector3(Random.Range(-30f, 30f), Random.Range(-180f, 180f), 0);
                        trsSoupContents[i].gameObject.SetRigidBodiesKinematic(false);
                        trsSoupContents[i].gameObject.SetColliderEnable(true);
                        if (i >= trsSoupContents.Count - 1)
                        {
                            allFinished = true;
                        }
                    }
                }
                else if (trsSoupContents.Count == 1)
                {
                    allFinished = true;
                    trsSoupContents[0].position = _objBoiled.transform.position + new Vector3(0, 10, 0);
                    trsSoupContents[0].gameObject.SetRigidBodiesKinematic(false);
                }
            }
            else
            {
                _objBoiled.SetRigidBodiesDrag(2);
                _objBoiled.SetRigidBodiesAngularDrag(0.8f);
                DoozyUI.UIManager.PlaySound("38物体入水", transform.position);
               _objBoiled.SetRigidBodiesKinematic(false);
                StartBoiling();
            }

            if (allFinished)
            {
                yield return new WaitForSeconds(0.5f);
                DoozyUI.UIManager.PlaySound("38物体入水", transform.position, false, 1);
                _objBoiled.SetRigidBodiesDrag(2);
                _objBoiled.SetRigidBodiesAngularDrag(0.8f);
                yield return new WaitForSeconds(0.5f);
                StartBoiling();
            }
        }

        public void StartBoiling()
        {
            if (_objBoiled != null)
                _objBoiled.SetColliderEnable(true);
            _bStartBoiling = true;
            //慢慢增加浮力,缩短煮开滚动间隔
            _asBoil = DoozyUI.UIManager.PlaySound("39沸腾声低质", transform.position, true, 0f);
            DOTween.To(() => _fBuoyancy, p => _fBuoyancy = p, _fBuoyancyLimit, _fBoilTimeLimit);
            DOTween.To(() => _fBoilDelta, p => _fBoilDelta = p, 0.1f, _fBoilTimeLimit);
            DOTween.To(() => _asBoil.volume, p => _asBoil.volume = p, DoozyUI.UIManager.soundVolume, _fBoilTimeLimit * 2);
        }

        new void OnDisable()
        {
            //重置
            if (_asBoil != null)
                AudioSourcePool.Instance.Free(_asBoil);
            if (_bNeedScale && _objBoiled != null)
                _objBoiled.transform.DOScale(_objBoiled.transform.localScale / 0.7f, 1f);
            psSteam.maxParticles = psBubble.maxParticles = 0;
            if (_objBoiled != null)
            {
                _objBoiled.SetRigidBodiesDrag();
                _objBoiled.SetRigidBodiesAngularDrag();
            }
            base.OnDisable();
        }

        new void FixedUpdate()
        {
            base.FixedUpdate();

            //if (gameObject.IsAllRigidBodyQuiet())
            //    _bStartBoiling = true;

            if (_objBoiled != null && _fBoilCounter > 0)
            {
                _fBoilCounter -= Time.fixedDeltaTime;
                if (_fBoilCounter <= 0)
                {
                    _fBoilCounter = _fBoilDelta;
                    var bodies = _objBoiled.transform.GetComponentsInChildren<Rigidbody>();
                    for (int i = 0; i < bodies.Length; i++)
                        bodies[i].AddForce(new Vector3(Random.Range(-_fForce, _fForce), _fBuoyancy , Random.Range(-_fForce, _fForce)) * bodies[i].mass, ForceMode.Impulse);
                }
            }
        }

        public float GetPotPerc()
        {
            return _fBoilTime / (_fBoilTimeLimit * 2);
        }

        void Update()
        {
            if (_bStartBoiling)
            {
                _fBoilTime += Time.deltaTime;
                psBubble.maxParticles = (int)(Mathf.Clamp01(_fBoilTime / (_fBoilTimeLimit * 2)) * 10);
                psSteam. maxParticles = (int)(Mathf.Clamp01(_fBoilTime / (_fBoilTimeLimit * 2)) * 50);
                SetRenderLerp(_fBoilTime / (_fBoilTimeLimit * 2));
                if (_fBoilTime > _fBoilTimeLimit * 2)
                {
                    if (_callbackCookOk != null)
                    {
                        _callbackCookOk.Invoke(false);
                        _callbackCookOk = null;

                        //只判断一次,鸡蛋的特殊处理
                        if (_objBoiled != null && 
                            _objBoiled.name.Contains("eggWhole") && 
                            _objBoiled.GetComponent<MeshRenderer>() != null && 
                            !_objBoiled.GetComponent<MeshRenderer>().material.name.Contains("Mat_EggPiece"))
                        {
                            _objBoiled.transform.DOLocalRotate(new Vector3(0, _objBoiled.transform.localEulerAngles.y, _objBoiled.transform.localEulerAngles.z), 0.5f);
                            _objBoiled.transform.DOLocalMoveY(0, 0.5f).OnComplete(()=>{ _bLockFloat = true; });
                        }
                    }

                    if (_objBoiled != null && _bLockFloat)
                    {
                        _objBoiled.transform.localPosition = new Vector3(_objBoiled.transform.localPosition.x, 0, _objBoiled.transform.localPosition.z);
                        _objBoiled.transform.localEulerAngles = new Vector3(0, _objBoiled.transform.localEulerAngles.y, _objBoiled.transform.localEulerAngles.z);
                    }


                }
            }

            if (_fRotSpeed != _fRotAngle)
            {
                var curDelta = 1f;
                if (_fShakeFixX != 0)
                    curDelta = Mathf.Clamp01(Mathf.Abs(_fRotSpeed - _fRotAngle) / _fShakeFixX);

                DOTween.To(() => _fRotAngle, p => _fRotAngle = p, _fRotSpeed, 1);
                trsSoup.localEulerAngles = new Vector3(_fShakeX * curDelta, -_fRotAngle, 0);
            }

            LimitPosition();
        }
        void LimitPosition()
        {
            if (trsSoupContents != null && trsSoupContents.Count > 0)
            {
                GameUtilities.LimitListPosition(trsSoupContents, transform.position, 3);

                trsSoupContents.ForEach(p =>
                {
                    if (_bStartBoiling && p.localPosition.y > 2)
                    {
                        p.localPosition = new Vector3(p.localPosition.x, 2, p.localPosition.z);
                    }
                });
            }
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            if (!_bStartBoiling)
                return;

            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition), 1000, 1 << LayerMask.NameToLayer("CookMachine"));
            if (hit.collider != null && hit.collider.transform.IsChildOf(transform))
            {
                _bTouchingSoup = true;
            }
        }

        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_bTouchingSoup)
            {
                _fRotSpeed += finger.GetDeltaDegrees(CameraManager.Instance.MainCamera.WorldToScreenPoint(trsSoup.position));
            }
        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            _fShakeFixX = Mathf.Abs(_fRotSpeed - _fRotAngle);
            _bTouchingSoup = false;
        }

        public override void Stop()
        {
            _bStartBoiling = false;
            if (_objBoiled != null)
            {
                _objBoiled.SetRigidBodiesKinematic(true);
                if (OnMachineFinish != null)
                    OnMachineFinish(_objBoiled);
            }
        }
    }
}

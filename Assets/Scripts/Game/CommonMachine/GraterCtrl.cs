using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;

namespace UncleBear
{
    //在刨子上加上这个脚本
    public class GraterCtrl : MachineCtrl
    {
        float _fGraterDelta;
        bool _bHittingSrc;

        int _nDesLimit = 24;
        int _nDesCount;
        //源材料和处理后的材料
        GameObject _objSrc;
        GameObject _objDes;
        List<GameObject> _lstGenedDesObjs = new List<GameObject>();
        public List<GameObject> GenedDesObjs
        {
            get { return _lstGenedDesObjs; }
        }

        public void RegisterObject(GameObject objSrc, System.Action<GameObject> finishCallback, GameObject objDes, int maxDesCount)
        {
            OnMachineFinish = finishCallback;

            _bHittingSrc = false;
            _fGraterDelta = 0;
            _objSrc = objSrc;
            _objSrc.transform.SetParent(transform);
            _objSrc.SetLocalPos(new Vector3(0, 1, 0));//根据表现给点高度偏移
            _objSrc.ResetLocalAngle();

            _objDes = objDes;
            _nDesCount = _nDesLimit = maxDesCount;

            GuideManager.Instance.SetGuideDoubleDir(_objSrc.transform.position - _objSrc.transform.right * 2, _objSrc.transform.position + _objSrc.transform.right * 2, 0.5f);
        }

        void Update()
        {
            if (_fGraterDelta > 0)
                _fGraterDelta -= Time.deltaTime;
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            base.OnFingerDown(finger);
            if (_nDesCount <= 0)
                return;
            //点到原材料才可以刨
            RaycastHit hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.gameObject == _objSrc)
                _bHittingSrc = true;
        }

        protected override void OnFingerSet(LeanFinger finger)
        {
            base.OnFingerSet(finger);
            if (!_bHittingSrc)
                return;
            if (finger.ScreenDelta.y != 0)
            {
                //拖动起司,1和-1是测试值
                float newX = Mathf.Clamp(_objSrc.transform.localPosition.x + finger.ScreenDelta.y * 0.05f, -1f, 1f);
                _objSrc.transform.localPosition = new Vector3(newX, _objSrc.transform.localPosition.y, _objSrc.transform.localPosition.z);

                if (_objSrc.transform.localScale.y > 0 && finger.ScreenDelta.y < -2 && newX > -1f)//只响应向下的手势,看刨子方向
                {
                    if (_fGraterDelta <= 0)
                    {
                        //根据速度一次刨2到6条出来
                        int stickNum = (int)Mathf.Clamp(-finger.ScreenDelta.y, 2, 6);
                        if (_nDesCount - stickNum > 0)
                            _nDesCount -= stickNum;
                        else
                        {
                            stickNum = _nDesCount;
                            _nDesCount = 0;
                        }

                        //目前模型只处理Y缩放
                        var needY = stickNum * 1.0f / (_nDesLimit * 5);
                        var newScaleY = _objSrc.transform.localScale.y > needY ? _objSrc.transform.localScale.y - needY : 0;
                        _objSrc.transform.localScale = new Vector3(_objSrc.transform.localScale.x, newScaleY, _objSrc.transform.localScale.z);

                        DoozyUI.UIManager.PlaySound("10刨子", _objSrc.transform.position);

                        for (int i = 0; i < stickNum; i++)
                        {
                            var ranPos = _objSrc.transform.position + new Vector3(0, -2f, 0) + _objSrc.transform.right * Random.Range(-1f, 1f) + _objSrc.transform.forward * Random.Range(-1f, 1f);
                            var piece = GameObject.Instantiate(_objDes, ranPos, Quaternion.identity) as GameObject;
                            piece.AddMissingComponent<SelfDestroy>();
                            //piece.name = "PizzaCheese";
                            piece.GetComponent<Rigidbody>().isKinematic = false;
                            piece.transform.localEulerAngles = new Vector3(0, 0, 100);//给点角度避免正好立在中间
                            //piece.transform.SetParent(_objBowl.transform);
                            //如果有需要,用list拿到外面去改名,并指定新的父级,比如碗
                            _lstGenedDesObjs.Add(piece);
                        }
                        _fGraterDelta = 0.3f;

                        //
                        if (_nDesCount <= 0 || _objSrc.transform.localScale.y <= 0)
                        {
                            _bHittingSrc = false;
                            if (OnMachineFinish != null) OnMachineFinish.Invoke(_objSrc);
                        }
                    }
                }
            }
        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            base.OnFingerUp(finger);
            _bHittingSrc = false;
        }
    }
}

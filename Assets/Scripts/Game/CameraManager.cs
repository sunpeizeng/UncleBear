using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class CameraManager : DoozyUI.Singleton<CameraManager>
    {
        //private Dictionary<string, CameraPathAnimator> _dicCamAnim = new Dictionary<string, CameraPathAnimator>();
        private Camera _camMain;
        private Camera _camRenderTex;
        //private CameraPathAnimator _curCPA;
        private Vector3 _v3LastPos;
        private Vector3 _v3LastAngle;
        //temp
        public Camera MainCamera
        {
            get
            {
                if (_camMain == null)
                {
                    var obj = GameObject.FindGameObjectWithTag("MainCamera");
                    if (obj != null)
                        _camMain = obj.GetComponent<Camera>();
                }
                return _camMain;
            }
        }

        public Camera RenderCamera
        {
            get
            {
                if (_camRenderTex == null)
                {
                    var obj = GameObject.FindGameObjectWithTag("RenderCam");
                    if (obj != null)
                        _camRenderTex = obj.GetComponent<Camera>();
                }
                return _camRenderTex;
            }
        }

        //public void PlayCamAnim(string name, bool isReverse = false, System.Action callback = null)
        //{
        //    _curCPA = null;
        //    if (_dicCamAnim.ContainsKey(name))
        //        _curCPA = _dicCamAnim[name];
        //    else {
        //        _curCPA = (Instantiate(Resources.Load("Prefabs/CamPath/" + name)) as GameObject).GetComponent<CameraPathAnimator>();
        //        DontDestroyOnLoad(_curCPA.gameObject);
        //        _dicCamAnim.Add(name, _curCPA);
        //    }

        //    if (_curCPA != null && MainCamera != null)
        //    {
        //        _curCPA.finishCallback = callback;

        //        _curCPA.animationObject = MainCamera.transform;
        //        if (!isReverse)
        //        {
        //            _curCPA.animationMode = CameraPathAnimator.animationModes.once;
        //            _curCPA.Play();
        //        }
        //        else
        //        {
        //            _curCPA.animationMode = CameraPathAnimator.animationModes.reverse;
        //            _curCPA.Play();
        //        }

        //    }
        //}

        public void SetCamTransform(Vector3 pos, Vector3 angle)
        {
            if (MainCamera != null)
            {
                //if (_curCPA != null)
                //    _curCPA.Stop();
                MainCamera.transform.DOKill();
                MainCamera.transform.position = pos;
                MainCamera.transform.localEulerAngles = angle;
            }
        }

        public void DoCamTween(Vector3 desPos, float dur = 1f, System.Action callback = null)
        {
            if (MainCamera != null)
            {
                _v3LastPos = MainCamera.transform.position;
                MainCamera.transform.DOMove(desPos, dur).OnComplete(() =>
                {
                    if (callback != null)
                        callback.Invoke();
                });
            }
        }
        public void DoCamTween(Vector3 desPos, Vector3 desAngle, float dur = 1f, System.Action callback = null)
        {
            if (MainCamera != null)
            {
                _v3LastPos = MainCamera.transform.position;
                _v3LastAngle = MainCamera.transform.localEulerAngles;
                MainCamera.transform.DOMove(desPos, dur).OnComplete(() =>
                {
                    if (callback != null)
                        callback.Invoke();
                });
                MainCamera.transform.DORotate(desAngle, dur);
            }
        }
        public void BackToLastPos(float dur = 1f, System.Action callback = null)
        {
            if (MainCamera != null)
            {
                MainCamera.transform.DOMove(_v3LastPos, dur).OnComplete(()=> {
                    if (callback != null)
                        callback.Invoke();
                });
            }
        }
        public void BackToLastTrans(float dur = 1f, System.Action callback = null)
        {
            if (MainCamera != null)
            {
                MainCamera.transform.DOMove(_v3LastPos, dur);
                MainCamera.transform.DORotate(_v3LastAngle, dur).OnComplete(()=> {
                    if (callback != null)
                        callback.Invoke();
                });
            }
        }
    }
}

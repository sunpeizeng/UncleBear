using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    public class CupCakeStatePaintCream : State<LevelCupCake>
    {
        AudioSource _asCream;

        Vector3 _v3BagPos = new Vector3(-31, 29, -32.5f);
        Vector3 _v3PlatePos = new Vector3(-14, 22.8f, -21.5f);//new Vector3(-55, 22.7f, -101)
        Vector3 _v3CamPos = new Vector3(-14, 86f, 13);//new Vector3(-55, 91, -64.5f)

        //模拟3层
        GameObject _objPipingBag;
        GameObject[] _objCreamTops;
        List<GameObject[]> _lstObjCreams = new List<GameObject[]>();
        bool _bHoldingBag;
        bool _bMovingBag;
        int _nCreamIndex;
        int _nCreamLevelIndex;
        Vector3 _v3CurCenterPoint;

        LeanGestureCircle _circleGesCtrl;

        public CupCakeStatePaintCream(int stateEnum) : base(stateEnum)
        {

        }
        public override void Enter(object param)
        {
            //Debug.Log("Paint cream");
            base.Enter(param);

            _nCreamLevelIndex = _nCreamIndex = 0;
            _objPipingBag = _owner.LevelObjs[Consts.ITEM_PIPINGBAG];
         
            //var screenRadius = Vector3.Distance(CameraManager.Instance.MainCamera.WorldToScreenPoint(_owner.LevelObjs[Consts.CUPCAKE_CAKE].transform.position),
            //   CameraManager.Instance.MainCamera.WorldToScreenPoint(_objPipingBag.transform.position));
            //Debug.Log(screenRadius);

            _circleGesCtrl = _objPipingBag.AddComponent<LeanGestureCircle>();
            _circleGesCtrl.OnRotateFinish = OnRotateAround;

            _owner.LevelObjs[Consts.ITEM_OVENPLATE].transform.DOMove(new Vector3(-4.3f, 27f, -64), 0.3f).OnComplete(()=> {
                _owner.LevelObjs[Consts.ITEM_OVENPLATE].transform.DOMove(_v3PlatePos, 0.3f);
            });

            CameraManager.Instance.DoCamTween(_v3CamPos, new Vector3(60, 180, 0), 1f, () =>
            {
                SetGestureCtrl();
                GenCakeCreams();
            });
        }

        void GenCakeCreams()
        {
            _objCreamTops = new GameObject[_owner.Cupcakes.Count];
            for (int m = 0; m < _owner.Cupcakes.Count; m++)
            {
                var matCream = _owner.Cupcakes[m].GetComponent<CupcakeMatsCtrller>().RandomCreamMat();
                //每个奶油由3层zucheng组成
                var objCreams = new GameObject[3];
                for (int i = 0; i < 3; i++)
                {
                    var creamCopy = GameObject.Instantiate(_owner.LevelObjs[Consts.ITEM_CREAM]) as GameObject;
                    creamCopy.transform.FindChild("cream/Mesh").GetComponent<SkinnedMeshRenderer>().material = matCream;
                    creamCopy.transform.SetParent(_owner.Cupcakes[m].transform);
                    creamCopy.transform.localPosition = new Vector3(-2f + i * 0.4f, 3 + i * 0.8f, -0.16f);
                    var scaleX = 1 - i * 0.2f;
                    var scaleY = 1f;
                    var scaleZ = 1 - i * 0.2f;
                    creamCopy.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
                    creamCopy.GetComponentInChildren<Animation>().SampleAnim("anim_cream", 0);
                    objCreams[i] = creamCopy;
                }
                //加上一个奶油尖
                _objCreamTops[m] = GameObject.Instantiate(_owner.LevelObjs[Consts.ITEM_CREAMTOP]) as GameObject;
                _objCreamTops[m].GetComponentInChildren<MeshRenderer>().material = matCream;
                _objCreamTops[m].transform.SetParent(_owner.Cupcakes[m].transform);
                _objCreamTops[m].transform.localScale = Vector3.zero;
                _objCreamTops[m].transform.localPosition = new Vector3(0, 4.9f, -0.2f);

                _lstObjCreams.Add(objCreams);
            }
        }

        void SetGestureCtrl()
        {
            _bHoldingBag = false;
            _circleGesCtrl.enabled = false;
            _bMovingBag = true;
            if (_nCreamIndex > 0)
            {
                _objPipingBag.SetPos(_owner.Cupcakes[_nCreamIndex - 1].transform.position + Vector3.up * 5);
                _objPipingBag.transform.DOMoveY(_objPipingBag.transform.position.y + 2, 0.5f).OnComplete(MoveToNextCup);
            }
            else
                MoveToNextCup();
         
          
        }

        void MoveToNextCup()
        {
            _objPipingBag.transform.DOMove(_owner.Cupcakes[_nCreamIndex].transform.position + Vector3.up * 6, 0.5f).SetDelay(0.2f).OnComplete(() =>
            {
                _bMovingBag = false;
                _v3CurCenterPoint = new Vector3(_owner.Cupcakes[_nCreamIndex].transform.position.x, _objPipingBag.transform.position.y, _owner.Cupcakes[_nCreamIndex].transform.position.z);
                GuideManager.Instance.SetGuideRotate(_v3CurCenterPoint);
                //Debug.Log(_v3CurCenterPoint);
                _circleGesCtrl.SetParams(_objPipingBag, _v3CurCenterPoint, 1.5f, false, false, true);
                _circleGesCtrl.enabled = true;
            });
        }

        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            _objCreamTops = null;
            _objPipingBag.transform.DOKill(true);
            _circleGesCtrl.enabled = false;
            _lstObjCreams.Clear();
            base.Exit();
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            if (_bHoldingBag || _bMovingBag || _nCreamIndex >= _owner.Cupcakes.Count)
                return;
            RaycastHit hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.gameObject == _objPipingBag)
            {
                _bHoldingBag = true;
            }
        }

        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_bMovingBag)
                return;
            if (_bHoldingBag && finger.GetDeltaDegrees(CameraManager.Instance.MainCamera.WorldToScreenPoint(_v3CurCenterPoint)) < 0)
            {
                var disVec = new Vector3(-Mathf.Sin(Mathf.Deg2Rad * _circleGesCtrl.fCurAngle), 0, -Mathf.Cos(Mathf.Deg2Rad * _circleGesCtrl.fCurAngle));
                _objPipingBag.transform.position = Vector3.Lerp(_objPipingBag.transform.position, _v3CurCenterPoint + disVec.normalized * 1.5f, 20 * Time.deltaTime);

                _lstObjCreams[_nCreamIndex][_nCreamLevelIndex].GetComponentInChildren<Animation>().SampleAnim("anim_cream", _circleGesCtrl.fRoundPerc);
                if (finger.ScreenDelta != Vector2.zero && (_asCream == null || !_asCream.isActiveAndEnabled))
                    _asCream = DoozyUI.UIManager.PlaySound("29上奶油", _objPipingBag.transform.position);
            }

        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            _bHoldingBag = false;
        }

        Vector3 GetFingerWorldPos(LeanFinger finger)
        {
            var fingerWorldPos = finger.GetWorldPosition(Vector3.Distance(CameraManager.Instance.MainCamera.transform.position, _objPipingBag.transform.position));
            fingerWorldPos.y = _objPipingBag.transform.position.y;
            return fingerWorldPos;
        }


        void OnRotateAround()
        {
            if (_nCreamLevelIndex < 2)
                _nCreamLevelIndex++;
            else
            {
                _objCreamTops[_nCreamIndex].transform.DOScale(Vector3.one, 0.3f).OnComplete(() => {
                    DoozyUI.UIManager.PlaySound("8成功");
                });
                _nCreamLevelIndex = 0;
                _nCreamIndex++;
                if (_nCreamIndex == _owner.Cupcakes.Count)
                {
                    _bHoldingBag = false;
                    _circleGesCtrl.enabled = false;
                    _objPipingBag.SetPos(_owner.Cupcakes[_nCreamIndex - 1].transform.position + Vector3.up * 5);
                    _objPipingBag.transform.DOMoveY(_objPipingBag.transform.position.y + 2, 0.5f).OnComplete(() =>
                    {
                        _objPipingBag.transform.DOMove(_v3PlatePos + Vector3.left * 30, 1f).OnComplete(() => {
                            StrStateStatus = "PaintCreamOver";
                            _objPipingBag.SetPos(Vector3.one * 500);
                        });
                    });
                }
                else
                    SetGestureCtrl();
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    public class FridgeCtrl : MonoBehaviour
    {
        Animation _animDoor;
        Vector3 _v3FridgePos = new Vector3(500, 0, 0);
        Vector3 _v3LookPos = new Vector3(535, -15, -50);
        Vector3 _v3PlatePosRelative = new Vector3(6.2f, 32f, 34f);//new Vector3(-7.8f, 2.3f, -3);
        Vector3 _v3PlateAngleRelative = new Vector3(3, -83, -5.5f);


        public class OutFridgeItem
        {
            public GameObject objItem;
            public Transform trsOrigin;
            public Vector3 localPosOrigin;

            public OutFridgeItem(GameObject obj, Transform parent, Vector3 localPos)
            {
                objItem = obj;
                trsOrigin = parent;
                localPosOrigin = localPos;
            }
        }

        public GameObject[] ObjsInPlate { get { return _objsInPlate; } } 
        public System.Action<bool> OnFridgePickOver;
        //页
        int _nTotalPage;
        int _nCurPage;
        int _nFridgePageSize = 3;
        List<FridgePage> _pages = new List<FridgePage>();
        FridgePage _pageLast;
        Vector3 _v3HidePagePos = new Vector3(0, 42, 0);

        //盘
        int _nFridgePlateSize = 3;
        //bool _bPicking;
        GameObject _objPicking;
        GameObject[] _objsInPlate;
        List<OutFridgeItem> _lstOutFridgeItems = new List<OutFridgeItem>();

        GameObject _objPlate;
        Vector3[] _v3PlatePoses = { new Vector3(-7.5f, 2f, 2), new Vector3(0f, 2f, -2), new Vector3(6.5f, 2f, 2) };

        bool _bFridgeInilized;
        bool IsPlateEmpty
        {
            get
            {
                for (int i = 0; i < _objsInPlate.Length; i++)
                    if (_objsInPlate[i] != null)
                        return false;
                return true;
            }
        }
        bool IsPlateFull
        {
            get
            {
                for (int i = 0; i < _objsInPlate.Length; i++)
                    if (_objsInPlate[i] == null)
                        return false;
                return true;
            }
        }

        void Awake()
        {
            transform.position = _v3FridgePos;
            _animDoor = transform.FindChild("Door").GetComponent<Animation>();
        }

        void SetCharaState(bool enter)
        {
            if (CharaCreator.Chef == null)
                return;

            if (enter)
            {
                //熊大
                CharaCreator.Chef.ChangeCharaState(CharaStateEnum.Wait, false);
                CharaCreator.Chef.SetTransform(_v3LookPos, new Vector3(0, -15, 0), 1.2f);
            }
            else
            {
                CharaCreator.Chef.ChangeCharaState(CharaStateEnum.Serve, true);
                _objPlate.transform.DOMove(_v3LookPos + _v3PlatePosRelative, 0.3f).OnComplete(()=> {
                    _objPlate.transform.SetParent(CharaCreator.Chef.GetCharaModel().trsHandR);
                });
                //_objPlate.transform.localEulerAngles = _v3PlateAngleRelative;
            }
        }


        public void InitializeFridge(GameObject layerObj, GameObject plateObj)
        {
            if (!_bFridgeInilized)
            {
                _objPlate = plateObj;
                _objsInPlate = new GameObject[_nFridgePlateSize];

                //这里从配置表加载冰箱里所有的东西
                List<FridgeLayerItem> items = SerializationManager.LoadFromCSV<FridgeLayerItem>("Configs/FridgeItems");
                List<string> itemIDs = new List<string>();

                _nTotalPage = Mathf.CeilToInt(items.Count * 1.0f/ _nFridgePageSize);

                for (int i = 0; i < _nTotalPage; i++)
                {
                    _pages.Add(new FridgePage());
                }

                for (int i = 0; i < items.Count; i++)
                {
                    itemIDs.Add(items[i].ID);
                    //把架子规成一页,用于切换
                    int pageIndex = i / _nFridgePageSize;
                    _pages[pageIndex].AddLayer(items[i].GenLayerItems(layerObj));
                    _pages[pageIndex].BaseTransform.SetParent(transform);
                }

                DishManager.Instance.PickRandomFavorItem(itemIDs);
                _bFridgeInilized = true;
            }

#if !UNITY_EDITOR
            AdHelper.HideBanner();
#endif

            _bScrolling = false;
            ShowFridgeGuide(true);
            SetCharaState(true);
            _pages.ForEach(p => p.BaseTransform.localPosition = _v3HidePagePos);
            _pages[_nCurPage].BaseTransform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutElastic);
        }

        public void ShowFridgeGuide(bool state)
        {
            if (GuideManager.Instance != null)
            {
                if (state)
                    GuideManager.Instance.SetGuideSingleDir(transform.position + new Vector3(8, 37, 0), transform.position + new Vector3(8, 19, 0), true, true, 1f);
                else
                    GuideManager.Instance.StopGuide();
            }
        }

        bool _bScrolling;
        public void ScrollPage()
        {
            if (_nTotalPage == 1 || _bScrolling)//只有一页不给翻唉
                return;

            _bScrolling = true;
            DoozyUI.UIManager.PlaySound("15开烤箱门", _v3FridgePos);
            _pageLast = _pages[_nCurPage];
            _pageLast.BaseTransform.DOLocalMove(_v3HidePagePos * -1, 0.2f).OnComplete(()=> { _pageLast.BaseTransform.localPosition = _v3HidePagePos; });//把老的往下滚走,然后藏回起点
            _nCurPage += 1;
            if (_nCurPage > _nTotalPage - 1)
                _nCurPage = 0;
            //拿到新的一页滚入
            _pages[_nCurPage].BaseTransform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutElastic).OnComplete(()=> {
                _bScrolling = false;
            });
        }

        void OnEnable()
        {
            //_bPicking = false;
            _animDoor["anim_fridgeDoor"].speed = 1;
            _animDoor["anim_fridgeDoor"].normalizedTime = 0;
            _animDoor.Play("anim_fridgeDoor");
          
            //Input.multiTouchEnabled = false;
            //TODO::可以在冰箱激活时加入一个新的摄像机,目前还是切位置

            LeanTouch.OnFingerDown += OnFingerDown;
            LeanTouch.OnFingerSet += OnFingerSet;
            LeanTouch.OnFingerUp += OnFingerUp;
            LeanTouch.OnFingerSwipe += OnFingerSwipe;

        }

        void OnDisable()
        {
#if !UNITY_EDITOR
			if (!GameData.AdsRemoved && UncleBear.GameUtilities.GetParam("isBannerOpened", "close") == "open")
			{
				AdHelper.ShowBanner();
			}
#endif

            ShowFridgeGuide(false);
            SetCharaState(false);

            DoozyUI.UIManager.PlaySound("16关烤箱门", _v3FridgePos);
            _animDoor["anim_fridgeDoor"].speed = -1;
            _animDoor["anim_fridgeDoor"].normalizedTime = 1;
            _animDoor.Play("anim_fridgeDoor");
            _lstOutFridgeItems.Clear();
            //Input.multiTouchEnabled = true;

            LeanTouch.OnFingerDown -= OnFingerDown;
            LeanTouch.OnFingerSet -= OnFingerSet;
            LeanTouch.OnFingerUp -= OnFingerUp;
            LeanTouch.OnFingerSwipe -= OnFingerSwipe;
        }

        void OnFingerDown(LeanFinger finger)
        {
            if (_objPicking != null)
                return;
            //点冰箱按钮
            //点冰箱中的食材
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));//, 1000, 1 << LayerMask.NameToLayer("Ingredient"));
            if (hit.collider != null && hit.collider.transform.parent != null)
            {
                //从冰箱里拿
                if (hit.collider.transform.parent.name.Contains("FridgeLayer"))
                {
                    //_bPicking = true;
                    _objPicking = hit.collider.gameObject;
                    if (_lstOutFridgeItems.Find(p => p.objItem == _objPicking) == null)
                        _lstOutFridgeItems.Add(new OutFridgeItem(_objPicking, _objPicking.transform.parent, _objPicking.transform.localPosition));

                    _objPicking.transform.SetLocalZ(1.5f);
                    _objPicking.transform.parent = null;
                }
                else if (hit.collider.transform.IsChildOf(_objPlate.transform))
                {
                    //_bPicking = true;
                    _objPicking = hit.collider.gameObject;
                    ReturnObjToFridge(_objPicking);
                    _objPicking.transform.parent = null;
                    //很关键
                    _objPicking.transform.DOKill();
                }

                if (_objPicking != null)
                    DishManager.Instance.JudgeIngredInFavor(_objPicking.name);
            }
        }

        void OnFingerSet(LeanFinger finger)
        {
            if (_objPicking != null)
            {
                var pos = GameUtilities.GetFingerTargetWolrdPos(finger, _objPicking);
                pos.z = 15f;
                if (pos.y < 15)
                    pos.y = 15;
                _objPicking.transform.position = Vector3.Lerp(_objPicking.transform.position, pos, 20 * Time.deltaTime);
            }
        }

        void OnFingerUp(LeanFinger finger)
        {
            if (_objPicking != null)
            {
                var hit = GameUtilities.GetRaycastHitInfo(_objPicking.transform.position, Vector3.down, 10);
                if (hit.collider != null && !IsPlateFull && 
                    (hit.collider.gameObject == _objPlate || hit.collider.transform.IsChildOf(_objPlate.transform)))
                {
                    PlaceObjToPlate(_objPicking);
                }
                else
                {
                    var item = _lstOutFridgeItems.Find(p => p.objItem == _objPicking);
                    if (item != null)
                    {
                        _objPicking.transform.SetParent(item.trsOrigin);
                        _objPicking.SetLocalPos(item.localPosOrigin);
                        _objPicking.transform.localScale = Vector3.zero;
                        _objPicking.transform.DOScale(Vector3.one, 0.5f);//.OnComplete(() => { _bPicking = false; });
                        //_lstOutFridgeItems.Remove(item);
                    }
                }


                if (OnFridgePickOver != null)
                    OnFridgePickOver.Invoke(IsPlateEmpty);
                _objPicking = null;
            }
        }

        void OnFingerSwipe(LeanFinger finger)
        {
            if (_objPicking == null && finger.SwipeScreenDelta.y < -Mathf.Abs(finger.SwipeScreenDelta.x))
                ScrollPage();
        }

        void ReturnObjToFridge(GameObject obj)
        {
            for (int i = 0; i < _objsInPlate.Length; i++)
            {
                if (_objsInPlate[i] == obj)
                {
                    _objsInPlate[i] = null;
                    break;
                }
            }
        }

        void PlaceObjToPlate(GameObject obj)
        {
            for (int i = 0; i < _objsInPlate.Length; i++)
            {
                if (_objsInPlate[i] == null)
                {
                    _objsInPlate[i] = obj;
                    _objsInPlate[i].transform.SetParent(_objPlate.transform);
                    //TODO::从配置表获取具体的缩放
                    var itemCtrller = _objsInPlate[i].GetComponent<FridgeItemCtrller>();
                    var scaleFactor = itemCtrller == null ? Vector3.one * 0.7f : itemCtrller.GetModelScale("plate") * Vector3.one;
                    _objsInPlate[i].transform.DOScale(scaleFactor, 0.5f);
                    _objsInPlate[i].transform.DOLocalMove(_v3PlatePoses[i] , 0.5f);
                        //.OnComplete(() => { _bPicking = false; });
                    break;
                }
            }
        }


    }

    public class FridgePage
    {
        float[] _fLayerYs = { 38, 25, 12 };
        GameObject _objPageBase;
        public Transform BaseTransform { get { return _objPageBase.transform; } }

        List<GameObject> _lstLayers = new List<GameObject>();

        public void AddLayer(GameObject layer)
        {
            if (_objPageBase == null)
                _objPageBase = new GameObject("Page");
            layer.transform.SetParent(BaseTransform);
            layer.transform.localPosition = new Vector3(0, _fLayerYs[_lstLayers.Count], 0);
            _lstLayers.Add(layer);
        }
    }


    public class FridgeLayerItem : ConveyorItem
    {
        bool _bCuttable;
        string _strEnterModels;
        string _strLeaveModels;
        float _fRotateY;
        Vector3 _juiceRGB;
        float _fScaleInPan;
        float _fScaleInPot;
        float _fScaleInPlate;
        float _fScaleInBowl;

        Vector3[] _v3OnePoses = { Vector3.zero };
        Vector3[] _v3DoublePoses = { new Vector3(2.5f, 0, -1.5f), new Vector3(-2.5f, 0, 2f), };
        Vector3[] _v3TripplePoses = { new Vector3(0, 0, -1f), new Vector3(-5f, 0, 3f), new Vector3(5f, 0, 3f) };

        GameObject _objLayer;
        List<GameObject> _items = new List<GameObject>();

        public override void CSVDeserialize(Dictionary<string, string[]> data, int index)
        {
            base.CSVDeserialize(data, index);
            _bCuttable = data["OriginCuttable"][index] == "1" ? true : false;
            _strEnterModels = data["OnEnterMachine"][index];
            _strLeaveModels = data["OnLeaveMachine"][index];
            _fRotateY = string.IsNullOrEmpty(data["Rotate"][index]) ? 0 : float.Parse(data["Rotate"][index]);
            //颜色
            var strRGB = data["RGB"][index];
            if (string.IsNullOrEmpty(strRGB))
                _juiceRGB = Vector3.zero;
            else {
                var rgbArray = strRGB.Split('|');
                _juiceRGB = new Vector3(float.Parse(rgbArray[0]), float.Parse(rgbArray[1]), float.Parse(rgbArray[2]));
            }
            //缩放
            _fScaleInPan = float.Parse(data["ScaleInPan"][index]);
            _fScaleInPot = float.Parse(data["ScaleInPot"][index]);
            _fScaleInPlate = float.Parse(data["ScaleInPlate"][index]);
            _fScaleInBowl = float.Parse(data["ScaleInBowl"][index]);
        }


        //创建物体的拷贝,在这里可以根据配置表的内容提前个每个物体加好一些规则,比如能否被切割,能被切割最大数目
        public GameObject GenLayerItems(GameObject layerbase)
        {
            var enterModels = ExtractSwitchModels(_strEnterModels);
            var leaveModels = ExtractSwitchModels(_strLeaveModels);

            _objLayer = GameObject.Instantiate(layerbase) as GameObject;
            _objLayer.name = "FridgeLayer";
            for (int i = 0; i < _mCount; i++)
            {
                var obj = GameObject.Instantiate(_mPrefab) as GameObject;
                obj.name = _mId;
                obj.transform.localEulerAngles = new Vector3(obj.transform.localEulerAngles.x, _fRotateY, obj.transform.localEulerAngles.z);
                obj.transform.SetParent(_objLayer.transform);
                obj.transform.localPosition = LocalPosesVec(i) + new Vector3(0, 0.5f);
                _items.Add(obj);

                obj.AddComponent<FridgeItemCtrller>().SetupCtrller(_bCuttable, enterModels, leaveModels, _juiceRGB, new float[] { _fScaleInPan, _fScaleInPot, _fScaleInPlate, _fScaleInBowl });
            }
            return _objLayer;
        }

        Dictionary<string, GameObject> ExtractSwitchModels(string modelInfo)
        {
            if (!string.IsNullOrEmpty(modelInfo))
            {
                Dictionary<string, GameObject> modelDict = new Dictionary<string, GameObject>();
                var models = modelInfo.Split(';');
                for (int i = 0; i < models.Length; i++)
                {
                    var keyvalue = models[i].Split('|');
                    modelDict.Add(keyvalue[0], ItemManager.Instance.GetItem(keyvalue[1]).Prefab);
                }
                return modelDict;
            }
            return null;
        }

        Vector3 LocalPosesVec(int index)
        {
            switch (_mCount)
            {
                case 1:
                    return _v3OnePoses[index];
                case 2:
                    return _v3DoublePoses[index];
                case 3:
                    return _v3TripplePoses[index];
            }
            return Vector3.zero;
        }

    }
}

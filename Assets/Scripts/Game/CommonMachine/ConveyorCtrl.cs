using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    //传送带
    public class ConveyorCtrl : MonoBehaviour
    {
        bool _bGenNewItem;
        bool _bPicking;
        Vector3 _v3SrcPos;
        GameObject _objPicking;
        Vector3 _v3SourceBowlPos;

        System.Action<GameObject, LeanFinger> _fingerSetCallback;
        System.Action<GameObject> _fingerUpCallback;

        int _nIgnoreLayers;

        void Awake()
        {
            _nIgnoreLayers |= (1 << LayerMask.NameToLayer("NoCollision"));
        }

        public List<GameObject> InitConveyorList(GameObject objBowl, string confPath, System.Action<GameObject> callbackUp, System.Action<GameObject, LeanFinger> callbackSet = null, bool newItem = true)
        {
            List<GameObject> conveyors = new List<GameObject>();
            _bPicking = false;
            _bGenNewItem = newItem;
            List<ConveyorItem> items = SerializationManager.LoadFromCSV<ConveyorItem>(confPath);
            List<string> itemIDs = new List<string>();
            for (int i = 0; i < items.Count; i++)
            {
                itemIDs.Add(items[i].ID);
                var newBowl = items[i].GenConveyorItems(objBowl);
                newBowl.name = "ConveyorPlate";
                newBowl.transform.position = Vector3.zero;
                conveyors.Add(newBowl);
            }

            DishManager.Instance.PickRandomFavorItem(itemIDs);
            _fingerUpCallback = callbackUp;
            _fingerSetCallback = callbackSet;

            return conveyors;
        }

        void OnEnable()
        {
            //Input.multiTouchEnabled = false;

            LeanTouch.OnFingerDown += OnFingerDown;
            LeanTouch.OnFingerSet += OnFingerSet;
            LeanTouch.OnFingerUp += OnFingerUp;
        }
        void OnDisable()
        {
            //Input.multiTouchEnabled = true;

            LeanTouch.OnFingerDown -= OnFingerDown;
            LeanTouch.OnFingerSet -= OnFingerSet;
            LeanTouch.OnFingerUp -= OnFingerUp;
        }


        void OnFingerDown(LeanFinger finger)
        {
            if (_bPicking || _objPicking != null) return;
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null)
            {
                if (hit.collider.name.Contains("item") && hit.collider.transform.parent != null && hit.collider.transform.parent.name.Contains("ConveyorPlate"))//点到菜
                {
                    _v3SourceBowlPos = hit.collider.transform.parent.position + new Vector3(0, 2, 0);
                    _objPicking = _bGenNewItem ? Lean.LeanPool.
                        Spawn(hit.collider.gameObject, _v3SourceBowlPos, Quaternion.Euler(Random.Range(-100, -80), hit.collider.transform.localEulerAngles.y, hit.collider.transform.localEulerAngles.z), null, true) : 
                        hit.collider.gameObject;
                    _v3SrcPos = _objPicking.transform.position;
                    _objPicking.name = hit.collider.gameObject.name;

                    //_objPicking.transform.position = _v3SourceBowlPos;
                    //_objPicking.transform.localEulerAngles = ;

                    DishManager.Instance.JudgeIngredInFavor(_objPicking.name);
                    _bPicking = true;
                    DoozyUI.UIManager.PlaySound("12物品拿起", _objPicking.transform.position);
                }
                else//补上点到传送带区域的逻辑,滑动传送带,改变localpos
                { }

                //if (hit.collider.gameObject.name.Contains("Bowl"))//点到碗
                //{
                //    DestroyPicking();
                //    _objPicking = GameObject.Instantiate(hit.collider.transform.GetChild(0).gameObject) as GameObject;
                //    _v3SourceBowlPos = hit.collider.transform.position + new Vector3(0, 2, 0);
                //    _objPicking.transform.position = _v3SourceBowlPos;
                //    _bPicking = true;
                //}
                //else
            }

            if (_objPicking != null)
                _objPicking.GetComponent<Rigidbody>().isKinematic = true;
        }
        void OnFingerSet(LeanFinger finger)
        {
            if (_bPicking && _objPicking != null)
            {
                if (_fingerSetCallback != null)
                    _fingerSetCallback.Invoke(_objPicking, finger);
                else
                {
                    var pos = GameUtilities.GetFingerTargetWolrdPos(finger, _objPicking, _v3SourceBowlPos.y + 2);
                    _objPicking.transform.position = Vector3.Lerp(_objPicking.transform.position, pos, 20 * Time.deltaTime);
                }

            }
        }
        void OnFingerUp(LeanFinger finger)
        {
            if (_bPicking && _objPicking != null)
            {
                //松手时只管把点击的物体传出去,由接收方处理
                if (_fingerUpCallback != null)
                    _fingerUpCallback(_objPicking);
                //_objPicking = null;
                _bPicking = false;
                //RaycastHit hit = GameUtilities.GetRaycastHitInfo(_objPicking.transform.position, Vector3.down, 1000, 1 << LayerMask.NameToLayer("Cuttable"));
                ////这里加了一个层,用来标识菜的主体
                //float vecDis = Vector2.Distance(new Vector2(hit.point.x, hit.point.z), new Vector2(_owner.ObjPizzaBody.transform.position.x, _owner.ObjPizzaBody.transform.position.z));
                //if (hit.collider != null && vecDis < _owner.PizzaRadius)
                //{
                //    _objPicking.layer = LayerMask.NameToLayer("Cuttable");
                //    _owner.AdditivesToPizza(_objPicking);
                //    _objPicking = null;
                //    _bPicking = false;
                //    _nIngredCount += 1;
                //}
                //else
                //{
                //    _objPicking.transform.DOMove(_v3SourceBowlPos, 0.3f).OnComplete(() =>
                //    {
                //        DestroyPicking();
                //        _bPicking = false;
                //    });
                //}
            }
        }

        public void ClearPicking()
        {
            _objPicking = null;
        }

        public void DestroyPicking(float scale = 1)
        {
            if (_bGenNewItem)
            {
                if (_objPicking != null)
                    Lean.LeanPool.Despawn(_objPicking);
                    //GameObject.Destroy(_objPicking);
                _objPicking = null;
            }
            else
            {
                _objPicking.SetPos(_v3SrcPos);
                _objPicking.transform.localScale = Vector3.zero;
                _objPicking.transform.DOScale(Vector3.one * scale, 0.2f).OnComplete(()=> { _objPicking = null; });
            }
        }
    }


    public class ConveyorItem : ICSVDeserializable
    {
        protected string _mId = null;
        public string ID { get { return _mId; } }
        protected GameObject _mPrefab = null;
        public GameObject Prefab { get { return _mPrefab; } }
        protected int _mCount; //显示的数量,如drpanda中冰箱一层中鱼只有1条,鸡蛋有3个

        //GameObject _objBowl;
        //List<GameObject> _items = new List<GameObject>();

        //显示的数量,一碗里面生成几个
        public int nShowCount { get { return _mCount; } }

        public virtual void CSVDeserialize(Dictionary<string, string[]> data, int index)
        {
            _mId = data["ID"][index];
            _mCount = int.Parse(data["Count"][index]);
            _mPrefab = ItemManager.Instance.GetItem(_mId).Prefab;
        }

        public GameObject GenConveyorItems(GameObject baseObj, bool ranTransform = true)
        {
            var objContainer = GameObject.Instantiate(baseObj) as GameObject;
            var factor = _mCount > 10 ? _mCount / 5f - 1 : 1;

            for (int i = 0; i < _mCount; i++)
            {
                var newObj = GameObject.Instantiate(_mPrefab) as GameObject;
                newObj.name = _mId;
                newObj.transform.SetParent(objContainer.transform);
                if (ranTransform)
                {
                    newObj.transform.localPosition = new Vector3(Random.Range(-0.5f, 0.5f) * factor, Random.Range(1.3f, 2.3f), Random.Range(-0.5f, 0.5f) * factor);
                    newObj.transform.localEulerAngles = new Vector3(Random.Range(-120, -60), Random.Range(-180, 180), Random.Range(-30, 30));
                }
                if (newObj.GetComponent<Rigidbody>() != null)
                    newObj.GetComponent<Rigidbody>().isKinematic = true;

                //_items.Add(newObj);

            }

            return objContainer;
        }
    }
}

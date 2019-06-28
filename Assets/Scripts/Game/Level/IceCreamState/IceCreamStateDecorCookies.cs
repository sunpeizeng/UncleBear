using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Touch;

namespace UncleBear
{


    public class IceCreamStateDecorCookies : State<LevelIceCream>
    {
        GameObject _objTray;
        Vector3 _v3TrayPos = new Vector3(-25, 24.2f, -19.5f);
        Vector3 _v3BowlLocalPos = new Vector3(6.5f, 1.6f, 0f);
        float _f3BottleXDelta = 6.5f;

        Dictionary<string, IceCreamDecorItem> _decorMaps;

        float _fDis2Cam;
        GameObject _objHolding;
        bool _bHolding;

        int _nDecorLimit = 15;

        public IceCreamStateDecorCookies(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            Debug.Log("decor cookies");
            base.Enter(param);

            _bHolding = false;
            _objHolding = null;
            _objTray = _owner.LevelObjs[Consts.ITEM_ICTRAY];
            _objTray.transform.DOMove(_v3TrayPos + Vector3.left * 50, 0.5f).OnComplete(CleanBarsForNewDecors);

            _fDis2Cam = Vector3.Distance(CameraManager.Instance.MainCamera.transform.position, _v3TrayPos) - 15;


            PhysicMaterial pm = new PhysicMaterial();
            pm.staticFriction = pm.dynamicFriction = 0.8f;
            pm.bounciness = 0;

            _owner.LevelObjs[Consts.ITEM_ICBALLSAUCE].GetComponentInChildren<MeshRenderer>().gameObject
                .AddMissingComponent<MeshCollider>().material = pm;

        }

        void CleanBarsForNewDecors()
        {
            var children = _objTray.transform.GetChildTrsList();
            children.ForEach(p =>
            {
                if (!p.name.Contains("Mesh"))
                    GameObject.Destroy(p.gameObject);
            });

            List<IceCreamDecorItem> items = SerializationManager.LoadFromCSV<IceCreamDecorItem>("Configs/IceCreamDecorItems");
            _decorMaps = new Dictionary<string, IceCreamDecorItem>();
            for (int i = 0; i < items.Count; i++)
            {
                _decorMaps.Add(items[i].ID, items[i]);
                var bowl = items[i].GenItems();
                bowl.GetComponent<FaceCam>().cam = CameraManager.Instance.MainCamera;
                bowl.transform.SetParent(_objTray.transform);
                bowl.SetLocalPos(_v3BowlLocalPos + Vector3.left * _f3BottleXDelta * i);
            }

            _objTray.transform.DOMove(_v3TrayPos, 1f).OnComplete(()=> {
                GuideManager.Instance.SetGuideSingleDir(_v3TrayPos, _owner.LevelObjs[Consts.ITEM_ICBALLBOWL].transform.position + Vector3.up * 3);
            });
        }


        public override string Execute(float deltaTime)
        {
            GameUtilities.LimitListPosition(_owner.BallDecors, _owner.LevelObjs[Consts.ITEM_ICBALLBOWL].transform.position, 3.5f);
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            _decorMaps.Clear();
            _decorMaps = null;
            _objTray = _objHolding = null;
            base.Exit();
        }


        protected override void OnFingerDown(LeanFinger finger)
        {
            if (_objHolding != null)
                return;
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.transform.IsChildOf(_objTray.transform))
            {
                _bHolding = true;
                var item = _decorMaps[hit.collider.name];
                _objHolding = GameObject.Instantiate(item.GenRandomItems(), hit.point, Quaternion.identity);
                if (_objHolding.name.ToLower().Contains("nut"))
                    _objHolding.transform.localEulerAngles = new Vector3(-120, Random.Range(0, 360), 0);
                _objHolding.GetComponent<Collider>().enabled = false;
                DoozyUI.UIManager.PlaySound("12物品拿起", hit.point);
                //Lean.LeanPool.Spawn(item.GenRandomItems(), hit.point, Quaternion.identity, null, true);
            }
        }

        //拖动传送带的逻辑,先写在这,不确定是否通用
        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_bHolding && _objHolding != null)
            {
                var holdPos = CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition).GetPoint(_fDis2Cam);
                _objHolding.transform.position = Vector3.Lerp(_objHolding.transform.position, holdPos, 30 * Time.deltaTime);
            }
        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            if (_bHolding && _objHolding != null)
            {
                _bHolding = false;
                var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
                if (hit.collider != null &&
                    hit.collider.name != "Mesh" &&
                    hit.collider.transform.IsChildOf(_owner.LevelObjs[Consts.ITEM_ICBALLBOWL].transform))
                {

                    GuideManager.Instance.StopGuide();
                    _objHolding.transform.SetParent(_owner.LevelObjs[Consts.ITEM_ICBALLBOWL].transform);
                    if (_objHolding.name.Contains("Cookie"))
                        _objHolding.transform.DORotate(new Vector3(-90, 0, 0), 0.3f);
                    _objHolding.transform.DOMove(hit.collider.transform.position +
                        new Vector3(Random.Range(-1.2f, 1.2f), 4, Random.Range(0f, 1f)), 0.5f).OnComplete(() =>
                       {
                           DoozyUI.UIManager.PlaySound("71撒果仁饼干", hit.point);
                           _objHolding.AddMissingComponent<SelfDestroy>();
                           _owner.BallDecors.Add(_objHolding);
                           _objHolding.GetComponent<Collider>().enabled = true;
                           _objHolding.GetComponent<Rigidbody>().isKinematic = false;
                           _objHolding = null;

                           if (_owner.BallDecors.Count > _nDecorLimit - 1)
                           {
                               StrStateStatus = "DecorCookiesFinished";
                           }
                           else if (_owner.BallDecors.Count > 3)
                           {
                               StrStateStatus = "DecorCookiesOk";
                           }
                       });
                }
                else
                {
                    GameObject.Destroy(_objHolding);
                    //Lean.LeanPool.Despawn(_objHolding);
                    _objHolding = null;
                }
            }
        }

    }

    public class IceCreamDecorItem : ICSVDeserializable
    {
        string _id;
        GameObject _prefab;
        string _subId;
        GameObject _subPrefab;

        public string ID { get { return _id; } }
        public GameObject Prefab { get { return _prefab; } }
        public string SubID { get { return _subId; } }

        public virtual void CSVDeserialize(Dictionary<string, string[]> data, int index)
        {
            _id = data["ID"][index];
            _prefab = ItemManager.Instance.GetItem(_id).Prefab;

            _subId = data["SubID"][index];
            _subPrefab = ItemManager.Instance.GetItem(_subId).Prefab;
        }

        public GameObject GenItems()
        {
            var newObj = GameObject.Instantiate(_prefab) as GameObject;
            newObj.name = _id;
            newObj.transform.localScale = Vector3.one * 2;
            return newObj;
        }

        public GameObject GenRandomItems()
        {
            var list = _subPrefab.transform.GetChildTrsList();
            int index = Random.Range(0, list.Count);

            return _subPrefab.transform.GetChild(index).gameObject;
        }
    }
}

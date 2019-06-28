using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Touch;

namespace UncleBear
{
    public class IceCreamStateSauce : State<LevelIceCream>
    {
        enum PhaseEnum
        {
            Ready,
            Returning,
            Moving,
            EnterPouring,
            Pouring,
            Enough,
        }

        Vector3 _v3CamPos = new Vector3(-14, 56.7f, 31.3f);
        Vector3 _v3CamAngle = new Vector3(30, 180, 0);

        Vector3 _v3BallBowlPos = new Vector3(-14, 23.5f, -20);
        Vector3 _v3PourSaucePos = new Vector3(-18, 33, -21.5f);

        Vector3 _v3TrayPos = new Vector3(-25, 24.2f, -19.5f);
        Vector3 _v3BottlePos = new Vector3(3.8f, 1.8f, 0.9f);
        float _f3BottleXDelta = 5f;

        Vector3 _v3SaucePos = new Vector3(-0.22f, 5f, -0.53f);

        GameObject _objTray;
        Dictionary<string, IceCreamBottleItem> _bottleMaps;

        GameObject _objHolding;
        bool _bHolding;
        Vector3 _v3SrcLocalPos;

        MeshRenderer _meshSauce;


        public IceCreamStateSauce(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            Debug.Log("sauce");
            base.Enter(param);

            //记录酱汁mesh
            _meshSauce = _owner.LevelObjs[Consts.ITEM_ICBALLSAUCE].GetComponentInChildren<MeshRenderer>();
            _meshSauce.material.color = Color.clear;
            _owner.LevelObjs[Consts.ITEM_ICBALLSAUCE].transform.SetParent(_owner.LevelObjs[Consts.ITEM_ICBALLBOWL].transform);
            _owner.LevelObjs[Consts.ITEM_ICBALLSAUCE].SetLocalPos(_v3SaucePos);

            //进场
            CameraManager.Instance.DoCamTween(_v3CamPos, _v3CamAngle);
            _owner.LevelObjs[Consts.ITEM_ICBALLBOWL].transform.DOMove(_v3BallBowlPos, 1).OnComplete(()=> {
                _objTray = _owner.LevelObjs[Consts.ITEM_ICTRAY];
                _objTray.SetAngle(Vector3.up * 90);
                _objTray.SetPos(_v3TrayPos + Vector3.left * 50);
                _objTray.transform.DOMove(_v3TrayPos, 1f).OnComplete(()=> {
                    GuideManager.Instance.SetGuideSingleDir(_v3TrayPos, _v3BallBowlPos + Vector3.up * 3);
                });
                GenBottles();
            });
            _bHolding = false;
            _objHolding = null;
        }

        void GenBottles()
        {
            List<IceCreamBottleItem> items = SerializationManager.LoadFromCSV<IceCreamBottleItem>("Configs/IceCreamBottleItems");
            _bottleMaps = new Dictionary<string, IceCreamBottleItem>();
            for (int i = 0; i < items.Count; i++)
            {
                _bottleMaps.Add(items[i].ID, items[i]);
                var bottle = items[i].GenItems();
                bottle.GetComponent<FaceCam>().cam = CameraManager.Instance.MainCamera;
                bottle.transform.SetParent(_objTray.transform);
                bottle.SetLocalPos(_v3BottlePos + Vector3.left * _f3BottleXDelta * i);
            }
        }

        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            _objTray = null;
            _meshSauce = null;

            _bottleMaps.Clear();
            _bottleMaps = null;
            base.Exit();
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            if (_objHolding != null)
                return;

            RaycastHit hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.name.Contains("Bottle"))
            {
                _bHolding = true;
                _objHolding = hit.collider.gameObject;
                _v3SrcLocalPos = _objHolding.transform.localPosition;
                DoozyUI.UIManager.PlaySound("12物品拿起", hit.point);
            }
        }

        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_bHolding && _objHolding != null)
            {
                //位置跟随指针
                var pos = GameUtilities.GetFingerTargetWolrdPos(finger, _objHolding);
                pos.z = _v3BallBowlPos.z;
                if (pos.y < _v3BallBowlPos.y + 8)
                    pos.y = _v3BallBowlPos.y + 8;
                _objHolding.transform.position = Vector3.Slerp(_objHolding.transform.position, pos, 20 * Time.deltaTime);
            }
        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            if (_bHolding && _objHolding != null)
            {
                _bHolding = false;
                RaycastHit hit = GameUtilities.GetRaycastHitInfo(_objHolding.transform.position, Vector3.down);
                if (hit.collider != null && hit.collider.transform.IsChildOf(_owner.LevelObjs[Consts.ITEM_ICBALLBOWL].transform))
                {
                    DoozyUI.UIManager.PlaySound("53咀嚼", hit.point);
                    GuideManager.Instance.StopGuide();
                    _objHolding.transform.DOMove(_v3PourSaucePos, 0.5f).OnComplete(()=> {
                        SpriteRenderFrameAnim spAnim = _objHolding.GetComponentInChildren<SpriteRenderFrameAnim>();
                        spAnim.cbOnSinglePlayFinished = onSauceToPos;
                        spAnim.ResetAndStop();
                        spAnim.Play();
                    });
                }
                else
                {
                    _objHolding.transform.DOLocalMove(_v3SrcLocalPos, 0.5f).OnComplete(() =>
                    {
                        _objHolding = null;
                    });
                }
            }
        }

        void onSauceToPos(bool isPour)
        {
            if (!isPour)
            {
                ParticleCtrller eff = EffectCenter.Instance.SpawnEffect(_bottleMaps[_objHolding.name].Effect,
                    _objHolding.transform.position + new Vector3(1.8f, -0.2f, 0), new Vector3(0, 90, 0));
                eff.transform.SetParent(_objHolding.transform);

                Vector3 colorVec = _bottleMaps[_objHolding.name].RGBVec / 255f;
                _meshSauce.gameObject.SetActive(true);
                if (_meshSauce.material.color == Color.clear)
                    _meshSauce.material.color = new Color(colorVec.x, colorVec.y, colorVec.z, 0);
                _meshSauce.material.DOColor(new Color(colorVec.x, colorVec.y, colorVec.z, 0.9f), 2.5f);

                _objHolding.transform.DOMove(_v3PourSaucePos + new Vector3(2, 0, 1), 1f).OnComplete(() =>
                {
                    _objHolding.transform.DOMove(_v3PourSaucePos, 1f).OnComplete(() =>
                    {
                        StrStateStatus = "SauceOver";
                        eff.transform.SetParent(null);
                        SpriteRenderFrameAnim spAnim = _objHolding.GetComponentInChildren<SpriteRenderFrameAnim>();
                        spAnim.Play(true);
                    });
                });
            }
            else
            {
                _objHolding.transform.DOLocalMove(_v3SrcLocalPos, 0.5f).OnComplete(() =>
                {
                    _objHolding = null;
                });
            }
        }
    }


    public class IceCreamBottleItem : ICSVDeserializable
    {
        string _id = null;
        string _effect;
        GameObject _prefab = null;
        Vector3 _rgb;

        public string ID { get { return _id; } }
        public GameObject Prefab { get { return _prefab; } }
        public string Effect { get { return _effect; } }
        public Vector3 RGBVec { get { return _rgb; } }

        public virtual void CSVDeserialize(Dictionary<string, string[]> data, int index)
        {
            _id = data["ID"][index];
            _prefab = ItemManager.Instance.GetItem(_id).Prefab;

            var strRGB = data["RGB"][index];
            if (string.IsNullOrEmpty(strRGB))
                _rgb = Vector3.one;
            else
            {
                var rgbArray = strRGB.Split('|');
                _rgb = new Vector3(float.Parse(rgbArray[0]), float.Parse(rgbArray[1]), float.Parse(rgbArray[2]));
            }

            _effect = data["Effect"][index];
        }

        public GameObject GenItems()
        {
            var newObj = GameObject.Instantiate(_prefab) as GameObject;
            newObj.name = _id;
            newObj.transform.localScale = Vector3.one * 2;
            return newObj;
        }
    }
}

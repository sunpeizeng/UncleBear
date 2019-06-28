using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Touch;

namespace UncleBear
{
    public class IceCreamStateBall : IngredientState<LevelIceCream>
    {
        enum PhaseEnum
        {
            Waiting,
            PourMix,
            Ingredient,
            PullStick,
            Over,
        }
        PhaseEnum _ballPhase;

        int _nBallCount;
        int _nIngredCount;

        Vector3 _v3ShelfPos = new Vector3(-64.2f, 31.85f, -106f);

        Vector3 _v3BallDropPos = new Vector3(-40.6f, 28f, -97.7f);
        Vector3 _v3BallBowl = new Vector3(-41f, 23.5f, -97f);
        Vector3 _v3MachinePos = new Vector3(-41f, 22.8f, -101.8f);
        Vector3 _v3CamPos = new Vector3(-50, 48.2f, -14.8f);
        Vector3 _v3CamRot = new Vector3(10, 180, 0);

        Vector3 _v3BowlPos = new Vector3(-70, 24, -64);
        Vector3 _v3PourPos = new Vector3(-51.8f, 38.5f, -103.5f);
        Vector3 _v3PourMixPos = new Vector3(4.22f, 3.42f, 0.3f);
        Vector3 _v3PourMixAngle = new Vector3(45f, 90, 0);
        ParticleCtrller _milkEff;

        GameObject _objMachine;
        GameObject _objBigBowl;
        GameObject _objBallBowl;

        Dictionary<string, IceCreamItem> _mapItems;
        GameObject _objHoliding;
        List<GameObject> _lstAddedObjs = new List<GameObject>();
        bool _bHolding;
        bool _bAddLock;
        Vector3 _v3HoldSrcPos;
        List<Vector3> _ballRGBs = new List<Vector3>();
        Vector3 _v3HoldColor;

        Vector3[] _v3BallInBowlPos = new Vector3[] {
            new Vector3(1.42f, 1.37f, 0.4f),
            new Vector3(-1.69f, 2.01f, 0.97f),
            new Vector3(0.24f, 2.62f, -1.65f)
        };

        Transform _trsShelf;

        public IceCreamStateBall(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            base.Enter(param);
            _ballPhase = PhaseEnum.Waiting;
            _nIngredCount = _nBallCount = 0;
            _ballRGBs.Clear();

            _objBigBowl = _owner.LevelObjs[Consts.ITEM_ICBOWLBIG];
            _objBigBowl.SetPos(_v3BowlPos);

            _objMachine = _owner.LevelObjs[Consts.ITEM_ICMACHINE];
            _objMachine.SetPos(_v3MachinePos + Vector3.right * 50);
            _objMachine.transform.DOMove(_v3MachinePos, 0.5f);
            _objBallBowl = _owner.LevelObjs[Consts.ITEM_ICBALLBOWL];
            _objBallBowl.SetPos(_v3BallBowl + Vector3.right * 50);
            _objBallBowl.SetAngle(new Vector3(0, 120, 0));
            _objBallBowl.transform.DOMove(_v3BallBowl, 0.5f);

            EnterKitchen.Instance.tmOvenCounter.gameObject.SetActive(false);
            CameraManager.Instance.DoCamTween(_v3CamPos, _v3CamRot, 0.5f, ()=> {
                _ballPhase = PhaseEnum.PourMix;
                HandleMixPouring();
            });

            _objHoliding = null;
            _bHolding = _bAddLock = false;
            GenIngredients();
        }

        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            _lstAddedObjs.Clear();
            _mapItems.Clear();
            _mapItems = null;
            base.Exit();
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            switch (_ballPhase)
            {
                case PhaseEnum.Ingredient:
                    {
                        if (_objHoliding == null && _nIngredCount < 3)
                        {
                            RaycastHit hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
                            if (hit.collider != null && hit.collider.transform.IsChildOf(_trsShelf))
                            {
                                _objHoliding = Lean.LeanPool.Spawn(_mapItems[hit.collider.gameObject.name].SubPrefab, hit.point, Quaternion.identity, null, true);
                                _objHoliding.transform.localScale = Vector3.one;
                                _objHoliding.name = _mapItems[hit.collider.gameObject.name].SubID;
                                _v3HoldColor = _mapItems[hit.collider.gameObject.name].RGBVec;
                                _bHolding = true;
                                _v3HoldSrcPos = _objHoliding.transform.position;

                                _objMachine.GetComponent<Animation>().Play("anim_big");

                                DishManager.Instance.JudgeIngredInFavor(_objHoliding.name);
                                DoozyUI.UIManager.PlaySound("12物品拿起", _objHoliding.transform.position);
                            }
                        }

                        if (_nIngredCount > 0 && !_bAddLock)
                        {
                            RaycastHit hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
                            //TODO::机器加一个单独脚本，用来获取子部件和控制动画
                            if (hit.collider != null &&
                                hit.collider.transform.IsChildOf(_objMachine.transform) &&
                                hit.collider.gameObject.name.Contains("Handle"))
                            {
                                GuideManager.Instance.StopGuide();
                                _ballPhase = PhaseEnum.PullStick;
                                DoozyUI.UIManager.PlaySound("70冰淇凌制作机器把手", _v3MachinePos);
                                DoozyUI.UIManager.PlaySound("59杯蛋糕-模具下压", _v3MachinePos, false, 1, 0.4f);
                                _objMachine.GetComponent<Animation>().Play("anim_down");
                                _lstAddedObjs.ForEach(p =>
                                {
                                    p.transform.DOScale(Vector3.zero, 0.5f);
                                    p.transform.DOMove(_v3MachinePos + new Vector3(0, 11, -3.5f), 0.8f).OnComplete(()=>
                                    {
                                        Lean.LeanPool.Despawn(p);
                                    });
                                });

                                LevelManager.Instance.CallWithDelay(() =>
                                {
                                    _lstAddedObjs.Clear();
                                    HandlePullMachineStick();
                                }, 1.9f);
                              
                            }
                        }
                    }
                    break;
            }
        }

        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_bHolding && _objHoliding != null)
            {
                var pos = GameUtilities.GetFingerTargetWolrdPos(finger, _objHoliding);
                pos.z = -87f;
                if (pos.y < _v3MachinePos.y)
                    pos.y = _v3MachinePos.y;
                _objHoliding.transform.position = Vector3.Lerp(_objHoliding.transform.position, pos, 20 * Time.deltaTime);
            }
        }

    
        protected override void OnFingerUp(LeanFinger finger)
        {
            if (_ballPhase == PhaseEnum.Ingredient && _bHolding && _objHoliding != null)
            {
                _bHolding = false;
                RaycastHit hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
                if (hit.collider != null && hit.collider.transform.IsChildOf(_objMachine.transform))
                {
                    _bAddLock = true;
                    _objHoliding.transform.DOMove(_v3MachinePos + new Vector3(0, 23,-2), 0.3f).OnComplete(()=> {
                        _objHoliding.transform.DOMove(_v3MachinePos + GetPosByItemName(_objHoliding), 0.25f).OnComplete(() =>
                        {
                            _objMachine.GetComponent<Animation>().Stop("anim_big");
                            DoozyUI.UIManager.PlaySound("14撒配料", _v3MachinePos);
                            GuideManager.Instance.SetGuideClick(_v3MachinePos + new Vector3(-1, 12, 0));

                            DishManager.Instance.IngredsInDish.Add(_objHoliding.name);
                            _lstAddedObjs.Add(_objHoliding);
                            //Lean.LeanPool.Despawn(_objHoliding);
                            _objHoliding = null;
                            _nIngredCount += 1;
                            _ballRGBs.Add(_v3HoldColor);
                            _bAddLock = false;
                        });
                    });
                }
                else
                {
                    _objMachine.GetComponent<Animation>().Stop("anim_big");
                    _objHoliding.transform.DOMove(_v3HoldSrcPos, 0.5f).OnComplete(() =>
                    {
                        Lean.LeanPool.Despawn(_objHoliding);
                        _objHoliding = null;
                    });
                }
            }
        }

        Vector3 GetPosByItemName(GameObject obj)
        {
            Vector3 defalutPos = new Vector3(0, 16, -2);
            switch (obj.name)
            {
                case "item_icApple":
                    { defalutPos = new Vector3(1.2f, 15.5f, -3.6f); }
                    break;
                case "item_icKiwi":
                    { defalutPos = new Vector3(-1.2f, 17.2f, -2); }
                    break;
                case "item_icPineapple":
                    {
                        obj.transform.DORotate(Vector3.right * 90, 0.2f);
                        defalutPos += Vector3.forward;
                    }
                    break;
                case "item_icStrawberry":
                    { defalutPos = new Vector3(0.44f, 18.1f, -0.78f); }
                    break;
                case "item_icBlueBerry":
                    { defalutPos = new Vector3(1.31f, 17.74f, -1.69f); }
                    break;
                case "item_icCherry":
                    { defalutPos = new Vector3(-0.84f, 18.37f, -2.04f); }
                    break;
                case "item_icWaterMelon":
                    { defalutPos = new Vector3(0, 16.7f, -4.38f); }
                    break;
            }

            return defalutPos;
        }

        void GenIngredients()
        {
            List<IceCreamItem> items = SerializationManager.LoadFromCSV<IceCreamItem>("Configs/IceCreamItems");
            _mapItems = new Dictionary<string, IceCreamItem>();
            List<string> itemIDs = new List<string>();
            items.ForEach(p =>
            {
                itemIDs.Add(p.SubID);
                _mapItems.Add(p.ID, p);
            });
            DishManager.Instance.PickRandomFavorItem(itemIDs);

            _trsShelf = _owner.LevelObjs[Consts.ITEM_ICSHELF].transform;
            _trsShelf.position = _v3ShelfPos + Vector3.left * 50;
            _trsShelf.DOMove(_v3ShelfPos, 0.8f);
            //for (int i = 0; i < items.Count; i++)
            //{
            //    GameObject plateObj = items[i].GenItems();
            //    plateObj.transform.position = _v3BowlPos;
            //    Vector3 pos = _v3Plate + new Vector3(_fDeltaX * -1 * (i % 3), 0, _fDeltaZ * (i / 3));
            //    plateObj.transform.DOMove(pos , 0.8f);
            //}
        }

        void HandleMixPouring()
        {
            _objBigBowl.transform.DOMove(_v3PourPos, 0.8f).OnComplete(()=> {
                _objBigBowl.transform.DORotate(new Vector3(0, 0, -45), 0.5f).OnComplete(()=> {
                    DoozyUI.UIManager.PlaySound("64-2往冰淇淋制作机倒牛奶", _v3MachinePos);
                    _objMachine.GetComponent<Animation>().Play("anim_small");
                    _milkEff = EffectCenter.Instance.SpawnEffect("Milk", Vector3.zero, Vector3.zero);
                    _milkEff.SetMaxTimeUseless();
                    _milkEff.transform.SetParent(_objBigBowl.transform);
                    _milkEff.gameObject.SetLocalPos(_v3PourMixPos);
                    _milkEff.gameObject.SetAngle(_v3PourMixAngle);

                    LevelManager.Instance.CallWithDelay(() =>
                    {
                        //GuideManager.Instance.SetGuideSingleDir(_v3ShelfPos, _v3MachinePos + Vector3.up * 22);
                        _milkEff.transform.SetParent(null);
                        _milkEff.ResetMaxTimeUseful();
                        //倒完奶可以开始加材料
                        _ballPhase = PhaseEnum.Ingredient;
                        _nIngredCount = 0;
                        _ballRGBs.Clear();
                        _objBigBowl.transform.DORotate(new Vector3(0, 0, -1), 0.5f).OnComplete(() =>
                        {
                            _objBigBowl.transform.DOMove(_v3BowlPos, 1f);
                        });
                    }, 1.5f);
                });
            });
          
        }

        void HandlePullMachineStick()
        {
            GameObject ballObj = null;
            switch (_ballRGBs.Count)
            {
                //生成一个球
                case 1:
                    {
                        ballObj = GameObject.Instantiate(_owner.LevelObjs[Consts.ITEM_ICBALLSINGLE]);
                        MeshRenderer[] renders = ballObj.GetComponentsInChildren<MeshRenderer>();
                        for (int i = 0; i < renders.Length; i++)
                        {
                            renders[i].material.color = new Color(_ballRGBs[0].x / 255f, _ballRGBs[0].y / 255f, _ballRGBs[0].z / 255f);
                        }
                    }
                    break;
                //生成两色球
                case 2:
                    {
                        ballObj = GameObject.Instantiate(_owner.LevelObjs[Consts.ITEM_ICBALLDOUBLE]);
                        MeshRenderer[] renders = ballObj.GetComponentsInChildren<MeshRenderer>();
                        for (int i = 0; i < renders.Length; i++)
                        {
                            renders[i].material.color = new Color(_ballRGBs[i].x / 255f, _ballRGBs[i].y / 255f, _ballRGBs[i].z / 255f);
                        }
                    }
                    break;
                //生成三色球
                case 3:
                    {
                        ballObj = GameObject.Instantiate(_owner.LevelObjs[Consts.ITEM_ICBALL]);
                        MeshRenderer[] renders = ballObj.GetComponentsInChildren<MeshRenderer>();
                        for (int i = 0; i < renders.Length; i++)
                        {
                            renders[i].material.color = new Color(_ballRGBs[i].x / 255f, _ballRGBs[i].y / 255f, _ballRGBs[i].z / 255f);
                        }
                    }
                    break;
                //白球
                default:
                    {
                        ballObj = GameObject.Instantiate(_owner.LevelObjs[Consts.ITEM_ICBALL]);
                        MeshRenderer[] renders = ballObj.GetComponentsInChildren<MeshRenderer>();
                        for (int i = 0; i < renders.Length; i++)
                        {
                            renders[i].material.color = Color.white;
                        }
                    }
                    break;
            }
            _owner.IceCreamBalls.Add(ballObj);
            ballObj.name = _nBallCount.ToString();
            //ballObj.transform.localScale = Vector3.one;
            ballObj.SetPos(_v3BallDropPos);
            ballObj.SetAngle(new Vector3(0, Random.Range(0, 360), 0));
            ballObj.transform.SetParent(_objBallBowl.transform);
            //ballObj.transform.DOScale(Vector3.one * 2.5f, 0.7f);
            ballObj.transform.DOLocalMove(_v3BallInBowlPos[_nBallCount], 0.3f).OnStart(()=> {
                DoozyUI.UIManager.PlaySound("58蝴蝶面杯蛋糕-模具抬起", ballObj.transform.position);
            }).OnComplete(() =>
            {
                if (_nBallCount >= 3)
                {
                    _ballPhase = PhaseEnum.Over;
                    _objBigBowl.SetPos(Vector3.one * 500);
                    LevelManager.Instance.CallWithDelay(() =>
                    {
                        StrStateStatus = "BallOver";
                    }, 0.5f);
                  
                }
                else
                {
                    _objBallBowl.transform.DORotate(new Vector3(0, (_nBallCount + 1) * 120, 0), 0.5f).SetDelay(0.5f).OnComplete(() =>
                    {
                        _ballPhase = PhaseEnum.PourMix;
                        HandleMixPouring();
                    });
                }
            });
            _nBallCount += 1;
        }
    }

    public class IceCreamItem : ICSVDeserializable
    {
        string _id = null;
        string _subItemId;
        //GameObject _prefab = null;
        GameObject _subPrefab = null;
        Vector3 _rgb;

        public string ID { get { return _id; } }
        //public GameObject Prefab { get { return _prefab; } }
        public string SubID { get { return _subItemId; } }
        public GameObject SubPrefab { get { return _subPrefab; } }
        public Vector3 RGBVec { get { return _rgb; } }

        public virtual void CSVDeserialize(Dictionary<string, string[]> data, int index)
        {
            _id = data["ID"][index];
            //_prefab = ItemManager.Instance.GetItem(_id).Prefab;
            _subItemId = data["Base"][index];
            _subPrefab = ItemManager.Instance.GetItem(_subItemId).Prefab;

            var strRGB = data["RGB"][index];
            if (string.IsNullOrEmpty(strRGB))
                _rgb = Vector3.one;
            else
            {
                var rgbArray = strRGB.Split('|');
                _rgb = new Vector3(float.Parse(rgbArray[0]), float.Parse(rgbArray[1]), float.Parse(rgbArray[2]));
            }
        }

        //public GameObject GenItems()
        //{
        //    var newObj = GameObject.Instantiate(_prefab) as GameObject;
        //    newObj.name = _id;
        //    return newObj;
        //}
        public GameObject GenSubItems()
        {
            var newObj = GameObject.Instantiate(_subPrefab) as GameObject;
            newObj.name = _subItemId;
            return newObj;
        }
    }
}

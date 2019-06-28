using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    public class SushiStateIngredient : IngredientState<LevelSushi>
    {

        public enum SushiIngredState
        {
            Normal,
            WaitingRiceAnim,
        }
        SushiIngredState _eState;
        Animation _animRice;
        BoxCollider _colRice;

        float _fDis2Cam;
        Vector3 _v3CamPos1 = new Vector3(2.5f, 95, 51f);
        Vector3 _v3CamPos2 = new Vector3(-17.5f, 95, 51f);

        bool _bRicePicked;
        bool _bNoriPicked;
        GameObject _objPicking;

        Vector3 _v3TurnTablePos = new Vector3(-50.5f, 22.5f, -21);
        Vector3 _v3ChopBoardPos = new Vector3(-8, 22.6f, -19f);
        Vector3 _v3RiceBowlPos = new Vector3(22f, 22.5f, -29f);
        Vector3 _v3NoriBasePos = new Vector3(17.7f, 23.5f, -13.2f);
        Vector3 _v3BambooPos = new Vector3(-8, 24.5f, -18.8f);

        //12个路点
        List<Vector3> _lstTableWps = new List<Vector3>() {
            new Vector3(22, 3f, 0),
            new Vector3(17.5f, 3f, -8),
            new Vector3(9, 3f, -9.5f),
            new Vector3(0, 3f, -9.5f),
            new Vector3(-9, 3f, -9.5f),
            new Vector3(-17.5f, 3f, -8),
            new Vector3(-22, 3f, 0),
            new Vector3(-17.5f, 3f, 8),
            new Vector3(-9, 3f, 9.5f),
            new Vector3(0, 3f, 9.5f),
            new Vector3(9, 3f, 9.5f),
            new Vector3(17.5f, 3f, 8)
        };

        Vector3[] _v3TrippleStuffPos = new Vector3[] { new Vector3(0, 1, 1), new Vector3(0, 1, -1), new Vector3(0, 2, 0) };

        bool _bHittingRice;
        bool _bHittingNori;
        bool _bHittingStuff;
        Vector3 _v3StuffInPlatePos;
        int _nSushiBottomPhase;

        public SushiStateIngredient(int stateEnum) : base(stateEnum)
        {
            _bShowCustomer = false;
        }

        public override void Enter(object param)
        {
            _fHoldRiceTime = 0;
            _eState = SushiIngredState.Normal;
            EnterKitchen.Instance.ObjHearth.SetActive(false);
            EnterKitchen.Instance.ObjHearthLight.SetActive(false);

            //Input.multiTouchEnabled = false;
            CameraManager.Instance.DoCamTween(_v3CamPos1, new Vector3(45, 180, 0), 0.5f);
            _owner.LevelObjs[Consts.ITEM_RICE].SetPos(_v3RiceBowlPos + Vector3.up * 7);
            _owner.LevelObjs[Consts.ITEM_CHOPBOARD].SetPos(_v3ChopBoardPos);
            _owner.LevelObjs[Consts.ITEM_NORIBASE].SetPos(_v3NoriBasePos);
            _owner.LevelObjs[Consts.ITEM_RICEBUCKET].SetPos(_v3RiceBowlPos);
            _owner.LevelObjs[Consts.ITEM_TURNTABLE].SetPos(_v3TurnTablePos);
            _owner.LevelObjs[Consts.ITEM_BAMBOO].SetPos(_v3BambooPos);
            GenSushiTurnTableItem();

            _objPicking = null;
            _bRicePicked = _bNoriPicked = false;
            _nSushiBottomPhase = 0;

            base.Enter(param);
            _fDis2Cam = Vector3.Distance((_v3CamPos1 + _v3CamPos2) / 2, _v3BambooPos) - 15;

            GuideManager.Instance.SetGuideSingleDir(_v3NoriBasePos, _v3BambooPos, true, true, 1.5f);
        }
        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            //Input.multiTouchEnabled = true;
            base.Exit();
        }

        //生成寿司转盘
        void GenSushiTurnTableItem()
        {
            int itemIndex = 0;
            List<ConveyorItem> items = SerializationManager.LoadFromCSV<ConveyorItem>("Configs/SushiIngredItems");
            List<string> itemIDs = new List<string>();
            items.ForEach(p => {
                itemIDs.Add(p.ID);
            });
            DishManager.Instance.PickRandomFavorItem(itemIDs);
            for (int i = 0; i < _lstTableWps.Count; i++)
            {
                GameObject newPlate = items[itemIndex].GenConveyorItems(_owner.LevelObjs[Consts.ITEM_PLATESTUFF], false);
                //不超过3
                switch (newPlate.transform.childCount)
                {
                    case 2:
                        newPlate.transform.GetChild(1).localPosition = Vector3.up * 2;
                        break;
                    case 3:
                        {
                            newPlate.transform.GetChild(1).localPosition = _v3TrippleStuffPos[0];
                            newPlate.transform.GetChild(2).localPosition = _v3TrippleStuffPos[1];
                        }
                        break;
                    case 4:
                        {
                            newPlate.transform.GetChild(1).localPosition = _v3TrippleStuffPos[0];
                            newPlate.transform.GetChild(2).localPosition = _v3TrippleStuffPos[1];
                            newPlate.transform.GetChild(3).localPosition = _v3TrippleStuffPos[2];
                        }
                        break;
                }
                newPlate.SetAngle(new Vector3(0, Random.Range(-180, 180), 0));

                newPlate.SetPos(_lstTableWps[i] + _v3TurnTablePos);
                newPlate.transform.DOPath(GetPathArray(i), 15, PathType.CatmullRom, PathMode.Full3D, 10, Color.red).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear).SetOptions(true);
                itemIndex += 1;
                if (itemIndex > items.Count - 1)
                    itemIndex = 0;
            }
        }

        Vector3[] GetPathArray(int index)
        {
            if (index > _lstTableWps.Count - 1)
                return null;
            var newPath = new List<Vector3>();
            while (newPath.Count < _lstTableWps.Count - 1)
            {
                index += 1;
                if (index > _lstTableWps.Count - 1)
                    index = 0;
                newPath.Add(_lstTableWps[index] + _v3TurnTablePos);
            }
            return newPath.ToArray();
        }

        #region 操作
        protected override void OnFingerDown(LeanFinger finger)
        {
            if (_bHittingRice || _bHittingNori || _bHittingStuff || _objPicking != null)
                return;
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null)
            {

                if (_eState != SushiIngredState.WaitingRiceAnim)
                {
                    if (!_bRicePicked && hit.collider.gameObject == _owner.LevelObjs[Consts.ITEM_RICEBUCKET])
                    {
                        //这里试一下池
                        _objPicking = _owner.LevelObjs[Consts.ITEM_RICE];//Lean.LeanPool.Spawn(_owner.LevelObjs[Consts.ITEM_RICE], _v3RiceBowlPos + Vector3.up * 2, Quaternion.identity);
                        _bHittingRice = true;
                        DoozyUI.UIManager.PlaySound("12物品拿起", _objPicking.transform.position);
                    }

                    else if (!_bNoriPicked && hit.collider.gameObject == _owner.LevelObjs[Consts.ITEM_NORIBASE])
                    {
                        _objPicking = Lean.LeanPool.Spawn(_owner.LevelObjs[Consts.ITEM_NORI], _v3NoriBasePos + Vector3.up * 2, Quaternion.identity, null, true);
                        _bHittingNori = true;
                        DoozyUI.UIManager.PlaySound("20海苔", _objPicking.transform.position);
                    }

                    else if (_bNoriPicked && _bRicePicked && hit.collider.transform.parent != null && hit.collider.transform.parent.name.Contains("Plate"))
                    {
                        _objPicking = hit.collider.gameObject;
                        _objPicking.layer = LayerMask.NameToLayer("Ignore Raycast");
                        _v3StuffInPlatePos = _objPicking.transform.localPosition;
                        _bHittingStuff = true;

                        DishManager.Instance.JudgeIngredInFavor(_objPicking.name);
                        DoozyUI.UIManager.PlaySound("12物品拿起", _objPicking.transform.position);
                    }
                }
                else if (hit.collider.name == "Rice")
                {
                    _bHittingRice = true;
                    DoozyUI.UIManager.PlaySound("11面团饭团", _v3BambooPos);
                }

            }

        }
        float _fHoldRiceTime;
        protected override void OnFingerSet(LeanFinger finger)
        {
           switch(_eState)
           {
               case SushiIngredState.Normal:
                   {
                       if ((_bHittingRice || _bHittingNori || _bHittingStuff) && _objPicking != null)
                       {
                           //如果点出米或者海苔,就控制移动,监控是否在竹简上方
                           var holdPos = CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition).GetPoint(_fDis2Cam);//GameUtilities.GetFingerTargetWolrdPos(finger, _objPicking, _v3ChopBoardPos.y + 5);
                           _objPicking.transform.position = Vector3.Lerp(_objPicking.transform.position, holdPos, 30 * Time.deltaTime);
                       }
                   }
                   break;
               case SushiIngredState.WaitingRiceAnim:
                   {
                       if (_bHittingRice)
                       {
                           _fHoldRiceTime += Time.deltaTime;
                           _fHoldRiceTime = Mathf.Clamp01(_fHoldRiceTime);
                           _animRice.SampleAnim("anim_rice", _fHoldRiceTime);
                           if (_fHoldRiceTime >= 1)
                           {
                               _bHittingRice = false;
                               GameObject.Destroy(_colRice);
                               _colRice = null;
                               _eState = SushiIngredState.Normal;
                               _bRicePicked = true;
                               CheckCamPhase();
                           }
                       }
                   }
                   break;
            }
        }
        protected override void OnFingerUp(LeanFinger finger)
        {
            if ((_bHittingRice || _bHittingNori || _bHittingStuff) && _objPicking != null)
            {
                var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
                if (hit.collider != null && hit.collider.gameObject == _owner.LevelObjs[Consts.ITEM_BAMBOO])
                {
                    if (_bHittingRice)
                    {
                        var posYOffset = _bNoriPicked? 0.8f : 0.5f;
                        _owner.IsNoriOutside = _bNoriPicked;
                        //_objPicking.GetComponentInChildren<Animation>().Play("anim_rice");
                        _objPicking.transform.GetChild(0).DOLocalMove(new Vector3(0, 0, 6.5f), 0.5f);

                        _objPicking.transform.DOMove(_v3BambooPos + new Vector3(0, posYOffset, -6.5f), 0.5f).OnComplete(() =>
                        {
                            DoozyUI.UIManager.PlaySound("14撒配料", _v3BambooPos);
                            _eState = SushiIngredState.WaitingRiceAnim;
                            _animRice = _objPicking.GetComponentInChildren<Animation>();
                            _colRice = _objPicking.AddComponent<BoxCollider>();
                            _colRice.size = new Vector3(15, 1, 15);
                            _colRice.center = new Vector3(0, 1, 7.5f);

                            GuideManager.Instance.SetGuideClick(_v3BambooPos - Vector3.right, 0.5f);
                            _objPicking.transform.SetParent(_owner.LevelObjs[Consts.ITEM_BAMBOO].transform);
                            _objPicking.name = "Rice";
                            _objPicking = null;
                           
                        });
                    }
                    else if (_bHittingNori)
                    {
                        var posYOffset = _bRicePicked ? 2f : 0.5f;
                        _bNoriPicked = true;
                        _objPicking.transform.DOMove(_v3BambooPos + new Vector3(0, posYOffset, 1.5f), 0.5f).OnComplete(() =>
                        {
                            DoozyUI.UIManager.PlaySound("20海苔", _v3BambooPos);
                            GuideManager.Instance.SetGuideSingleDir(_v3RiceBowlPos, _v3BambooPos, true, true, 1.5f);
                            _objPicking.transform.SetParent(_owner.LevelObjs[Consts.ITEM_BAMBOO].transform);
                            _objPicking.name = "Nori";
                            _objPicking = null;
                            CheckCamPhase();
                        });
                    }
                    else if (_bHittingStuff)
                    {
                        _objPicking.transform.SetParent(_owner.LevelObjs[Consts.ITEM_BAMBOO].transform);
                        //_objPicking.transform.DOScale(new Vector3(1, 0.55f, 0.55f), 0.3f);
                        _objPicking.transform.localEulerAngles = new Vector3(-90, 0, 0);//Vector3.zero;
                        _objPicking.layer = LayerMask.NameToLayer("Cuttable");
                        var hitPos = hit.point;
                        var disVec = hitPos - _v3BambooPos;
                        if (disVec.magnitude > 6)
                        {
                            hitPos = _v3BambooPos + disVec.normalized * 6;
                        }
                        float offsetY = _owner.LevelObjs[Consts.ITEM_BAMBOO].transform.FindChild("Nori").position.y 
                            < _owner.LevelObjs[Consts.ITEM_BAMBOO].transform.FindChild("Rice").position.y ? 1.4f : 2.5f;
                        _objPicking.transform.DOMove(new Vector3(hitPos.x, _v3BambooPos.y + offsetY, hitPos.z), 0.5f).OnComplete(() =>
                        {
                            DoozyUI.UIManager.PlaySound("14撒配料", _v3BambooPos);
                            DishManager.Instance.IngredsInDish.Add(_objPicking.name);
                            _nSushiBottomPhase++;
                            if (_nSushiBottomPhase >= 1)
                            {
                                StrStateStatus = "IngredientOK";
                                if (_nSushiBottomPhase >= 5)
                                    StrStateStatus = "IngredientOver";
                            }
                            _objPicking = null;
                        });
                    }
                }
                else
                {
                    if (_bHittingRice)
                    {
                        _objPicking.SetPos(_v3RiceBowlPos + Vector3.up * 7);
                        _objPicking.transform.localScale = Vector3.zero;
                        _objPicking.transform.DOScale(Vector3.one, 0.3f).OnComplete(() => { _objPicking = null; });
                    }
                    else if (_bHittingNori)
                    {
                        Lean.LeanPool.Despawn(_objPicking);
                        _objPicking = null;
                    }
                    else if (_bHittingStuff)
                    {
                        _objPicking.layer = LayerMask.NameToLayer("Cuttable");
                        _objPicking.SetLocalPos(_v3StuffInPlatePos);
                        _objPicking.transform.localScale = Vector3.zero;
                        _objPicking.transform.DOScale(Vector3.one, 0.3f).OnComplete(()=> { _objPicking = null; });
                    }
                }
            }
            _bHittingNori = false;
            _bHittingRice = false;
            _bHittingStuff = false;
        }
        #endregion

        void CheckCamPhase()
        {
            if (_bNoriPicked && _bRicePicked)
                CameraManager.Instance.DoCamTween(_v3CamPos2, 0.5f, () =>
                {

                    GuideManager.Instance.StopGuide();
                    DishManager.Instance.FixRenderCamHeightByCustomer();
                    UIPanelManager.Instance.GetPanel("UIPanelDishLevel").ShowSubElements("UIRenderChara");
                });
            else if (!_bNoriPicked)
            {
                GuideManager.Instance.SetGuideSingleDir(_v3NoriBasePos, _v3BambooPos, true, true, 1.5f);
            }
        }
    }
}

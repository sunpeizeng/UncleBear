using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    //公用的自由烹饪状态
    public class FreeCookingState<T> : State<T>
    {
        public enum FingeringState
        {
            Nothing,
            Cutboard,
            Pot,
            Oven,
            Pan,
        }
        FingeringState _eFingering;

        PanCtrl _ctrllerPan;
        CutboardCtrl _ctrllerCutboard;
        PotCtrl _ctrllerPot;
        OvenCtrl _ctrllerOven;

        //不管怎么处理,最后的物体就是在原来盘子上的子物体
        GameObject _objIngredPlate;
        GameObject _objPan;
        GameObject _objPot;
        GameObject _objCutboard;
        GameObject _objOven;
        Animation _animOven;
        Animation _animPan;
        Animation _animPot;
        Animation _animCutboard;

        Vector3 _v3CamPos = new Vector3(155, 134, 104);
        Vector3 _v3CamRot = new Vector3(25, 228, 0);

        Vector3 _v3PlatePos = new Vector3(12.6f, 22.5f, -21);
        Vector3 _v3CutBoardPos = new Vector3(-46.9f, 22.5f, -100);//180度
        Vector3 _v3PanPos = new Vector3(4f, 23.9f, -99f);//90度
        Vector3 _v3PotPos = new Vector3(-12.4f, 23.9f, -99f);

        GameObject _objPicking;
        Transform _trsPickOrigin;
        Vector3 _v3PickOriginPos;
        float _fPickIngredDis;

        bool _bEnableCook;

        protected int _nIngredsCount;
        //protected List<Transform> _lstIngreds = new List<Transform>();
        protected GameObject _objFinalPlace;


        public FreeCookingState(int stateEnum) : base(stateEnum)
        {

        }
        public override void Enter(object param)
        {
            //Debug.Log("free");
            base.Enter(param);

            _bEnableCook = false;

            Vector3 chefPos = CharaCreator.Chef.transform.position;

            CharaCreator.Chef.PathMove(new List<Vector3>() { chefPos + Vector3.forward * 20, chefPos + Vector3.right * 50 },true, ()=> {
               
             
                _objCutboard.SetPos(_v3CutBoardPos);
                _objPan.SetPos(_v3PanPos);
                _objPot.SetPos(_v3PotPos);

                //记录盘子里的原材料
                _nIngredsCount = 0;
                for (int i = 0; i < _objIngredPlate.transform.childCount; i++)
                {
                    var child = _objIngredPlate.transform.GetChild(i);
                    if (child.name.Contains("item"))
                        _nIngredsCount += 1;
                }
                CheckIngredPlate();

                //TODO::最好调UI遮一下,摄像机变成大局位置
                CameraManager.Instance.SetCamTransform(_v3CamPos, _v3CamRot);
                CharaCreator.Chef.SetTransform(new Vector3(124, 0, -62.7f), new Vector3(0, 270, 0));
                CharaCreator.Chef.PathMove(new List<Vector3>() { new Vector3(17.5f, 0, -60) } , true, () =>
                {
                    CharaCreator.Chef.transform.DORotate(Vector3.zero, 0.3f).OnComplete(() =>
                    {
                        CharaCreator.Chef.ChangeCharaState(CharaStateEnum.Move);
                        _objIngredPlate.transform.SetParent(null);
                        _objIngredPlate.transform.DORotate(Vector3.zero, 0.5f);
                        _objIngredPlate.transform.DOMove(_v3PlatePos, 0.5f).OnComplete(()=> {
                            //CharaCreator.Chef.ChangeCharaState(CharaStateEnum.Move);
                            var newPoint = new Vector3(-55.9f, 0, -61f);
                            CharaCreator.Chef.transform.DOLookAt(newPoint, 0.3f).OnComplete(()=> {
                                CharaCreator.Chef.PathMove(new List<Vector3>() { new Vector3(0 , 0, -67), new Vector3(-21.5f, 0 ,-67), newPoint }, true, () =>
                                {
                                    CharaCreator.Chef.transform.DORotate(new Vector3(0, 422, 0), 0.3f).OnComplete(() =>
                                    {
                                        CharaCreator.Chef.ChangeCharaState(CharaStateEnum.Wait, false);
                                    });
                                });
                            });
                        });
                    });
                });
            });

            var tools = param as Dictionary<string, GameObject>;
            _objIngredPlate = tools[Consts.ITEM_FRIDGEPLATE];
            _objCutboard = tools[Consts.ITEM_CUTBOARD];
            _objPan = tools[Consts.ITEM_PAN];
            _objPot = tools[Consts.ITEM_POT];
           
            _objOven = EnterKitchen.Instance.ObjOvenDoor;
            _animOven = _objOven.GetComponent<Animation>();
            _animPan = _objPan.transform.FindChild("Mesh").GetComponent<Animation>();
            _animPot = _objPot.transform.FindChild("Mesh").GetComponent<Animation>();
            _animCutboard = _objCutboard.transform.FindChild("Mesh").GetComponent<Animation>();
        }

        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            _as2Pan = _as2Cutboard = _as2Pot = null;
            base.Exit();
            if (_objFinalPlace != null)
            {
                _objFinalPlace.SetRigidBodiesKinematic(true);
                GameObject.Destroy(_objFinalPlace.GetComponent<IngredsLimitter>());
            }
        }
        #region Animation
        void PlayMachineCheckingAnim(Animation anim, string state, float speed)
        {
            anim[state].speed = speed;
            if (anim[state].normalizedTime > 1)
                anim[state].normalizedTime = 1;
            else if (anim[state].normalizedTime < 0)
            {
                if (speed > 0 && (_as2Oven == null || !_as2Oven.isActiveAndEnabled))
                    _as2Oven = DoozyUI.UIManager.PlaySound("15开烤箱门", _animOven.transform.position);
                anim[state].normalizedTime = 0;
            }
            anim.Play(state);
        }

        AudioSource _as2Pan;
        AudioSource _as2Cutboard;
        AudioSource _as2Pot;
        AudioSource _as2Oven;
        void CheckMachineAnim()
        {
            //if (_eFingering != FingeringState.Oven)
            //    PlayMachineCheckingAnim(_animOven, "anim_toOven", -1);
            //else
            //    PlayMachineCheckingAnim(_animOven, "anim_toOven", 1);
            switch (_eFingering)
            { 
                case FingeringState.Oven:
                    PlayMachineCheckingAnim(_animOven, "anim_toOven", 1);
                    break;
                case FingeringState.Pan:
                    _animPan["anim_toPan"].speed = 1;
                    _animPan.Play("anim_toPan");
                    if (_as2Pan == null || !_as2Pan.isActiveAndEnabled)
                        _as2Pan = DoozyUI.UIManager.PlaySound("35铁锅移动声", _animPan.transform.position);
                    break;
                case FingeringState.Pot:
                    _animPot["anim_toPot"].speed = 1;
                    _animPot.Play("anim_toPot");
                    if (_as2Pot == null || !_as2Pot.isActiveAndEnabled)
                        _as2Pot = DoozyUI.UIManager.PlaySound("35铁锅移动声", _animPot.transform.position);
                    break;
                case FingeringState.Cutboard:
                    _animCutboard["anim_toCutboard"].speed = 1;
                    _animCutboard.Play("anim_toCutboard");
                    if (_as2Cutboard == null || !_as2Cutboard.isActiveAndEnabled)
                        _as2Cutboard = DoozyUI.UIManager.PlaySound("56菜板声", _animCutboard.transform.position);
                    break;
                default:
                    PlayMachineCheckingAnim(_animOven, "anim_toOven", -1);
                    _animPan.SampleAnim("anim_toPan", 0);
                    _animPot.SampleAnim("anim_toPot", 0);
                    _animCutboard.SampleAnim("anim_toCutboard", 0);
                    break;
            }
        }
        #endregion

        #region LeanFinger
        //这里把三种操作逻辑直接写了,如果以后涉及到处理方式不同的地方,可以拆分成多个方法供重载
        protected override void OnFingerDown(LeanFinger finger)
        {
            if (_objPicking != null || _bEnableCook)
                return;
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null)
            {
                if (hit.collider.transform.parent != null)
                {
                    if (hit.collider.transform.parent == _objIngredPlate.transform)//点到托盘里的菜
                        _objPicking = hit.collider.gameObject;
                    else if (hit.collider.transform.parent.parent != null && hit.collider.transform.parent.parent == _objIngredPlate.transform)//点到托盘里菜碎片
                        _objPicking = hit.collider.transform.parent.gameObject;
                }
                if (_objPicking != null)
                {
                    _trsPickOrigin = _objPicking.transform.parent;
                    _v3PickOriginPos = _objPicking.transform.localPosition;
                    _fPickIngredDis = Vector3.Distance(CameraManager.Instance.MainCamera.transform.position, _objPicking.transform.position);
                }
            }

        }
        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_bEnableCook)
                return;

            _eFingering = FingeringState.Nothing;
            if (_objPicking != null)
            {
                var fingerWolrdPos = GameUtilities.GetScreenRayWorldPos(finger.ScreenPosition, _fPickIngredDis - 20);
                _objPicking.transform.position = Vector3.Lerp(_objPicking.transform.position, fingerWolrdPos, Time.deltaTime * 20);
                var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition), 1000, 1 << LayerMask.NameToLayer("CookMachine"));
                if (hit.collider != null)
                {
                    //TODO::判断是否支持切割,播动画或显示UI提示不支持
                    if (hit.collider.transform.IsChildOf(_objPan.transform))
                    {
                        _eFingering = FingeringState.Pan;
                    }
                    else if (hit.collider.transform.IsChildOf(_objOven.transform))
                    {
                        _eFingering = FingeringState.Oven;
                    }
                    else if (hit.collider.transform.IsChildOf(_objPot.transform))
                    {
                        _eFingering = FingeringState.Pot;
                    }
                    else if (hit.collider.transform.IsChildOf(_objCutboard.transform))
                    {
                        FridgeItemCtrller itemCtrller = _objPicking.GetComponent<FridgeItemCtrller>();
                        if (itemCtrller != null)
                        {
                            if (itemCtrller.bOriginCuttable || itemCtrller.bModelChanged)
                            {
                                _eFingering = FingeringState.Cutboard;
                            }
                        }
                    }
                }
                CheckMachineAnim();
            }
           
        }
        protected override void OnFingerUp(LeanFinger finger)
        {
            if (_bEnableCook)
                return;

            _eFingering = FingeringState.Nothing;
            CheckMachineAnim();
            if (_objPicking != null)
            {
                List<FridgeItemCtrller> itemCtrllers = new List<FridgeItemCtrller>(_objPicking.GetComponentsInChildren<FridgeItemCtrller>());
                _bEnableCook = false;
                //激活对应料理设备的控制脚本
                var hits = GameUtilities.GetRaycastAllHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));//,1000, 1 << LayerMask.NameToLayer("CookMachine"));
                if (hits != null)
                {
                    for (int i = 0; i < hits.Length; i++)
                    {
                        if (hits[i].collider.transform.IsChildOf(_objPan.transform))//放进煎锅
                        {
                            //Debug.Log("Enter Pan");
                            _ctrllerPan = _objPan.AddMissingComponent<PanCtrl>();
                            _ctrllerPan.RegisterObject(_objPicking, OnPanFriedFinish, OnIngredientDealt);
                            _ctrllerPan.enabled = true;
                            //CameraManager.Instance.PlayCamAnim("CP_Free2Pan");
                            CameraManager.Instance.DoCamTween(new Vector3(10.7f, 68, -58), new Vector3(45, 190, 0), 0.5f);

                            itemCtrllers.ForEach(p => {
                                p.OnEnterMachine("pan");
                                p.ModelScale("pan");
                            });

                            _objPicking = null;
                            _bEnableCook = true;
                            break;
                        }
                        else if (hits[i].collider.transform.IsChildOf(_objOven.transform))//放进烤箱
                        {
                            //Debug.Log("Enter Oven");
                            _ctrllerOven = _objOven.AddMissingComponent<OvenCtrl>();
                            _ctrllerOven.RegisterObject(_objPicking, OnOvenBakedFinish, OnIngredientDealt);
                            _ctrllerOven.enabled = true;
                            CameraManager.Instance.DoCamTween(new Vector3(-6.5f, 44.8f, -16.7f), new Vector3(20, 180, 0), 0.5f);

                             itemCtrllers.ForEach(p => {
                                p.OnEnterMachine("oven");
                            });

                            _objPicking = null;
                            _bEnableCook = true;
                            break;
                        }
                        else if (hits[i].collider.transform.IsChildOf(_objPot.transform))//放进煮锅
                        {
                            //Debug.Log("Enter Pot");
                            _ctrllerPot = _objPot.AddMissingComponent<PotCtrl>();
                            _ctrllerPot.RegisterObject(_objPicking, OnPotFinish, OnIngredientDealt);
                            _ctrllerPot.enabled = true;
                            //CameraManager.Instance.PlayCamAnim("CP_Free2Pot");
                            CameraManager.Instance.DoCamTween(new Vector3(-5.7f, 61.6f, -81), new Vector3(60, 200, 0), 0.5f);

                            itemCtrllers.ForEach(p =>
                            {
                                p.OnEnterMachine("pot");
                                p.ModelScale("pot");
                            });

                            _objPicking = null;
                            _bEnableCook = true;
                            break;
                        }
                        else if (hits[i].collider.transform.IsChildOf(_objCutboard.transform))//放进切板
                        {
                            var isLimit = false;
                            if (itemCtrllers.Count > 0)
                            {
                                if (!itemCtrllers[0].bOriginCuttable && !itemCtrllers[0].bModelChanged)
                                {
                                    //Debug.Log("Forbidden");
                                    isLimit = true;
                                }
                            }
                            if (_objPicking.GetComponent<CutterCounter>() != null)
                            {
                                if (_objPicking.GetComponent<CutterCounter>().bIsLimitCount)
                                {
                                    //Debug.Log("CountLimited");
                                    isLimit = true;
                                }
                            }
                            if (!isLimit)
                            {
                                //Debug.Log("Enter Cutboard");
                                _ctrllerCutboard = _objCutboard.AddMissingComponent<CutboardCtrl>();
                                _ctrllerCutboard.SetKnife(false);
                                //CameraManager.Instance.PlayCamAnim("CP_Free2Cutboard");
                                CameraManager.Instance.DoCamTween(new Vector3(-48.2f, 85, -89), new Vector3(80, 190, 0), 0.5f, () =>
                                {
                                    _ctrllerCutboard.RegisterObject(_objPicking, OnCutboardFinish, OnIngredientDealt);
                                    _ctrllerCutboard.enabled = true;
                                    _objPicking = null;
                                });

                                itemCtrllers.ForEach(p =>
                                {
                                    p.OnEnterMachine("cutboard");
                                });
                                _bEnableCook = true;
                            }
                            else
                                DoozyUI.UIManager.PlaySound("52错误音效");
                            break;
                        }
                        else if (hits[i].collider.transform.IsChildOf(_objFinalPlace.transform))//放进最后的碗
                        {
                            _objPicking.transform.SetParent(_objFinalPlace.transform);
                            _objFinalPlace.AddMissingComponent<IngredsLimitter>();
                            _objPicking.SetLocalPos(Vector3.up * 8);
                            var colliders = _objPicking.GetComponentsInChildren<BoxCollider>();
                            for (int j = 0; j < colliders.Length; j++)
                                colliders[j].size *= 0.65f;
                            //_objPicking.transform.DOLocalMoveY(2, 0.5f);
                            itemCtrllers.ForEach(p =>
                            {
                                p.OnEnterMachine("pan");
                                p.ModelScale("bowl");
                            });

                            _objPicking.SetRigidBodiesKinematic(false);

                            _nIngredsCount -= 1;
                            DoozyUI.UIManager.PlaySound("14撒配料", _objPicking.transform.position, false, 1, 0.3f);
                            _objPicking = null;
                            CheckIngredPlate();
                            break;
                        }
                    }
                }

                if (!_bEnableCook && _objPicking != null)
                {
                    _objPicking.transform.DOLocalMove(_v3PickOriginPos, 0.5f).OnComplete(() => { _objPicking = null; });
                }
            }


        }

        #endregion

        //每个工具都要有回调
        protected virtual void OnOvenBakedFinish(GameObject obj)
        {
            _ctrllerOven.enabled = false;
            BackToChoose(obj, "oven");
        }
        protected virtual void OnPanFriedFinish(GameObject obj)
        {
            _ctrllerPan.enabled = false;
            BackToChoose(obj, "pan");
        }
        protected virtual void OnCutboardFinish(GameObject obj)
        {
            _ctrllerCutboard.enabled = false;
            _ctrllerCutboard.SetKnife(true);
            BackToChoose(obj, "cutboard");
        }
        protected virtual void OnPotFinish(GameObject obj)
        {
            _ctrllerPot.enabled = false;
            BackToChoose(obj, "pot");
        }
        protected virtual void OnIngredientDealt(bool isLimit)
        {
            StrStateStatus = isLimit ? "IngredientDealLimited" : "IngredientDealt";
        }

        void BackToChoose(GameObject animObj, string machineName, float dur = 1)
        {
            CameraManager.Instance.BackToLastTrans(dur, () =>
            {
                FridgeItemCtrller[] itemCtrllers = animObj.GetComponentsInChildren<FridgeItemCtrller>();
                if (itemCtrllers != null)
                {
                    for (int i = 0; i < itemCtrllers.Length; i++)
                    {
                        itemCtrllers[i].OnLeaveMachine(machineName);
                        itemCtrllers[i].ModelScale("plate");
                    }
                }

                _eFingering = FingeringState.Nothing;
                CheckMachineAnim();

                ResetCookedObj(animObj);
                NeatenIngredParts(animObj);
            });
        }
        void NeatenIngredParts(GameObject obj)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                obj.transform.GetChild(i).position = obj.transform.position + new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(-1.5f, 1.5f));
                obj.transform.GetChild(i).localEulerAngles = new Vector3(0, 0, obj.transform.GetChild(i).localEulerAngles.z);
            }
        }
        void ResetCookedObj(GameObject obj)
        {
            obj.transform.SetParent(_trsPickOrigin);
            obj.transform.localPosition = _v3PickOriginPos + Vector3.up * 10;
            obj.transform.localEulerAngles = new Vector3(-90, obj.transform.localEulerAngles.y, 0);
            obj.transform.DOLocalMove(_v3PickOriginPos, 0.5f).OnComplete(() => { _bEnableCook = false; });
        }

        void CheckIngredPlate()
        {
            if (_nIngredsCount <= 0)
                StrStateStatus = "FreeCookedOver";
        }

        public void StopCookMachine()
        {
            if (_ctrllerCutboard != null && _ctrllerCutboard.enabled)
                _ctrllerCutboard.Stop();
            else if (_ctrllerPan != null && _ctrllerPan.enabled)
                _ctrllerPan.Stop();
            else if (_ctrllerPot != null && _ctrllerPot.enabled)
                _ctrllerPot.Stop();
            else if (_ctrllerOven != null && _ctrllerOven.enabled)
                _ctrllerOven.Stop();
            if (StrStateStatus == "IngredientDealt")
                StrStateStatus = null;
        }
    }
}

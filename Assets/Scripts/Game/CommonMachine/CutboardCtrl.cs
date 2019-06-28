using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    //切板
    public class CutboardCtrl : MachineCtrl
    {
        GameObject _objKnife;

        Vector3 _v3temp = new Vector3(0, 2f, 0);

        GameObject _objCutted;
        string _strOriginName;

        int _nCutCounter;
        LeanCutterFree _cutter;
        CutterCounter _cutterCounter;
        System.Action<bool> _callbackCut;

        Color _colorCutted;

        void Awake()
        {
            _objKnife = transform.FindChild("Mesh/Knife").gameObject;
        }

        public void SetKnife(bool state)
        {
            _objKnife.SetActive(state);
        }

        //各种料理工具应该可以共用一些方法
        public void RegisterObject(GameObject obj, System.Action<GameObject> callbackCutMax, System.Action<bool> callbackCut)
        {
            _callbackCut = callbackCut;
            OnMachineFinish = callbackCutMax;
            obj.transform.SetParent(transform);
            _cutterCounter = obj.GetComponent<CutterCounter>();
           
            if (_cutterCounter != null)
            {
                _objCutted = obj;
                _colorCutted = obj.GetComponentInChildren<FridgeItemCtrller>().cItemColor;
                _nCutCounter = obj.GetComponent<CutterCounter>().nCutCount;
                List<Transform> parts = _objCutted.transform.GetChildTrsList();
                for (int i = 0; i < parts.Count; i++)
                {
                    Vector3 originLocalPos = parts[i].localPosition;
                    parts[i].SetParent(transform);
                    parts[i].localPosition = new Vector3(originLocalPos.x, _v3temp.y * 3, originLocalPos.z);
                }
                _objCutted.SetLocalPos(_v3temp);
                gameObject.SetRigidBodiesKinematic(false);
                _cutter.enabled = true;
            }
            else
            {
                _strOriginName = obj.name;
                _colorCutted = obj.GetComponent<FridgeItemCtrller>().cItemColor;
                _objCutted = new GameObject(_strOriginName);

                _cutterCounter = _objCutted.AddComponent<CutterCounter>();
                _objCutted.transform.SetParent(transform);
                _objCutted.SetLocalPos(_v3temp);

                obj.layer = LayerMask.NameToLayer("Cuttable");
                _nCutCounter = 0;
                obj.SetLocalPos(_v3temp + Vector3.up * 3);
                obj.transform.DOLocalMove(_v3temp, 0.5f).OnComplete(()=>{
                    if (obj.GetComponent<Rigidbody>() != null)
                        obj.GetComponent<Rigidbody>().isKinematic = false;
                    _cutter.enabled = true;
                    GuideManager.Instance.SetGuideSingleDir(obj.transform.position - Vector3.forward * 5, obj.transform.position + Vector3.forward * 5);
                });
            }

            FormPartsInCircle(_objCutted.transform);
            _cutter = gameObject.AddMissingComponent<LeanCutterFree>();
            _cutter.OnCut = OnCutCallback;
            _cutter.SetParams(transform, "Cuttable", false);
        }

        void Update()
        {
        }

        void FinishCut()
        {
            _cutterCounter.nCutCount = _nCutCounter;
            List<Transform> parts = transform.GetChildTrsList();
            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i].name == "Part")
                {
                    if (parts[i].GetComponent<Renderer>() != null)
                        parts[i].SetParent(_objCutted.transform);
                    else if(parts[i].childCount == 0)
                        GameObject.Destroy(parts[i].gameObject);
                }
            }
            gameObject.SetRigidBodiesKinematic(true);

            parts.Clear();

            if (OnMachineFinish != null) OnMachineFinish(_objCutted);
            _nCutCounter = 0;
        }

        new void OnDisable()
        {
            base.OnDisable();
            _cutter.enabled = false;
        }


        void OnCutCallback(Vector3 point)
        {
            if (_colorCutted != null)
            {
                var eff = EffectCenter.Instance.SpawnEffect("Juice", new Vector3(point.x, 24.7f, point.z), new Vector3(90, Random.Range(0, 360), 0));
                eff.ResetMaxTimeUseful(1.5f);
                eff.transform.localScale = Vector3.zero;
                eff.transform.DOScale(new Vector3(Random.Range(5f, 10f), Random.Range(3f, 7f), 1), 0.2f);
                eff.GetComponent<MeshRenderer>().material.color = _colorCutted;
                eff.GetComponent<MeshRenderer>().material.DOKill();
                eff.GetComponent<MeshRenderer>().material.DOColor(new Color(_colorCutted.r, _colorCutted.g, _colorCutted.b, 0), 1f).SetDelay(0.4f);
            }

            _nCutCounter += 1;
            GuideManager.Instance.StopGuide();
            DoozyUI.UIManager.PlaySound("37切水果");

            if (_callbackCut != null)
                _callbackCut.Invoke(_nCutCounter >= _cutterCounter.nCutLimit);
            Debug.Log(_nCutCounter);
            if (_nCutCounter >= _cutterCounter.nCutLimit)
            {
                _cutter.enabled = false;
                FinishCut();
            }
        }

        public override void Stop()
        {
            _cutter.enabled = false;
            FinishCut();
        }
    }
}
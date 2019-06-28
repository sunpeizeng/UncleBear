using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public class FridgeItemCtrller : MonoBehaviour
    {
        public bool bOriginCuttable = true;
        public bool bModelChanged = false;
        public Color cItemColor;
        Dictionary<string, GameObject> _dictEnterModels;
        Dictionary<string, GameObject> _dictLeaveModels;

        public float fScaleInPan;
        public float fScaleInPot;
        public float fScaleInPlate;
        public float fScaleInBowl;



        public bool bMatCooked = false;

        Material _mat;

        public void SetupCtrller(bool cuttable, Dictionary<string, GameObject> enters, Dictionary<string, GameObject> leaves, Vector3 rgb, float[] scales)
        {
            bOriginCuttable = cuttable;
            _dictEnterModels = enters;
            _dictLeaveModels = leaves;

            fScaleInPan = scales[0];
            fScaleInPot = scales[1];
            fScaleInPlate = scales[2];
            fScaleInBowl = scales[3];

            if (rgb != Vector3.zero)
                cItemColor = new Color(rgb.x / 255f, rgb.y / 255f, rgb.z / 255f, 1);
        }

        //进入退出时更换模型
        public void OnEnterMachine(string keyword)
        {
            if (bModelChanged)
                return;
            ModelSwitch(keyword, _dictEnterModels);
        }
        public void OnLeaveMachine(string keyword)
        {
            if (bModelChanged)
                return;
            ModelSwitch(keyword, _dictLeaveModels);
        }

        void Awake()
        {
            gameObject.AddMissingComponent<SelfDestroy>();
            _mat = GetComponent<MeshRenderer>().material;
        }

        #region modelSwitch
        void ModelSwitch(string keyword, Dictionary<string, GameObject> dictInfo)
        {
            if (dictInfo == null)
                return;
            string key = "all";
            if (!dictInfo.ContainsKey(key))
                key = keyword;

            if (dictInfo.ContainsKey(key))
            {
                var uvMapper = dictInfo[key].GetComponent<ShatterToolkit.TargetUvMapper>();
                if (uvMapper != null)
                {
                    var copyMapper = gameObject.AddMissingComponent<ShatterToolkit.TargetUvMapper>();
                    copyMapper.targetSize = uvMapper.targetSize;
                    copyMapper.targetStart = uvMapper.targetStart;
                    copyMapper.square = uvMapper.square;
                }

                GetComponent<MeshFilter>().mesh = dictInfo[key].GetComponent<MeshFilter>().sharedMesh;
                GetComponent<MeshRenderer>().material = dictInfo[key].GetComponent<MeshRenderer>().sharedMaterial;
                _mat = GetComponent<MeshRenderer>().material;
                ResetCollider(keyword);
                bModelChanged = true;
            }
        }
        void ResetCollider(string keyword)
        {
            var col = GetComponent<BoxCollider>();
            if (col != null)
            {
                GameObject.DestroyImmediate(col);
                BoxCollider newCol = gameObject.AddComponent<BoxCollider>();

                if ((keyword == "pan" || keyword == "pot") && 
                    gameObject.name.Contains("eggWhole") && 
                    gameObject.GetComponent<MeshRenderer>() != null && 
                    gameObject.GetComponent<MeshRenderer>().material.name.Contains("Mat_EggPiece"))
                {
                    newCol.size = new Vector3(newCol.size.x, newCol.size.y, 0.5f);
                }
            }
        }
        #endregion

        public void ModelScale(string key)
        {
            switch (key)
            {
                case "pan":
                    transform.localScale = fScaleInPan * Vector3.one;
                    break;
                case "pot":
                    transform.localScale = fScaleInPot * Vector3.one;
                    break;
                case "plate":
                    transform.localScale = fScaleInPlate * Vector3.one;
                    break;
                case "bowl":
                    transform.localScale = fScaleInBowl * Vector3.one;
                    break;
            }
        }

        public float GetModelScale(string key)
        {
            switch (key)
            {
                case "pan":
                    return fScaleInPan;
                case "pot":
                    return fScaleInPot;
                case "plate":
                    return fScaleInPlate;
                case "bowl":
                    return fScaleInBowl;
            }
            return fScaleInPlate;
        }

        public void SetMatValue(float val)
        {
            if (bMatCooked)
                return;

            if (val >= 1)
                bMatCooked = true;

            if (_mat.HasProperty("_Slider_Val"))
            {
                _mat.SetFloat("_Slider_Val", val);
            }
        }
    }
}

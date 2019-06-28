using DoozyUI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UncleBear
{
    public class LevelBase
    {
        protected bool _bPausedAtUI;
        protected bool _bLevelEnded;
        protected Dictionary<string, GameObject> _dictLevelObjs = new Dictionary<string, GameObject>();
        public Dictionary<string, GameObject> LevelObjs
        { get { return _dictLevelObjs; } }

        public LevelBase()
        {
            CreateLevelFsm();
        }

        //关卡加载
        public virtual void LoadLevel()
        {
            _bPausedAtUI = _bLevelEnded = false;
        }

        public virtual void ResetLevel()
        {

        }

        protected void SetLevelCamFOV(bool fix)
        {
            float actualAspect = (float)Camera.main.pixelWidth / Camera.main.pixelHeight;
            if (fix)
                CameraManager.Instance.MainCamera.fieldOfView = 
                    actualAspect > 1.5f ||
                    LevelManager.Instance.CurLevelEnum != LevelEnum.IceCream ?
                    20 : 24;
            else
            {
                float designedVFOV = 45f;
                CanvasScaler canvasScaler2dUI = UIManager.GetUiContainer.GetComponent<CanvasScaler>();
                float desginedAspect = canvasScaler2dUI.referenceResolution.x / canvasScaler2dUI.referenceResolution.y;

                float newVFOVInRads = 2 * Mathf.Atan(desginedAspect / actualAspect * Mathf.Tan(designedVFOV * Mathf.Deg2Rad * 0.5f));
                Camera.main.fieldOfView = newVFOVInRads * Mathf.Rad2Deg;
            }
        }

        protected void GenLevelObjects(string path)
        {
            var objs = SerializationManager.LoadFromCSV<LevelObjectsData>(path);
            if (objs != null)
            {
                objs.ForEach(p =>
                {
                    var inited = GameObject.Instantiate(ItemManager.Instance.GetItem(p.ObjId).Prefab, Vector3.one * 500, Quaternion.identity);
                    inited.name = p.ObjName;
                    if (inited.GetComponent<ParticleSystem>() != null)
                        inited.SetActive(false);
                    _dictLevelObjs.Add(p.ObjId, inited);
                });

                UIPanelManager.Instance.HidePanel("UILoading");
            }
        }


        //关卡退出清理
        public virtual void CleanLevel()
        {
            _dictLevelObjs.Clear();
            _bLevelEnded = true;
        }

        //构造时创建关卡所有状态
        protected virtual void CreateLevelFsm()
        { }

        //用于检查关卡内状态切换
        public virtual void TickStateExecuting(float deltaTime)
        { }

        public virtual void OnContinueLevelPhase()
        { }
    }

    public class LevelObjectsData : ICSVDeserializable
    {
        public string ObjName;
        public string ObjId;

        public void CSVDeserialize(Dictionary<string, string[]> data, int index)
        {
            ObjName = data["Name"][index];
            ObjId = data["ID"][index];
        }
    }
}

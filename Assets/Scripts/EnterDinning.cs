using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    //用来记录游戏中的一些全局信息
    public class EnterDinning : MonoBehaviour
    {
        public Transform trsDishPoint;

        private static EnterDinning _instance;
        public static EnterDinning Instance
        {
            get { return _instance; }
        }

        //进场路径
        public DOTweenPath[] EnterPathes;
        public DOTweenPath ServeDishPath;
        public DOTweenPath ChefPath;

        public GameObject ObjMenu;
        public Camera CamMenu;

        private Vector3 _v3Originscale;
        private Dictionary<string, Vector3> _dictSeatOffsets = new Dictionary<string, Vector3>();
        public Dictionary<string,Vector3> OffsetsToSeat
        {
            get { return _dictSeatOffsets; }
        }

        void Awake()
        {
            _instance = this;
            _v3Originscale = ObjMenu.transform.localScale;
            _dictSeatOffsets.Add("Giraffe", new Vector3(0, -0.5f, -5.5f));
            _dictSeatOffsets.Add("Elephant", new Vector3(0, 3f, -5.5f));
            _dictSeatOffsets.Add("Rabbit", new Vector3(0, 0f, -5.5f));
        }

        void Start()
        {
            ObjMenu.transform.localScale = Vector3.zero;
            GameUtilities.SetCamFOVByScreen(CamMenu);
            ActiveMenuCam(false);
        }

        public void ShowMenuObj(bool state)
        {
            if (state)
            {
                ObjMenu.transform.localScale = Vector3.zero;
                ObjMenu.GetComponent<MenuUI>().ShowMenuAnim(true);
                ObjMenu.transform.DOScale(_v3Originscale, 0.5f);
            }
            else
            {
                ObjMenu.transform.DOScale(Vector3.zero, 0.5f);
            }
        }

        public void ActiveMenuCam(bool state)
        {
            if (CamMenu != null)
            {
                CamMenu.gameObject.SetActive(state);
            }
        }
    }
}

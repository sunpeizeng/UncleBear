using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    //与enterdining功能类似,记录一些物件信息,相比游戏逻辑,跟场景关联比较大
    public class EnterKitchen : MonoBehaviour
    {
        public enum ButtonStateEnum
        {
            Close,
            Cooking,
            Finish,
        }

        AudioSource _asTap;

        private static EnterKitchen _instance;
        public static EnterKitchen Instance { get { return _instance; } }

        public GameObject ObjTap;//水龙头
        public GameObject ObjWaterEff;//水龙头特效
        public GameObject ObjOvenDoor;//烤箱门
        public GameObject ObjOvenPlate;//烤盘
        public GameObject ObjOvenLight;//烤箱提示灯
        public GameObject ObjHearth;//灶台
        public GameObject ObjHearthLight;//灶台灯
        public Transform TrsTableCloth;
        public TextMesh tmOvenCounter;
        private ParticleSystem _psWater;

        private Material _matOvenLight;
        private Material _matOvenDoor;

        // Use this for initialization
        void Awake()
        {
            _instance = this;
            _psWater = ObjWaterEff.GetComponent<ParticleSystem>();
            _psWater.Stop();
            _matOvenLight = ObjOvenLight.GetComponent<MeshRenderer>().sharedMaterial;
            _matOvenDoor = ObjOvenDoor.transform.GetChild(0).GetComponent<MeshRenderer>().material;
            SetOvenButtonLight(EnterKitchen.ButtonStateEnum.Close);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OpenTap(bool state)
        {
            DoozyUI.UIManager.PlaySound("33开关水龙头", ObjTap.transform.position);
            if (state)
            {
                _psWater.Play();
                ObjTap.transform.DOKill();
                ObjTap.transform.DOLocalRotate(new Vector3(0, 0, -90), 0.5f);
                _asTap = DoozyUI.UIManager.PlaySound("34水龙头流水声", ObjTap.transform.position, true);
            }
            else
            {
                _psWater.Stop();
                ObjTap.transform.DOKill();
                ObjTap.transform.DOLocalRotate(Vector3.zero, 0.5f);
                AudioSourcePool.Instance.Free(_asTap);
            }
        }

        public void SetOvenButtonLight(ButtonStateEnum state)
        {
            switch (state)
            {
                case ButtonStateEnum.Close:
                    _matOvenDoor.DOKill();
                    _matOvenDoor.color = Color.white;
                    _matOvenLight.mainTextureOffset = new Vector2(0, 0.5f);
                    break;
                case ButtonStateEnum.Cooking:
                    _matOvenDoor.DOKill();
                    _matOvenDoor.DOColor(Color.red, 1f);
                    _matOvenLight.mainTextureOffset = new Vector2(0.5f, 0);
                    break;
                case ButtonStateEnum.Finish:
                    _matOvenDoor.DOKill();
                    _matOvenDoor.DOColor(Color.white, 1f);
                    _matOvenLight.mainTextureOffset = Vector2.zero;
                    break;

            }
        }


        public void ShowOvenTime(bool state)
        {
            tmOvenCounter.gameObject.SetActive(state);
        }
        public void SetOvenTime(float time)
        {
            time = Mathf.Clamp(time, 0, 5);
            int num = Mathf.CeilToInt(time);
            tmOvenCounter.text = "00:0" + num;
        }
    }
}

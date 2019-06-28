using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public class PastaStatePot : State<LevelPasta>
    {

        Vector3 _v3CamPos = new Vector3(-12.4f, 64, -65);
        Vector3 _v3CamAngle = new Vector3(45, 180, 0);
        Vector3 _v3PotPos = new Vector3(-12.4f, 24, -99);
        PotCtrl _potCtrl;

        public PastaStatePot(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("Pot");
            base.Enter(param);

            _owner.PastaPieces.ForEach(p => { GameObject.Destroy(p); });
            _owner.PastaPieces.Clear();

            GameObject basePasta = _owner.LevelObjs[Consts.ITEM_FARFALLE];
            for (int i = 0; i < 20; i++)
            {
                GameObject copyPasta = GameObject.Instantiate(basePasta);
                //copyPasta.GetComponentInChildren<Animation>().Play("anim_farfalle");
                copyPasta.name = basePasta.name;
                _owner.PastaPieces.Add(copyPasta);
            }


            _owner.LevelObjs[Consts.ITEM_POT].SetPos(_v3PotPos);
            _potCtrl = _owner.LevelObjs[Consts.ITEM_POT].AddMissingComponent<PotCtrl>();
          
            CameraManager.Instance.DoCamTween(_v3CamPos, _v3CamAngle, 1f, () =>
            {
                LevelManager.Instance.StartCoroutine(CollectPastaToPot());
            });
        }

        Material[] _mats;
        void SetrenderLerp(float val)
        {
            if (_mats == null)
                return;
            val = Mathf.Clamp01(val);
            for (int i = 0; i < _mats.Length; i++)
            {
                //TODO::想一个办法,尽可能还是用sharedmat减少new消耗
                if (_mats[i].HasProperty("_Slider_Val"))
                {
                    _mats[i].SetFloat("_Slider_Val", val);
                }
            }
        }

        public override string Execute(float deltaTime)
        {
            if (_potCtrl != null && _potCtrl.enabled)
                SetrenderLerp(_potCtrl.GetPotPerc());
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            _mats = null;
            if (_potCtrl != null)
                _potCtrl.Stop();
            base.Exit();
        }

        IEnumerator CollectPastaToPot()
        {
            GameObject pastaRoot = new GameObject("Pasta");
            for (int i = 0; i < _owner.PastaPieces.Count; i++)
            {
                _owner.PastaPieces[i].transform.position = pastaRoot.transform.position + new Vector3(Random.Range(-3f, 3f), Random.Range(8, 12), Random.Range(-3f, 3f));
                _owner.PastaPieces[i].transform.localEulerAngles = new Vector3(Random.Range(-30f, 30f), Random.Range(-180f, 180f), 0);
                _owner.PastaPieces[i].transform.SetParent(pastaRoot.transform);
                yield return null;
            }

            var renderers = pastaRoot.GetComponentsInChildren<Renderer>();
            _mats = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                _mats[i] = renderers[i].material;
            }

            _potCtrl.enabled = true;
            _potCtrl.RegisterObject(pastaRoot, OnPotOver, OnPastaCookedOk, true, false);
        }

        void OnPotOver(GameObject obj)
        {
            if (_potCtrl != null)
                _potCtrl.enabled = false;
        }

        void OnPastaCookedOk(bool isLimited)
        {
            DoozyUI.UIManager.PlaySound("8成功");
            StrStateStatus = "PastaBoiled";
        }
    }
}

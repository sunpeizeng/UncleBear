using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;

namespace UncleBear
{
    public class SushiStateScroll : State<LevelSushi>
    {
        Vector3 _v3ScrollPos = new Vector3(-8, 24.8f, -28.5f);
        float _fScrollRate;
        bool _bFinishRoll;
        bool _bScollReady;
        Transform _trsRice;
        Transform _trsNori;
        Transform _trsScrollMain;
        Animation _animBamboo;
        Animation _animNori;

        List<Transform> _trsStuffs = new List<Transform>();
        List<float> _stuffXs = new List<float>();



        public SushiStateScroll(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            CameraManager.Instance.DoCamTween(new Vector3(-8, 62.3f, 13.6f), new Vector3(50, 180, 0), 0.5f);
            base.Enter(param);

            _bScollReady = _bFinishRoll = false;
            _fScrollRate = 0;
            _animBamboo = _owner.LevelObjs[Consts.ITEM_BAMBOO].GetComponent<Animation>();
            _trsScrollMain = _owner.LevelObjs[Consts.ITEM_SUSHISCROLL].transform.GetChild(0);
            _trsScrollMain.localScale = Vector3.zero;

            _trsScrollMain.GetComponent<MeshRenderer>().sharedMaterial.mainTextureOffset = _owner.IsNoriOutside ? Vector2.zero : new Vector2(0, 0.5f);
            //_trsScrollMain.GetComponent<ShatterToolkit.TargetUvMapper>().targetStart = _owner.IsNoriOutside ? new Vector2(0.83f, 0.69f) : new Vector2(0.84f, 0.185f);

            for (int i = 0; i < _owner.LevelObjs[Consts.ITEM_BAMBOO].transform.childCount; i++)
            {
                var trs = _owner.LevelObjs[Consts.ITEM_BAMBOO].transform.GetChild(i);
                if (trs.name == "Rice")
                    _trsRice = trs;
                else if (trs.name == "Nori")
                {
                    _trsNori = trs;
                    _animNori = _trsNori.GetComponent<Animation>();
                }
                else if (trs.name.Contains("item_"))
                {
                    _trsStuffs.Add(trs);
                    _stuffXs.Add(trs.localPosition.x);
                }
            }
            _trsStuffs.ForEach(p => p.SetParent(_trsRice));

            var bambooPos = _owner.LevelObjs[Consts.ITEM_BAMBOO].transform.position;
            GuideManager.Instance.SetGuideSingleDir(bambooPos + Vector3.forward * 7, bambooPos - Vector3.forward * 3);
        }
        public override string Execute(float deltaTime)
        {
            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
            _trsStuffs.Clear();
            _stuffXs.Clear();
            _animNori = _animBamboo = null;
        }


        protected override void OnFingerSet(LeanFinger finger)
        {
            if (!_bFinishRoll)
            {
                _fScrollRate += finger.ScreenDelta.y * 0.005f;
                _fScrollRate = Mathf.Clamp01(_fScrollRate);

                if (_fScrollRate < 0.4f)
                {
                    var angleRate = _fScrollRate * 30;
                    if (_fScrollRate > 0.2f && _fScrollRate < 0.3f)
                        angleRate = _fScrollRate * 48;
                    else if (_fScrollRate > 0.3 && _fScrollRate < 0.4f)
                        angleRate = _fScrollRate * (_owner.IsNoriOutside ? 70 : 48);
                    _trsRice.localEulerAngles = new Vector3(angleRate * -1, _trsRice.localEulerAngles.y, _trsRice.localEulerAngles.z);

                    _animBamboo.SampleAnim("anim_bamboo", _fScrollRate);
                    _animNori.SampleAnim("anim_nori", _fScrollRate);
                }
                else
                {
                    DoozyUI.UIManager.PlaySound("21卷寿司", _v3ScrollPos);
                    GuideManager.Instance.StopGuide();
                    _bFinishRoll = true;
                    _animBamboo["anim_bamboo"].speed = 1;
                    _animNori["anim_nori"].speed = 1;

                    _owner.LevelObjs[Consts.ITEM_SUSHISCROLL].transform.position = _v3ScrollPos;
                    _trsScrollMain.localScale = new Vector3(1, 1, 0);

                    _trsNori.localScale = Vector3.zero;
                    _trsRice.DOScale(Vector3.zero, 0.3f);

                    for (int i = 0; i < _trsStuffs.Count; i++)
                    {
                        _trsStuffs[i].SetParent(_owner.LevelObjs[Consts.ITEM_SUSHISCROLL].transform);
                        _trsStuffs[i].localEulerAngles = new Vector3(Random.Range(-180, 0), 0, 0);// new Vector3(Random.Range(-180, 180), 0, 0);
                        _trsStuffs[i].DOLocalMove(new Vector3(_stuffXs[i], Random.Range(0.8f, 1.8f), Random.Range(-0.5f, 0.5f)), 0.2f);
                        //_trsStuffs[i].DOScale(new Vector3(1, 0.3f, 0.6f), 0.3f);
                    }

                    _trsScrollMain.DOScale(Vector3.one, 1).OnComplete(()=>
                    {
                        _animBamboo["anim_bamboo"].speed = -1;
                        _animBamboo.Play();

                        //让寿司滚到中间来
                        _owner.LevelObjs[Consts.ITEM_SUSHISCROLL].transform.DOMove(_v3ScrollPos + new Vector3(0, 0, 11), 0.5f).SetDelay(1f).OnComplete(() => { StrStateStatus = "ScrollOver"; });
                    });

                   
                }
            }
        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            if (!_bFinishRoll)
            {
                //如果需要恢复动画,在这里处理
            }
        }
    }
}

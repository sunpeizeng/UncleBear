using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class PastaStatePourOut : State<LevelPasta>
    {
        AudioSource _asPotWater;
        //AudioSource _asPourPasta;
        Vector3 _v3CoLanderPour = new Vector3(-83.9f, 36f, -25f);
        Vector3 _v3PlatePos = new Vector3(-83.9f, 23f, -15.5f);

        Vector3 _v3WaterEff = new Vector3(0, 8.8f, 5.6f);
        Vector3 _v3PourPos = new Vector3(-83.9f, 28, -45.5f);
        Vector3 _v3PotPos = new Vector3(-83.9f, 32, -57.1f);
        Vector3 _v3Colander = new Vector3(-83.9f, 17.5f, -41.9f);
        Vector3 _v3CamPos = new Vector3(7.7f, 63.5f, -44);
        Vector3 _v3CamPosPour = new Vector3(-4.7f, 57.1f, -15.5f);
        Vector3 _v3CamAngle = new Vector3(20, 270, 0);

        bool _bPourOutAll;
        bool _bHittingPot;
        int _nPastaIndex;
        float _fPourDelta = 0.2f;
        float _fPourCounter;

        ParticleCtrller _waterEff;

        public PastaStatePourOut(int stateEnum) : base(stateEnum)
        {

        }

        public override void Enter(object param)
        {
            //Debug.Log("PourOut");
            base.Enter(param);

            _bPourOutAll = _bHittingPot = false;
            _fPourCounter = 0;
            _nPastaIndex = 0;
            _waterEff = EffectCenter.Instance.SpawnEffect("SteamyWater", Vector3.zero, Vector3.zero);
            _waterEff.SetMaxTimeUseless();
            _waterEff.transform.SetParent(_owner.LevelObjs[Consts.ITEM_POT].transform);
            _waterEff.gameObject.SetLocalPos(_v3WaterEff);

            _owner.LevelObjs[Consts.ITEM_POT].transform.DOMove(_v3PotPos, 0.5f).OnComplete(()=> {
                GuideManager.Instance.SetGuideSingleDir(_v3PotPos + new Vector3(0, 7, 4), _v3PotPos + new Vector3(0, 0, 4));
            });
            _owner.LevelObjs[Consts.ITEM_COLANDER].SetPos(_v3Colander);

            _owner.LevelObjs[Consts.ITEM_PASTAPLATE].SetPos(_v3PlatePos);
            CameraManager.Instance.DoCamTween(_v3CamPos, _v3CamAngle, 1f);
            _asPotWater = DoozyUI.UIManager.PlaySound("60蝴蝶面-倒水.2", _waterEff.transform.position, true);
            _asPotWater.Pause();
        }

        public override string Execute(float deltaTime)
        {
            //根据角度控制特效和香肠出锅
            if (_owner.LevelObjs[Consts.ITEM_POT].transform.eulerAngles.x > 60)
            {
                _waterEff.gameObject.SetActive(true);
                //if (_asPourPasta == null || !_asPourPasta.isActiveAndEnabled)
                //    _asPourPasta = DoozyUI.UIManager.PlaySound("62蝴蝶面-面从锅中倒出", _v3PotPos);
                if (_asPotWater != null)
                    _asPotWater.UnPause();
                if (_nPastaIndex < _owner.PastaPieces.Count)
                {
                    _fPourCounter += deltaTime;
                    if (_fPourCounter > _fPourDelta)
                    {
                        _fPourCounter = 0;
                        _owner.PastaPieces[_nPastaIndex].SetPos(_v3PourPos);
                        _owner.PastaPieces[_nPastaIndex].GetComponent<Rigidbody>().isKinematic = false;
                        _owner.PastaPieces[_nPastaIndex].GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-300, 300), Random.Range(-200, 200), Random.Range(150, 400)));
                        _owner.PastaPieces[_nPastaIndex].transform.SetParent(_owner.LevelObjs[Consts.ITEM_COLANDER].transform);
                        _owner.PastaPieces[_nPastaIndex].transform.localEulerAngles = new Vector3(Random.Range(-60, 60), Random.Range(-30, 30), 0);
                        _nPastaIndex++;
                    }
                }
            }
            else
            {
                _waterEff.gameObject.SetActive(false);
                if (_asPotWater != null)
                    _asPotWater.Pause();
            }

            //控制特效角度
            if (_waterEff.gameObject.activeSelf)
                _waterEff.gameObject.transform.localEulerAngles = _owner.LevelObjs[Consts.ITEM_POT].transform.eulerAngles * -1;

            //如果已经全部倒出,再倒到盘子里
            if (!_bPourOutAll)
            {
                if (_nPastaIndex >= _owner.PastaPieces.Count)
                {
                    _nPastaIndex = -1;
                    _bHittingPot = false;
                    _owner.LevelObjs[Consts.ITEM_POT].transform.DORotate(Vector3.zero, 0.5f).OnComplete(() =>
                    {
                        _owner.LevelObjs[Consts.ITEM_POT].transform.DOMove(Vector3.one * 500, 1);
                        CameraManager.Instance.DoCamTween(_v3CamPosPour, 0.5f);
                        _owner.LevelObjs[Consts.ITEM_COLANDER].transform.DOMove(_v3CoLanderPour, 1f).OnComplete(() =>
                        {
                            _owner.LevelObjs[Consts.ITEM_COLANDER].transform.DORotate(new Vector3(120, 0), 0.5f).OnComplete(() =>
                            {
                                for (int i = 0; i < _owner.PastaPieces.Count; i++)
                                {
                                    _owner.PastaPieces[i].transform.SetParent(_owner.LevelObjs[Consts.ITEM_PASTAPLATE].transform);
                                }
                               
                                //_owner.PastaSausages.ForEach(p => {
                                //    //p.layer = LayerMask.NameToLayer("Default");
                                //    p.transform.SetParent(_owner.LevelObjs[Consts.ITEM_PLATE].transform);
                                //    p.transform.transform.localEulerAngles = new Vector3(0, p.transform.transform.localEulerAngles.y, 0);
                                //});
                                _bPourOutAll = true;
                                _fPourCounter = 0;
                                DoozyUI.UIManager.PlaySound("61蝴蝶面-面从碗中倒出", _owner.LevelObjs[Consts.ITEM_COLANDER].transform.position);
                            });
                        });
                    });


                }
            }

            if (_bPourOutAll && string.IsNullOrEmpty(StrStateStatus))
            {
                GameUtilities.LimitListPosition(_owner.PastaPieces, _owner.LevelObjs[Consts.ITEM_PASTAPLATE].transform.position, 3.5f);
                _fPourCounter += deltaTime;
                if (GameUtilities.IsAllRigidBodyQuiet(_owner.PastaPieces, 3) || _fPourCounter > 5)
                {
                    _owner.LevelObjs[Consts.ITEM_COLANDER].transform.DOMoveY(50, 0.5f).OnComplete(() =>
                    {
                        _owner.LevelObjs[Consts.ITEM_COLANDER].SetPos(Vector3.one * 500);
                    });
                    StrStateStatus = "PourOutAll";
                    DoozyUI.UIManager.PlaySound("8成功");
                }
            }
                

            return base.Execute(deltaTime);
        }

        public override void Exit()
        {
            //_asPourPasta = null;
            AudioSourcePool.Instance.Free(_asPotWater);
            base.Exit();
        }

        protected override void OnFingerDown(LeanFinger finger)
        {
            var hit = GameUtilities.GetRaycastHitInfo(CameraManager.Instance.MainCamera.ScreenPointToRay(finger.ScreenPosition));
            if (hit.collider != null && hit.collider.transform.IsChildOf(_owner.LevelObjs[Consts.ITEM_POT].transform))
            {
                GuideManager.Instance.StopGuide();

                _owner.LevelObjs[Consts.ITEM_POT].transform.DOKill();
                _bHittingPot = true;
            }
        }

        protected override void OnFingerSet(LeanFinger finger)
        {
            if (_bHittingPot)
            {
                if (finger.ScreenDelta.y < 0)
                {
                    var newX = Mathf.Clamp(_owner.LevelObjs[Consts.ITEM_POT].transform.eulerAngles.x - finger.ScreenDelta.y, 0, 80);
                    _owner.LevelObjs[Consts.ITEM_POT].transform.eulerAngles = new Vector3(newX, _owner.LevelObjs[Consts.ITEM_POT].transform.eulerAngles.y, _owner.LevelObjs[Consts.ITEM_POT].transform.eulerAngles.z);
                }
            }
        }

        protected override void OnFingerUp(LeanFinger finger)
        {
            _bHittingPot = false;
            _owner.LevelObjs[Consts.ITEM_POT].transform.DORotate(Vector3.zero, 0.5f);
            if(_nPastaIndex < _owner.PastaPieces.Count)
                GuideManager.Instance.SetGuideSingleDir(_v3PotPos + new Vector3(0, 7, 4), _v3PotPos + new Vector3(0, 0, 4));

            base.OnFingerUp(finger);
        }
    }
}

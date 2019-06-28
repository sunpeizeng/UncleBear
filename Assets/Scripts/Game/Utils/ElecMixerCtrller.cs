using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ElecMixerCtrller : MonoBehaviour
{
    public enum MixerState
    {
        Low,
        High,
        Closed,
    }
    public MixerState eState = MixerState.Closed;

    float _fOriginZ = 3.15f;
    float _fPressedZ = 2.75f;

    public GameObject objBTRed;
    public GameObject objBTBlue;
    public MeshRenderer meshRed;
    public MeshRenderer meshBlue;

    public Transform trsBody;

    public Transform trsBar;

    float _fRotSpeed;
    float _fRotAngle;

    public float fCurSpeed;

    bool _bMixing;
    bool _bHighSpeed;

    public void OnPressRed()
    {
        trsBody.DOShakePosition(2, 0.1f, 30, 90, false, false).SetLoops(-1);
        ChangeBTState(MixerState.High);
        _bMixing = _bHighSpeed = true;
    }
    public void OnPressBlue()
    {
        trsBody.DOShakePosition(2, 0.05f, 20, 90, false, false).SetLoops(-1);
        ChangeBTState(MixerState.Low);
        _bMixing = true;
        _bHighSpeed = false;
    }

    public void Close()
    {
        trsBody.DOKill();
        ChangeBTState(MixerState.Closed);
        _bHighSpeed = _bMixing = false;
    }

    void ChangeBTState(MixerState state)
    {
        objBTRed.transform.DOKill();
        objBTBlue.transform.DOKill();
        meshRed.material.color = Color.gray;
        meshBlue.material.color = Color.gray;
        eState = state;
        switch (state)
        {
            case MixerState.High:
                meshRed.material.color = Color.white;
                objBTRed.transform.DOLocalMoveZ(_fPressedZ, 0.2f);
                objBTBlue.transform.DOLocalMoveZ(_fOriginZ, 0.2f);
                break;
            case MixerState.Low:
                meshBlue.material.color = Color.white;
                objBTRed.transform.DOLocalMoveZ(_fOriginZ, 0.2f);
                objBTBlue.transform.DOLocalMoveZ(_fPressedZ, 0.2f);
                break;
            case MixerState.Closed:
                objBTRed.transform.DOLocalMoveZ(_fOriginZ, 0.2f);
                objBTBlue.transform.DOLocalMoveZ(_fOriginZ, 0.2f);
                break;
        }
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            OnPressBlue();
        if (Input.GetKeyDown(KeyCode.D))
            OnPressRed();
        if (Input.GetKeyDown(KeyCode.S))
            Close();

        fCurSpeed = 0;
        if (_bMixing)
        {
            fCurSpeed = _bHighSpeed ? 60 : 10;
            _fRotSpeed += fCurSpeed;
        }

        if (_fRotSpeed != _fRotAngle)
        {
            DOTween.To(() => _fRotAngle, p => _fRotAngle = p, _fRotSpeed, 1);
            trsBar.localEulerAngles = new Vector3(0, 0, -_fRotAngle);
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DoozyUI;
using UncleBear;
using DG.Tweening;

public class UIPanelGuide : UIPanel
{
    Camera _mainCam;

    public Image imgHand;
    public Image imgDoubleArrow;
    public Image imgAround;
    public Image imgCross;
    public Image imgSingleArrow;
    public Image imgDot;

    float _fMatchRatio;
    float _fResolutionFixX;
    float _fResolutionFixY;//383.4   251.5

    Vector2 _v2HidePos;
    Animator _animHand;
    Animator _animRotate;

    //两点拖拽引导
    public void ShowDoubleDirDrag(Vector3 worldPos1, Vector3 worldPos2, float duration = 1f)
    {
        var screenPos1 = _mainCam.WorldToScreenPoint(worldPos1);
        var screenPos2 = _mainCam.WorldToScreenPoint(worldPos2);

        var disVec = screenPos2 - screenPos1;
        //拉伸箭头
        var width = disVec.magnitude / imgDoubleArrow.rectTransform.localScale.x * _fResolutionFixX - 10;
        imgDoubleArrow.rectTransform.sizeDelta = new Vector2(width, imgDoubleArrow.rectTransform.sizeDelta.y);
        imgDoubleArrow.rectTransform.right = disVec.normalized;
        //修正坐标,左下对齐
        var fixPos = FixUIPosByCanvasMatch((screenPos1 + screenPos2) / 2);
        imgDoubleArrow.rectTransform.anchoredPosition = fixPos;
        imgHand.rectTransform.anchoredPosition = fixPos - disVec.normalized * width / 4;// FixUIPosByCanvasMatch(screenPos1);
        imgHand.rectTransform.DOAnchorPos(fixPos + disVec.normalized * width / 4, duration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
    }

    //点击引导
    public void ShowClick(Vector3 worldPos, float speed = 1f)
    {
        imgHand.rectTransform.anchoredPosition = FixUIPosByCanvasMatch(_mainCam.WorldToScreenPoint(worldPos));
        _animHand.SetFloat("Speed", speed);
        _animHand.Play("anim_guideClick", 0, 0);
    }
    //画圈引导
    public void ShowRotateAround(Vector3 worldPos, float speed = 1f)
    {
        imgAround.rectTransform.anchoredPosition = FixUIPosByCanvasMatch(_mainCam.WorldToScreenPoint(worldPos));
        _animRotate.SetFloat("Speed", speed);
        _animRotate.Play("anim_guideRotate", 0, 0);
    }
    //单向引导
    public void ShowSingeDirDrag(Vector3 srcPos, Vector3 desPos, bool showDir = true, bool showHand = true, float duration = 1f)
    {
        var screenPos1 = _mainCam.WorldToScreenPoint(srcPos);
        var screenPos2 = _mainCam.WorldToScreenPoint(desPos);

        if (showDir)
        {
            var disVec = screenPos2 - screenPos1;
            //拉伸箭头
            var width = disVec.magnitude / imgDoubleArrow.rectTransform.localScale.x * _fResolutionFixX - 30;
            imgSingleArrow.rectTransform.sizeDelta = new Vector2(width, imgDoubleArrow.rectTransform.sizeDelta.y);
            imgSingleArrow.rectTransform.right = disVec.normalized * -1;
            //修正坐标,左下对齐
            var fixPos = FixUIPosByCanvasMatch((screenPos1 + screenPos2) / 2);
            imgSingleArrow.rectTransform.anchoredPosition = fixPos;
        }

        if (showHand)
        {
            imgHand.rectTransform.anchoredPosition = FixUIPosByCanvasMatch(screenPos1);// FixUIPosByCanvasMatch(screenPos1);
            imgHand.rectTransform.DOAnchorPos(FixUIPosByCanvasMatch(screenPos2), duration).SetLoops(-1, LoopType.Restart).SetEase(Ease.OutSine);
        }
    }
    public void ShowFreeDir(Vector3 worldPos)
    {
        imgCross.rectTransform.anchoredPosition = FixUIPosByCanvasMatch(_mainCam.WorldToScreenPoint(worldPos));
        imgCross.rectTransform.DOScale(Vector3.one * 1.1f, 1).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
    }

    //全部隐藏
    public void HideGuide()
    {
        imgCross.rectTransform.DOKill();
        imgHand.rectTransform.DOKill();
        imgDoubleArrow.rectTransform.anchoredPosition = _v2HidePos;
        imgAround.rectTransform.anchoredPosition = _v2HidePos;
        imgCross.rectTransform.anchoredPosition = _v2HidePos;
        imgSingleArrow.rectTransform.anchoredPosition = _v2HidePos;
        imgDot.rectTransform.anchoredPosition = _v2HidePos;
        imgHand.rectTransform.anchoredPosition = _v2HidePos;
        _animHand.Play("Empty");
        _animRotate.Play("Empty");
    }


    protected override void Awake()
    {
        base.Awake();
        _v2HidePos = Vector2.one * 50000;

        _mainCam = CameraManager.Instance.MainCamera;
        UnityEngine.UI.CanvasScaler canvasScaler2dUI = DoozyUI.UIManager.GetUiContainer.GetComponent<UnityEngine.UI.CanvasScaler>();
        _fMatchRatio = canvasScaler2dUI.matchWidthOrHeight;
        _fResolutionFixX = canvasScaler2dUI.referenceResolution.x / (float)_mainCam.pixelWidth;
        _fResolutionFixY = canvasScaler2dUI.referenceResolution.y / (float)_mainCam.pixelHeight;

        _animHand = imgHand.GetComponent<Animator>() ;
        _animRotate = imgAround.GetComponent<Animator>();
    }


    //GameObject _obj1;
    //GameObject _obj2;
    //void Start()
    //{
    //    _obj1 = GameObject.Find("Cube");
    //    _obj2 = GameObject.Find("Cube1");
    //    //ShowDoubleArrowDrag(obj1.transform.position, obj2.transform.position, 0.5f);
    //}

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Alpha1))
    //    {
    //        HideGuide();
    //        ShowClick(_obj1.transform.position);
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha2))
    //    {
    //        HideGuide();
    //        ShowDoubleDirDrag(_obj1.transform.position, _obj2.transform.position, 0.5f);
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha3))
    //    {
    //        HideGuide();
    //        ShowRotateAround(_obj1.transform.position);
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha4))
    //    {
    //        HideGuide();
    //        ShowSingeDirDrag(_obj1.transform.position, _obj2.transform.position, false);
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha5))
    //    {
    //        HideGuide();
    //        ShowFreeDir(_obj1.transform.position);
    //    }
    //    if (Input.GetKeyDown(KeyCode.A))
    //        HideGuide();
       
    //}

    Vector3 FixUIPosByCanvasMatch(Vector3 pos)
    {
        return new Vector3(pos.x * _fResolutionFixX * (1 - _fMatchRatio) + pos.x * _fResolutionFixY * _fMatchRatio, pos.y * _fResolutionFixX * (1 - _fMatchRatio) + pos.y * _fResolutionFixY * _fMatchRatio, pos.z);
    }
}

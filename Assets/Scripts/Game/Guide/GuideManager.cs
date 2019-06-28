using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DoozyUI;
using Lean.Touch;

namespace UncleBear
{
    public class GuideManager : DoozyUI.Singleton<GuideManager>
    {
        UIPanelGuide _panelGuide;

        //指引类型
        public enum GuideTypeEnum
        {
            None,
            ClickPoint,
            DragDoubleDir,
            SwipeSingleDir,
            RotateAround,
            DragFree,
        }

        //无操作提示时间
        float _fGuideTimer;
        float _fGuideTime = 2f;

        Vector3 _v3SrcPos;
        Vector3 _v3DesPos;
        float _fGuideDuration;
        float _fGuideAnimSpeed;
        bool _bShowPath;
        bool _bShowHand;

        bool _bScreenTouched;
        GuideTypeEnum _eCurGuide;
        //外部指定要显示的教程类型
        public GuideTypeEnum GuideType
        {
            set {
                _eCurGuide = value;
            }
            get {
                return _eCurGuide;
            }
        }

        void Awake()
        {
            _eCurGuide = GuideTypeEnum.None;
            _panelGuide = UIPanelManager.Instance.ShowPanel("UIPanelGuide") as UIPanelGuide;
            _bScreenTouched = false;
        }

        void OnEnable()
        {
            LeanTouch.OnFingerDown += OnFingerDown;
            LeanTouch.OnFingerUp += OnFingerUp;
        }

        //关游戏和隐藏都取消事件
        void OnDisable()
        {
            LeanTouch.OnFingerDown -= OnFingerDown;
            LeanTouch.OnFingerUp -= OnFingerUp;
        }

        void Update()
        {
            TickGuideTime(Time.deltaTime);
        }

        void TickGuideTime(float deltaTime)
        {
            if (_eCurGuide == GuideTypeEnum.None || _bScreenTouched)
                return;

            if (_fGuideTimer >= 0)
            {
                _fGuideTimer -= deltaTime;

                if (_fGuideTimer < 0)
                {
                    ShowGuide();
                }
            }
        }

        void ShowGuide()
        {
            _panelGuide.HideGuide();
            switch (_eCurGuide)
            {
                case GuideTypeEnum.ClickPoint:
                    _panelGuide.ShowClick(_v3SrcPos, _fGuideAnimSpeed);
                    break;
                case GuideTypeEnum.RotateAround:
                    _panelGuide.ShowRotateAround(_v3SrcPos, _fGuideAnimSpeed);
                    break;
                case GuideTypeEnum.DragDoubleDir:
                    _panelGuide.ShowDoubleDirDrag(_v3SrcPos, _v3DesPos, _fGuideDuration);
                    break;
                case GuideTypeEnum.SwipeSingleDir:
                    _panelGuide.ShowSingeDirDrag(_v3SrcPos, _v3DesPos, _bShowPath, _bShowHand, _fGuideDuration);
                    break;
                case GuideTypeEnum.DragFree:
                    _panelGuide.ShowFreeDir(_v3SrcPos);
                    break;
            }
        }

        public void ResetGuideTimer(bool resetTime = true)
        {
            _panelGuide.HideGuide();
            if (resetTime)
                _fGuideTimer = _fGuideTime;
        }

        public void StopGuide()
        {
            if (_eCurGuide != GuideTypeEnum.None)
            {
                _eCurGuide = GuideTypeEnum.None;
                _panelGuide.HideGuide();
            }
        }

        public void SetGuideClick(Vector3 point, float speed = 1)
        {
            ResetGuideTimer();
            _eCurGuide = GuideTypeEnum.ClickPoint;
            _v3SrcPos = point;
            _fGuideAnimSpeed = speed;
        }

        public void SetGuideRotate(Vector3 point, float speed = 1)
        {
            ResetGuideTimer();
            _eCurGuide = GuideTypeEnum.RotateAround;
            _v3SrcPos = point;
            _fGuideAnimSpeed = speed;
        }

        public void SetGuideFree(Vector3 point)
        {
            ResetGuideTimer();
            _eCurGuide = GuideTypeEnum.DragFree;
            _v3SrcPos = point;
        }

        public void SetGuideDoubleDir(Vector3 point1, Vector3 point2, float duration = 1)
        {
            ResetGuideTimer();
            _eCurGuide = GuideTypeEnum.DragDoubleDir;
            _v3SrcPos = point1;
            _v3DesPos = point2;
            _fGuideDuration = duration;
        }

        public void SetGuideSingleDir(Vector3 point1, Vector3 point2, bool showPath = true, bool showHand = true, float duration = 1)
        {
            ResetGuideTimer();
            _eCurGuide = GuideTypeEnum.SwipeSingleDir;
            _v3SrcPos = point1;
            _v3DesPos = point2;
            _bShowPath = showPath;
            _bShowHand = showHand;
            _fGuideDuration = duration;
        }

        void OnFingerDown(LeanFinger finger)
        {
            _bScreenTouched = true;
            ResetGuideTimer();
        }
        void OnFingerUp(LeanFinger finger)
        {
            _bScreenTouched = false;
        }
    }
}

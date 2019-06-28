using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using DoozyUI;

[DisallowMultipleComponent]
public class ScaleOnTouch : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    public Vector3 mScaleChange = new Vector3(-0.2f, -0.2f, 1f);
    public float mDuration = 0.3f;
    public bool mButtonTransitionOff = true;

    private Vector3 _mScaleOrigin;
    private bool _mPressingDown = false;

    public bool PressingDown
    {
        get
        {
            return _mPressingDown;
        }
    }

    void Awake()
    {
        _mScaleOrigin = transform.localScale;
        if (mButtonTransitionOff)
        {
            Button button = GetComponent<Button>();
            if (button != null)
                button.transition = Selectable.Transition.None;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _mPressingDown = true;

        transform.DOScale(mScaleChange, mDuration)
            .SetRelative(true)
            .SetAutoKill(true)
            .OnStart(() =>
            {
                UIButton uiBtn = GetComponent<UIButton>();
                if (uiBtn != null && uiBtn.AreNormalAnimationsEnabled)
                    uiBtn.StopNormalStateAnimations();

                UIElement element = GetComponent<UIElement>();
                if (element != null && element.AreLoopAnimationsEnabled)
                    element.StopLoopAnimations();
            });
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _mPressingDown = false;
        transform.DOScale(_mScaleOrigin, mDuration)
            .SetAutoKill(true)
            .OnComplete(() =>
            {
                UIButton uiBtn = GetComponent<UIButton>();
                if (uiBtn != null && uiBtn.AreNormalAnimationsEnabled)
                    uiBtn.StartNormalStateAnimations();

                UIElement element = GetComponent<UIElement>();
                if (element != null && element.AreLoopAnimationsEnabled)
                    element.StartLoopAnimations();

            });
    }
}

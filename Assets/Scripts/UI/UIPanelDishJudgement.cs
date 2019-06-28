using UnityEngine.UI;
using UnityEngine;
using DoozyUI;
using UncleBear;
using DG.Tweening;

public class UIPanelDishJudgement : UIPanel {

    public Image[] imgStars;

    //public void SetStarCount(UncleBear.LevelEnum levelType, int count) {
    //    if (imgStars == null)
    //        return;
    //    for (int i = 0; i < imgStars.Length; i++)
    //    {
    //        imgStars[i].sprite = i < count ? AtlasManager.Instance.GetSprite("UIAtlas/star") : AtlasManager.Instance.GetSprite("UIAtlas/star_grey");
    //    }

    //    var item = GameData.FoodItemList.Find(p => p.ID.Contains(levelType.ToString().ToLower()));
    //    if (item != null)
    //        item.Stars = count;
    //}

    System.Action _showedCallback;
    System.Action _overCallback;


    protected override void OnPanelHideCompleted()
    {
        base.OnPanelHideCompleted();

        if (imgStars == null)
            return;

        for (int i = 0; i < imgStars.Length; i++)
        {
            var fillStar = imgStars[i].transform.FindChild("Fill").GetComponent<RectTransform>();
            fillStar.DOKill();
            fillStar.gameObject.SetActive(false);
            fillStar.transform.parent.DOKill();
            fillStar.transform.localEulerAngles = Vector3.zero;
        }
    }


    int _nStarCount;
    int _nStarCounter;
    bool _bShowed;
    float _fWaitTimer;
    public void ShowDishStar(UncleBear.LevelEnum levelType, int count, System.Action showCB, System.Action overCB)
    {
        if (imgStars == null)
            return;
        _showedCallback = showCB;
        _overCallback = overCB;
        _nStarCounter = 0;
        _nStarCount = count;
        _bShowed = false;
        _fWaitTimer = 1.5f;

	    var item = GameData.FoodItemList.Find(p => p.ID.Contains(levelType.ToString().ToLower()));
	    if (item != null)
	        item.Stars = count;

        for (int i = 0; i < imgStars.Length; i++)
        {
            var fillStar = imgStars[i].transform.FindChild("Fill").GetComponent<RectTransform>();
            fillStar.gameObject.SetActive(false);
            if (i < count)
            {
                fillStar.localScale = Vector3.one * 2f;
                fillStar.DOScale(Vector3.one, 0.5f).SetDelay(i * 1f).OnStart(()=> {
                    fillStar.gameObject.SetActive(true);
                    UIManager.PlaySound("26获得星星");
                }).OnComplete(() =>
                {
                    fillStar.transform.parent.DOLocalRotate(new Vector3(0, 0, 15), 0.4f).OnComplete(() =>
                    {
                        _nStarCounter += 1;
                        fillStar.transform.parent.DOLocalRotate(new Vector3(0, 0, -15), 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
                    });
                });
            }
        }
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (_nStarCounter >= _nStarCount)
        {
            if (!_bShowed)
            {
                _bShowed = true;
                if (_showedCallback != null)
                    _showedCallback.Invoke();
            }

            if (_fWaitTimer > 0)
            {
                _fWaitTimer -= Time.deltaTime;
                if (_fWaitTimer <= 0)
                {
                    if (_overCallback != null)
                        _overCallback.Invoke();
                }
            }
        }
    }
}

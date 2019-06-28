using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public class DishManager : Singleton<DishManager>
    {
        public DishManager()
        {
            _lstDishIngreds = new List<string>();
            _lstFavorIngreds = new List<string>();

        }

        #region Dish
        private GameObject _objDishCover;
        public GameObject ObjDishCover { get { return _objDishCover; } }
        Vector3 _v3OriginScale;


        public bool AddCoverToDish()
        {
            if (_objFinishedDish == null)
                return false;
            if (_objDishCover == null)
                _objDishCover = GameObject.Instantiate(ItemManager.Instance.GetItem(Consts.ITEM_DISHCOVER).Prefab);

            _v3OriginScale = _objFinishedDish.transform.localScale;
            _objFinishedDish.transform.localScale = _v3OriginScale * 0.1f;

            switch (_dishType)
            {
                case LevelEnum.Pizza:
                case LevelEnum.Burger:
                case LevelEnum.Cupcake:
                case LevelEnum.Sushi:
                case LevelEnum.Salad:
                case LevelEnum.Farfalle:
                case LevelEnum.IceCream:
                    {
                        _objDishCover.transform.SetParent(_objFinishedDish.transform);
                        _objDishCover.transform.localEulerAngles = new Vector3(-90, 0, 0);
                        _objDishCover.transform.localPosition = new Vector3(0, 1, 0);
                        //_objDishCover.transform.localScale = Vector3.one / 0.1f;
                        return true;
                    }
                default:
                    return false;

            }
        }

        public void BackDishToOriginScale()
        {
            if (_objFinishedDish == null) return;
            switch (_dishType)
            {
                case LevelEnum.IceCream:
                    _v3OriginScale = Vector3.one * 1.4f;
                    break;
            }
            _objFinishedDish.transform.DOScale(_v3OriginScale, 0.3f);
        }


        LevelEnum _dishType;
        public LevelEnum DishType { get { return _dishType; } }

        //做好的菜
        private GameObject _objFinishedDish;
        public GameObject ObjFinishedDish
        {
            set
            {
                if (_objFinishedDish != null)
                    GameObject.Destroy(_objFinishedDish);
                _objFinishedDish = value;
                if (_objFinishedDish != null)
                {
                    _dishType = LevelManager.Instance.CurLevelEnum;
                    GameUtilities.ShowFinishEff();
                    _objFinishedDish.AddMissingComponent<DontDestroyOnLoad>();
                    //LevelManager.Instance.StartCoroutine(DishToDining());
                    var panel = UIPanelManager.Instance.GetPanel("UIPanelDishLevel");
                    if (panel != null)
                        panel.ShowSubElements("UIReturnButton");
                }
            }
            get { return _objFinishedDish; }
        }

        public IEnumerator DishToDining()
        {
            yield return new WaitForSeconds(2f);
            LevelManager.Instance.ChangeLevel(LevelEnum.Main);
        }

        //一道菜实际加入的材料
        private List<string> _lstDishIngreds;
        public List<string> IngredsInDish
        {
            set { _lstDishIngreds = value; }
            get { return _lstDishIngreds; }
        }

        //一道菜被喜欢的材料
        private List<string> _lstFavorIngreds;
        public List<string> FavorIngredsInDish
        {
            get { return _lstFavorIngreds; }
        }

        public void PickRandomFavorItem(List<string> ingreds)
        {
            _lstFavorIngreds.Clear();
            int favorCount = 3;//随3个这次喜欢的材料
            if (ingreds.Count < favorCount)
                favorCount = ingreds.Count;
            while (favorCount > 0)
            {
                int pickId = Random.Range(0, ingreds.Count);
                _lstFavorIngreds.Add(ingreds[pickId]);
                Debug.Log(ingreds[pickId]);
                ingreds.RemoveAt(pickId);
                favorCount -= 1;
            }
        }



        //用于摇头点头
        public void JudgeIngredInFavor(string id)
        {
            if (_lstFavorIngreds != null && _lstFavorIngreds.Contains(id))
                CurCustomer.ShowLike();
            else
                CurCustomer.ShowDislike();
        }
        //用于评分
        public int GetDishPoint()
        {
            int score = 0;
            var favors = new List<string>();
            favors.AddRange(_lstFavorIngreds);
            _lstDishIngreds.ForEach(p =>
            {
                if (favors.Contains(p))
                {
                    favors.Remove(p);
                    score += 1;
                }
            });

            return score;
        }

        private GameObject _coins;
        public GameObject ObjCoins { get { return _coins; } }

        public void ShowMoney(int points)
        {
            _coins = GameObject.Instantiate(ItemManager.Instance.GetItem(Consts.ITEM_COINS).Prefab, 
                EnterDinning.Instance.trsDishPoint.position + new Vector3(7, 0, -10), Quaternion.Euler(30, 0, 0)) as GameObject;
            for (int i = 0; i < _coins.transform.childCount; i++)
            {
                _coins.transform.GetChild(i).gameObject.SetActive(i < points);
            }
            _coins.transform.localScale = Vector3.zero;
            _coins.transform.DOScale(Vector3.one * 1.2f, 1f).SetDelay(0.3f).OnComplete(() => {
                GameData.Coins += points * 10;
                UIPanelManager.Instance.GetPanel("UIGameHUD").Repaint();
                //coins.transform.DOMove(coins.transform.position + new Vector3(-100, 100, 100), 2).OnComplete(()=> {
                 
                //});
            });
        }

        public void ClearDish()
        {
            ObjFinishedDish = null;
            if (_objDishCover != null)
                GameObject.Destroy(_objDishCover);
            if (_coins != null)
            {
                _coins.transform.DOKill();
                GameObject.Destroy(_coins, 0.1f);
            }
        }

        #endregion

        #region customer
        private CharaCustomer _curCustomer;
        public CharaCustomer CurCustomer
        {
            get { return _curCustomer; }
            set { _curCustomer = value; }
        }

        public Vector3 GetCustomerMouthPoint(CharaCustomer customer)
        {
            if (customer != null)
            {
                var mouthPos = customer.GetCharaModel().trsMouth.position;
                switch (customer.name)
                {
                    case "Elephant":
                        mouthPos += new Vector3(0, 2.5f, 5.5f);
                        break;
                    case "Rabbit":
                        mouthPos += new Vector3(0, 1, 4.8f);
                        break;
                }
                return mouthPos;
            }
            return Vector3.zero;
        }

        public void FixRenderCamHeightByCustomer()
        {
            if (_curCustomer != null)
            {
                switch (_curCustomer.name)
                {
                    case "Giraffe":
                        CameraManager.Instance.RenderCamera.transform.position = new Vector3(CameraManager.Instance.RenderCamera.transform.position.x, 550, CameraManager.Instance.RenderCamera.transform.position.z);
                        break;
                    case "Elephant":
                        CameraManager.Instance.RenderCamera.transform.position = new Vector3(CameraManager.Instance.RenderCamera.transform.position.x, 545, CameraManager.Instance.RenderCamera.transform.position.z);
                        break;
                    case "Rabbit":
                        CameraManager.Instance.RenderCamera.transform.position = new Vector3(CameraManager.Instance.RenderCamera.transform.position.x, 550, CameraManager.Instance.RenderCamera.transform.position.z);
                        break;
                }
            }
        }
        #endregion
    }
}

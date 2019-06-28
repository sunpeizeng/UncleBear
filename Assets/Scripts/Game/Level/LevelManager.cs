using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UncleBear
{
    public enum LevelEnum
    {
        Main,
        Pizza,
        Cupcake,
        Sushi,
        Salad,
        Burger,
        Farfalle,
        IceCream,
    }

    //用于关卡和场景的管理
    public class LevelManager : DoozyUI.Singleton<LevelManager>
    {
        private int _nCurLevelId;
        public LevelEnum CurLevelEnum
        {
            get { return (LevelEnum)_nCurLevelId; }
        }
        private int _nLastLevelId;
        public LevelEnum LastLevelEnum
        {
            get { return (LevelEnum)_nLastLevelId; }
        }


        private LevelBase _curLevel;
        public LevelBase CurLevel
        {
            get { return _curLevel; }
        }

        private bool _bGameStarted;
        public bool IsInGame { get { return _bGameStarted; } }


        private Dictionary<int, LevelBase> _dicLevels = new Dictionary<int, LevelBase>();

        void Awake()
        {
            DishManager.CreateInstance();
            //默认的重力加速度不太适用
            Physics.gravity = new Vector3(0, -30, 0);
            _bGameStarted = false;
        }

        void Update()
        {
            float deltaTime = Time.deltaTime;

            TickLevel(deltaTime);
        }

        //切换场景方法供外部调用
        public void ChangeLevel(LevelEnum levelType)
        {
            string sceneName = "";
            switch (levelType)
            {
                case LevelEnum.Pizza:
                case LevelEnum.Cupcake:
                case LevelEnum.Sushi:
                case LevelEnum.Salad:
                case LevelEnum.Burger:
                case LevelEnum.Farfalle:
                case LevelEnum.IceCream:
                    {
                        sceneName = "Game";
                        StartCoroutine(LoadLevelScene(levelType, sceneName, true));
                        
                    }
                    break;
                case LevelEnum.Main:
                    {
                        sceneName = "Main";
                        if (_bGameStarted)
                        {
                            StartCoroutine(UnloadAdditiveLevelScene(levelType, "Game"));
                        }
                        else
                            StartCoroutine(LoadLevelScene(levelType, sceneName));
                    }
                    break;
            }
        }

        public void ContinueLevel()
        {
            if (_curLevel != null)
                _curLevel.OnContinueLevelPhase();
        }

        //加载场景
        private IEnumerator LoadLevelScene(LevelEnum levelType, string name, bool additive = false)
        {
            CleanCurrentLevel();
            int loadingProgress = 0;
            //SceneManager.LoadSceneAsync("Loading");
            AsyncOperation oprAsync = additive ? SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive) : SceneManager.LoadSceneAsync(name);
            oprAsync.allowSceneActivation = false;

            while (!oprAsync.isDone)//done这个变量在allowSceneActivation为true以后自然会设置
            {
                int progressLimit = 0;
                if (oprAsync.progress < 0.85f)//unity的机制是卡在0.9,防止浮点数不准确,写0.85
                    progressLimit = (int)(oprAsync.progress * 100);
                else
                    progressLimit = 100;
                if (loadingProgress <= progressLimit)
                {
                    loadingProgress += 2;//这个阶段速度可以控制
                }
                else if (progressLimit == 100)
                {
                    oprAsync.allowSceneActivation = true;
                }
                yield return new WaitForEndOfFrame();
            }
            //!!很关键
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(name));
            LoadLevelLogic(levelType);
            oprAsync = null;
        }
        private IEnumerator UnloadAdditiveLevelScene(LevelEnum newxtLevelType, string lastName)
        {
            CleanCurrentLevel();
            var oprAsync = SceneManager.UnloadSceneAsync(lastName);
            while (!oprAsync.isDone)
            {
                yield return new WaitForEndOfFrame();
            }
            LoadLevelLogic(newxtLevelType);
            _bGameStarted = false;
        }

        //加载关卡逻辑控制
        private void LoadLevelLogic(LevelEnum levelType)
        {
            _nLastLevelId = _nCurLevelId;
            _nCurLevelId = (int)levelType;
            if(levelType!= LevelEnum.Main)
                _bGameStarted = true;

            if (_dicLevels.ContainsKey(_nCurLevelId))
            {
                _curLevel = _dicLevels[_nCurLevelId];
                _curLevel.LoadLevel();
            }
            else
            {
                bool haveLevelCtrl = true;
                switch (levelType)
                {
                    case LevelEnum.Pizza:
                        {
                            _dicLevels.Add(_nCurLevelId, new LevelPizza());
                        }
                        break;
                    case LevelEnum.Cupcake:
                        {
                            _dicLevels.Add(_nCurLevelId, new LevelCupCake());
                        }
                        break;
                    case LevelEnum.Sushi:
                        {
                            _dicLevels.Add(_nCurLevelId, new LevelSushi());
                        }
                        break;
                    case LevelEnum.Salad:
                        {
                            _dicLevels.Add(_nCurLevelId, new LevelSalad());
                        }
                        break;
                    case LevelEnum.Burger:
                        {
                            _dicLevels.Add(_nCurLevelId, new LevelBurger());
                        }
                        break;
                    case LevelEnum.Farfalle:
                        {
                            _dicLevels.Add(_nCurLevelId, new LevelPasta());
                        }
                        break;
                    case LevelEnum.IceCream:
                        {
                            _dicLevels.Add(_nCurLevelId, new LevelIceCream());
                        }
                        break;
                    case LevelEnum.Main:
                        {
                            _dicLevels.Add(_nCurLevelId, new LevelMain());
                        }
                        break;
                    default:
                        {
                            _curLevel = null;
                            haveLevelCtrl = false;
                        }
                        break;
                }
                if (haveLevelCtrl)
                {
                    _curLevel = _dicLevels[_nCurLevelId];
                    _curLevel.LoadLevel();
                }
            }
         
        }

        private void CleanCurrentLevel()
        {
            if (_curLevel != null)
                _curLevel.CleanLevel();
        }

        private void TickLevel(float deltaTime)
        {
            if (_curLevel != null)
                _curLevel.TickStateExecuting(deltaTime);
        }

        public override void OnDestroy()
        {
            DishManager.DestroyInstance();
            base.OnDestroy();
        }
    }
}

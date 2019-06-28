using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DoozyUI;
using DG.Tweening;

namespace UncleBear
{
    public class LevelMain : LevelBase
    {
        public static Vector3 v3CamDefaultPos = new Vector3(-522, 573, 133);
        public static Vector3 v3CamDefaultAngle = new Vector3(15, 180, 0);

        private StateMachine<LevelMain> _fsmLevel;
        public enum PhaseEnum
        {
            Idle,//没客人
            Wait,//招待
            Serve,//上菜
            Eat,//吃菜
            Judge,//评价
        }
        private Vector3 _v3Fix = new Vector3(-500, 500, 0);

        public LevelMain()
        {
            GenCharacter();
        }

        protected override void CreateLevelFsm()
        {
            _fsmLevel = new StateMachine<LevelMain>(this);

            //可以加参数设置每个状态的一些常量,或者在每个状态构造里自己处理
            _fsmLevel.AddState(new MainStateIdle((int)PhaseEnum.Idle));
            _fsmLevel.AddState(new MainStateWait((int)PhaseEnum.Wait));
            _fsmLevel.AddState(new MainStateEat((int)PhaseEnum.Eat));
            _fsmLevel.AddState(new MainStateServe((int)PhaseEnum.Serve));
            _fsmLevel.AddState(new MainStateJudge((int)PhaseEnum.Judge));
        }

        public override void LoadLevel()
        {
            base.LoadLevel();

            SetLevelCamFOV(false);
                //new Vector3(-448, 572, 120), new Vector3(20, 195, 0));

            if (!LevelManager.Instance.IsInGame)
                LevelManager.Instance.StartCoroutine(EnterGame());
            else
                LevelManager.Instance.StartCoroutine(ReturnFromGame());

            //没菜就当招待,有菜就陪吃
            if (DishManager.Instance.ObjFinishedDish == null)
                _fsmLevel.ChangeState((int)PhaseEnum.Idle);
            else
                _fsmLevel.ChangeState((int)PhaseEnum.Serve);
        }

        public override void CleanLevel()
        {
            if (_fsmLevel.CurState != null)
                _fsmLevel.CurState.Exit();
            _fsmLevel.CleanState();

            base.CleanLevel();
            Debug.Log("clean");
        }

        public override void TickStateExecuting(float deltaTime)
        {
            if (_bLevelEnded)
                return;
            string status = _fsmLevel.TickState(deltaTime);
            switch (status)
            {
                case "CustomerReady":
                    _fsmLevel.ChangeState((int)PhaseEnum.Wait);
                    break;
                case "DishReady":
                    _fsmLevel.ChangeState((int)PhaseEnum.Eat);
                    break;
                case "EatOver":
                    _fsmLevel.ChangeState((int)PhaseEnum.Judge);
                    break;
                case "CustomerLeaved":
                    _fsmLevel.ChangeState((int)PhaseEnum.Idle);
                    break;
            }
        }

        public override void ResetLevel()
        {
            if ((_fsmLevel.CurState != null && _fsmLevel.CurState.StateEnum != (int)PhaseEnum.Idle) || DishManager.Instance.CurCustomer != null)
            {
                GuideManager.Instance.StopGuide();
                UIPanelManager.Instance.HidePanel("UIPanelDishJudgement");
                DishManager.Instance.ClearDish();
                if (DishManager.Instance.ObjCoins != null)
                    DishManager.Instance.ObjCoins.SetActive(false);

                //deal with charas
                CharaCreator.Chef.transform.DOKill();
                CharaCreator.Waiter.transform.DOKill();
                if (DishManager.Instance.CurCustomer != null)
                {
                    DishManager.Instance.CurCustomer.StopMove();
                    DishManager.Instance.CurCustomer.transform.position = Vector3.one * 500;
                    DishManager.Instance.CurCustomer = null;
                }

                _fsmLevel.ChangeState((int)PhaseEnum.Idle);
            }
        }

        #region MainUI
        IEnumerator ReturnFromGame()
        {
            //one-frame delay to avoid jitter
            yield return null;

            UIPanelManager.Instance.HidePanel("UILoading");
            (UIPanelManager.Instance.ShowPanel("UIGameHUD") as UIPanel).ShowSubElements("UIHomeButton");
        }
        IEnumerator EnterGame()
        {
            yield return null;
            UIPanelManager.Instance.ShowPanel("UIStartPanel");
            UIPanelManager.Instance.GetPanel("UICover").GetComponentInChildren<SpriteFrameAnim>().Play();
        }
        #endregion

        #region MainLogic
        //角色不多,一次全部加载进来
        void GenCharacter()
        {
            for (int i = 0; i < (int)CharaEnum.Max; i++)
            {
                CharaCreator.CreateCharacter((CharaEnum)i);
            }
        }
        #endregion
    }
}

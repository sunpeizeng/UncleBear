using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public class LevelGame : LevelBase
    {
        public override void LoadLevel()
        {
            base.LoadLevel();

            SetLevelCamFOV(true);
            CameraManager.Instance.SetCamTransform(new Vector3(-32, 70, 12), new Vector3(45, 180, 0));
            UIPanelManager.Instance.ShowPanel("UIPanelDishLevel");
            DoozyUI.UIManager.PlayMusic("bgm03");
        }

        protected virtual void OnLevelStateContinued()
        {
            //if(_bPausedAtUI)
            //    DoozyUI.UIManager.PlaySound("8成功");

            _bPausedAtUI = false;
            UIPanel panel = UIPanelManager.Instance.GetPanel("UIPanelDishLevel");
            if (panel != null)
                panel.HideSubElements("UIContinueButton");
        }

        public override void CleanLevel()
        {
            base.CleanLevel();
            UIPanelManager.Instance.HidePanel("UIPanelDishLevel");
        }
    }
}

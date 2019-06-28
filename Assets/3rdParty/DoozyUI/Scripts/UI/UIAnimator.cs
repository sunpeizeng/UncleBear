// Copyright (c) 2015 - 2016 Doozy Entertainment / Marlink Trading SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using UnityEngine;
using DG.Tweening;

#if dUI_MasterAudio
using DarkTonic.MasterAudio;
#endif

namespace DoozyUI
{
    public class UIAnimator : MonoBehaviour
    {
        #region Enums - MoveDetails, SoundOutput, ButtonAnimationType, ResetType
        public enum MoveDetails
        {
            ParentPosition,
            LocalPosition,
            TopScreenEdge,
            RightScreenEdge,
            BottomScreenEdge,
            LeftScreenEdge,
            TopLeft,
            TopCenter,
            TopRight,
            MiddleLeft,
            MiddleCenter,
            MiddleRight,
            BottomLeft,
            BottomCenter,
            BottomRight
        }

        public enum SoundOutput
        {
            AudioSource,
            MasterAudioPlaySoundAndForget,
            MasterAudioFireCustomEvent
        }

        public enum ButtonAnimationType
        {
            None,
            PunchPosition,      // Punches a Transform's localPosition towards the given direction and then back to the starting one as if it was connected to the starting position via an elastic.
            PunchRotation,      //Punches a Transform's localRotation towards the given size and then back to the starting one as if it was connected to the starting rotation via an elastic.
            PunchScale
        }

        public enum AnimationTarget
        {
            None,
            UIElement,
            UIButton
        }

        public enum ResetType
        {
            All,
            Position,
            Rotation,
            Scale,
            Fade
        }
        #endregion

        #region Internal Classes - initialData, SoundDetails
        [System.Serializable]
        public class InitialData
        {
            public Vector3 startAnchoredPosition3D = Vector3.zero;
            public Vector3 startRotation = Vector3.zero;
            public Vector3 startScale = Vector3.one;
            public float startFadeAlpha = 1f;
            public bool soundOn = true;
        }

        [System.Serializable]
        public class SoundDetails
        {
            public string soundName = UIManager.DEFAULT_SOUND_NAME;
        }
        #endregion

        #region IN ANIMATION CLASSES

        [System.Serializable]
        public class MoveIn
        {
            //[Tooltip("Is the animation enabled?")]
            public bool enabled = false;
            //[Tooltip("Where does the animation begin from?")]
            public MoveDetails moveFrom = MoveDetails.BottomCenter;
            //[Tooltip("Use this if you need to adjust the target position. You add or subtract (if the number is negative) values to the position of the target location")]
            public Vector3 positionAdjustment = Vector3.zero;
            //[Tooltip("This is used when the Move From LocalPosition is selected")]
            public Vector3 positionFrom = Vector3.zero;
            //[Tooltip("Easing is the rate of change of animation over time")]
            public DG.Tweening.Ease easeType = DG.Tweening.Ease.OutBack;
            //[Tooltip("Time is amount (seconds) that the animation will take to complete")]
            public float time = 0.5f;
            //[Tooltip("Delay is amount (seconds) that the animation will wait before beginning")]
            public float delay = 0;
            //[Tooltip("Sends trigger sounds")]
            public SoundDetails soundAtStartReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            public SoundDetails soundAtFinishReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            //DoozyUI 1.2d old sounds - used for upgrade purposes only
            public string soundAtStart;
            public string soundAtFinish;
        }

        [System.Serializable]
        public class RotationIn
        {
            //[Tooltip("Is the animation enabled?")]
            public bool enabled = false;
            public Vector3 rotateFrom = Vector3.zero;
            //[Tooltip("Easing is the rate of change of animation over time")]
            public DG.Tweening.Ease easeType = DG.Tweening.Ease.OutBack;
            //[Tooltip("Time is amount (seconds) that the animation will take to complete")]
            public float time = 0.5f;
            //[Tooltip("Delay is amount (seconds) that the animation will wait before beginning")]
            public float delay = 0;
            //[Tooltip("Sends trigger sounds")]
            public SoundDetails soundAtStartReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            public SoundDetails soundAtFinishReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            //DoozyUI 1.2d old sounds - used for upgrade purposes only
            public string soundAtStart;
            public string soundAtFinish;
        }

        [System.Serializable]
        public class ScaleIn
        {
            //[Tooltip("Is the animation enabled?")]
            public bool enabled = false;
            //[Tooltip("From what scale factor does the animation begin? (default: 0)")]
            public Vector3 scaleBegin = Vector3.zero;
            //[Tooltip("Easing is the rate of change of animation over time")]
            public DG.Tweening.Ease easeType = DG.Tweening.Ease.OutBack;
            //[Tooltip("Time is amount (seconds) that the animation will take to complete")]
            public float time = 0.5f;
            //[Tooltip("Delay is amount (seconds) that the animation will wait before beginning")]
            public float delay = 0;
            //[Tooltip("Sends trigger sounds")]
            public SoundDetails soundAtStartReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            public SoundDetails soundAtFinishReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            //DoozyUI 1.2d old sounds - used for upgrade purposes only
            public string soundAtStart;
            public string soundAtFinish;
        }

        [System.Serializable]
        public class FadeIn
        {
            //[Tooltip("Is the animation enabled?")]
            public bool enabled = false;
            //[Tooltip("Easing is the rate of change of animation over time")]
            public DG.Tweening.Ease easeType = DG.Tweening.Ease.Linear;
            //[Tooltip("Time is amount (seconds) that the animation will take to complete")]
            public float time = 0.5f;
            //[Tooltip("Delay is amount (seconds) that the animation will wait before beginning")]
            public float delay = 0;
            //[Tooltip("Sends trigger sounds")]
            public SoundDetails soundAtStartReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            public SoundDetails soundAtFinishReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            //DoozyUI 1.2d old sounds - used for upgrade purposes only
            public string soundAtStart;
            public string soundAtFinish;
        }
        #endregion

        #region LOOP ANIMATION CLASSES

        [System.Serializable]
        public class MoveLoop
        {
            //[Tooltip("Is the animation enabled?")]
            public bool enabled = false;
            //[Tooltip("If you want this animation to ignore IN and OUT animations and auto start then select this as true")]
            public bool autoStart = false;
            //[Tooltip("This movement is calculated startAnchoredPosition-movement for min and startAnchoredPosition+movment for max")]
            public Vector3 movement = Vector3.zero;
            //[Tooltip("Easing is the rate of change of animation over time")]
            public DG.Tweening.Ease easeType = DG.Tweening.Ease.InOutSine;
            //[Tooltip("Number of loops (-1 = infinite loops)")]
            public int loops = -1;
            //[Tooltip("Types of loop")]
            public LoopType loopType = LoopType.Yoyo;
            //[Tooltip("Time is amount (seconds) that the animation will take to complete")]
            public float time = 0.5f;
            //[Tooltip("Delay is amount (seconds) that the animation will wait before beginning")]
            public float delay = 0;
            //[Tooltip("Sends trigger sounds")]
            public SoundDetails soundAtStartReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            public SoundDetails soundAtFinishReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            //DoozyUI 1.2d old sounds - used for upgrade purposes only
            public string soundAtStart;
            public string soundAtFinish;
        }

        [System.Serializable]
        public class RotationLoop
        {
            //[Tooltip("Is the animation enabled?")]
            public bool enabled = false;
            //[Tooltip("If you want this animation to ignore IN and OUT animations and auto start then select this as true")]
            public bool autoStart = false;
            //[Tooltip("This rotation is calculated startRotation-rotation for min and startRotation+rotation for max")]
            public Vector3 rotation = Vector3.zero;
            //[Tooltip("Easing is the rate of change of animation over time")]
            public DG.Tweening.Ease easeType = DG.Tweening.Ease.InOutSine;
            //[Tooltip("Number of loops (-1 = infinite loops)")]
            public int loops = -1;
            //[Tooltip("Types of loop")]
            public LoopType loopType = LoopType.Yoyo;
            //[Tooltip("Time is amount (seconds) that the animation will take to complete")]
            public float time = 0.5f;
            //[Tooltip("Delay is amount (seconds) that the animation will wait before beginning")]
            public float delay = 0;
            //[Tooltip("Sends trigger sounds")]
            public SoundDetails soundAtStartReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            public SoundDetails soundAtFinishReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            //DoozyUI 1.2d old sounds - used for upgrade purposes only
            public string soundAtStart;
            public string soundAtFinish;
        }

        [System.Serializable]
        public class ScaleLoop
        {
            //[Tooltip("Is the animation enabled?")]
            public bool enabled = false;
            //[Tooltip("If you want this animation to ignore IN and OUT animations and auto start then select this as true")]
            public bool autoStart = false;
            //[Tooltip("The minimum values for the scale factor of the scale loop animation (default: 1)")]
            public Vector3 min = new Vector3(1, 1, 1);
            //[Tooltip("The maximum values for the scale factor of the scale loop animation (default: 1.05)")]
            public Vector3 max = new Vector3(1.05f, 1.05f, 1.05f);
            //[Tooltip("Easing is the rate of change of animation over time")]
            public DG.Tweening.Ease easeType = DG.Tweening.Ease.Linear;
            //[Tooltip("Number of loops (-1 = infinite loops)")]
            public int loops = -1;
            //[Tooltip("Types of loop")]
            public LoopType loopType = LoopType.Yoyo;
            //[Tooltip("Time is amount (seconds) that the animation will take to complete")]
            public float time = 0.5f;
            //[Tooltip("Delay is amount (seconds) that the animation will wait before beginning")]
            public float delay = 0;
            //[Tooltip("Sends trigger sounds")]
            public SoundDetails soundAtStartReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            public SoundDetails soundAtFinishReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            //DoozyUI 1.2d old sounds - used for upgrade purposes only
            public string soundAtStart;
            public string soundAtFinish;
        }

        [System.Serializable]
        public class FadeLoop
        {
            //[Tooltip("Is the animation enabled?")]
            public bool enabled = false;
            //[Tooltip("If you want this animation to ignore IN and OUT animations and auto start then select this as true")]
            public bool autoStart = false;
            //[Tooltip("The minimum alpha value for the fade animation loop")]
            public float min = 0;
            //[Tooltip("The maximum alpha value for the fade animation loop")]
            public float max = 1;
            //[Tooltip("Easing is the rate of change of animation over time")]
            public DG.Tweening.Ease easeType = DG.Tweening.Ease.Linear;
            //[Tooltip("Number of loops (-1 = infinite loops)")]
            public int loops = -1;
            //[Tooltip("Types of loop")]
            public LoopType loopType = LoopType.Yoyo;
            //[Tooltip("Time is amount (seconds) that the animation will take to complete")]
            public float time = 0.5f;
            //[Tooltip("Delay is amount (seconds) that the animation will wait before beginning")]
            public float delay = 0;
            //[Tooltip("Sends trigger sounds")]
            public SoundDetails soundAtStartReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            public SoundDetails soundAtFinishReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            //DoozyUI 1.2d old sounds - used for upgrade purposes only
            public string soundAtStart;
            public string soundAtFinish;
        }
        #endregion

        #region OUT ANIMATION CLASSES
        [System.Serializable]
        public class MoveOut
        {
            //[Tooltip("Is the animation enabled?")]
            public bool enabled = false;
            //[Tooltip("Where does the animation end?")]
            public MoveDetails moveTo = MoveDetails.BottomCenter;
            //[Tooltip("Use this if you need to adjust the target position. You add or substract (if the number is negative) values to the position of the target location")]
            public Vector3 positionAdjustment = Vector3.zero;
            //[Tooltip("This is used when the Move From LocalPosition is selected")]
            public Vector3 positionTo = Vector3.zero;
            //[Tooltip("Easing is the rate of change of animation over time")]
            public DG.Tweening.Ease easeType = DG.Tweening.Ease.InBack;
            //[Tooltip("Time is amount (seconds) that the animation will take to complete")]
            public float time = 0.5f;
            //[Tooltip("Delay is amount (seconds) that the animation will wait before beginning")]
            public float delay = 0;
            //[Tooltip("Sends trigger sounds")]
            public SoundDetails soundAtStartReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            public SoundDetails soundAtFinishReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            //DoozyUI 1.2d old sounds - used for upgrade purposes only
            public string soundAtStart;
            public string soundAtFinish;
        }

        [System.Serializable]
        public class RotationOut
        {
            //[Tooltip("Is the animation enabled?")]
            public bool enabled = false;
            public Vector3 rotateTo = Vector3.zero;
            //[Tooltip("Easing is the rate of change of animation over time")]
            public DG.Tweening.Ease easeType = DG.Tweening.Ease.InBack;
            //[Tooltip("Time is amount (seconds) that the animation will take to complete")]
            public float time = 0.5f;
            //[Tooltip("Delay is amount (seconds) that the animation will wait before beginning")]
            public float delay = 0;
            //[Tooltip("Sends trigger sounds")]
            public SoundDetails soundAtStartReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            public SoundDetails soundAtFinishReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            //DoozyUI 1.2d old sounds - used for upgrade purposes only
            public string soundAtStart;
            public string soundAtFinish;
        }

        [System.Serializable]
        public class ScaleOut
        {
            //[Tooltip("Is the animation enabled?")]
            public bool enabled = false;
            //[Tooltip("This is the scale factor at which the animation ends at")]
            public Vector3 scaleEnd = Vector3.zero;
            //[Tooltip("Easing is the rate of change of animation over time")]
            public DG.Tweening.Ease easeType = DG.Tweening.Ease.InBack;
            //[Tooltip("Time is amount (seconds) that the animation will take to complete")]
            public float time = 0.5f;
            //[Tooltip("Delay is amount (seconds) that the animation will wait before beginning")]
            public float delay = 0;
            //[Tooltip("Sends trigger sounds")]
            public SoundDetails soundAtStartReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            public SoundDetails soundAtFinishReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            //DoozyUI 1.2d old sounds - used for upgrade purposes only
            public string soundAtStart;
            public string soundAtFinish;

        }

        [System.Serializable]
        public class FadeOut
        {
            //[Tooltip("Is the animation enabled?")]
            public bool enabled = false;
            //[Tooltip("Easing is the rate of change of animation over time")]
            public DG.Tweening.Ease easeType = DG.Tweening.Ease.Linear;
            //[Tooltip("Time is amount (seconds) that the animation will take to complete")]
            public float time = 0.5f;
            //[Tooltip("Delay is amount (seconds) that the animation will wait before beginning")]
            public float delay = 0;
            //[Tooltip("Sends trigger sounds")]
            public SoundDetails soundAtStartReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            public SoundDetails soundAtFinishReference = new SoundDetails() { soundName = UIManager.DEFAULT_SOUND_NAME };
            //DoozyUI 1.2d old sounds - used for upgrade purposes only
            public string soundAtStart;
            public string soundAtFinish;
        }
        #endregion

        #region Sound Methods - PlayClipAt, PlaySound
        /// <summary>
        /// Plays a clip at the specified location
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static AudioSource PlayClipAt(AudioClip clip, Vector3 pos, bool loop = false, float volume = 1, float delay = 0f)
        {
            //Kevin.Zhang, 1/23/2017
            AudioSource aSource = AudioSourcePool.Instance.Spawn(clip, pos);
            aSource.volume = volume * UIManager.soundVolume;
            aSource.loop = loop;
            if (delay > 0)
                aSource.PlayDelayed(delay);
            else
                aSource.Play();

            //make sure the audio source will finished playing before being destroyed
            if (!loop)
                AudioSourcePool.Instance.Free(aSource, clip.length + 0.5f + delay);

            return aSource;

//             var tempGO = new GameObject("TempAudio - " + clip.name); // create the temp object
//             tempGO.transform.position = pos; // set its position
//             var aSource = tempGO.AddComponent<AudioSource>(); ; // add an audio source
//             aSource.clip = clip; // define the clip
//                                  // set other aSource properties here, if desired - custom volume, custom pitch, etc...
//             aSource.Play(); // start the sound
//             Destroy(tempGO, clip.length); // destroy object after clip duration
//             return aSource; // return the AudioSource reference
        }

        /// <summary>
        /// Plays a sound with an AudioSource or with Master Audio. If the string is empty or null this function does nothing.
        /// This function will play a sound regardless if the sound is off in UIManager.
        /// To play a sound and check if the sound is on in UIManager call UIAnimator.PlaySound(soundName, UIManager.Instance.soundOn);
        /// </summary>
        /// <param name="soundName"></param>
        public static void PlaySound(string soundName)
        {
            PlaySound(soundName, true);
        }

        /// <summary>
        /// Plays a sound with an AudioSource or with Master Audio. If the string is empty or null this function does nothing.
        /// Also if the sound is off (soundOn == false) then this function will do nothing.
        /// </summary>
        /// <param name="soundName">Sound name.</param>
        public static void PlaySound(string soundName, bool soundOn)
        {
            if (string.IsNullOrEmpty(soundName) || soundOn == false || soundName.Equals(UIManager.DEFAULT_SOUND_NAME))
                return;

            SoundOutput soundOutput = SoundOutput.AudioSource;

            if (UIManager.usesMA_FireCustomEvent)
            {
                soundOutput = SoundOutput.MasterAudioFireCustomEvent;
            }
            else if (UIManager.usesMA_PlaySoundAndForget)
            {
                soundOutput = SoundOutput.MasterAudioPlaySoundAndForget;
            }

            switch (soundOutput)
            {
                case SoundOutput.AudioSource:
                    //AudioClip clip = Resources.Load(soundName) as AudioClip;
                    
                    //Kevin.Zhang, 1/23/2017
                    AudioClip clip = UIManager.LoadOrGetClip(soundName);
                    if (clip == null)
                    {
                        Debug.Log("[DoozyUI] There is no sound file with the name [" + soundName + "] in any of the Resources folders.\n Check that the spelling of the fileName (without the extension) is correct or if the file exists in under a Resources folder");
                        return;
                    }
                    //play without an AudioSource component
                    PlayClipAt(clip, Vector3.zero);
                    break;

                case SoundOutput.MasterAudioPlaySoundAndForget:
#if dUI_MasterAudio
                    MasterAudio.PlaySoundAndForget(soundName);
#else
                    Debug.Log("[DoozyUI] You are trying to use MasterAudio by calling the PlaySoundAndForget method, but you don't have it enabled. Please check if the 'dUI_MasterAudio' symbol is defined in 'Scripting Define Symbol'. You can find it by going to [Edit] -> [Project Settings] -> [Player] -> look at the inspector -> [Other Settings] -> look for [Scripting Define Symbols] => if you do not see 'dUI_MasterAudio' there, please add it.");
#endif
                    break;

                case SoundOutput.MasterAudioFireCustomEvent:
#if dUI_MasterAudio
                    MasterAudio.FireCustomEvent(soundName, UIManager.GetUiContainer);
#else
                    Debug.Log("[DoozyUI] You are trying to use MasterAudio by calling the FireCustomEvent method, but you don't have it enabled. Please check if the 'dUI_MasterAudio' symbol is defined in 'Scripting Define Symbol'. You can find it by going to [Edit] -> [Project Settings] -> [Player] -> look at the inspector -> [Other Settings] -> look for [Scripting Define Symbols] => if you do not see 'dUI_MasterAudio' there, please add it.");
#endif
                    break;
            }
        }
        #endregion

        #region Reset Rect Transfrom
        /// <summary>
        /// Resets the rect transform to it's initial states.
        /// </summary>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="initialData">Rect transform data.</param>
        public static void ResetRectTransform(RectTransform rectTransform, InitialData initialData, ResetType resetType = ResetType.All)
        {
            if (rectTransform == null || initialData == null)
                return;

            CanvasGroup canvasGroup = rectTransform.GetComponent<CanvasGroup>();

            switch (resetType)
            {
                case ResetType.All:
                    rectTransform.anchoredPosition = initialData.startAnchoredPosition3D;
                    rectTransform.localRotation = Quaternion.Euler(initialData.startRotation);
                    rectTransform.localScale = initialData.startScale;

                    if (canvasGroup != null)
                    {
                        canvasGroup.interactable = true;
                        canvasGroup.blocksRaycasts = true;
                        canvasGroup.alpha = 1f;
                    }
                    break;

                case ResetType.Position:
                    rectTransform.anchoredPosition = initialData.startAnchoredPosition3D;
                    break;

                case ResetType.Rotation:
                    rectTransform.localRotation = Quaternion.Euler(initialData.startRotation);
                    break;

                case ResetType.Scale:
                    rectTransform.localScale = initialData.startScale;
                    break;

                case ResetType.Fade:
                    if (canvasGroup != null)
                    {
                        canvasGroup.interactable = true;
                        canvasGroup.blocksRaycasts = true;
                        canvasGroup.alpha = 1f;
                    }
                    break;
            }
        }
        #endregion

        #region IN ANIMATIONS

        #region MoveIN
        /// <summary>
        /// Plays the Move In view animation for a RectTransform with a PanelController component added to it.
        /// </summary>
        /// <param name="moveIn">Move in.</param>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="startAnchoredPosition">Start anchored position.</param>
        public static void DoMoveIn(MoveIn moveIn, RectTransform rectTransform, InitialData initialData, bool instantAction)
        {
            if (!moveIn.enabled) { return; }
            RectTransform parent = rectTransform.parent.GetComponent<RectTransform>();  //We need to do this check because when we Instantiate a notification we need to use the uiContainer if the parent is null.
            if (parent == null) { parent = UIManager.GetUiContainer.GetComponent<RectTransform>(); }
            Vector2 targetPosition = initialData.startAnchoredPosition3D;
#if UNITY_5_3 == false
            Canvas tempCanvas = rectTransform.GetComponent<Canvas>();
#endif
            Canvas rootCanvas = null;
#if UNITY_5_3
            rootCanvas = rectTransform.root.GetComponentInChildren<Canvas>();
#else
            if (tempCanvas == null) //this might be a button or an UIElement that does not have a Canvas component (this should not happen)
            {
                rootCanvas = rectTransform.root.GetComponentInChildren<Canvas>();
            }
            else
            {
                rootCanvas = tempCanvas.rootCanvas;
            }
#endif
            Rect rootCanvasRect = rootCanvas.GetComponent<RectTransform>().rect;
            float xOffset = rootCanvasRect.width / 2 + rectTransform.rect.width * rectTransform.pivot.x;
            float yOffset = rootCanvasRect.height / 2 + rectTransform.rect.height * rectTransform.pivot.y;

            switch (moveIn.moveFrom)
            {
                case MoveDetails.ParentPosition:
                    if (parent == null)
                        return;

                    targetPosition = new Vector2(parent.anchoredPosition.x + moveIn.positionAdjustment.x,
                                                 parent.anchoredPosition.y + moveIn.positionAdjustment.y);
                    break;

                case MoveDetails.LocalPosition:
                    if (parent == null)
                        return;

                    targetPosition = new Vector2(moveIn.positionFrom.x + moveIn.positionAdjustment.x,
                                                 moveIn.positionFrom.y + moveIn.positionAdjustment.y);
                    break;

                case MoveDetails.TopScreenEdge:
                    targetPosition = new Vector2(moveIn.positionAdjustment.x + initialData.startAnchoredPosition3D.x,
                                                 moveIn.positionAdjustment.y + yOffset);
                    break;

                case MoveDetails.RightScreenEdge:
                    targetPosition = new Vector2(moveIn.positionAdjustment.x + xOffset,
                                                 moveIn.positionAdjustment.y + initialData.startAnchoredPosition3D.y);
                    break;

                case MoveDetails.BottomScreenEdge:
                    targetPosition = new Vector2(moveIn.positionAdjustment.x + initialData.startAnchoredPosition3D.x,
                                                 moveIn.positionAdjustment.y - yOffset);
                    break;

                case MoveDetails.LeftScreenEdge:
                    targetPosition = new Vector2(moveIn.positionAdjustment.x - xOffset,
                                                 moveIn.positionAdjustment.y + initialData.startAnchoredPosition3D.y);
                    break;

                case MoveDetails.TopLeft:
                    targetPosition = new Vector2(moveIn.positionAdjustment.x - xOffset,
                                                 moveIn.positionAdjustment.y + yOffset);
                    break;

                case MoveDetails.TopCenter:
                    targetPosition = new Vector2(moveIn.positionAdjustment.x,
                                                 moveIn.positionAdjustment.y + yOffset);
                    break;

                case MoveDetails.TopRight:
                    targetPosition = new Vector2(moveIn.positionAdjustment.x + xOffset,
                                                 moveIn.positionAdjustment.y + yOffset);
                    break;

                case MoveDetails.MiddleLeft:
                    targetPosition = new Vector2(moveIn.positionAdjustment.x - xOffset,
                                                 moveIn.positionAdjustment.y);
                    break;

                case MoveDetails.MiddleCenter:
                    targetPosition = new Vector2(moveIn.positionAdjustment.x,
                                                 moveIn.positionAdjustment.y);
                    break;

                case MoveDetails.MiddleRight:
                    targetPosition = new Vector2(moveIn.positionAdjustment.x + xOffset,
                                                 moveIn.positionAdjustment.y);
                    break;

                case MoveDetails.BottomLeft:
                    targetPosition = new Vector2(moveIn.positionAdjustment.x - xOffset,
                                                 moveIn.positionAdjustment.y - yOffset);
                    break;

                case MoveDetails.BottomCenter:
                    targetPosition = new Vector2(moveIn.positionAdjustment.x,
                                                 moveIn.positionAdjustment.y - yOffset);
                    break;

                case MoveDetails.BottomRight:
                    targetPosition = new Vector2(moveIn.positionAdjustment.x + xOffset,
                                                 moveIn.positionAdjustment.y - yOffset);
                    break;

                default:
                    Debug.LogWarning("[DoozyUI] This should not happen! DoMoveIn in UIAnimator went to the default setting!");
                    return;
            }

            DoMoveInAnimation(targetPosition, moveIn, rectTransform, initialData, instantAction);
        }

        /// <summary>
        /// This is a helper method for the DoMoveIn. It simplifies a lot the switch case for each MoveDetails
        /// </summary>
        /// <param name="position"></param>
        /// <param name="moveIn"></param>
        /// <param name="time"></param>
        /// <param name="delay"></param>
        /// <param name="rectTransform"></param>
        /// <param name="initialData"></param>
        /// <param name="instantAction"></param>
        public static void DoMoveInAnimation(Vector2 position, MoveIn moveIn, RectTransform rectTransform, InitialData initialData, bool instantAction)
        {
            string tweenID = rectTransform.GetInstanceID() + "_DoMoveIn";
            rectTransform.anchoredPosition = position;
            rectTransform
                .DOAnchorPos(initialData.startAnchoredPosition3D, instantAction ? 0 : moveIn.time, false)
                    .SetDelay(instantAction ? 0 : moveIn.delay)
                    .SetEase(moveIn.easeType)
                    .SetId(tweenID)
                    .SetUpdate(UpdateType.Normal, UIManager.isTimeScaleIndependent)
                    .OnStart(() =>
                    {
                        if (!instantAction) { UIAnimator.PlaySound(moveIn.soundAtStartReference.soundName, initialData.soundOn); }
                    })
                    .OnComplete(() =>
                    {
                        if (!instantAction) { UIAnimator.PlaySound(moveIn.soundAtFinishReference.soundName, initialData.soundOn); }
                        UIAnimator.StartLoopAnimations(rectTransform);
                    })
                    .Play();
        }
        #endregion

        #region RotateIN
        /// <summary>
        /// Plays the Rotate In animation for a RectTransform with a PanelController component added to it.
        /// </summary>
        /// <param name="rotationIn">Rotation in.</param>
        /// <param name="rectTransform">Rect transform.</param>
        public static void DoRotationIn(RotationIn rotationIn, RectTransform rectTransform, InitialData initialData, bool instantAction)
        {
            if (!rotationIn.enabled) { return; }
            rectTransform.localRotation = Quaternion.Euler(initialData.startRotation);
            string tweenID = rectTransform.GetInstanceID() + "_DoRotationIn";

            rectTransform
            .DOLocalRotate(rotationIn.rotateFrom, instantAction ? 0 : rotationIn.time, RotateMode.FastBeyond360)
                .SetDelay(instantAction ? 0 : rotationIn.delay)
                .SetEase(rotationIn.easeType)
                .SetId(tweenID)
                .SetUpdate(UpdateType.Normal, UIManager.isTimeScaleIndependent)
                .OnStart(() =>
                {
                    if (!instantAction) { UIAnimator.PlaySound(rotationIn.soundAtStartReference.soundName, initialData.soundOn); }
                })
                .OnComplete(() =>
                {
                    if (!instantAction) { UIAnimator.PlaySound(rotationIn.soundAtFinishReference.soundName, initialData.soundOn); }
                    UIAnimator.StartLoopAnimations(rectTransform);
                })
                .From()
                .Play();
        }
        #endregion

        #region ScaleIN
        /// <summary>
        /// Plays the Scale In animation for a RectTransform with a PanelController component added to it.
        /// </summary>
        /// <param name="scaleIn">Scale in.</param>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="startScale">Start scale.</param>
        public static void DoScaleIn(ScaleIn scaleIn, RectTransform rectTransform, InitialData initialData, bool instantAction)
        {
            if (!scaleIn.enabled) { return; }
            string tweenID = rectTransform.GetInstanceID() + "_DoScaleIn";

            rectTransform
            .DOScale(scaleIn.scaleBegin, instantAction ? 0 : scaleIn.time)
                .SetDelay(instantAction ? 0 : scaleIn.delay)
                .SetEase(scaleIn.easeType)
                .SetId(tweenID)
                .SetUpdate(UpdateType.Normal, UIManager.isTimeScaleIndependent)
                .OnStart(() =>
                {
                    if (!instantAction) { UIAnimator.PlaySound(scaleIn.soundAtStartReference.soundName, initialData.soundOn); }
                })
                .OnComplete(() =>
                {
                    if (!instantAction) { UIAnimator.PlaySound(scaleIn.soundAtFinishReference.soundName, initialData.soundOn); }
                    UIAnimator.StartLoopAnimations(rectTransform);
                })
                .From()
                .Play();
        }
        #endregion

        #region FadeIN
        /// <summary>
        /// Plays the Fade In animation for a RectTransform with a PanelController component added to it.
        /// </summary>
        /// <param name="fadeIn">Fade in.</param>
        /// <param name="rectTransform">Rect transform.</param>
        public static void DoFadeIn(FadeIn fadeIn, RectTransform rectTransform, InitialData initialData, bool instantAction)
        {
            if (!fadeIn.enabled) { return; }
            CanvasGroup canvasGroup = rectTransform.GetComponent<CanvasGroup>();
            if (canvasGroup == null) { canvasGroup = rectTransform.gameObject.AddComponent<CanvasGroup>(); }
            canvasGroup.alpha = 0f;
            string tweenID = rectTransform.GetInstanceID() + "_DoFadeIn";

            canvasGroup
            .DOFade(1f, instantAction ? 0 : fadeIn.time)
                .SetDelay(instantAction ? 0 : fadeIn.delay)
                .SetEase(fadeIn.easeType)
                .SetId(tweenID)
                .SetUpdate(UpdateType.Normal, UIManager.isTimeScaleIndependent)
                .OnStart(() =>
                {
                    if (!instantAction) { UIAnimator.PlaySound(fadeIn.soundAtStartReference.soundName, initialData.soundOn); }
                })
                .OnComplete(() =>
                {
                    if (!instantAction) { UIAnimator.PlaySound(fadeIn.soundAtFinishReference.soundName, initialData.soundOn); }
                    UIAnimator.StartLoopAnimations(rectTransform);
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                    canvasGroup.alpha = 1f;
                })
                .Play();
        }
        #endregion

        #region IN ANIMATIONS - Stop method
        /// <summary>
        /// Stops the IN animations on a rectTransform. This is used before any OUT animations to avoid calling a hide method while a show method is still playing (without this, unexpected behaviour can occur)
        /// </summary>
        /// <param name="rectTransform">Rect transform.</param>
        public static void StopInAnimations(RectTransform rectTransform, InitialData initialData)
        {
            if (rectTransform == null || rectTransform.GetComponent<UIElement>() == null) { return; }
            int id = rectTransform.GetInstanceID();
            DOTween.Kill(id + "_DoMoveIn");
            DOTween.Kill(id + "_DoRotationIn");
            DOTween.Kill(id + "_DoScaleIn");
            DOTween.Kill(id + "_DoFadeIn");
        }
        #endregion

        #endregion

        #region LOOP ANIMATIONS

        #region MoveLOOP
        /// <summary>
        /// This initialises and plays (if set to autoStart) the idle animation Move Loop for a RectTransform with a PanelController component added to it.
        /// </summary>
        /// <param name="moveLoop">Move loop.</param>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="startAnchoredPosition">Start anchored position.</param>
        public static void DoMoveLoop(MoveLoop moveLoop, RectTransform rectTransform, InitialData initialData)
        {
            if (!moveLoop.enabled) { return; }

            Vector3 animBeginPosition = new Vector3(initialData.startAnchoredPosition3D.x - moveLoop.movement.x,
                                                    initialData.startAnchoredPosition3D.y - moveLoop.movement.y,
                                                    initialData.startAnchoredPosition3D.z - moveLoop.movement.z);

            Vector3 animEndPosition = new Vector3(initialData.startAnchoredPosition3D.x + moveLoop.movement.x,
                                                   initialData.startAnchoredPosition3D.y + moveLoop.movement.y,
                                                   initialData.startAnchoredPosition3D.z + moveLoop.movement.z);

            string tweenID = rectTransform.GetInstanceID() + "_DoMoveLoop";

            if (moveLoop.loopType == LoopType.Yoyo)
            {
                rectTransform
                .DOAnchorPos(animBeginPosition, moveLoop.time / 2f, false)
                    .SetDelay(moveLoop.delay)
                    .SetEase(moveLoop.easeType)
                    .SetId(tweenID)
                    .SetUpdate(UpdateType.Normal, UIManager.isTimeScaleIndependent)
                    .OnStart(() =>
                    {
                        if (initialData.soundOn) { UIAnimator.PlaySound(moveLoop.soundAtStartReference.soundName, initialData.soundOn); }
                    })
                    .OnComplete(() =>
                    {
                        rectTransform
                        .DOAnchorPos(animEndPosition, moveLoop.time, false)
                            .SetEase(moveLoop.easeType)
                            .SetLoops(moveLoop.loops, moveLoop.loopType)
                            .SetId(tweenID)
                            .SetUpdate(UpdateType.Normal, UIManager.isTimeScaleIndependent)
                            .OnComplete(() =>
                            {
                                if (initialData.soundOn) { UIAnimator.PlaySound(moveLoop.soundAtFinishReference.soundName, initialData.soundOn); }
                            })
                            .Play();
                    })
                    .Pause();
            }
            else
            {
                rectTransform
                .DOAnchorPos(moveLoop.movement, moveLoop.time, false)
                    .SetRelative(true)
                    .SetDelay(moveLoop.delay)
                    .SetLoops(moveLoop.loops, moveLoop.loopType)
                    .SetEase(moveLoop.easeType)
                    .SetId(tweenID)
                    .SetUpdate(UpdateType.Normal, UIManager.isTimeScaleIndependent)
                    .OnStart(() =>
                    {
                        if (initialData.soundOn) { UIAnimator.PlaySound(moveLoop.soundAtStartReference.soundName, initialData.soundOn); }
                    })
                    .OnComplete(() =>
                    {
                        if (initialData.soundOn) { UIAnimator.PlaySound(moveLoop.soundAtFinishReference.soundName, initialData.soundOn); }
                    })
                    .Pause();
            }

            if (moveLoop.autoStart) { DOTween.Play(tweenID); }
        }
        #endregion

        #region RotateLOOP
        /// <summary>
        /// This initialises and plays (if set to autoStart) the idle animation Rotation Loop for a RectTransform with a PanelController component added to it.
        /// </summary>
        /// <param name="rotationLoop">Rotation loop.</param>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="initialData">Rect transform data.</param>
        public static void DoRotationLoop(RotationLoop rotationLoop, RectTransform rectTransform, InitialData initialData)
        {
            if (!rotationLoop.enabled) { return; }

            Vector3 animBeginRotation = new Vector3(initialData.startRotation.x - rotationLoop.rotation.x,
                                                     initialData.startRotation.y - rotationLoop.rotation.y,
                                                     initialData.startRotation.z - rotationLoop.rotation.z - rotationLoop.rotation.z / 4f);

            Vector3 animEndRotation = new Vector3(initialData.startRotation.x + rotationLoop.rotation.x,
                                                   initialData.startRotation.y + rotationLoop.rotation.y,
                                                   initialData.startRotation.z + rotationLoop.rotation.z);

            string tweenID = rectTransform.GetInstanceID() + "_DoRotationLoop";

            if (rotationLoop.loopType == LoopType.Yoyo)
            {
                rectTransform.DOLocalRotate(animBeginRotation, rotationLoop.time / 2f, RotateMode.Fast)
                   .SetDelay(rotationLoop.delay)
                   .SetEase(rotationLoop.easeType)
                   .SetId(tweenID)
                   .SetUpdate(UpdateType.Normal, UIManager.isTimeScaleIndependent)
                   .OnStart(() =>
                   {
                       if (initialData.soundOn) { UIAnimator.PlaySound(rotationLoop.soundAtStartReference.soundName, initialData.soundOn); }
                   })
                   .OnComplete(() =>
                   {
                       rectTransform.DOLocalRotate(animEndRotation, rotationLoop.time, RotateMode.Fast)
                               .SetEase(rotationLoop.easeType)
                               .SetLoops(rotationLoop.loops, rotationLoop.loopType)
                               .SetId(tweenID)
                               .SetUpdate(UpdateType.Normal, UIManager.isTimeScaleIndependent)
                               .OnComplete(() =>
                               {
                                   if (initialData.soundOn) { UIAnimator.PlaySound(rotationLoop.soundAtFinishReference.soundName, initialData.soundOn); }
                               })
                               .Play();
                   })
                   .Pause();
            }
            else
            {
                rectTransform.DOLocalRotate(rotationLoop.rotation, rotationLoop.time, RotateMode.FastBeyond360)
                  .SetRelative(true)
                  .SetDelay(rotationLoop.delay)
                  .SetLoops(rotationLoop.loops, rotationLoop.loopType)
                  .SetEase(rotationLoop.easeType)
                  .SetId(tweenID)
                  .SetUpdate(UpdateType.Normal, UIManager.isTimeScaleIndependent)
                  .OnStart(() =>
                  {
                      if (initialData.soundOn) { UIAnimator.PlaySound(rotationLoop.soundAtStartReference.soundName, initialData.soundOn); }
                  })
                  .OnComplete(() =>
                  {
                      if (initialData.soundOn) { UIAnimator.PlaySound(rotationLoop.soundAtFinishReference.soundName, initialData.soundOn); }
                  })
                  .Pause();
            }

            if (rotationLoop.autoStart) { DOTween.Play(tweenID); }
        }
        #endregion

        #region ScaleLOOP
        /// <summary>
        /// This initialises and plays (if set to autoStart) the idle animation Scale Loop for a RectTransform with a PanelController component added to it.
        /// </summary>
        /// <param name="scaleLoop">Scale loop.</param>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="initialData">Rect transform data.</param>
        public static void DoScaleLoop(ScaleLoop scaleLoop, RectTransform rectTransform, InitialData initialData)
        {
            if (!scaleLoop.enabled) { return; }
            string tweenID = rectTransform.GetInstanceID() + "_DoScaleLoop";
            rectTransform.localScale = scaleLoop.min;

            rectTransform
                .DOScale(scaleLoop.max, scaleLoop.time)
                .SetDelay(scaleLoop.delay)
                .SetEase(scaleLoop.easeType)
                .SetLoops(scaleLoop.loops, scaleLoop.loopType)
                .SetId(tweenID)
                .SetUpdate(UpdateType.Normal, UIManager.isTimeScaleIndependent)
               .OnStart(() =>
               {
                   if (initialData.soundOn) { UIAnimator.PlaySound(scaleLoop.soundAtStartReference.soundName, initialData.soundOn); }
               })
               .OnComplete(() =>
               {
                   if (initialData.soundOn) { UIAnimator.PlaySound(scaleLoop.soundAtFinishReference.soundName, initialData.soundOn); }
               })
               .Pause();

            if (scaleLoop.autoStart) { DOTween.Play(tweenID); }
        }
        #endregion

        #region FadeLOOP
        /// <summary>
        /// This initialises and plays (if set to autoStart) the idle animation Fade Loop for a RectTransform with a PanelController component added to it.
        /// </summary>
        /// <param name="fadeLoop">Fade loop.</param>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="initialData">Rect transform data.</param>
        public static void DoFadeLoop(FadeLoop fadeLoop, RectTransform rectTransform, InitialData initialData, AnimationTarget animationTarget = AnimationTarget.UIElement)
        {
            if (!fadeLoop.enabled) { return; }
            string tweenID = rectTransform.GetInstanceID() + "_DoFadeLoop";
            CanvasGroup canvasGroup = rectTransform.GetComponent<CanvasGroup>();
            if (canvasGroup == null) { canvasGroup = rectTransform.gameObject.AddComponent<CanvasGroup>(); }
            canvasGroup.alpha = fadeLoop.max;
            switch (animationTarget)
            {
                case AnimationTarget.UIElement:
                    canvasGroup.blocksRaycasts = false; //we do this so that the UIElement ignores the clicks
                    break;

                case AnimationTarget.UIButton:
                    canvasGroup.blocksRaycasts = true; //we do this so that we can click on the button
                    break;
            }

            canvasGroup
                .DOFade(fadeLoop.min, fadeLoop.time)
                    .SetDelay(fadeLoop.delay)
                    .SetEase(fadeLoop.easeType)
                    .SetLoops(fadeLoop.loops, fadeLoop.loopType)
                    .SetId(tweenID)
                    .SetUpdate(UpdateType.Normal, UIManager.isTimeScaleIndependent)
                    .OnStart(() =>
                    {
                        if (initialData.soundOn) { UIAnimator.PlaySound(fadeLoop.soundAtStartReference.soundName, initialData.soundOn); }
                    })
                    .OnComplete(() =>
                    {
                        if (initialData.soundOn) { UIAnimator.PlaySound(fadeLoop.soundAtFinishReference.soundName, initialData.soundOn); }
                    })
                    .Pause();

            if (fadeLoop.autoStart) { DOTween.Play(tweenID); }
        }
        #endregion

        #region LOOP ANIMATIONS - Start and Stop methods
        /// <summary>
        /// Starts the idle animations set up on a rectTransform.
        /// </summary>
        /// <param name="rectTransform">Rect transform.</param>
        public static void StartLoopAnimations(RectTransform rectTransform)
        {
            UIElement uiElement = rectTransform.GetComponent<UIElement>();

            if (uiElement == null)
            {
                Debug.LogWarning("[DoozyUI] " + rectTransform.name + " does not have a UIElement Component attached and you are trying to start an idle animation");
                return;
            }

            int id = rectTransform.GetInstanceID();

            if (uiElement.moveLoop.enabled)
            {
                DoMoveLoop(uiElement.moveLoop, rectTransform, uiElement.GetInitialData);
                DOTween.Play(id + "_DoMoveLoop");
            }

            if (uiElement.rotationLoop.enabled)
            {
                DoRotationLoop(uiElement.rotationLoop, rectTransform, uiElement.GetInitialData);
                DOTween.Play(id + "_DoRotationLoop");
            }

            if (uiElement.scaleLoop.enabled)
            {
                DoScaleLoop(uiElement.scaleLoop, rectTransform, uiElement.GetInitialData);
                DOTween.Play(id + "_DoScaleLoop");
            }

            if (uiElement.fadeLoop.enabled)
            {
                DoFadeLoop(uiElement.fadeLoop, rectTransform, uiElement.GetInitialData);
                DOTween.Play(id + "_DoFadeLoop");
            }
        }

        /// <summary>
        /// Stops the idle animations on a rectTransform.
        /// </summary>
        /// <param name="rectTransform">Rect transform.</param>
        public static void StopLoopAnimations(RectTransform rectTransform, InitialData initialData)
        {
            if (rectTransform == null || initialData == null || rectTransform.GetComponent<UIElement>() == null) { return; }
            ResetRectTransform(rectTransform, initialData);
            int id = rectTransform.GetInstanceID();
            DOTween.Kill(id + "_DoMoveLoop");
            DOTween.Kill(id + "_DoRotationLoop");
            DOTween.Kill(id + "_DoScaleLoop");
            DOTween.Kill(id + "_DoFadeLoop");
        }
        #endregion

        #endregion

        #region OUT ANIMATIONS

        #region MoveOUT
        /// <summary>
        /// Plays the Move Out of view animation for a RectTransform with a PanelController component added to it.
        /// </summary>
        /// <param name="moveOut">Move out.</param>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="startAnchoredPosition">Start anchored position.</param>
        public static void DoMoveOut(MoveOut moveOut, RectTransform rectTransform, InitialData initialData, bool instantAction)
        {
            if (!moveOut.enabled) { return; }
            RectTransform parent = rectTransform.parent.GetComponent<RectTransform>();  //We need to do this check because when we Instantiate a notification we need to use the uiContainer if the parent is null.
            if (parent == null) { parent = UIManager.GetUiContainer.GetComponent<RectTransform>(); }
            Vector2 targetPosition = Vector2.zero;
#if UNITY_5_3 == false
            Canvas tempCanvas = rectTransform.GetComponent<Canvas>();
#endif
            Canvas rootCanvas = null;
#if UNITY_5_3
            rootCanvas = rectTransform.root.GetComponentInChildren<Canvas>();
#else
            if (tempCanvas == null) //this might be a button or an UIElement that does not have a Canvas component (this should not happen)
            {
                rootCanvas = rectTransform.root.GetComponentInChildren<Canvas>();
            }
            else
            {
                rootCanvas = tempCanvas.rootCanvas;
            }
#endif
            Rect rootCanvasRect = rootCanvas.GetComponent<RectTransform>().rect;
            float xOffset = rootCanvasRect.width / 2 + rectTransform.rect.width * rectTransform.pivot.x;
            float yOffset = rootCanvasRect.height / 2 + rectTransform.rect.height * rectTransform.pivot.y;

            switch (moveOut.moveTo)
            {
                case MoveDetails.ParentPosition:
                    if (parent == null)
                        return;

                    targetPosition = new Vector2(moveOut.positionAdjustment.x + parent.anchoredPosition.x,
                                                 moveOut.positionAdjustment.y + parent.anchoredPosition.y);
                    break;

                case MoveDetails.LocalPosition:
                    if (parent == null)
                        return;

                    targetPosition = new Vector2(moveOut.positionAdjustment.x + moveOut.positionTo.x,
                                                 moveOut.positionAdjustment.y + moveOut.positionTo.y);
                    break;

                case MoveDetails.TopScreenEdge:
                    targetPosition = new Vector2(moveOut.positionAdjustment.x + initialData.startAnchoredPosition3D.x,
                                                 moveOut.positionAdjustment.y + yOffset);
                    break;

                case MoveDetails.RightScreenEdge:
                    targetPosition = new Vector2(moveOut.positionAdjustment.x + xOffset,
                                                 moveOut.positionAdjustment.y + initialData.startAnchoredPosition3D.y);
                    break;

                case MoveDetails.BottomScreenEdge:
                    targetPosition = new Vector2(moveOut.positionAdjustment.x + initialData.startAnchoredPosition3D.x,
                                                 moveOut.positionAdjustment.y - yOffset);
                    break;

                case MoveDetails.LeftScreenEdge:
                    targetPosition = new Vector2(moveOut.positionAdjustment.x - xOffset,
                                                 moveOut.positionAdjustment.y + initialData.startAnchoredPosition3D.y);
                    break;

                case MoveDetails.TopLeft:
                    targetPosition = new Vector2(moveOut.positionAdjustment.x - xOffset,
                                                 moveOut.positionAdjustment.y + yOffset);
                    break;

                case MoveDetails.TopCenter:
                    targetPosition = new Vector2(moveOut.positionAdjustment.x,
                                                 moveOut.positionAdjustment.y + yOffset);
                    break;

                case MoveDetails.TopRight:
                    targetPosition = new Vector2(moveOut.positionAdjustment.x + xOffset,
                                                 moveOut.positionAdjustment.y + yOffset);
                    break;

                case MoveDetails.MiddleLeft:
                    targetPosition = new Vector2(moveOut.positionAdjustment.x - xOffset,
                                                 moveOut.positionAdjustment.y);
                    break;

                case MoveDetails.MiddleCenter:
                    targetPosition = new Vector2(moveOut.positionAdjustment.x,
                                                 moveOut.positionAdjustment.y);
                    break;

                case MoveDetails.MiddleRight:
                    targetPosition = new Vector2(moveOut.positionAdjustment.x + xOffset,
                                                 moveOut.positionAdjustment.y);
                    break;

                case MoveDetails.BottomLeft:
                    targetPosition = new Vector2(moveOut.positionAdjustment.x - xOffset,
                                                 moveOut.positionAdjustment.y - yOffset);
                    break;

                case MoveDetails.BottomCenter:
                    targetPosition = new Vector2(moveOut.positionAdjustment.x,
                                                 moveOut.positionAdjustment.y - yOffset);
                    break;

                case MoveDetails.BottomRight:
                    targetPosition = new Vector2(moveOut.positionAdjustment.x + xOffset,
                                                 moveOut.positionAdjustment.y - yOffset);
                    break;

                default:
                    Debug.LogWarning("[DoozyUI] This should not happen! DoMoveOut in UIAnimator went to the default setting!");
                    return;
            }

            DoMoveOutAnimation(targetPosition, moveOut, rectTransform, initialData, instantAction);
        }

        /// <summary>
        /// This is a helper method for the DoMoveOut. It simplifies a lot the switch case for each MoveDetails
        /// </summary>
        /// <param name="position"></param>
        /// <param name="moveOut"></param>
        /// <param name="time"></param>
        /// <param name="delay"></param>
        /// <param name="rectTransform"></param>
        /// <param name="initialData"></param>
        /// <param name="instantAction"></param>
        public static void DoMoveOutAnimation(Vector2 position, MoveOut moveOut, RectTransform rectTransform, InitialData initialData, bool instantAction)
        {
            string tweenID = rectTransform.GetInstanceID() + "_DoMoveOut";

            //Kevin.Zhang, 1/17/2017
            //DOTween changes the anchored position with 1 frame delay, this may result in an UIElement flashing on screen when it is loaded at runtime
            //change the position right away
            if (instantAction)
            {
                rectTransform.anchoredPosition3D = position;
            }

            rectTransform
                .DOAnchorPos(position, instantAction ? 0 : moveOut.time, false)
                    .SetDelay(instantAction ? 0 : moveOut.delay)
                    .SetEase(moveOut.easeType)
                    .SetId(tweenID)
                    .SetUpdate(UpdateType.Normal, UIManager.isTimeScaleIndependent)
                    .OnStart(() =>
                    {
                        if (!instantAction) { UIAnimator.PlaySound(moveOut.soundAtStartReference.soundName, initialData.soundOn); }
                    })
                    .OnComplete(() =>
                    {
                        if (!instantAction) { UIAnimator.PlaySound(moveOut.soundAtFinishReference.soundName, initialData.soundOn); }
                    })
                    .Play();
        }
        #endregion

        #region RotateOUT
        /// <summary>
        /// Plays the Rotate Out animation for a RectTransform with a PanelController component added to it.
        /// </summary>
        /// <param name="rotationOut">Rotation out.</param>
        /// <param name="rectTransform">Rect transform.</param>
        public static void DoRotationOut(RotationOut rotationOut, RectTransform rectTransform, InitialData initialData, bool instantAction)
        {
            if (!rotationOut.enabled) { return; }
            string tweenID = rectTransform.GetInstanceID() + "_DoRotationOut";

			//Kevin.Zhang, 1/20/2017
            //DOTween changes the local rotation with 1 frame delay, this may result in an UIElement flashing on screen when it is loaded at runtime
            //change the local rotation right away
            if (instantAction)
            {
                rectTransform.localRotation = Quaternion.Euler(rotationOut.rotateTo);
            }
			
            rectTransform
                .DOLocalRotate(rotationOut.rotateTo, instantAction ? 0 : rotationOut.time, RotateMode.FastBeyond360)
                    .SetDelay(instantAction ? 0 : rotationOut.delay)
                    .SetEase(rotationOut.easeType)
                    .SetId(tweenID)
                    .SetUpdate(UpdateType.Normal, UIManager.isTimeScaleIndependent)
                    .OnStart(() =>
                    {
                        if (!instantAction)
                            UIAnimator.PlaySound(rotationOut.soundAtStartReference.soundName, initialData.soundOn);
                    })
                    .OnComplete(() =>
                    {
                        if (!instantAction)
                            UIAnimator.PlaySound(rotationOut.soundAtFinishReference.soundName, initialData.soundOn);
                    })
                    .Play();
        }
        #endregion

        #region ScaleOUT
        /// <summary>
        /// Plays the Scale Out animation for a RectTransform with a PanelController component added to it.
        /// </summary>
        /// <param name="scaleOut">Scale out.</param>
        /// <param name="rectTransform">Rect transform.</param>
        public static void DoScaleOut(ScaleOut scaleOut, RectTransform rectTransform, InitialData initialData, bool instantAction)
        {
            if (!scaleOut.enabled) { return; }
            string tweenID = rectTransform.GetInstanceID() + "_DoScaleOut";

            //Kevin.Zhang, 1/17/2017
            //DOTween changes the local scale with 1 frame delay, this may result in an UIElement flashing on screen when it is loaded at runtime
            //change the local scale right away
            if (instantAction)
            {
                rectTransform.localScale = scaleOut.scaleEnd;
            }

            rectTransform
                .DOScale(scaleOut.scaleEnd, instantAction ? 0 : scaleOut.time)
                    .SetDelay(instantAction ? 0 : scaleOut.delay)
                    .SetEase(scaleOut.easeType)
                    .SetId(tweenID)
                    .SetUpdate(UpdateType.Normal, UIManager.isTimeScaleIndependent)
                    .OnStart(() =>
                    {
                        if (!instantAction) { UIAnimator.PlaySound(scaleOut.soundAtStartReference.soundName, initialData.soundOn); }
                    })
                    .OnComplete(() =>
                    {
                        if (!instantAction) { UIAnimator.PlaySound(scaleOut.soundAtFinishReference.soundName, initialData.soundOn); }
                        if (rectTransform.localScale != scaleOut.scaleEnd) { rectTransform.DOScale(scaleOut.scaleEnd, 0).Play(); }
                    })
                    .Play();
        }
        #endregion

        #region FadeOUT
        /// <summary>
        /// Plays the Fade Out animation for a RectTransform with a PanelController component added to it.
        /// </summary>
        /// <param name="fadeOut">Fade out.</param>
        /// <param name="rectTransform">Rect transform.</param>
        public static void DoFadeOut(FadeOut fadeOut, RectTransform rectTransform, InitialData initialData, bool instantAction)
        {
            if (!fadeOut.enabled) { return; }
            CanvasGroup canvasGroup = rectTransform.GetComponent<CanvasGroup>();
            if (canvasGroup == null) { canvasGroup = rectTransform.gameObject.AddComponent<CanvasGroup>(); }
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            string tweenID = rectTransform.GetInstanceID() + "_DoFadeOut";

            //Kevin.Zhang, 1/17/2017
            //DOTween changes the alpha with 1 frame delay, this may result in an UIElement flashing on screen when it is loaded at runtime
            //change the alpha right away
            if (instantAction)
            {
                canvasGroup.alpha = 0f;
            }

            canvasGroup
                .DOFade(0f, instantAction ? 0 : fadeOut.time)
                    .SetDelay(instantAction ? 0 : fadeOut.delay)
                    .SetEase(fadeOut.easeType)
                    .SetId(tweenID)
                    .SetUpdate(UpdateType.Normal, UIManager.isTimeScaleIndependent)
                    .OnStart(() =>
                    {
                        if (!instantAction) { UIAnimator.PlaySound(fadeOut.soundAtStartReference.soundName, initialData.soundOn); }
                    })
                    .OnComplete(() =>
                    {
                        if (!instantAction) { UIAnimator.PlaySound(fadeOut.soundAtFinishReference.soundName, initialData.soundOn); }
                    })
                    .Play();
        }
        #endregion

        #region OUT ANIMATIONS - Stop method
        /// <summary>
        /// Stops the OUT animations on a rectTransform. This is used before any IN animations to avoid calling a show method while a hide method is still playing (without this, unexpected behaviour can occur)
        /// </summary>
        /// <param name="rectTransform">Rect transform.</param>
        public static void StopOutAnimations(RectTransform rectTransform, InitialData initialData)
        {
            if (rectTransform == null || rectTransform.GetComponent<UIElement>() == null) { return; }
            int id = rectTransform.GetInstanceID();
            DOTween.Kill(id + "_DoMoveOut");
            DOTween.Kill(id + "_DoRotationOut");
            DOTween.Kill(id + "_DoScaleOut");
            DOTween.Kill(id + "_DoFadeOut");
        }
        #endregion

        #endregion

        #region BUTTON ANIMATIONS

        #region OnClick Animations
        public static void StartOnClickAnimations(RectTransform rectTransform, InitialData initialData, UIAnimationManager.OnClickAnimations onClickAnimSettings)
        {
            if (onClickAnimSettings.punchPositionEnabled)
            {
                rectTransform.DOPunchAnchorPos(onClickAnimSettings.punchPositionPunch, onClickAnimSettings.punchPositionDuration, onClickAnimSettings.punchPositionVibrato, onClickAnimSettings.punchPositionElasticity, onClickAnimSettings.punchPositionSnapping)
                    .SetDelay(onClickAnimSettings.punchPositionDelay)
                    .SetUpdate(UpdateType.Normal, UIManager.isTimeScaleIndependent)
                    .OnComplete(() =>
                    {
                        rectTransform.DOAnchorPos3D(initialData.startAnchoredPosition3D, 0.1f)
                            .SetUpdate(UpdateType.Normal, true)
                            .Play();
                    })
                    .Play();
            }

            if (onClickAnimSettings.punchRotationEnabled)
            {
                rectTransform.DOPunchRotation(onClickAnimSettings.punchRotationPunch, onClickAnimSettings.punchRotationDuration, onClickAnimSettings.punchRotationVibrato, onClickAnimSettings.punchPositionElasticity)
                        .SetDelay(onClickAnimSettings.punchRotationDelay)
                        .SetUpdate(UpdateType.Normal, true)
                        .OnComplete(() =>
                        {
                            rectTransform.DORotate(initialData.startRotation, 0.1f)
                                .SetUpdate(UpdateType.Normal, true)
                                .Play();
                        })
                        .Play();
            }

            if (onClickAnimSettings.punchScaleEnabled)
            {
                rectTransform.DOPunchScale(onClickAnimSettings.punchScalePunch, onClickAnimSettings.punchScaleDuration, onClickAnimSettings.punchScaleVibrato, onClickAnimSettings.punchScaleElasticity)
                        .SetDelay(onClickAnimSettings.punchScaleDelay)
                        .SetUpdate(UpdateType.Normal, UIManager.isTimeScaleIndependent)
                        .OnComplete(() =>
                        {
                            rectTransform.DOScale(initialData.startScale, 0.1f)
                                .SetUpdate(UpdateType.Normal, true)
                                .Play();
                        })
                        .Play();
            }
        }
        #endregion

        #region Normal and Highlighted Animations
        /// <summary>
        /// Starts button loop animations for an UIButton
        /// </summary>
        /// <param name="rectTransform">the target rect transform of the button</param>
        /// <param name="initialData">the initial rect transfrom data (used for reset and calibration)</param>
        /// <param name="animationSettings">the animation settings variable (normal or highlighted)</param>
        public static void StartButtonLoopsAnimations(RectTransform rectTransform, InitialData initialData, UIAnimationManager.ButtonLoopsAnimations animationSettings)
        {
            if (rectTransform.GetComponent<UIButton>() == null)
            {
                Debug.LogWarning("[DoozyUI] " + rectTransform.name + " does not have a UIButton Component attached and you are trying to start a normal state animation");
                return;
            }

            int id = rectTransform.GetInstanceID();

            if (animationSettings.moveLoop.enabled)
            {
                DoMoveLoop(animationSettings.moveLoop, rectTransform, initialData);
                DOTween.Play(id + "_DoMoveLoop");
            }

            if (animationSettings.rotationLoop.enabled)
            {
                DoRotationLoop(animationSettings.rotationLoop, rectTransform, initialData);
                DOTween.Play(id + "_DoRotationLoop");
            }

            if (animationSettings.scaleLoop.enabled)
            {
                DoScaleLoop(animationSettings.scaleLoop, rectTransform, initialData);
                DOTween.Play(id + "_DoScaleLoop");
            }

            if (animationSettings.fadeLoop.enabled)
            {
                DoFadeLoop(animationSettings.fadeLoop, rectTransform, initialData, AnimationTarget.UIButton);
                DOTween.Play(id + "_DoFadeLoop");
            }
        }

        /// <summary>
        /// Stops the button loops animations for a UIButton
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="initialData"></param>
        public static void StopButtonLoopsAnimations(RectTransform rectTransform, InitialData initialData)
        {
            if (rectTransform == null || initialData == null || rectTransform.GetComponent<UIButton>() == null)
                return;

            ResetRectTransform(rectTransform, initialData);

            int id = rectTransform.GetInstanceID();

            DOTween.Kill(id + "_DoMoveLoop");
            DOTween.Kill(id + "_DoRotationLoop");
            DOTween.Kill(id + "_DoScaleLoop");
            DOTween.Kill(id + "_DoFadeLoop");
        }
        #endregion

        #endregion
    }
}

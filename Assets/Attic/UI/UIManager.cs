using System;
using System.Collections.Generic;
using System.Linq;
using Attic.DI;
using Attic.Utilities;
using UnityEngine;

#if CC_DI
#endif

#if UNITY_URP
using UnityEngine.Rendering.Universal;
#endif


namespace Attic.UI
{
    /// <summary>
    /// Used to show UIScreens and to show/hide UIWidgets.
    /// </summary>
#if CC_DI
    [Injectable]
#endif
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Camera uiCamera;

        private UIScreen[] screens;
        private UIWidget[] widgets;

        private UIScreen currentUIScreen;

        public Camera UICamera => uiCamera;
        public Canvas UICanvas { get; private set; }

        private void Awake()
        {
            Canvas[] canvasArray = GetComponentsInChildren<Canvas>();

            foreach (Canvas canvas in canvasArray)
            {
                if (!canvas.isRootCanvas)
                {
                    continue;
                }

                UICanvas = canvas;
                break;
            }

            List<UIScreen> uiScreens = new List<UIScreen>();
            List<UIWidget> uiWidgets = new List<UIWidget>();

            UIView[] uiViews = GetComponentsInChildren<UIView>(true);

            foreach (UIView uiView in uiViews)
            {
                if (uiView is UIScreen uiScreen)
                {
                    uiScreens.Add(uiScreen);
                }
                else if (uiView is UIWidget uiWidget)
                {
                    uiWidgets.Add(uiWidget);
                }

                uiView.gameObject.SetActive(false);
                uiView.Initialize(this);
            }

            screens = uiScreens.ToArray();
            widgets = uiWidgets.ToArray();
        }

        private void Update()
        {
            if (currentUIScreen != null && currentUIScreen.VisibleState == VisibleState.Shown)
            {
                currentUIScreen.Tick(Time.deltaTime);
            }

            for (int i = 0; i < widgets.Length; i++)
            {
                if (widgets[i].VisibleState == VisibleState.Shown)
                {
                    widgets[i].Tick(Time.deltaTime);
                }
            }
        }

        public void SetCameraToBase()
        {
#if UNITY_URP
			UniversalAdditionalCameraData data = uiCamera.GetComponent<UniversalAdditionalCameraData>();
			data.renderType = CameraRenderType.Base;
#endif
        }

        public void SetCameraToOverlay()
        {
#if UNITY_URP
			UniversalAdditionalCameraData data = uiCamera.GetComponent<UniversalAdditionalCameraData>();
			data.renderType = CameraRenderType.Overlay;
#endif
        }

        /// <summary>
        /// Show the screen which contains given type T as a UIScreen
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T ShowScreen<T>() where T : UIScreen
        {
            T newUIScreen = screens.FirstOrDefault(x => x.GetType() == typeof(T)) as T;

            if (newUIScreen == null)
            {
#if CC_DI
                throw Log.Exception($"Cannot find UIScreen {typeof(T).Name}. Did you add it as a child of the <b>UIManager</b>?");
#else
                throw new Exception($"Cannot find UIScreen {typeof(T).Name}. Did you add it as a child of the <b>UIManager</b>?");
#endif
            }

            if (currentUIScreen != null)
            {
                currentUIScreen.Hide();
            }

            newUIScreen.Show();
            currentUIScreen = newUIScreen;

            return newUIScreen;
        }

        /// <summary>
        /// Show the screen which contains given type T1 as a UIScreen, with custom data as T2.
        /// </summary>
        /// <param name="genericObject"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T1 ShowScreen<T1, T2>(T2 genericObject) where T1 : UIScreen<T2>
        {
            T1 newUIScreen = screens.FirstOrDefault(x => x.GetType() == typeof(T1)) as T1;

            if (newUIScreen == null)
            {
#if CC_DI
                throw Log.Exception(
                    $"Cannot find UIScreen {typeof(T1).Name}. Did you add it as a child of the <b>UIManager</b>?");
#else
                throw new Exception($"Cannot find UIScreen {typeof(T1).Name}. Did you add it as a child of the <b>UIManager</b>?");
#endif
            }

            if (currentUIScreen != null)
            {
                currentUIScreen.Hide();
            }

            newUIScreen.Show(genericObject);
            currentUIScreen = newUIScreen;

            return newUIScreen;
        }

        /// <summary>
        /// Show a widget of given type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ShowWidget<T>() where T : UIWidget
        {
            T uiWidget = widgets.FirstOrDefault(x => x.GetType() == typeof(T)) as T;

            if (uiWidget == null)
            {
                return null;
            }

            uiWidget.Show();

            return uiWidget;
        }

        /// <summary>
        /// Show a widget of given type T1, with custom data as T2.
        /// </summary>
        /// <param name="genericObject"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public T1 ShowWidget<T1, T2>(T2 genericObject) where T1 : UIWidget<T2>
        {
            T1 uiWidget = widgets.FirstOrDefault(x => x.GetType() == typeof(T1)) as T1;

            if (uiWidget == null)
            {
                return null;
            }

            uiWidget.Show(genericObject);

            return uiWidget;
        }

        /// <summary>
        /// Show a widget of given type T1, with custom data as T2 and T3.
        /// </summary>
        /// <param name="genericObject1"></param>
        /// <param name="genericObject2"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <returns></returns>
        public T1 ShowWidget<T1, T2, T3>(T2 genericObject1, T3 genericObject2) where T1 : UIWidget<T2, T3>
        {
            T1 uiWidget = widgets.FirstOrDefault(x => x.GetType() == typeof(T1)) as T1;

            if (uiWidget == null)
            {
                return null;
            }

            uiWidget.Show(genericObject1, genericObject2);

            return uiWidget;
        }

        /// <summary>
        /// Show a widget of given type T1, with custom data as T2 and T3.
        /// </summary>
        /// <param name="genericObject1"></param>
        /// <param name="genericObject2"></param>
        /// <param name="genericObject3"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <returns></returns>
        public T1 ShowWidget<T1, T2, T3, T4>(T2 genericObject1, T3 genericObject2, T4 genericObject3)
            where T1 : UIWidget<T2, T3, T4>
        {
            T1 uiWidget = widgets.FirstOrDefault(x => x.GetType() == typeof(T1)) as T1;

            if (uiWidget == null)
            {
                return null;
            }

            uiWidget.Show(genericObject1, genericObject2, genericObject3);

            return uiWidget;
        }

        /// <summary>
        /// Show a widget of given type T1, with custom data as T2 and T3.
        /// </summary>
        /// <param name="genericObject1"></param>
        /// <param name="genericObject2"></param>
        /// <param name="genericObject3"></param>
        /// <param name="genericObject4"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <returns></returns>
        public T1 ShowWidget<T1, T2, T3, T4, T5>(T2 genericObject1, T3 genericObject2, T4 genericObject3, T5 genericObject4)
            where T1 : UIWidget<T2, T3, T4, T5>
        {
            T1 uiWidget = widgets.FirstOrDefault(x => x.GetType() == typeof(T1)) as T1;

            if (uiWidget == null)
            {
                return null;
            }

            uiWidget.Show(genericObject1, genericObject2, genericObject3, genericObject4);

            return uiWidget;
        }

        /// <summary>
        /// Hide the current visible screen. There's never more than one active at any time.
        /// </summary>
        public void HideCurrentScreen()
        {
            if (!currentUIScreen)
            {
                return;
            }

            HideScreen(currentUIScreen);
        }

        /// <summary>
        /// Hide screen of type T. Screens are automatically hidden if another screen is shown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void HideScreen<T>() where T : UIScreen
        {
            UIScreen uiScreen = screens.FirstOrDefault(x => x.GetType() == typeof(T));

            HideScreen(uiScreen);
        }

        /// <summary>
        /// Hide given screen. Screens are automatically hidden if another screen is shown.
        /// </summary>
        /// <param name="uiScreen"></param>
        /// <typeparam name="T"></typeparam>
        public void HideScreen<T>(T uiScreen) where T : UIScreen
        {
            if (uiScreen == null)
            {
                return;
            }

            uiScreen.Hide();

            if (currentUIScreen == uiScreen)
            {
                currentUIScreen = null;
            }
        }

        /// <summary>
        /// Hide widget of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void HideWidget<T>() where T : UIWidget
        {
            UIWidget uiWidget = widgets.FirstOrDefault(x => x.GetType() == typeof(T));

            if (uiWidget == null)
            {
                return;
            }

            uiWidget.Hide();
        }

        /// <summary>
        /// Get a screen of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetScreen<T>() where T : UIScreen
        {
            return screens.FirstOrDefault(x => x.GetType() == typeof(T)) as T;
        }

        /// <summary>
        /// Get a widget of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetWidget<T>() where T : UIWidget
        {
            return widgets.FirstOrDefault(x => x.GetType() == typeof(T)) as T;
        }

        /// <summary>
        /// Check if screen of type T is the currently visible screen
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsCurrentScreen<T>() where T : UIScreen
        {
            if (currentUIScreen == null)
            {
                return false;
            }

            return currentUIScreen.GetType() == typeof(T);
        }
    }
}

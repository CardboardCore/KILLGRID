using Attic.DI;
using UnityEngine;

#if CC_DI
#endif

namespace Attic.UI
{
    public enum VisibleState
    {
        Shown,
        Hidden
    }

    /// <summary>
    /// Core object for UIManager to use.
    /// Extend from this class to add new ways of handling UI, much like UIScreen and UIWidget.
    ///
    /// Automatically injects and releases Injected fields.
    /// </summary>
    public abstract class UIView : MonoBehaviour
    {
        internal VisibleState VisibleState = VisibleState.Hidden;

        protected UIManager UIManager;

        protected virtual void OnInitialize() { }
        protected abstract void OnShow();
        protected virtual void OnTick(float deltaTime) { }
        protected abstract void OnHide();

        internal void Initialize(UIManager uiManager)
        {
            UIManager = uiManager;

            OnInitialize();
        }

        internal void Show()
        {
#if CC_DI
            Injector.Inject(this);
#endif

            if (VisibleState == VisibleState.Shown)
            {
                return;
            }

            VisibleState = VisibleState.Shown;

            gameObject.SetActive(true);

            OnShow();

            // Not caching UIElementViews as they can be created/destroyed dynamically
            UIViewElement[] uiViewElements = GetComponentsInChildren<UIViewElement>(true);

            for (int i = 0; i < uiViewElements.Length; i++)
            {
                uiViewElements[i].Show();
            }
        }

        internal void Hide()
        {
            if (VisibleState == VisibleState.Hidden)
            {
                return;
            }

            VisibleState = VisibleState.Hidden;

            // Not caching UIElementViews as they can be created/destroyed dynamically
            UIViewElement[] uiViewElements = GetComponentsInChildren<UIViewElement>(true);

            for (int i = 0; i < uiViewElements.Length; i++)
            {
                uiViewElements[i].Hide();
            }

            OnHide();

            gameObject.SetActive(false);

#if CC_DI
            Injector.Release(this);
#endif
        }

        internal void Tick(float deltaTime)
        {
            OnTick(deltaTime);
        }
    }
}

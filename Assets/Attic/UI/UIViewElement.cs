using Attic.DI;
using UnityEngine;

#if CC_DI
#endif

namespace Attic.UI
{
    /// <summary>
    /// Elements are objects which are children of UIViews, such as UIScreens or UIWidgets.
    /// Automatically injects and releases Injected fields.
    /// </summary>
    public abstract class UIViewElement : MonoBehaviour
    {
        internal void Show()
        {
#if CC_DI
            Injector.Inject(this);
#endif
            gameObject.SetActive(true);
            OnShow();
        }

        internal void Hide()
        {
            OnHide();
            gameObject.SetActive(false);

#if CC_DI
            Injector.Release(this);
#endif
        }

        protected abstract void OnShow();
        protected abstract void OnHide();
    }

    public abstract class UIViewElement<T> : UIViewElement
    {
        new internal void Show() { }
        protected override void OnShow() { }

        protected abstract void OnShow(T data);

        internal void Show(T data)
        {
#if CC_DI
            Injector.Inject(this);
#endif

            gameObject.SetActive(true);
            OnShow(data);
        }
    }
}

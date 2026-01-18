namespace Attic.UI
{
    /// <summary>
    /// Extend from this to create your own UI Screen.
    /// Only one UI Screen will be active at all times.
    ///
    /// Automatically injects and releases Injected fields.
    /// </summary>
    public abstract class UIScreen : UIView
    {
    }

    public abstract class UIScreen<T> : UIScreen
    {
        protected abstract void OnShow(T playerId);

        protected override void OnShow()
        {
            // This method exists to override the abstract OnShow method
        }

        internal void Show(T genericObject)
        {
            Show();
            OnShow(genericObject);
        }
    }
}

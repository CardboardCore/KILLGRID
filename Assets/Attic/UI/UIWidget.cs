namespace Attic.UI
{
    /// <summary>
    /// Extend from this to create your own UI Widget.
    /// Needs to be manually shown and hidden. Ignores UI Screen's show/hide calls.
    ///
    /// Automatically injects and releases Injected fields.
    /// </summary>
    public abstract class UIWidget : UIView
    {
    }

    public abstract class UIWidget<T> : UIWidget
    {
        protected abstract void OnShow(T data);

        protected override void OnShow()
        {
            // This method exists to override the abstract OnShow method
        }

        internal void Show(T data)
        {
            Show();
            OnShow(data);
        }
    }

    public abstract class UIWidget<T1, T2> : UIWidget
    {
        protected abstract void OnShow(T1 genericObjectOne, T2 genericObjectTwo);

        protected override void OnShow()
        {
            // This method exists to override the abstract OnShow method
        }

        internal void Show(T1 genericObjectOne, T2 genericObjectTwo)
        {
            Show();
            OnShow(genericObjectOne, genericObjectTwo);
        }
    }

    public abstract class UIWidget<T1, T2, T3> : UIWidget
    {
        protected abstract void OnShow(T1 genericObjectOne, T2 genericObjectTwo, T3 genericObjectThree);

        protected override void OnShow()
        {
            // This method exists to override the abstract OnShow method
        }

        internal void Show(T1 genericObjectOne, T2 genericObjectTwo, T3 genericObjectThree)
        {
            Show();
            OnShow(genericObjectOne, genericObjectTwo, genericObjectThree);
        }
    }

    public abstract class UIWidget<T1, T2, T3, T4> : UIWidget
    {
        protected abstract void OnShow(T1 genericObjectOne, T2 genericObjectTwo, T3 genericObjectThree, T4 genericObjectFour);

        protected override void OnShow()
        {
            // This method exists to override the abstract OnShow method
        }

        internal void Show(T1 genericObjectOne, T2 genericObjectTwo, T3 genericObjectThree, T4 genericObjectFour)
        {
            Show();
            OnShow(genericObjectOne, genericObjectTwo, genericObjectThree, genericObjectFour);
        }
    }
}

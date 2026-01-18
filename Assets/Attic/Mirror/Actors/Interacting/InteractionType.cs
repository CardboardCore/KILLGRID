namespace Attic.Mirror.Actors.Interacting
{
    public enum InteractionType
    {
        /// <summary>
        /// Begins interaction and ends it immediately. Single use purpose. Needs to be reset to interact again.
        /// </summary>
        OneOff,

        /// <summary>
        /// Toggles interaction between on and off.
        /// </summary>
        Toggle,

        /// <summary>
        /// Begins interaction once and does not end it. Needs to be forcefully stopped.
        /// </summary>
        Continuous,

        /// <summary>
        /// Begins interaction and ends it immediately.
        /// </summary>
        Single
    }
}

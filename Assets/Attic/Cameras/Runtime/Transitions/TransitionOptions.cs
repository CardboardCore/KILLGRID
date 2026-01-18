using System;

namespace Attic.Cameras.Transitions
{
    [Flags]
    public enum TransitionOptions
    {
        None = 0,
        Instant = 1 << 0,
        NotInterruptable = 1 << 1,
    }
}

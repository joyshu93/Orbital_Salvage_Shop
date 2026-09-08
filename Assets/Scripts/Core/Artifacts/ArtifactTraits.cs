using System;

namespace CurioClerk.Core.Artifacts
{
    [Flags]
    public enum ArtifactTraits
    {
        None = 0,
        Cursed = 1 << 0,
        Fragile = 1 << 1,
        Alive = 1 << 2,
        Temporal = 1 << 3,
        Wet = 1 << 4,
        Metallic = 1 << 5,
        Frosted = 1 << 6
    }
}

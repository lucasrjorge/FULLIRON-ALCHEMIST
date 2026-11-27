using System;

namespace Fulliron.Alchemy
{
    [Flags]
    public enum EffectTag
    {
        None = 0,
        Heat = 1 << 0,
        Cold = 1 << 1,
        Moist = 1 << 2,
        Dry = 1 << 3,
        Volatile = 1 << 4,
        Pure = 1 << 5,
        Corrosive = 1 << 6
    }
}

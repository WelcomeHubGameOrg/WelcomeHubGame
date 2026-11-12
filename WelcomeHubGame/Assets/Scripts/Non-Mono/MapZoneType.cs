using System;
using UnityEngine;

[Flags]
public enum MapZoneType
{
    None = 0,
    A = 1 << 0,
    B = 1 << 1,
    C = 1 << 2,
    D = 1 << 3,
    Any = A | B | C | D
}

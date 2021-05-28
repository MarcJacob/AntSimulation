using System.Runtime.InteropServices;
using UnityEngine;

namespace PerformAnts
{
    public enum AntState
    {
        EXPLORING,
        CARRYING_FOOD,
        RETREATING
    }

    /// <summary>
    /// Stored Data per active Ant.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Ant_Data
    {
        public int ix, iy;
        public float x, y;
        public float steering;
        [MarshalAs(UnmanagedType.SysInt)]
        public AntState state;
        public float timeSinceStateChange;
    }
}




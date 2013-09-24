using System;
using System.Collections.Concurrent;

namespace Game_Of_Life
{
    /// <summary>
    /// Used to pass the information about the next generation of cells when it has been calculated.
    /// </summary>
    public sealed class NextGenReadyArgs : EventArgs
    {
        public ConcurrentDictionary<int, bool> CellWorldGeneration { get; private set; }
        public int NumGenerations { get; private set; }

        public NextGenReadyArgs(ConcurrentDictionary<int, bool> cellWorldGeneration, int numGenerations)
        {
            this.CellWorldGeneration = cellWorldGeneration;
            this.NumGenerations = numGenerations;
        }
    }
}

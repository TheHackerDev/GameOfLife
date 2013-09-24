using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Game_Of_Life
{
	/// <summary>
	/// Contains all data related to the cells' existence and locations.
	/// </summary>
    public class CellWorld
	{
        /// <summary>
        /// Placeholder for tracking the total number of generations that the cells have gone through
        /// </summary>
        private int numGenerations;
        /// <summary>
        /// The number of cells in the 2d array along all sides.
        /// </summary>
        private readonly int sideLength;
        /// <summary>
        /// Random number generator that determines the existence of cells within the cellLocations array
        /// </summary>
        private readonly Random birther;
        /// <summary>
        /// The timer that will regulates the lifespan of each generation of cells
        /// </summary>
		public DispatcherTimer TicksOfLife;
        /// <summary>
        /// The current generation of cells.
        /// </summary>
        private ConcurrentDictionary<int, bool> currentGeneration;
        /// <summary>
        /// The next generation of cells.
        /// </summary>
	    private ConcurrentDictionary<int, bool> nextGeneration; 
        /// <summary>
        /// A function that takes a randomly generated double between 0 and 1 and outputs true if the number is less than 0.5
        /// </summary>
		private readonly Func<double, bool> cellExists;
        /// <summary>
        /// Occurs when the next generation of cells is ready.
        /// </summary>
        public event NextGenReadyHandler NextGenReady;
        /// <summary>
        /// The delegate to pass the information from the NextGenReady event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The args.</param>
        public delegate void NextGenReadyHandler(object sender, NextGenReadyArgs args);

	    /// <summary>
	    /// Initializes a new instance of the <see cref="CellWorld" /> class.
	    /// </summary>
	    /// <param name="seed">The seed used for Random cell generation.</param>
	    /// <param name="sideLength">Number of cells on each side of the cell array.</param>
	    /// <param name="tickTimeSeconds">The number of seconds for every tick of life; default = 1 second.</param>
	    public CellWorld(Int32 seed, int sideLength, int tickTimeSeconds=1)
		{
			this.birther = new Random(seed); // Initializes the "birther" random number generator with the seed
			this.cellExists = num => num <= 0.3; // Returns true if the number is less than 0.3 (30% chance of birth)
            this.numGenerations = 0;
            this.sideLength = sideLength > 0 ? sideLength : 10; // if the side length given is less than 0, set it to 10.

			// Initialize and populate the cellLocations 2d boolean array using the cellExists function and the random generator
            this.currentGeneration = new ConcurrentDictionary<int, bool>();
            Parallel.For(0, (sideLength * sideLength), cellNumber => this.currentGeneration[cellNumber] = this.cellExists(birther.NextDouble()));
            // Initialize and populate the next generation (all false [dead] to begin)
            this.nextGeneration = new ConcurrentDictionary<int, bool>();
            Parallel.For(0, (sideLength * sideLength), cellNumber => this.nextGeneration[cellNumber] = false);
            // TODO: Implement asynchronous timer
            // Initialize the ticksOfLife timer, and have it run the nextGeneration() method on each tick
            this.TicksOfLife = new DispatcherTimer {Interval = new TimeSpan(0, 0, 0, tickTimeSeconds, 0)};
	        this.TicksOfLife.Tick += CalculateNextGeneration; // Have the CalculateNextGeneration() method run on every timer tick
            this.TicksOfLife.Start(); // Start the ticker
		}

        /// <summary>
        /// Calculates the next generation of cells.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void CalculateNextGeneration(object sender, EventArgs e)
		{
			// Iterate through all cells and calculate the next generation
            Parallel.For(0, (sideLength * sideLength), cellNumber =>
                {
                    this.nextGeneration[cellNumber] = CheckSurroundingCells(cellNumber); // returns true if cell lives into next generation.
                });
            // Check if the 2 cell generations match up; if so, stop the timer, they're not going to change anymore.
            bool cellGenerationsEqual = true;
            Parallel.For(0, (sideLength*sideLength), (key, loopState) =>
                {
                    if (!currentGeneration[key].Equals(nextGeneration[key]))
                    {
                        cellGenerationsEqual = false;
                        loopState.Stop(); // stop the parallel loop using the passed-in "ParallelLoopState" object
                    }
                });

            if (cellGenerationsEqual)
                this.TicksOfLife.Stop();
            else
            {
                // Make the next generation of cells the current one, and reset the 2d array that holds the next generation of cells
                this.currentGeneration = this.nextGeneration;
                this.nextGeneration = new ConcurrentDictionary<int, bool>();
                this.numGenerations++; // Increment the number of generations that this CellWorld has gone through
                this.OnNextGenReady(currentGeneration, numGenerations); // Raise the event notifying any listeners that the "CurrentGeneration" public field has changed
            }
		}

        /// <summary>
        /// Calculates the cell location asynchronously.
        /// </summary>
        /// <param name="location">The location of the cell in the CellWorld</param>
        /// <returns>
        /// True if the cell lives on to the next generation
        /// </returns>
		private bool CheckSurroundingCells(int location)
		{
			int neighbors = 0; // a placeholder that keeps track of the number of alive neighbors for the current cell
            bool rightSide = ((location + 1) % sideLength == 0 );
            bool leftSide = (location%sideLength == 0);
			
			// Sides
            if ((!rightSide || !leftSide) && currentGeneration.ContainsKey(location + 1) && currentGeneration[location + 1])
                neighbors++;
            else if (rightSide && currentGeneration[location + 1 - sideLength])
                neighbors++;
            if ((!rightSide || !leftSide) && currentGeneration.ContainsKey(location - 1) && currentGeneration[location - 1])
                neighbors++;
            else if (leftSide && currentGeneration[location - 1 + sideLength])
                neighbors++;
            // Top & bottom
            if (currentGeneration.ContainsKey(location + sideLength) && currentGeneration[location + sideLength])
                neighbors++;
            if (currentGeneration.ContainsKey(location - sideLength) && currentGeneration[location - sideLength])
                neighbors++;
            // Touching corners
            if (!rightSide && currentGeneration.ContainsKey(location + sideLength + 1) && currentGeneration[location + sideLength + 1])
                neighbors++;
            else if (rightSide && currentGeneration.ContainsKey(location + 1) && currentGeneration[location + 1])
                neighbors++;
            if (!rightSide && currentGeneration.ContainsKey(location - sideLength + 1) && currentGeneration[location - sideLength + 1])
                neighbors++;
            else if (rightSide && currentGeneration.ContainsKey(location - (sideLength * 2) + 1) && currentGeneration[location - (sideLength * 2) + 1])
                neighbors++;
            if (!leftSide && currentGeneration.ContainsKey(location + sideLength - 1) && currentGeneration[location + sideLength - 1])
                neighbors++;
            else if (leftSide && currentGeneration.ContainsKey(location + (sideLength * 2) - 1) && currentGeneration[location + (sideLength * 2) - 1])
                neighbors++;
            if (!leftSide && currentGeneration.ContainsKey(location - sideLength - 1) && currentGeneration[location - sideLength - 1])
                neighbors++;
            else if (leftSide && currentGeneration.ContainsKey(location - 1) && currentGeneration[location - 1])
                neighbors++;
            // Top & Bottom wrap-around
            if ((location + sideLength) > (sideLength * sideLength) && currentGeneration[(location + sideLength) - (sideLength * sideLength)])
                neighbors++;
            if ((location - sideLength) < 0 && currentGeneration[(location - sideLength) + (sideLength * sideLength)])
                neighbors++;
            if ((location + sideLength + 1) > (sideLength * sideLength) && currentGeneration[(location + sideLength + 1) - (sideLength * sideLength)])
                neighbors++;
            if ((location + sideLength - 1) > (sideLength * sideLength) && currentGeneration[(location + sideLength - 1) - (sideLength * sideLength)])
                neighbors++;
            if ((location - sideLength + 1) < 0 && currentGeneration[(location - sideLength + 1) + (sideLength * sideLength)])
                neighbors++;
            if ((location - sideLength - 1) < 0 && currentGeneration[(location - sideLength - 1) + (sideLength * sideLength)])
                neighbors++;

            return Judgement(this.currentGeneration[location], neighbors); // returns true if the cell lives on to the next generation
		}

        /// <summary>
        /// Determines whether the cell lives or dies based on a set of rules.
        /// Rules:
        /// 1. Any live cell with fewer than two live neighbours dies, as if caused by under-population.
        /// 2. Any live cell with two or three live neighbours lives on to the next generation.
        /// 3. Any live cell with more than three live neighbours dies, as if by overcrowding.
        /// 4. Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
        /// </summary>
        /// <param name="isAlive">If set to <c>true</c>, the cell is currently living.</param>
        /// <param name="neighbors">The maximum number of neighbors for the cell.</param>
        /// <returns><c>true</c> if the cell will live on to the next generation.</returns>
        private bool Judgement(bool isAlive, int neighbors)
        {
            bool lifeStatus; // True if the cell will live to the next generation
            if (isAlive)
            {
                if (neighbors > 3)
                    lifeStatus = false;
                else if (neighbors >= 2)
                    lifeStatus = true;
                else
                    lifeStatus = false;
            }
            else
                lifeStatus = (neighbors == 3);
            
            return lifeStatus;
        }

        /// <summary>
        /// Called when the next generation of cells is ready.
        /// </summary>
        /// <param name="nextGenCells">The next generation of cells.</param>
        /// <param name="numGenerationsTotal">The current number of cell generations.</param>
        private void OnNextGenReady(ConcurrentDictionary<int, bool> nextGenCells, int numGenerationsTotal)
        {
            if (NextGenReady != null)
                NextGenReady(this, new NextGenReadyArgs(nextGenCells, numGenerationsTotal));
        }
	}
}

// TODO: Make UI prettier
// TODO: Add pause button (switches from "pause" to "start" on click as well)
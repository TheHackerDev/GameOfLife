using System;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace Game_Of_Life
{
    /// <summary>
    /// This class is in charge of communicating between the UI and the Cell World.
    /// </summary>
    public sealed class Viewmodel: INotifyPropertyChanged
    {
        /// <summary>
        /// The cell world in use, containing all the cells and data.
        /// </summary>
        private CellWorld cellWorld;
        /// <summary>
        /// Used to generate a random seed to pass into the CellWorld constructor
        /// </summary>
        private readonly Random seedGenerator;
        /// <summary>
        /// The side lengths for the cell world's dimensions.
        /// </summary>
        private readonly int sideLength;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
        /// <summary>
        /// Property for the cell locations.
        /// </summary>
        /// <value>
        /// The cell locations.
        /// </value>
        public ConcurrentDictionary<int, bool> CellLocations { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore MemberCanBePrivate.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
        /// <summary>
        /// Property for the number of generations for the current Cell World.
        /// </summary>
        /// <value>
        /// The number of generations for the current Cell World.
        /// </value>
        public string NumGenerations { get; set; }
        /// <summary>
        /// Value to send to the UI for when the CellWorld is paused.
        /// </summary>
        /// <value>
        /// "Start" when paused; "Stop" when running.
        /// </value>
        public string StartOrStopStatus { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore MemberCanBePrivate.Global
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Viewmodel"/> class.
        /// </summary>
        /// <param name="sideLength">Side lengths (uniform) for the dimensions of the cell world..</param>
        public Viewmodel(int sideLength)
        {
            this.sideLength = sideLength;
            this.seedGenerator = new Random();
            this.cellWorld = new CellWorld(seedGenerator.Next(int.MaxValue), sideLength); // Create a new CellWorld with a random int that is no bigger than the max allowed (prevent buffer overflow)
            cellWorld.NextGenReady += OnNextGenReady;
            this.StartOrStopStatus = "Stop";
            // Notify the UI that the property has changed
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("StartOrStopStatus"));
            }
        }

        /// <summary>
        /// Called when the next generation of cells has been calculated.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">NextGenReadyArgs Arguments.</param>
        private void OnNextGenReady(object sender, NextGenReadyArgs args)
        {
            this.CellLocations = args.CellWorldGeneration;
            // ReSharper disable SpecifyACultureInStringConversionExplicitly
            this.NumGenerations = args.NumGenerations.ToString();
            // ReSharper restore SpecifyACultureInStringConversionExplicitly

            // Notify the view that the "CellLocations" and "NumGenerations" properties have changed.
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("CellLocations"));
                PropertyChanged(this, new PropertyChangedEventArgs("NumGenerations"));
            }
        }

        /// <summary>
        /// Creates a new instance of the Cell World class to replace the current one.
        /// </summary>
        public void NewCellFamily()
        {
            this.cellWorld.NextGenReady -= OnNextGenReady;
            this.cellWorld = new CellWorld(seedGenerator.Next(int.MaxValue), sideLength);
            this.cellWorld.NextGenReady += OnNextGenReady;
        }

        /// <summary>
        /// Starts the stop the timer in the current CellWorld instance.
        /// </summary>
        public void StartStopTicks()
        {
            if (this.cellWorld.TicksOfLife.IsEnabled)
            {
                this.cellWorld.TicksOfLife.Stop();
                this.StartOrStopStatus = "Start"; // set the button text to "Start" again
                // Notify the UI that the property has changed
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("StartOrStopStatus"));
                }
            }
            else
            {
                this.cellWorld.TicksOfLife.Start();
                this.StartOrStopStatus = "Stop"; // set the button text to "Stop"
                // Notify the UI that the property has changed
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("StartOrStopStatus"));
                }
            }
        }
    }
}

// TODO: Create and implement a "Command" class that implements the ICommand interface to allow data binding from the UI back to the viewmodel via the buttons.
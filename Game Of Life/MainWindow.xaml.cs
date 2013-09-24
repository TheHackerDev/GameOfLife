using System.Windows;

namespace Game_Of_Life
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly int sideLength;
        private readonly Viewmodel viewModel;
        public MainWindow()
        {
            InitializeComponent();
            this.sideLength = 50;
            this.viewModel = new Viewmodel(sideLength);
            this.DataContext = viewModel;
        }

        private void NewCells(object sender, RoutedEventArgs e)
        {
            // TODO: Switch this to a Command
            this.viewModel.NewCellFamily();
        }

        private void StartStopCellWorld(object sender, RoutedEventArgs e)
        {
            // TODO: Switch this to a command
            this.viewModel.StartStopTicks();
        }
    }
}
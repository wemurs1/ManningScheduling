using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Scheduling
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private PoSorter Sorter = new PoSorter();

        private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.DefaultExt = ".po";
                dialog.Filter = "Partial Ordering Files|*.po|All Files|*.*";

                // Display the dialog.
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    // Load the data.
                    Sorter.LoadPoFile(dialog.FileName);

                    // Build the PERT chart.
                    Sorter.BuildPertChart();

                    // Draw the PERT chart.
                    Sorter.DrawPertChart(mainCanvas);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TestSort_Command(object sender, RoutedEventArgs e)
        {
            try
            {
                Sorter.TopoSort();
                MessageBox.Show(Sorter.VerifySort());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        private void ExitCommand_Executed(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

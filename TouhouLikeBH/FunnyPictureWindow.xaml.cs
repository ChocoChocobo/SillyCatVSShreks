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
using System.Windows.Shapes;

namespace TouhouLikeBH
{
    /// <summary>
    /// Interaction logic for FunnyPictureWindow.xaml
    /// </summary>
    public partial class FunnyPictureWindow : Window
    {
        public FunnyPictureWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is GameWindow gameWindow)
                {
                    gameWindow.Close();
                    break; // Exit the loop after closing the first GameWindow
                }
            }

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            Close();
        }
    }
}

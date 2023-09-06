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
    /// Interaction logic for GameOverWindow.xaml
    /// </summary>
    public partial class GameOverWindow : Window
    {
        public GameOverWindow()
        {
            InitializeComponent();
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            // Set the dialog result to true to indicate that the user clicked "Restart"
            DialogResult = true;

            // Close the game over window
            Close();
        }

        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            // Find and close the GameWindow (if it's open)
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

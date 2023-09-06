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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TouhouLikeBH
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        private bool isGameInProgress = true;

        private Enemy enemy;

        private DispatcherTimer bulletTimer;
        private DispatcherTimer gameLoopTimer;
        private DispatcherTimer playerBulletTimer;
        private double playerBulletSpeed = 5;

        private int characterHealth = 3;

        public GameWindow()
        {
            InitializeComponent();

            GameCanvas.MouseMove += GameCanvas_MouseMove;
            GameCanvas.MouseLeave += GameCanvas_MouseLeave;

            enemy = new Enemy(GameCanvas, EnemyHealthIndicator);

            bulletTimer = new DispatcherTimer();
            bulletTimer.Interval = TimeSpan.FromSeconds(1);
            bulletTimer.Tick += BulletTimer_Tick!;
            bulletTimer.Start();

            UpdateHealthIndicator();

            gameLoopTimer = new DispatcherTimer();
            gameLoopTimer.Interval = TimeSpan.FromMilliseconds(16); // 60 FPS
            gameLoopTimer.Tick += GameLoopTimer_Tick;
            gameLoopTimer.Start();

            playerBulletTimer = new DispatcherTimer();
            playerBulletTimer.Interval = TimeSpan.FromSeconds(0.5);
            playerBulletTimer.Tick += PlayerBulletTimer_Tick;
            playerBulletTimer.Start();
        }

        private void GameLoopTimer_Tick(object sender, EventArgs e)
        {
            if (!isGameInProgress)
            {
                return; // Don't create bullets when the game is paused
            }

            List<UIElement> childrenCopy = GameCanvas.Children.OfType<UIElement>().ToList();

            // Update the enemy and its projectiles
            enemy.Update();

            // Check for collision between player's bullets and the enemy
            List<UIElement> playerBulletsToRemove = new List<UIElement>();
            foreach (UIElement element in childrenCopy)
            {
                if (element is Ellipse ellipse && ellipse.Tag as string == "PlayerBullet")
                {
                    Ellipse playerBullet = (Ellipse)element;
                    Rect playerBulletRect = new Rect(Canvas.GetLeft(playerBullet), Canvas.GetTop(playerBullet), playerBullet.Width, playerBullet.Height);

                    Rect enemyRect = new Rect(enemy.Left, enemy.Top, enemy.Width, enemy.Height);

                    if (playerBulletRect.IntersectsWith(enemyRect))
                    {
                        // Collision between player's bullet and enemy
                        playerBulletsToRemove.Add(playerBullet);
                        enemy.TakeDamage(10); // Subtract ??? HP from the enemy
                        if (enemy.enemyHealth <= 0)
                        {
                            ShowFunnyPictureAndCloseWindows();

                        }
                    }
                }
            }

            // Remove player's bullets that hit the enemy
            foreach (UIElement bullet in playerBulletsToRemove)
            {
                GameCanvas.Children.Remove(bullet);
            }

            // Check for collision between enemy bullets and the player
            List<UIElement> enemyBulletsToRemove = new List<UIElement>();
            foreach (UIElement element in childrenCopy)
            {
                if (element is Ellipse ellipse && ellipse.Tag as string == "EnemyProjectile")
                {
                    Ellipse enemyBullet = (Ellipse)element;
                    Rect enemyBulletRect = new Rect(Canvas.GetLeft(enemyBullet), Canvas.GetTop(enemyBullet), enemyBullet.Width, enemyBullet.Height);

                    Rect playerRect = new Rect(Canvas.GetLeft(PlayerCharacter), Canvas.GetTop(PlayerCharacter), PlayerCharacter.Width, PlayerCharacter.Height);

                    if (enemyBulletRect.IntersectsWith(playerRect))
                    {
                        // Collision between enemy bullet and player
                        enemyBulletsToRemove.Add(enemyBullet);
                        characterHealth--; // Subtract health from the player
                        UpdateHealthIndicator();

                        if (characterHealth <= 0)
                        {
                            // Handle player defeat
                            GameOver();
                        }
                    }
                }
            }

            // Remove enemy bullets that hit the player
            foreach (UIElement bullet in enemyBulletsToRemove)
            {
                GameCanvas.Children.Remove(bullet);
            }
        }


        private void PlayerBulletTimer_Tick(object sender, EventArgs e)
        {
            // Player bullets creation
            Ellipse playerBullet = new Ellipse
            {
                Width = 20,
                Height = 20,
                Tag = "PlayerBullet" // Tag for collision detection
            };

            // Setting image source of the ellipse to a picture because at the start of making this project 
            // I did not think about adding any visuals so I just filled ellipses with images xd
            ImageBrush bulletBrush = new ImageBrush();
            bulletBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/TouhouLikeBH;component/Resources/bulletsPlayer.png"));

            // Set the player bullet's Fill to the ImageBrush
            playerBullet.Fill = bulletBrush;

            // Set the bullet's initial position to the character's position
            double playerLeft = Canvas.GetLeft(PlayerCharacter) + (PlayerCharacter.Width / 2) - (playerBullet.Width / 2);
            double playerTop = Canvas.GetTop(PlayerCharacter) - playerBullet.Height;

            Canvas.SetLeft(playerBullet, playerLeft);
            Canvas.SetTop(playerBullet, playerTop);

            // Add the player bullet to the canvas
            GameCanvas.Children.Add(playerBullet);

            // Move the player bullet upwards
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = playerTop;
            animation.To = -playerBullet.Height; // Adjusting the end position
            animation.Duration = TimeSpan.FromSeconds(playerBulletSpeed);

            playerBullet.BeginAnimation(Canvas.TopProperty, animation);

            animation.Completed += (bulletSender, bulletArgs) =>
            {
                // Remove the player bullet when it reaches the top
                GameCanvas.Children.Remove(playerBullet);
            };
        }

        private void UpdateHealthIndicator()
        {
            if (characterHealth < 0)
            {
                characterHealth = 0;
            }

            string healthString = new string('❤', characterHealth);

            HealthIndicator.Text = healthString;
        }

        private void BulletTimer_Tick(object sender, EventArgs e)
        {
            GenerateRandomBullets();
        }

        private void GameCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(GameCanvas);

            // Calculating the character's potential new position
            double newLeft = mousePosition.X - PlayerCharacter.Width / 2;
            double newTop = mousePosition.Y - PlayerCharacter.Height / 2;

            // Check for collision with bullets
            foreach (UIElement element in GameCanvas.Children)
            {
                if (element is Ellipse && element != PlayerCharacter)
                {
                    Ellipse bullet = (Ellipse)element;

                    if (bullet.Tag as string == "PlayerBullet")
                    {
                        continue;
                    }

                    Rect bulletRect = new Rect(Canvas.GetLeft(bullet), Canvas.GetTop(bullet), bullet.Width, bullet.Height);
                    Rect characterRect = new Rect(newLeft, newTop, PlayerCharacter.Width, PlayerCharacter.Height);

                    if (bulletRect.IntersectsWith(characterRect))
                    {
                        // A collision occurred, decrease character's health
                        characterHealth--;
                        UpdateHealthIndicator();

                        // Remove the bullet from the canvas
                        GameCanvas.Children.Remove(bullet);

                        // Exit the loop since we don't need to check other bullets
                        break;
                    }
                }
            }

            // Check if the character has run out of health
            if (characterHealth <= 0)
            {
                GameOver();
                return; // Exit the method to prevent further character movement
            }

            // Check for collision with the left and top borders
            if (newLeft < 0)
            {
                newLeft = 0;
            }
            if (newTop < 0)
            {
                newTop = 0;
            }

            // Check for collision with the right and bottom borders
            double maxLeft = GameCanvas.ActualWidth - PlayerCharacter.Width;
            double maxTop = GameCanvas.ActualHeight - PlayerCharacter.Height;
            if (newLeft > maxLeft)
            {
                newLeft = maxLeft;
            }
            if (newTop > maxTop)
            {
                newTop = maxTop;
            }

            Cursor = Cursors.None;
            Canvas.SetLeft(PlayerCharacter, newLeft);
            Canvas.SetTop(PlayerCharacter, newTop);
        }

        private void GameCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        private void GenerateRandomBullets() // On the screen
        {
            Random random = new Random();

            // Random number of bullets
            int numBullets = random.Next(5, 10);

            for (int i = 0; i < numBullets; i++)
            {
                double startX = random.NextDouble() * GameCanvas.ActualWidth;
                double startY = -20; // Start above the game area
                double speed = random.Next(3, 8); // Speed of projectiles

                // Creation of random bullets on screen
                Ellipse bullet = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Tag = "Bullet"
                };

                ImageBrush randomBulletBrush = new ImageBrush();
                randomBulletBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/TouhouLikeBH;component/Resources/randomBullet.png"));

                // Set the player bullet's Fill to the ImageBrush
                bullet.Fill = randomBulletBrush;

                // Set the bullet's initial position
                Canvas.SetLeft(bullet, startX);
                Canvas.SetTop(bullet, startY);

                GameCanvas.Children.Add(bullet);

                MoveBullet(bullet, startY, speed);
            }
        }

        private void MoveBullet(Ellipse bullet, double startY, double speed)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = startY;
            animation.To = GameCanvas.ActualHeight;
            animation.Duration = TimeSpan.FromSeconds(speed);

            bullet.BeginAnimation(Canvas.TopProperty, animation);

            animation.Completed += (sender, e) =>
            {
                // Remove the bullet when it reaches the bottom
                GameCanvas.Children.Remove(bullet);
            };
        }

        private void GameOver()
        {
            isGameInProgress = false;
            // Create an instance of the game over window
            GameOverWindow gameOverWindow = new GameOverWindow();
            
            // Show the game over window and wait for it to close
            if (gameOverWindow.ShowDialog() == true)
            {
                // The user clicked "Restart" in the game over window
                ResetGame();
                isGameInProgress = true;// Restart the game
            }
            else
            {
                // The user clicked "Main Menu" in the game over window
                Close(); // Close the GameWindow
            }
        }

        private void ShowFunnyPictureAndCloseWindows()
        {
            isGameInProgress = false;

            FunnyPictureWindow funnyPictureWindow = new FunnyPictureWindow();
            funnyPictureWindow.ShowDialog();

            Close();
        }

        private void ResetGame()
        {
            // Reset character's health and health indicator
            characterHealth = 3;
            UpdateHealthIndicator();
            enemy.ResetHealth();

            // Remove existing bullets and enemy projectiles from the canvas
            List<UIElement> elementsToRemove = new List<UIElement>();

            foreach (UIElement element in GameCanvas.Children)
            {
                if (element is Ellipse ellipse)
                {
                    string tag = ellipse.Tag as string;
                    if (tag == "Bullet" || tag == "EnemyProjectile" || tag == "PlayerBullet")
                    {
                        // Add the element to the list to be removed
                        elementsToRemove.Add(ellipse);
                    }
                }
            }
            foreach (UIElement element in elementsToRemove)
            {
                GameCanvas.Children.Remove(element);
            }
        }        
    }
}

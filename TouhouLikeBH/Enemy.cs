using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TouhouLikeBH
{
    public class Enemy
    {
        private readonly Canvas gameCanvas;
        private readonly Random random;
        private readonly DispatcherTimer moveTimer;
        private readonly DispatcherTimer shootTimer;
        private readonly TextBlock enemyHealthIndicator; // Enemy`s health textblock
        private readonly Ellipse enemyEllipse; // Enemy`s model

        private double enemyLeft;
        private double enemyTop;
        private double enemyDirection = 1; // 1 for right, -1 for left
        private double enemySpeed = 2;
        public int enemyHealth = 200;

        private readonly List<Ellipse> enemyProjectiles;
        private readonly double projectileSpeed = 7;

        public double Left
        {
            get { return enemyLeft; }
        }

        public double Top
        {
            get { return enemyTop; }
        }

        public double Width
        {
            get { return enemyEllipse.Width; }
        }

        public double Height
        {
            get { return enemyEllipse.Height; }
        }

        public Enemy(Canvas canvas, TextBlock healthIndicator)
        {
            gameCanvas = canvas;
            random = new Random();
            enemyHealthIndicator = healthIndicator;

            // Creation of enemy model
            enemyEllipse = new Ellipse
            {
                Width = 100,
                Height = 60,
                //Fill = Brushes.Gold,
                Tag = "Enemy"
            };

            ImageBrush enemyBrush = new ImageBrush();
            enemyBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/TouhouLikeBH;component/Resources/enemy.png"));

            // Set the player bullet's Fill to the ImageBrush
            enemyEllipse.Fill = enemyBrush;

            enemyProjectiles = new List<Ellipse>();

            moveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10) // Mov speed
            };
            moveTimer.Tick += MoveTimer_Tick;

            shootTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2) // Shooting interval
            };
            shootTimer.Tick += ShootTimer_Tick;

            // Add the enemy ellipse to the game canvas
            gameCanvas.Children.Add(enemyEllipse);

            // Initialize enemy position at the top center of the game area
            enemyLeft = (gameCanvas.ActualWidth - enemyEllipse.Width) / 2;
            enemyTop = 0;

            moveTimer.Start();
            shootTimer.Start();
        }

        private void MoveTimer_Tick(object sender, EventArgs e)
        {
            // Move the enemy left and right within the game area
            enemyLeft += enemyDirection * enemySpeed;

            // Check if the enemy is at the left or right border
            if (enemyLeft < 0)
            {
                enemyLeft = 0;
                enemyDirection = 1; // Change direction to right
            }
            else if (enemyLeft > gameCanvas.ActualWidth - enemyEllipse.Width)
            {
                enemyLeft = gameCanvas.ActualWidth - enemyEllipse.Width;
                enemyDirection = -1; // Change direction to left
            }

            // Update the enemy's position
            Canvas.SetLeft(enemyEllipse, enemyLeft);
            Canvas.SetTop(enemyEllipse, enemyTop);
        }

        private void ShootTimer_Tick(object sender, EventArgs e)
        {
            CreateEnemyProjectile();
        }

        private void CreateEnemyProjectile()
        {
            // Create a new projectile ellipse
            Ellipse projectile = new Ellipse
            {
                Width = 50,
                Height = 50,
                //Fill = Brushes.Yellow,
                Tag = "EnemyProjectile"
            };

            ImageBrush enemyBulletBrush = new ImageBrush();
            enemyBulletBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/TouhouLikeBH;component/Resources/enemyBullet.png"));

            projectile.Fill = enemyBulletBrush;

            // Set the initial position of the projectile to the enemy's position
            double projectileLeft = enemyLeft + (enemyEllipse.Width / 2) - (projectile.Width / 2);
            double projectileTop = enemyTop + enemyEllipse.Height;

            Canvas.SetLeft(projectile, projectileLeft);
            Canvas.SetTop(projectile, projectileTop);

            // Add the projectile to the game canvas
            gameCanvas.Children.Add(projectile);

            // Store the projectile in the list
            enemyProjectiles.Add(projectile);
        }

        public void TakeDamage(int damage)
        {
            // Subtract the specified damage from the enemy's health
            enemyHealth -= damage;

            // Update the enemy health indicator
            UpdateEnemyHealthIndicator();
        }

        private void UpdateEnemyHealthIndicator()
        {
            string healthString = "Enemy Health: " + enemyHealth;

            enemyHealthIndicator.Text = healthString;
        }

        public void ResetHealth()
        {
            enemyHealth = 200;
            UpdateEnemyHealthIndicator();
        }

        public void Update()
        {
            // Update the position of enemy projectiles and remove any that are out of bounds
            for (int i = enemyProjectiles.Count - 1; i >= 0; i--)
            {
                Ellipse projectile = enemyProjectiles[i];
                double top = Canvas.GetTop(projectile);
                top += projectileSpeed;

                // Update the position of the projectile
                Canvas.SetTop(projectile, top);

                // Remove projectiles that are out of bounds
                if (top > gameCanvas.ActualHeight)
                {
                    enemyProjectiles.RemoveAt(i);
                    gameCanvas.Children.Remove(projectile);
                }
            }
        }        
    }
}

using Microsoft.SqlServer.Server;
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
using WpfApp1;

namespace kibers
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool IsAdmin { get; set; } = false;
        public MainWindow()
        {
            InitializeComponent();

        }



        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();
        }

        private void admin_Click(object sender, RoutedEventArgs e)
        {

        }

        private void registration_Click(object sender, RoutedEventArgs e)
        {
            // Создаем экземпляр нового окна
            RegistrationForm newWindow = new RegistrationForm();

            // Показываем новое окно
            newWindow.Show();

            // Закрываем текущее окно
            this.Close();
        }

        private void spravka_Click(object sender, RoutedEventArgs e)
        {
            // Создаем экземпляр нового окна
            AboutUs newWindow = new AboutUs();

            // Показываем новое окно
            newWindow.Show();

            // Закрываем текущее окно
            this.Close();

        }

        private void prise_Click(object sender, RoutedEventArgs e)
        { // Создаем экземпляр нового окна
           PriseForm newWindow = new PriseForm();

            // Показываем новое окно
            newWindow.Show();

            // Закрываем текущее окно
            this.Close();

        }
      


    }
}





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

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для RoomsGalleryWindow.xaml
    /// </summary>
    public partial class RoomsGalleryWindow : Window
    {
        public class RoomInfo
        {
            public string ImagePath { get; set; }
            public string RoomNumber { get; set; }
            public string Description { get; set; }
            public string Capacity { get; set; }
        }

        public RoomsGalleryWindow()
        {
            InitializeComponent();
            LoadRooms();
        }

        private void LoadRooms()
        {
            var rooms = new List<RoomInfo>
            {
                new RoomInfo
                {
                    ImagePath = "Images/common.jpg",
                    RoomNumber = "Комната 1",
                    Description = "Малая комната",
                    Capacity = "Вместимость: 5 человек"
                },
                new RoomInfo
                {
                    ImagePath = "Images/641c1212f4acbd85660ce956_642553baee3017.92152189.jpg",
                    RoomNumber = "Комната 2",
                    Description = "Средняя комната",
                    Capacity = "Вместимость: 10 человек"
                },
                new RoomInfo
                {
                    ImagePath = "Images/901777e65527510149e444a1342b12bb.jpg",
                    RoomNumber = "Комната 3",
                    Description = "Большая комната",
                    Capacity = "Вместимость: 15 человек"
                },
                new RoomInfo
                {
                    ImagePath = "Images/large_80.jpg",
                    RoomNumber = "Комната 4",
                    Description = "VIP-комната",
                    Capacity = "Вместимость: 20 человек"
                },
                new RoomInfo
                {
                    ImagePath = "Images/orig.jpg",
                    RoomNumber = "Комната 5",
                    Description = "Комната с Playstation",
                    Capacity = "Вместимость: 8 человек"
                },
                new RoomInfo
                {
                    ImagePath = "Images/r_1800504_polf54lie9vsfkui_1626099101.jpg",
                    RoomNumber = "Комната 6",
                    Description = "Комната с Xbox",
                    Capacity = "Вместимость: 12 человек"
                },
                new RoomInfo
                {
                    ImagePath = "Images/img_1844.jpg",
                    RoomNumber = "Комната 7",
                    Description = "Комната с VR",
                    Capacity = "Вместимость: 6 человек"
                },
                new RoomInfo
                {
                    ImagePath = "Images/common (1).jpg",
                    RoomNumber = "Комната 8",
                    Description = "VIP-комната для двоих",
                    Capacity = "Вместимость: 4 человека"
                },
                new RoomInfo
                {
                    ImagePath = "Images/5fabe102cabcc704fc13e642_64954d975146a7.38485425 (1).jpg",
                    RoomNumber = "Комната 9",
                    Description = "Зал для киберспорта",
                    Capacity = "Вместимость: 25 человек"
                },
                new RoomInfo
                {
                    ImagePath = "Images/Cyber-Isand2-e1599124579757.jpg",
                    RoomNumber = "Комната 10",
                    Description = "Комната с проектором",
                    Capacity = "Вместимость: 7 человек"
                }
            };

            RoomsGallery.ItemsSource = rooms;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            UserForm userForm = new UserForm();
            userForm.Show();
            this.Close();
        }
    }
}
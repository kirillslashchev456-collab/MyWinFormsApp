using System;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using kibers;

namespace WpfApp1
{
    public partial class PriseForm : Window, INotifyPropertyChanged
    {
        public PriseForm()
        {
            InitializeComponent();
            DataContext = this;
            LoadPriceListData();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private List<PriceListItem> _priceListItems;
        public List<PriceListItem> PriceListItems
        {
            get => _priceListItems;
            set
            {
                _priceListItems = value;
                OnPropertyChanged();
            }
        }

        private void LoadPriceListData()
        {
            PriceListItems = new List<PriceListItem>
            {
                // Тарифы
                new PriceListItem("Стандартный", "Обычный тариф", 150),
                new PriceListItem("VIP", "Повышенный комфорт", 300),
                new PriceListItem("Геймерский", "Мощные компьютеры", 200),
                new PriceListItem("Студенческий", "Скидка для студентов", 100),
                new PriceListItem("Дневной", "Скидка в дневное время", 120),
                new PriceListItem("Ночной", "Скидка в ночное время", 80),
                new PriceListItem("Премиум", "Максимальный комфорт", 350),
                new PriceListItem("Бронзовый", "Базовый тариф", 90),
                new PriceListItem("Серебряный", "Улучшенный тариф", 170),
                new PriceListItem("Золотой", "Премиальный тариф, но чуть хуже чем \"Премиум\"", 250),

             

                // Дополнительные услуги
                new PriceListItem("Напитки", "Различные напитки", 100),
                new PriceListItem("Закуски", "Чипсы, орешки и т.д.", 150),
                new PriceListItem("Аренда наушников", "Профессиональные наушники", 50),
                new PriceListItem("Аренда геймпада", "Геймпад для консоли", 70),
                new PriceListItem("VIP-обслуживание", "Персональный менеджер", 500),
                new PriceListItem("Услуги печати", "Печать документов", 20),
                new PriceListItem("Копирование документов", "Копирование документов", 10),
                new PriceListItem("Сканирование документов", "Сканирование документов", 30),
                new PriceListItem("Аренда VR-шлема", "VR-шлем", 200),
                new PriceListItem("Игровой коучинг", "Обучение от профессионала", 300)
            };
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow newWindow = new MainWindow();
            newWindow.Show();
            this.Close();
        }
    }

    public class PriceListItem
    {
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        public PriceListItem(string serviceName, string description, decimal price)
        {
            ServiceName = serviceName;
            Description = description;
            Price = price;
        }
    }
}
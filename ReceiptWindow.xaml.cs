using kibers;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfApp1
{
    public partial class ReceiptWindow : Window
    {
        public CartItem Item { get; set; }

        public ReceiptWindow(CartItem item)
        {
            InitializeComponent();
            Item = item;
            DataContext = this;

            // Установка темно-фиолетовой цветовой схемы
            this.Background = new SolidColorBrush(Color.FromRgb(0x1A, 0x00, 0x36));
            this.Foreground = Brushes.White;
        }

        public string ReceiptDetails
        {
            get
            {
                if (Item == null)
                {
                    return "Информация о бронировании недоступна";
                }

                try
                {
                    string customerInfo = !string.IsNullOrEmpty(Item.Имя) && !string.IsNullOrEmpty(Item.Фамилия)
                        ? $"Клиент: {Item.Имя} {Item.Фамилия}\n\n"
                        : "Клиент: не указан\n\n";

                    string timeInfo = Item.Time != null
                        ? $"Дата: {Item.Date.ToShortDateString()}\nВремя: {Item.Time.Время_начала} - {Item.Time.Время_начала.Add(new TimeSpan(Item.Duration, 0, 0))}\nПродолжительность: {Item.Duration} ч."
                        : "Время не выбрано";

                    string roomInfo = Item.Room != null
                        ? $"Комната: №{Item.Room.Номер_Комнаты}\nВместимость: {Item.Room.Вместимость} чел."
                        : "Комната не выбрана";

                    string tariffInfo = Item.Tariff != null
                        ? $"Тариф: {Item.Tariff.Название_Тарифа}\nЦена за час: {Item.Tariff.Цена_за_час} руб."
                        : "Тариф не выбран";

                    string services = Item.Services != null && Item.Services.Count > 0
                        ? $"\n\nДоп. услуги:\n{string.Join("\n", Item.Services.Select(s => $"• {s.Название_Услуги} - {s.Цена} руб."))}"
                        : "\n\nДоп. услуги: не выбраны";

                    return $"{customerInfo}{timeInfo}\n\n{roomInfo}\n\n{tariffInfo}{services}";
                }
                catch (Exception ex)
                {
                    return $"Ошибка при формировании квитанции: {ex.Message}";
                }
            }
        }

        public string TotalPrice => Item != null ? $"{Item.TotalPrice} руб." : "0 руб.";

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем экземпляр нового окна
            MainWindow newWindow = new MainWindow();

            // Показываем новое окно
            newWindow.Show();

            // Закрываем текущее окно
            this.Close();
        }
    }
}
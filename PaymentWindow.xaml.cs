using kibers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
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
    /// Логика взаимодействия для PaymentWindow.xaml
    /// </summary>
    public partial class PaymentWindow : Window
    {
        public CartItem Item { get; set; }
        private string connectionString = ConfigurationManager.ConnectionStrings["RoomBookingDB"].ConnectionString;
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        public string ItemDetails
        {
            get
            {
                if (Item == null) return string.Empty;

                string services = Item.Services != null && Item.Services.Count > 0
                    ? $"\n\nДоп. услуги:\n{string.Join("\n", Item.Services.Select(s => $"• {s.Название_Услуги} - {s.Цена} руб."))}"
                    : "\n\nДоп. услуги: не выбраны";

                string timeInfo = Item.Time != null
                    ? $"{Item.Date.ToShortDateString()} {Item.Time.Время_начала} ({Item.Duration} ч.)"
                    : "Время не выбрано";

                string roomInfo = Item.Room != null
                    ? $"Комната {Item.Room.Номер_Комнаты} (вместимость: {Item.Room.Вместимость})"
                    : "Комната не выбрана";

                string tariffInfo = Item.Tariff != null
                    ? $"{Item.Tariff.Название_Тарифа} ({Item.Tariff.Цена_за_час} руб./час)"
                    : "Тариф не выбран";

                string customerInfo = !string.IsNullOrEmpty(Item.Имя) && !string.IsNullOrEmpty(Item.Фамилия)
                    ? $"Клиент: {Item.Имя} {Item.Фамилия}\n"
                    : "Клиент: не указан\n";

                return $"{customerInfo}\n{timeInfo}\n{tariffInfo}\n{roomInfo}{services}";
            }
        }

        public string TotalPrice => Item != null ? $"{Item.TotalPrice} руб." : "0 руб.";

        public PaymentWindow(CartItem item)
        {
            InitializeComponent();
            Item = item;
            DataContext = this;
        }

        private void PayButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CardNumber.Text) ||
                string.IsNullOrEmpty(CardMonth.Text) ||
                string.IsNullOrEmpty(CardYear.Text) ||
                string.IsNullOrEmpty(CardCvv.Text) ||
                string.IsNullOrEmpty(CardName.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все данные карты", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CardNumber.Text.Replace(" ", "").Length < 16)
            {
                MessageBox.Show("Номер карты должен содержать 16 цифр", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CardCvv.Text.Length != 3)
            {
                MessageBox.Show("CVV код должен содержать 3 цифры", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Mark as paid in database
            UpdatePaymentStatus(true);

            // Show receipt
            var receiptWindow = new ReceiptWindow(Item);
            receiptWindow.Show();

            DialogResult = true;
            this.Close();
        }
        private void PayCashButton_Click(object sender, RoutedEventArgs e)
        {
            // Mark as not paid in database
            UpdatePaymentStatus(false);

            // Show receipt
            var receiptWindow = new ReceiptWindow(Item);
            receiptWindow.Show();

            DialogResult = true;
            this.Close();
        }
        private void UpdatePaymentStatus(bool isPaid)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(
                        "UPDATE Записи SET Оплачено = @isPaid WHERE ID_Записи = @bookingId",
                        connection);

                    command.Parameters.AddWithValue("@isPaid", isPaid ? 1 : 0);
                    command.Parameters.AddWithValue("@bookingId", Item.BookingId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении статуса оплаты: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;

            // Создаем экземпляр нового окна
            UserForm newWindow = new UserForm();

            // Показываем новое окно
            newWindow.Show();

            // Закрываем текущее окно
            this.Close();
        }
    }
    // Обработчик для валидации числового ввода
    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}




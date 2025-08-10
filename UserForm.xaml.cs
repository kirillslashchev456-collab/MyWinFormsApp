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
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.ObjectModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using System.Data;
using kibers;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для UserForm.xaml
    /// </summary>
    public partial class UserForm : Window
    {
        private string connectionString;
        private ObservableCollection<CartItem> cartItems = new ObservableCollection<CartItem>();
        private Тарифы selectedTariff;
        private Комнаты selectedRoom;
        private DateTime selectedDate;
        private Время selectedTime;
        private int selectedDuration = 1;
        private List<ДопУслуги> selectedServices = new List<ДопУслуги>();
        public UserForm()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["RoomBookingDB"].ConnectionString;
            lstCart.ItemsSource = cartItems;
            LoadData();

            // Set date restrictions
            calendar.DisplayDateStart = DateTime.Today;
            calendar.DisplayDateEnd = DateTime.Today.AddDays(365);
            // Устанавливаем ограничения по датам (текущий месяц + половина следующего)
            DateTime today = DateTime.Today;
            DateTime endOfCurrentMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            DateTime endOfNextHalfMonth = endOfCurrentMonth.AddDays(15); // Половина следующего месяца

            calendar.DisplayDateStart = today;
            calendar.DisplayDateEnd = endOfNextHalfMonth;
            calendar.BlackoutDates.Add(new CalendarDateRange(endOfNextHalfMonth.AddDays(1), DateTime.MaxValue));
            // Set initial panel
            ShowPanel(pnlTariffs);
           
            DateTime maxAllowedDate = today;

            if (today.Day > 20)
            {
                maxAllowedDate = new DateTime(today.Year, today.Month + 1, 15);
            }
            else
            {
                maxAllowedDate = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            }

            calendar.DisplayDateStart = today;
            calendar.DisplayDateEnd = maxAllowedDate;
            calendar.SelectedDate = today;
            // Style the time combobox
            cmbTime.Foreground = Brushes.Black;
            cmbDuration.Foreground = Brushes.Black;

            lstCart.Height = 300;
            lstCart.Width = 500;  
            lstCart.FontSize = 14;
            cartItems.Add(new CartItem());
            lstCart.Items.Refresh();
        }

        private void LoadData()
        {
            LoadTariffs();
            LoadRooms();
            LoadServices();
            LoadTimes();
            // Устанавливаем ограничение на выбор только текущего месяца
            DateTime today = DateTime.Today;
            DateTime endOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            calendar.DisplayDateStart = today;
            calendar.DisplayDateEnd = endOfMonth;
            calendar.BlackoutDates.Add(new CalendarDateRange(endOfMonth.AddDays(1), DateTime.MaxValue));

        }
        private void BtnSortCheapest_Click(object sender, RoutedEventArgs e)
        {
            if (lstTariffs.ItemsSource is List<Тарифы> tariffs)
            {
                lstTariffs.ItemsSource = tariffs.OrderBy(t => t.Цена_за_час).ToList();
            }
        }

        private void BtnSortMedium_Click(object sender, RoutedEventArgs e)
        {
            if (lstTariffs.ItemsSource is List<Тарифы> tariffs)
            {
                // Средний по цене - просто сортируем по цене, средний будет в середине
                lstTariffs.ItemsSource = tariffs.OrderBy(t => t.Цена_за_час).ToList();
            }
        }

        private void BtnSortExpensive_Click(object sender, RoutedEventArgs e)
        {
            if (lstTariffs.ItemsSource is List<Тарифы> tariffs)
            {
                lstTariffs.ItemsSource = tariffs.OrderByDescending(t => t.Цена_за_час).ToList();
            }
        }

        // Update the BtnCheckout_Click method to handle room capacity
        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            // Обновляем все данные перед проверкой
            UpdateCustomerInfo();
            UpdateTotal();

            if (cartItems.Count == 0)
            {
                MessageBox.Show("Корзина пуста", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var item = cartItems.First();

            // Проверяем, заполнены ли все необходимые поля
            if (string.IsNullOrEmpty(item.Имя) || string.IsNullOrEmpty(item.Фамилия))
            {
                MessageBox.Show("Пожалуйста, введите имя и фамилию клиента", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int bookingId = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверяем текущую занятость комнаты
                    SqlCommand checkCmd = new SqlCommand(
                        "SELECT Вместимость, Занятые_места FROM Комнаты WHERE ID_Комнаты = @roomId",
                        connection);
                    checkCmd.Parameters.AddWithValue("@roomId", item.Room.ID_Комнаты);

                    using (SqlDataReader reader = checkCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int capacity = reader.GetInt32(0);
                            int occupied = reader.GetInt32(1);

                            if (occupied >= capacity)
                            {
                                MessageBox.Show("В этой комнате нет свободных мест", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }

                    // Если места есть, продолжаем бронирование
                    SqlCommand command = new SqlCommand(
     "INSERT INTO Записи (ID_Тарифа, ID_Комнаты, ID_Времени, Дата, Имя, Фамилия) " +
     "OUTPUT INSERTED.ID_Записи " +
     "VALUES (@tariffId, @roomId, @timeId, @date, @firstName, @lastName); " +
     "UPDATE Комнаты SET Занятые_места = Занятые_места + 1 WHERE ID_Комнаты = @roomId;",
     connection);

                    command.Parameters.AddWithValue("@tariffId", item.Tariff.ID_Тарифа);
                    command.Parameters.AddWithValue("@roomId", item.Room.ID_Комнаты);
                    command.Parameters.AddWithValue("@timeId", item.Time.ID_Времени); // Убедитесь, что это правильный ID
                    command.Parameters.AddWithValue("@date", item.Date);
                    command.Parameters.AddWithValue("@firstName", item.Имя);
                    command.Parameters.AddWithValue("@lastName", item.Фамилия);

                    // Получаем ID новой записи
                    bookingId = (int)command.ExecuteScalar();

                    // Добавляем выбранные услуги, если они есть
                    if (item.Services != null && item.Services.Count > 0)
                    {
                        // Обновляем информацию о выбранных услугах в записи бронирования
                        SqlCommand updateCmd = new SqlCommand(
                            "UPDATE Записи SET ДопУслуги = @servicesList WHERE ID_Записи = @bookingId",
                            connection);

                        // Формируем список услуг через запятую
                        string servicesList = string.Join(", ", item.Services.Select(s => s.Название_Услуги));
                        updateCmd.Parameters.AddWithValue("@servicesList", servicesList);
                        updateCmd.Parameters.AddWithValue("@bookingId", bookingId);
                        updateCmd.ExecuteNonQuery();

                        // Можно также добавить общую стоимость услуг к итоговой цене
                        decimal servicesTotal = item.Services.Sum(s => s.Цена);
                        SqlCommand updatePriceCmd = new SqlCommand(
                            "UPDATE Записи SET Стоимость_услуг = @servicesTotal WHERE ID_Записи = @bookingId",
                            connection);
                        updatePriceCmd.Parameters.AddWithValue("@servicesTotal", servicesTotal);
                        updatePriceCmd.Parameters.AddWithValue("@bookingId", bookingId);
                        updatePriceCmd.ExecuteNonQuery();
                    }

                    // Добавляем bookingId в CartItem
                    item.BookingId = bookingId;

                    // Показываем форму оплаты с передачей CartItem
                    var paymentForm = new PaymentWindow(item);
                    this.Close();
                    if (paymentForm.ShowDialog() == true)
                    {
                        MessageBox.Show("Бронирование и оплата успешно оформлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Сброс данных только если оплата прошла успешно
                        cartItems.Clear();
                        UpdateTotal();

                        // Сброс выбора
                        selectedTariff = null;
                        selectedRoom = null;
                        selectedTime = null;
                        selectedServices.Clear();
                        lstTariffs.SelectedIndex = -1;
                        dgRooms.SelectedIndex = -1;
                        cmbTime.SelectedIndex = -1;
                        calendar.SelectedDate = null;
                        txtFirstName.Text = "";
                        txtLastName.Text = "";

                        

                        // Обновляем список комнат
                        LoadRooms();
                    }
                    else
                    {
                        // Если пользователь отменил оплату, отменяем бронирование
                        SqlCommand cancelCmd = new SqlCommand(
                            "DELETE FROM Записи WHERE ID_Записи = @bookingId; " +
                            "UPDATE Комнаты SET Занятые_места = Занятые_места - 1 WHERE ID_Комнаты = @roomId;",
                            connection);
                        cancelCmd.Parameters.AddWithValue("@bookingId", bookingId);
                        cancelCmd.Parameters.AddWithValue("@roomId", item.Room.ID_Комнаты);
                        cancelCmd.ExecuteNonQuery();

                        MessageBox.Show("Бронирование отменено", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении брони: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTariffs()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("SELECT * FROM Тарифы", connection);
                    SqlDataReader reader = command.ExecuteReader();

                    List<Тарифы> tariffs = new List<Тарифы>();
                    while (reader.Read())
                    {
                        tariffs.Add(new Тарифы
                        {
                            ID_Тарифа = reader.GetInt32(0),
                            Название_Тарифа = reader.GetString(1),
                            Цена_за_час = reader.GetDecimal(2),
                            Описание = reader.IsDBNull(3) ? "" : reader.GetString(3)
                        });
                    }
                    reader.Close();

                    lstTariffs.ItemsSource = tariffs;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке тарифов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRooms()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Получаем выбранную дату или текущую дату по умолчанию
                    DateTime selectedDate = calendar.SelectedDate ?? DateTime.Today;

                    // Запрос для получения информации о комнатах и их занятости
                    SqlCommand command = new SqlCommand(
                        @"SELECT 
                    r.ID_Комнаты, 
                    r.Номер_Комнаты, 
                    r.Вместимость, 
                    r.Описание,
                    ISNULL((SELECT COUNT(*) FROM Записи z WHERE z.ID_Комнаты = r.ID_Комнаты AND z.Дата = @selectedDate), 0) AS Занятые_места,
                    CASE 
                        WHEN ISNULL((SELECT COUNT(*) FROM Записи z WHERE z.ID_Комнаты = r.ID_Комнаты AND z.Дата = @selectedDate), 0) = 0 THEN 'Свободна'
                        WHEN ISNULL((SELECT COUNT(*) FROM Записи z WHERE z.ID_Комнаты = r.ID_Комнаты AND z.Дата = @selectedDate), 0) >= r.Вместимость THEN 'Нет свободных мест'
                        WHEN ISNULL((SELECT COUNT(*) FROM Записи z WHERE z.ID_Комнаты = r.ID_Комнаты AND z.Дата = @selectedDate), 0) > (r.Вместимость / 2) THEN 'Больше половины занято'
                        ELSE 'Частично занята'
                    END AS Статус
                FROM Комнаты r",
                        connection);

                    command.Parameters.AddWithValue("@selectedDate", selectedDate);

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgRooms.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке комнат: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (calendar.SelectedDate.HasValue)
            {
                // Проверяем, не выходит ли выбранная дата за допустимые пределы
                DateTime today = DateTime.Today;
                DateTime maxAllowedDate = today;

                // Если сегодня после 20 числа, разрешаем бронирование до 15 числа следующего месяца
                if (today.Day > 20)
                {
                    maxAllowedDate = new DateTime(today.Year, today.Month + 1, 15);
                }
                else
                {
                    // Иначе разрешаем бронирование на весь текущий месяц
                    maxAllowedDate = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
                }

                if (calendar.SelectedDate.Value > maxAllowedDate)
                {
                    MessageBox.Show($"Бронирование возможно только до {maxAllowedDate:dd.MM.yyyy}", "Ограничение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    calendar.SelectedDate = maxAllowedDate;
                    return;
                }

                selectedDate = calendar.SelectedDate.Value;
                LoadRooms(); // Перезагружаем комнаты при изменении даты
                LoadTimes(); // Также перезагружаем доступное время
            }
        }

        private void LoadTimes()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("SELECT * FROM Время ORDER BY Время_начала", connection);
                    SqlDataReader reader = command.ExecuteReader();

                    List<Время> times = new List<Время>();
                    TimeSpan currentTime = DateTime.Now.TimeOfDay;

                    while (reader.Read())
                    {
                        TimeSpan time = reader.GetTimeSpan(1);

                        // Добавляем только время, которое еще не наступило сегодня
                        if (calendar.SelectedDate == DateTime.Today)
                        {
                            if (time > currentTime)
                            {
                                times.Add(new Время
                                {
                                    ID_Времени = reader.GetInt32(0),
                                    Время_начала = time
                                });
                            }
                        }
                        else
                        {
                            // Для других дней добавляем все время
                            times.Add(new Время
                            {
                                ID_Времени = reader.GetInt32(0),
                                Время_начала = time
                            });
                        }
                    }
                    reader.Close();

                    cmbTime.ItemsSource = times;
                    if (times.Count > 0)
                        cmbTime.SelectedIndex = 0;
                    else if (calendar.SelectedDate == DateTime.Today)
                        MessageBox.Show("На сегодня доступное время закончилось", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке времени: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadServices()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(
                        "SELECT ID_ДопУслуги, Название_Услуги, Цена, Описание FROM ДопУслуги",
                        connection);

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Добавляем только столбец с информацией об оплате
                    dt.Columns.Add("Оплата", typeof(string));
                    foreach (DataRow row in dt.Rows)
                    {
                        row["Оплата"] = "Оплата на кассе";
                    }

                    dgServices.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке услуг: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BtnDeleteService_Click(object sender, RoutedEventArgs e)
        {
            if (dgServices.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите услугу для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                DataRowView row = (DataRowView)dgServices.SelectedItem;
                int serviceId = (int)row["ID_ДопУслуги"];

                var result = MessageBox.Show("Вы уверены, что хотите удалить эту услугу?", "Подтверждение",
                                            MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlCommand command = new SqlCommand("DELETE FROM ДопУслуги WHERE ID_ДопУслуги = @ServiceId", connection);
                        command.Parameters.AddWithValue("@ServiceId", serviceId);
                        command.ExecuteNonQuery();
                    }

                    LoadServices();
                    MessageBox.Show("Услуга успешно удалена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении услуги: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowPanel(StackPanel panelToShow)
        {
            pnlTariffs.Visibility = Visibility.Collapsed;
            pnlRooms.Visibility = Visibility.Collapsed;
            pnlTime.Visibility = Visibility.Collapsed;
            pnlServices.Visibility = Visibility.Collapsed;

            panelToShow.Visibility = Visibility.Visible;
        }

        private void BtnTariffs_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(pnlTariffs);
        }

        private void BtnRooms_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(pnlRooms);
        }

        private void BtnTime_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(pnlTime);
        }

        private void BtnServices_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(pnlServices);
        }

        private void LstTariffs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstTariffs.SelectedItem != null)
            {
                selectedTariff = (Тарифы)lstTariffs.SelectedItem;
                UpdateTimeButtonState();
            }
        }

        private void DgRooms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgRooms.SelectedItem != null)
            {
                DataRowView row = (DataRowView)dgRooms.SelectedItem;
                selectedRoom = new Комнаты
                {
                    ID_Комнаты = (int)row["ID_Комнаты"],
                    Номер_Комнаты = (int)row["Номер_Комнаты"],
                    Вместимость = (int)row["Вместимость"],
                    Описание = row["Описание"].ToString()
                };
                UpdateTimeButtonState();
            }
        }

        private void UpdateTimeButtonState()
        {
            btnTime.IsEnabled = selectedTariff != null && selectedRoom != null;
        }

      

        private void BtnAddTariff_Click(object sender, RoutedEventArgs e)
        {
            if (lstTariffs.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите тариф", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newTariff = (Тарифы)lstTariffs.SelectedItem;

            // Если в корзине уже есть тариф, спрашиваем подтверждение замены
            if (cartItems.Count > 0 && cartItems[0].Tariff != null)
            {
                var result = MessageBox.Show($"Вы уже выбрали тариф '{cartItems[0].Tariff.Название_Тарифа}'. Заменить на '{newTariff.Название_Тарифа}'?",
                                            "Подтверждение",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                // Удаляем все выбранные услуги при смене тарифа
                cartItems[0].Services.Clear();
                dgServices.SelectedItem = null; // Сбрасываем выделение в DataGrid
            }

            // Создаем/обновляем элемент корзины
            CartItem item;
            if (cartItems.Count == 0)
            {
                item = new CartItem();
                cartItems.Add(item);
            }
            else
            {
                item = cartItems[0];
            }

            item.Tariff = newTariff;
            item.TotalPrice = item.Tariff.Цена_за_час * item.Duration;

            // Добавляем стоимость услуг, если они есть
            if (item.Services != null && item.Services.Count > 0)
                item.TotalPrice += item.Services.Sum(s => s.Цена);

            UpdateTotal();
            MessageBox.Show("Тариф добавлен в корзину", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void BtnAddRoom_Click(object sender, RoutedEventArgs e)
        {
            if (dgRooms.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите комнату", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cartItems.Count == 0 || cartItems[0].Tariff == null)
            {
                MessageBox.Show("Сначала выберите тариф", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView row = (DataRowView)dgRooms.SelectedItem;

            // Перед добавлением проверяем актуальный статус комнаты
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand checkCmd = new SqlCommand(
                        "SELECT COUNT(*) FROM Записи WHERE ID_Комнаты = @roomId AND Дата = @date",
                        connection);
                    checkCmd.Parameters.AddWithValue("@roomId", row["ID_Комнаты"]);
                    checkCmd.Parameters.AddWithValue("@date", calendar.SelectedDate ?? DateTime.Today);

                    int bookingsCount = (int)checkCmd.ExecuteScalar();
                    int capacity = (int)row["Вместимость"];

                    if (bookingsCount >= capacity)
                    {
                        MessageBox.Show("Эта комната уже полностью занята", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        LoadRooms(); // Обновляем список комнат
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке комнаты: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var newRoom = new Комнаты
            {
                ID_Комнаты = (int)row["ID_Комнаты"],
                Номер_Комнаты = (int)row["Номер_Комнаты"],
                Вместимость = (int)row["Вместимость"],
                Описание = row["Описание"].ToString()
            };

            // Проверяем, не выбрана ли уже эта комната
            if (cartItems[0].Room != null && cartItems[0].Room.ID_Комнаты == newRoom.ID_Комнаты)
            {
                MessageBox.Show("Эта комната уже выбрана", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            cartItems[0].Room = newRoom;
            cartItems[0].TotalPrice = cartItems[0].Tariff.Цена_за_час * cartItems[0].Duration;

            if (cartItems[0].Services != null)
                cartItems[0].TotalPrice += cartItems[0].Services.Sum(s => s.Цена);

            UpdateTotal();
            MessageBox.Show("Комната добавлена в корзину", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void UpdateRoomsAvailability()
        {
            LoadRooms();
        }


        private void BtnAddTime_Click(object sender, RoutedEventArgs e)
        {
            if (cartItems.Count == 0 || cartItems[0].Tariff == null)
            {
                MessageBox.Show("Сначала выберите тариф", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!calendar.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите дату", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbTime.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите время", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            selectedTime = (Время)cmbTime.SelectedItem;
            selectedDuration = int.Parse(((ComboBoxItem)cmbDuration.SelectedItem).Content.ToString());
            selectedDate = calendar.SelectedDate.Value;

            var item = cartItems.First();
            item.Date = selectedDate;
            item.Time = selectedTime;
            item.Duration = selectedDuration;

            item.TotalPrice = item.Tariff.Цена_за_час * item.Duration;
            if (item.Services != null)
                item.TotalPrice += item.Services.Sum(s => s.Цена);

            UpdateTotal();
            MessageBox.Show("Время добавлено в корзину", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void BtnAddServices_Click(object sender, RoutedEventArgs e)
        {
            if (cartItems.Count == 0 || cartItems[0].Tariff == null)
            {
                MessageBox.Show("Сначала выберите тариф", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dgServices.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите услугу", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView row = (DataRowView)dgServices.SelectedItem;
            var newService = new ДопУслуги
            {
                ID_ДопУслуги = (int)row["ID_ДопУслуги"],
                Название_Услуги = row["Название_Услуги"].ToString(),
                Цена = (decimal)row["Цена"],
                Описание = row["Описание"].ToString()
            };

            var item = cartItems.First();

            // Проверяем, не добавлена ли уже эта услуга
            if (item.Services.Any(s => s.ID_ДопУслуги == newService.ID_ДопУслуги))
            {
                MessageBox.Show("Эта услуга уже добавлена", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            item.Services.Add(newService);

            // Обновляем общую стоимость
            item.TotalPrice = item.Tariff.Цена_за_час * item.Duration;
            if (item.Services != null)
                item.TotalPrice += item.Services.Sum(s => s.Цена);

            UpdateTotal();

            MessageBox.Show("Услуга добавлена в корзину (оплата только на кассе)",
                           "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            lstCart.Items.Refresh();
        }

        // Добавляем новый метод для обновления имени и фамилии
        private void UpdateCustomerInfo()
        {
            if (cartItems.Count == 0)
            {
                cartItems.Add(new CartItem());
            }

            var item = cartItems.First();
            item.Имя = txtFirstName.Text;
            item.Фамилия = txtLastName.Text;

            lstCart.Items.Refresh();
        }

        // Обработчики событий для текстовых полей
        private void TxtFirstName_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCustomerInfo();
        }

        private void TxtLastName_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCustomerInfo();
        }

        // Обновляем метод UpdateTotal
        private void UpdateTotal()
        {
            if (cartItems.Count == 0)
            {
                txtTotal.Text = "Итого: 0 руб.";
                return;
            }

            var item = cartItems[0];
            decimal total = 0;

            if (item.Tariff != null)
            {
                total = item.Tariff.Цена_за_час * item.Duration;
            }

            if (item.Services != null && item.Services.Count > 0)
            {
                total += item.Services.Sum(s => s.Цена);
            }

            item.TotalPrice = total;
            txtTotal.Text = $"Итого: {total} руб.";
            lstCart.Items.Refresh(); // Обязательно обновляем отображение корзины
        }






        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            // Создаем экземпляр нового окна
            MainWindow newWindow = new MainWindow();

            // Показываем новое окно
            newWindow.Show();

            // Закрываем текущее окно
            this.Close();
        }

        private void lstCart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void BtnViewRooms_Click(object sender, RoutedEventArgs e)
        { // Создаем экземпляр нового окна
            RoomsGalleryWindow newWindow = new RoomsGalleryWindow();

            // Показываем новое окно
            newWindow.Show();

            // Закрываем текущее окно
            this.Close();

        }
    }


    public class Тарифы
    {
        public int ID_Тарифа { get; set; }
        public string Название_Тарифа { get; set; }
        public decimal Цена_за_час { get; set; }
        public string Описание { get; set; }

        public override string ToString()
        {
            return $"{Название_Тарифа} ({Цена_за_час} руб./час)";
        }
    }

    public class Комнаты
    {
        public int ID_Комнаты { get; set; }
        public int Номер_Комнаты { get; set; }
        public int Вместимость { get; set; }
        public string Описание { get; set; }
    }

    public class Время
    {
        public int ID_Времени { get; set; }
        public TimeSpan Время_начала { get; set; }

        public override string ToString()
        {
            return Время_начала.ToString(@"hh\:mm");
        }
    }

    public class ДопУслуги
    {
        public int ID_ДопУслуги { get; set; }
        public string Название_Услуги { get; set; }
        public decimal Цена { get; set; }
        public string Описание { get; set; }
    }

    public class CartItem
    {
        public string Имя { get; set; }
        public string Фамилия { get; set; }
        public Тарифы Tariff { get; set; }
        public Комнаты Room { get; set; }
        public DateTime Date { get; set; }
        public Время Time { get; set; }
        public int Duration { get; set; } = 1;
        public List<ДопУслуги> Services { get; set; } = new List<ДопУслуги>();
        public decimal TotalPrice { get; set; }
        public int BookingId { get; set; }

        public override string ToString()
        {
            string services = Services != null && Services.Count > 0
                ? $"\nДоп. услуги (оплата только на кассе):\n{string.Join("\n", Services.Select(s => $"- {s.Название_Услуги} ({s.Цена} руб.)"))}"
                : "\nДоп. услуги: не выбраны";

            string timeInfo = Time != null
                ? $"{Date.ToShortDateString()} {Time.Время_начала} ({Duration} ч.)"
                : "Время не выбрано";

            string roomInfo = Room != null
                ? $"Комната {Room.Номер_Комнаты} (вместимость: {Room.Вместимость})"
                : "Комната не выбрана";

            string tariffInfo = Tariff != null
                ? $"{Tariff.Название_Тарифа} ({Tariff.Цена_за_час} руб./час)"
                : "Тариф не выбран";

            string nameInfo = !string.IsNullOrEmpty(Имя) && !string.IsNullOrEmpty(Фамилия)
                ? $"Клиент: {Имя} {Фамилия}\n"
                : "Клиент: не указан\n";

            string bookingInfo = BookingId > 0
                ? $"\nНомер бронирования: {BookingId}"
                : "";

            return $"{nameInfo}{timeInfo}\n{tariffInfo}\n{roomInfo}{services}{bookingInfo}\n\nИтого: {TotalPrice} руб.\n";
        }
    }





}



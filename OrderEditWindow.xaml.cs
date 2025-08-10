using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace WpfApp1
{
    public partial class OrderEditWindow : Window
    {
        private string connectionString;
        private bool isEditMode;
        private DataRowView orderToEdit;

        public OrderEditWindow(bool isEditMode = false, DataRowView orderToEdit = null)
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["RoomBookingDB"].ConnectionString;
            this.isEditMode = isEditMode;
            this.orderToEdit = orderToEdit;
         

            LoadComboBoxData();
            DataContext = this;

            if (isEditMode && orderToEdit != null)
            {
                WindowTitle = "Редактирование заказа";
                LoadOrderData();
            }
            else
            {
                WindowTitle = "Добавление нового заказа";
                SelectedDate = DateTime.Today;
            }
        }

        public string WindowTitle { get; set; }
        public int SelectedTariffId { get; set; }
        public int SelectedRoomId { get; set; }
        public DateTime SelectedDate { get; set; }
        public int SelectedTimeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal Price { get; set; }

        private void LoadComboBoxData()
        {
            try
            {
                // Загрузка тарифов
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT ID_Тарифа, Название_Тарифа FROM Тарифы";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    cmbTariff.ItemsSource = table.DefaultView;
                }

                // Загрузка комнат
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT ID_Комнаты, Номер_Комнаты FROM Комнаты";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    cmbRoom.ItemsSource = table.DefaultView;
                }

                // Загрузка времени
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT ID_Времени, Время_начала FROM Время";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    cmbTime.ItemsSource = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOrderData()
        {
            if (orderToEdit != null)
            {
                SelectedTariffId = (int)orderToEdit["ID_Тарифа"];
                SelectedRoomId = (int)orderToEdit["ID_Комнаты"];
                SelectedDate = (DateTime)orderToEdit["Дата"];
                SelectedTimeId = GetTimeIdFromTime((TimeSpan)orderToEdit["Время_начала"]);
                FirstName = orderToEdit["Имя"].ToString();
                LastName = orderToEdit["Фамилия"].ToString();
                Price = (decimal)orderToEdit["Стоимость"];
            }
        }

        private int GetTimeIdFromTime(TimeSpan time)
        {
            // Здесь должна быть логика получения ID времени по значению TimeSpan
            // Для примера возвращаем 1
            return 1;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTariff.SelectedItem == null || cmbRoom.SelectedItem == null ||
                cmbTime.SelectedItem == null || string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (isEditMode)
                {
                    UpdateOrder();
                }
                else
                {
                    AddOrder();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddOrder()
        {
            string query = @"INSERT INTO Записи (ID_Тарифа, ID_Комнаты, ID_Времени, Дата, Имя_Клиента, Фамилия_Клиента) 
                             VALUES (@TariffId, @RoomId, @TimeId, @Date, @FirstName, @LastName)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@TariffId", SelectedTariffId);
                command.Parameters.AddWithValue("@RoomId", SelectedRoomId);
                command.Parameters.AddWithValue("@TimeId", SelectedTimeId);
                command.Parameters.AddWithValue("@Date", SelectedDate);
                command.Parameters.AddWithValue("@FirstName", FirstName);
                command.Parameters.AddWithValue("@LastName", LastName);
                command.ExecuteNonQuery();
            }
        }

        private void UpdateOrder()
        {
            string query = @"UPDATE Записи SET 
                            ID_Тарифа = @TariffId, 
                            ID_Комнаты = @RoomId, 
                            ID_Времени = @TimeId, 
                            Дата = @Date, 
                            Имя_Клиента = @FirstName,
                            Фамилия_Клиента = @LastName
                            WHERE ID_Записи = @OrderId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@TariffId", SelectedTariffId);
                command.Parameters.AddWithValue("@RoomId", SelectedRoomId);
                command.Parameters.AddWithValue("@TimeId", SelectedTimeId);
                command.Parameters.AddWithValue("@Date", SelectedDate);
                command.Parameters.AddWithValue("@FirstName", FirstName);
                command.Parameters.AddWithValue("@LastName", LastName);
                command.Parameters.AddWithValue("@OrderId", orderToEdit["ID_Записи"]);
                command.ExecuteNonQuery();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace WpfApp1
{
    public partial class TariffEditWindow : Window
    {
        private string connectionString;
        private bool isEditMode;
        private DataRowView tariffToEdit;

        public TariffEditWindow(bool isEditMode = false, DataRowView tariffToEdit = null)
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["RoomBookingDB"].ConnectionString;
            this.isEditMode = isEditMode;
            this.tariffToEdit = tariffToEdit;

            DataContext = this;

            if (isEditMode && tariffToEdit != null)
            {
                WindowTitle = "Редактирование тарифа";
                TariffName = tariffToEdit["Название_Тарифа"].ToString();
                PricePerHour = Convert.ToDecimal(tariffToEdit["Цена_за_час"]);
                Description = tariffToEdit["Описание"].ToString();
            }
            else
            {
                WindowTitle = "Добавление нового тарифа";
                TariffName = "Новый тариф";
                PricePerHour = 100;
                Description = "";
            }
        }

        public string WindowTitle { get; set; }
        public string TariffName { get; set; }
        public decimal PricePerHour { get; set; }
        public string Description { get; set; }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TariffName))
            {
                MessageBox.Show("Пожалуйста, введите название тарифа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (isEditMode)
                {
                    UpdateTariff();
                }
                else
                {
                    AddTariff();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении тарифа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddTariff()
        {
            string query = @"INSERT INTO Тарифы (Название_Тарифа, Цена_за_час, Описание) 
                             VALUES (@Name, @Price, @Description)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", TariffName);
                command.Parameters.AddWithValue("@Price", PricePerHour);
                command.Parameters.AddWithValue("@Description", Description);
                command.ExecuteNonQuery();
            }
        }

        private void UpdateTariff()
        {
            string query = @"UPDATE Тарифы SET 
                            Название_Тарифа = @Name, 
                            Цена_за_час = @Price, 
                            Описание = @Description
                            WHERE ID_Тарифа = @TariffId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", TariffName);
                command.Parameters.AddWithValue("@Price", PricePerHour);
                command.Parameters.AddWithValue("@Description", Description);
                command.Parameters.AddWithValue("@TariffId", tariffToEdit["ID_Тарифа"]);
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
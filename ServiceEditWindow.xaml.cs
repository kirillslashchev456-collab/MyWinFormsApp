using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace WpfApp1
{
    public partial class ServiceEditWindow : Window
    {
        private string connectionString;
        private bool isEditMode;
        private DataRowView serviceToEdit;

        public ServiceEditWindow(bool isEditMode = false, DataRowView serviceToEdit = null)
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["RoomBookingDB"].ConnectionString;
            this.isEditMode = isEditMode;
            this.serviceToEdit = serviceToEdit;

            DataContext = this;

            if (isEditMode && serviceToEdit != null)
            {
                WindowTitle = "Редактирование услуги";
                ServiceName = serviceToEdit["Название_Услуги"].ToString();
                Price = Convert.ToDecimal(serviceToEdit["Цена"]);
                Description = serviceToEdit["Описание"].ToString();
            }
            else
            {
                WindowTitle = "Добавление новой услуги";
                ServiceName = "Новая услуга";
                Price = 50;
                Description = "";
            }
        }

        public string WindowTitle { get; set; }
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ServiceName))
            {
                MessageBox.Show("Пожалуйста, введите название услуги", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Price <= 0)
            {
                MessageBox.Show("Пожалуйста, введите корректную цену", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (isEditMode)
                {
                    UpdateService();
                }
                else
                {
                    AddService();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении услуги: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddService()
        {
            string query = @"INSERT INTO ДопУслуги (Название_Услуги, Цена, Описание) 
                             VALUES (@Name, @Price, @Description)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", ServiceName);
                command.Parameters.AddWithValue("@Price", Price);
                command.Parameters.AddWithValue("@Description", Description);
                command.ExecuteNonQuery();
            }
        }

        private void UpdateService()
        {
            string query = @"UPDATE ДопУслуги SET 
                            Название_Услуги = @Name, 
                            Цена = @Price, 
                            Описание = @Description
                            WHERE ID_ДопУслуги = @ServiceId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", ServiceName);
                command.Parameters.AddWithValue("@Price", Price);
                command.Parameters.AddWithValue("@Description", Description);
                command.Parameters.AddWithValue("@ServiceId", serviceToEdit["ID_ДопУслуги"]);
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
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace WpfApp1
{
    public partial class RoomEditWindow : Window
    {
        private string connectionString;
        private bool isEditMode;
        private DataRowView roomToEdit;

        public RoomEditWindow(bool isEditMode = false, DataRowView roomToEdit = null)
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["RoomBookingDB"].ConnectionString;
            this.isEditMode = isEditMode;
            this.roomToEdit = roomToEdit;

            DataContext = this;

            if (isEditMode && roomToEdit != null)
            {
                WindowTitle = "Редактирование комнаты";
                RoomNumber = Convert.ToInt32(roomToEdit["Номер_Комнаты"]);
                Capacity = Convert.ToInt32(roomToEdit["Вместимость"]);
                Description = roomToEdit["Описание"].ToString();
            }
            else
            {
                WindowTitle = "Добавление новой комнаты";
                RoomNumber = 0;
                Capacity = 1;
                Description = "";
            }
        }

        public string WindowTitle { get; set; }
        public int RoomNumber { get; set; }
        public int Capacity { get; set; }
        public string Description { get; set; }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (RoomNumber <= 0)
            {
                MessageBox.Show("Пожалуйста, введите корректный номер комнаты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Capacity <= 0)
            {
                MessageBox.Show("Пожалуйста, введите корректную вместимость", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (isEditMode)
                {
                    UpdateRoom();
                }
                else
                {
                    AddRoom();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении комнаты: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddRoom()
        {
            string query = @"INSERT INTO Комнаты (Номер_Комнаты, Вместимость, Описание) 
                             VALUES (@Number, @Capacity, @Description)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Number", RoomNumber);
                command.Parameters.AddWithValue("@Capacity", Capacity);
                command.Parameters.AddWithValue("@Description", Description);
                command.ExecuteNonQuery();
            }
        }

        private void UpdateRoom()
        {
            string query = @"UPDATE Комнаты SET 
                            Номер_Комнаты = @Number, 
                            Вместимость = @Capacity, 
                            Описание = @Description
                            WHERE ID_Комнаты = @RoomId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Number", RoomNumber);
                command.Parameters.AddWithValue("@Capacity", Capacity);
                command.Parameters.AddWithValue("@Description", Description);
                command.Parameters.AddWithValue("@RoomId", roomToEdit["ID_Комнаты"]);
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
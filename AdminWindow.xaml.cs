using kibers;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class AdminWindow : Window
    {

        private string connectionString;
        private DataTable currentDataTable;
        private DataView currentDataView;

        public AdminWindow()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["RoomBookingDB"].ConnectionString;
            LoadOrders();
        }

        private void ShowPanel(StackPanel panelToShow)
        {
            pnlOrders.Visibility = Visibility.Collapsed;
            pnlTariffs.Visibility = Visibility.Collapsed;
            pnlRooms.Visibility = Visibility.Collapsed;
            pnlServices.Visibility = Visibility.Collapsed;

            panelToShow.Visibility = Visibility.Visible;
        }

        #region Navigation Buttons
        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(pnlOrders);
            LoadOrders();
        }

        private void BtnTariffs_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(pnlTariffs);
            LoadTariffs();
        }

        private void BtnRooms_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(pnlRooms);
            LoadRooms();
        }

        private void BtnServices_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(pnlServices);
            LoadServices();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow newWindow = new MainWindow();
            newWindow.Show();
            this.Close();
        }

        private void LoadOrders()
        {
            try
            {
                string query = @"SELECT 
            ROW_NUMBER() OVER (ORDER BY z.ID_Записи) AS RowNum,
            z.ID_Записи, 
            t.Название_Тарифа, 
            r.Номер_Комнаты, 
            r.ID_Комнаты, 
            z.Дата, 
            v.Время_начала, 
            z.Имя AS Имя_Клиента, 
            z.Фамилия AS Фамилия_Клиента,
            (t.Цена_за_час * DATEDIFF(HOUR, v.Время_начала, DATEADD(HOUR, 1, v.Время_начала))) AS Стоимость,
            z.ДопУслуги,  
            CASE 
                WHEN z.Оплачено = 1 THEN 'Да'
                ELSE 'Нет'
            END AS Оплачено
        FROM Записи z
        JOIN Тарифы t ON z.ID_Тарифа = t.ID_Тарифа
        JOIN Комнаты r ON z.ID_Комнаты = r.ID_Комнаты
        JOIN Время v ON z.ID_Времени = v.ID_Времени";

                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                {
                    currentDataTable = new DataTable();
                    adapter.Fill(currentDataTable);
                    currentDataView = new DataView(currentDataTable);
                    dgOrders.ItemsSource = currentDataView;

                    // Enable column sorting
                    foreach (var column in dgOrders.Columns)
                    {
                        column.CanUserSort = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заказов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #region Sorting and Filtering
        private void SortOrders(string columnName, ListSortDirection direction)
        {
            currentDataView.Sort = $"{columnName} {(direction == ListSortDirection.Ascending ? "ASC" : "DESC")}";
            dgOrders.ItemsSource = currentDataView;
        }

        private void BtnSortByPrice_Click(object sender, RoutedEventArgs e)
        {
            SortOrders("Стоимость", ListSortDirection.Ascending);
        }

        private void BtnSortByDate_Click(object sender, RoutedEventArgs e)
        {
            SortOrders("Дата", ListSortDirection.Descending);
        }

        private void BtnSortByTariff_Click(object sender, RoutedEventArgs e)
        {
            SortOrders("Название_Тарифа", ListSortDirection.Ascending);
        }

        private void BtnSortByName_Click(object sender, RoutedEventArgs e)
        {
            SortOrders("Имя_Клиента", ListSortDirection.Ascending);
        }

        private void BtnSortByLastName_Click(object sender, RoutedEventArgs e)
        {
            SortOrders("Фамилия_Клиента", ListSortDirection.Ascending);
        }

        private void BtnFilterByName_Click(object sender, RoutedEventArgs e)
        {
            string searchText = txtNameFilter.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                currentDataView.RowFilter = $"(Имя_Клиента LIKE '%{searchText}%' OR Фамилия_Клиента LIKE '%{searchText}%')";
            }
            else
            {
                currentDataView.RowFilter = "";
            }
            dgOrders.ItemsSource = currentDataView;
        }

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            txtNameFilter.Text = "";
            currentDataView.RowFilter = "";
            dgOrders.ItemsSource = currentDataView;
        }
        #endregion
   

private void LoadTariffs()
        {
            try
            {
                string query = "SELECT * FROM Тарифы";
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                {
                    currentDataTable = new DataTable();
                    adapter.Fill(currentDataTable);
                    dgTariffs.ItemsSource = currentDataTable.DefaultView;
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
                string query = @"
            SELECT 
                ID_Комнаты,
                Номер_Комнаты,
                Вместимость,
                Занятые_места,
                (Вместимость - Занятые_места) AS Свободные_места,
                Описание,
                CASE 
                    WHEN Занятые_места >= Вместимость THEN 'Занята'
                    WHEN Занятые_места > 0 THEN 'Частично занята'
                    ELSE 'Свободна'
                END AS Статус
            FROM Комнаты";

                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                {
                    currentDataTable = new DataTable();
                    adapter.Fill(currentDataTable);
                    dgRooms.ItemsSource = currentDataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке комнат: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadServices()
        {
            try
            {
                string query = "SELECT * FROM ДопУслуги";
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                {
                    currentDataTable = new DataTable();
                    adapter.Fill(currentDataTable);
                    dgServices.ItemsSource = currentDataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке услуг: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region CRUD Operations for Orders
        private void BtnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrders.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите заказ для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView row = (DataRowView)dgOrders.SelectedItem;
            int orderId = (int)row["ID_Записи"];
            int roomId = (int)row["ID_Комнаты"];

            var result = MessageBox.Show("Вы уверены, что хотите удалить этот заказ?", "Подтверждение",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        SqlCommand command = new SqlCommand(
                            "DELETE FROM Записи WHERE ID_Записи = @OrderId; " +
                            "UPDATE Комнаты SET Занятые_места = Занятые_места - 1 WHERE ID_Комнаты = @RoomId;",
                            connection);

                        command.Parameters.AddWithValue("@OrderId", orderId);
                        command.Parameters.AddWithValue("@RoomId", roomId);
                        command.ExecuteNonQuery();
                    }

                    // После удаления перезагружаем данные и обновляем нумерацию
                    LoadOrders();
                    MessageBox.Show("Заказ успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnSaveOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"SELECT 
                        z.ID_Записи, 
                        t.Название_Тарифа, 
                        r.Номер_Комнаты, 
                        r.ID_Комнаты, 
                        z.Дата, 
                        v.Время_начала, 
                        z.Имя AS Имя_Клиента, 
                        z.Фамилия AS Фамилия_Клиента,
                        (t.Цена_за_час * DATEDIFF(HOUR, v.Время_начала, DATEADD(HOUR, 1, v.Время_начала))) AS Стоимость,
                        z.ДопУслуги,  
                        CASE 
                            WHEN z.Оплачено = 1 THEN 'Да'
                            ELSE 'Нет'
                        END AS Оплачено
                    FROM Записи z
                    JOIN Тарифы t ON z.ID_Тарифа = t.ID_Тарифа
                    JOIN Комнаты r ON z.ID_Комнаты = r.ID_Комнаты
                    JOIN Время v ON z.ID_Времени = v.ID_Времени";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    SqlCommandBuilder commandBuilder = new SqlCommandBuilder(adapter);
                    adapter.Update(currentDataTable);
                }

                MessageBox.Show("Изменения сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region CRUD Operations for Tariffs
        private void BtnAddTariff_Click(object sender, RoutedEventArgs e)
        {
            TariffEditWindow tariffWindow = new TariffEditWindow();
            if (tariffWindow.ShowDialog() == true)
            {
                LoadTariffs();
            }
        }

        private void BtnEditTariff_Click(object sender, RoutedEventArgs e)
        {
            if (dgTariffs.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите тариф для редактирования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedTariff = (DataRowView)dgTariffs.SelectedItem;
            TariffEditWindow tariffWindow = new TariffEditWindow(true, selectedTariff);
            if (tariffWindow.ShowDialog() == true)
            {
                LoadTariffs();
            }
        }

        private void BtnDeleteTariff_Click(object sender, RoutedEventArgs e)
        {
            if (dgTariffs.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите тариф для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView row = (DataRowView)dgTariffs.SelectedItem;
            int tariffId = (int)row["ID_Тарифа"];

            var result = MessageBox.Show("Вы уверены, что хотите удалить этот тариф?", "Подтверждение",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlCommand command = new SqlCommand("DELETE FROM Тарифы WHERE ID_Тарифа = @TariffId", connection);
                        command.Parameters.AddWithValue("@TariffId", tariffId);
                        command.ExecuteNonQuery();
                    }

                    LoadTariffs();
                    MessageBox.Show("Тариф успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении тарифа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnSaveTariff_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Тарифы";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    SqlCommandBuilder commandBuilder = new SqlCommandBuilder(adapter);
                    adapter.Update(currentDataTable);
                }

                MessageBox.Show("Изменения сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region CRUD Operations for Rooms
        private void BtnAddRoom_Click(object sender, RoutedEventArgs e)
        {
            RoomEditWindow roomWindow = new RoomEditWindow();
            if (roomWindow.ShowDialog() == true)
            {
                LoadRooms();
            }
        }

        private void BtnEditRoom_Click(object sender, RoutedEventArgs e)
        {
            if (dgRooms.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите комнату для редактирования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedRoom = (DataRowView)dgRooms.SelectedItem;
            RoomEditWindow roomWindow = new RoomEditWindow(true, selectedRoom);
            if (roomWindow.ShowDialog() == true)
            {
                LoadRooms();
            }
        }

        private void BtnDeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            if (dgRooms.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите комнату для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView row = (DataRowView)dgRooms.SelectedItem;
            int roomId = (int)row["ID_Комнаты"];

            var result = MessageBox.Show("Вы уверены, что хотите удалить эту комнату?", "Подтверждение",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlCommand command = new SqlCommand("DELETE FROM Комнаты WHERE ID_Комнаты = @RoomId", connection);
                        command.Parameters.AddWithValue("@RoomId", roomId);
                        command.ExecuteNonQuery();
                    }

                    LoadRooms();
                    MessageBox.Show("Комната успешно удалена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении комнаты: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnSaveRoom_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"
                    SELECT 
                        ID_Комнаты,
                        Номер_Комнаты,
                        Вместимость,
                        Занятые_места,
                        (Вместимость - Занятые_места) AS Свободные_места,
                        Описание,
                        CASE 
                            WHEN Занятые_места >= Вместимость THEN 'Занята'
                            WHEN Занятые_места > 0 THEN 'Частично занята'
                            ELSE 'Свободна'
                        END AS Статус
                    FROM Комнаты";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    SqlCommandBuilder commandBuilder = new SqlCommandBuilder(adapter);
                    adapter.Update(currentDataTable);
                }

                MessageBox.Show("Изменения сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region CRUD Operations for Services
        private void BtnAddService_Click(object sender, RoutedEventArgs e)
        {
            ServiceEditWindow serviceWindow = new ServiceEditWindow();
            if (serviceWindow.ShowDialog() == true)
            {
                LoadServices();
            }
        }

        private void BtnEditService_Click(object sender, RoutedEventArgs e)
        {
            if (dgServices.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите услугу для редактирования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedService = (DataRowView)dgServices.SelectedItem;
            ServiceEditWindow serviceWindow = new ServiceEditWindow(true, selectedService);
            if (serviceWindow.ShowDialog() == true)
            {
                LoadServices();
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

                // Диагностика - выводим список всех доступных колонок
                var columns = string.Join(", ", row.Row.Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                MessageBox.Show($"Доступные колонки: {columns}", "Отладка");

                // Используем правильное имя колонки (возможные варианты)
                string idColumnName = row.Row.Table.Columns.Cast<DataColumn>()
                                        .FirstOrDefault(c => c.ColumnName.Contains("ID"))?.ColumnName;

                if (string.IsNullOrEmpty(idColumnName))
                {
                    MessageBox.Show("Не удалось найти колонку с ID", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int serviceId = (int)row[idColumnName];

                var result = MessageBox.Show("Вы уверены, что хотите удалить эту услугу?", "Подтверждение",
                                            MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlCommand command = new SqlCommand($"DELETE FROM ДопУслуги WHERE {idColumnName} = @ServiceId", connection);
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
        private void BtnSaveService_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM ДопУслуги";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    SqlCommandBuilder commandBuilder = new SqlCommandBuilder(adapter);
                    adapter.Update(currentDataTable);
                }

                MessageBox.Show("Изменения сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Selection Changed Events
        private void DgOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Можно добавить логику при выборе заказа
        }

        private void DgTariffs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Можно добавить логику при выборе тарифа
        }

        private void DgRooms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Можно добавить логику при выборе комнаты
        }

        private void DgServices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Можно добавить логику при выборе услуги
        }
        #endregion

        private void BtnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            OrderEditWindow orderWindow = new OrderEditWindow();
            if (orderWindow.ShowDialog() == true)
            {
                LoadOrders();
            }
        }
        
        private void BtnEditOrder_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrders.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите заказ для редактирования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView selectedOrder = (DataRowView)dgOrders.SelectedItem;
            OrderEditWindow orderWindow = new OrderEditWindow(true, selectedOrder);
            if (orderWindow.ShowDialog() == true)
            {
                LoadOrders();
            }
        }

    }
}
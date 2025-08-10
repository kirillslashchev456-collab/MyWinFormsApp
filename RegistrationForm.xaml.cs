using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using kibers;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для RegistrationForm.xaml
    /// </summary>
    public partial class RegistrationForm : Window
    {
        // Регулярное выражение для проверки логина и пароля
        private readonly Regex _validInputRegex = new Regex(@"^[a-zA-Z0-9_\-@]+$");

        public RegistrationForm()
        {
            InitializeComponent();
            this.WindowStyle = WindowStyle.None;
        }

        private bool ValidateInput(string input)
        {
            // Проверяем, что строка не содержит запрещенных символов
            return _validInputRegex.IsMatch(input);
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            // Проверка ввода
            if (!ValidateInput(username) || !ValidateInput(password))
            {
                MessageBox.Show("Логин и пароль не должны содержать точек, запятых и других специальных символов (кроме _, -, @)");
                return;
            }

            string connectionString = @"Data Source=DESKTOP-9UEGDC7;Initial Catalog=UserRegistrationDB;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Users (Username, Password) VALUES (@Username, @Password)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();
                        MessageBox.Show("Регистрация прошла успешно!");
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Ошибка при регистрации: " + ex.Message);
                    }
                }
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            // Проверка ввода
            if (!ValidateInput(username) || !ValidateInput(password))
            {
                MessageBox.Show("Логин и пароль не должны содержать точек, запятых и других специальных символов (кроме _, -, @)");
                return;
            }

            try
            {
                string connectionString = @"Data Source=DESKTOP-9UEGDC7;Initial Catalog=UserRegistrationDB;Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT Password FROM [Users] WHERE Username = @Username";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                string storedPassword = reader.GetString(0);
                                if (password == storedPassword)
                                {
                                    // Проверка на администратора
                                    if (username == "Admin" && password == "123")
                                    {
                                        MessageBox.Show("Вход как администратор выполнен успешно!");
                                        AdminWindow adminForm = new AdminWindow(); // Предполагается, что у вас есть форма AdminForm
                                        adminForm.Show();
                                    }
                                    else
                                    {
                                        MessageBox.Show("Вход успешен!");
                                        UserForm userForm = new UserForm();
                                        userForm.Show();
                                    }
                                    this.Close();
                                }
                                else
                                {
                                    MessageBox.Show("Неверный пароль.");
                                }
                            }
                            else
                            {
                                MessageBox.Show("Пользователь не найден.");
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Ошибка базы данных: " + ex.Message);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow newWindow = new MainWindow();
            newWindow.Show();
            this.Close();
        }

        // Обработчик для проверки ввода в реальном времени
        private void txtUsername_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!_validInputRegex.IsMatch(e.Text))
            {
                e.Handled = true; // Отменяем ввод
            }
        }

        // Обработчик для проверки ввода в реальном времени для пароля
        private void txtPassword_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!_validInputRegex.IsMatch(e.Text))
            {
                e.Handled = true; // Отменяем ввод
            }
        }
    }
}
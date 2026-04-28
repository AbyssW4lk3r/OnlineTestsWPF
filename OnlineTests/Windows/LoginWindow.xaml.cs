using System;
using System.Windows;
using OnlineTests.Services;

namespace OnlineTests.Windows
{
    public partial class LoginWindow : Window
    {
        private DatabaseService db = new DatabaseService();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ErrorText.Text = "Введи логин и пароль!";
                return;
            }

            try
            {
                var user = db.AuthenticateUser(login, password);
                if (user != null)
                {
                    if (user.Role == "admin")
                    {
                        new AdminWindow(user.FullName).Show();
                    }
                    else
                    {
                        new StudentWindow(user.Id, user.FullName).Show();
                    }
                    this.Close();
                }
                else
                {
                    ErrorText.Text = "Неверный логин или пароль!";
                }
            }
            catch (Exception ex)
            {
                ErrorText.Text = "Ошибка подключения!";
                MessageBox.Show(ex.Message);
            }
        }
    }
}
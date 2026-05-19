using System.Data.OleDb;
using System.Windows;
using ToyStore.Classes;

namespace ToyStore.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            using (OleDbConnection connection = DatabaseService.GetConnection())
            {
                connection.Open();

                string query =
                    "SELECT * FROM Пользователи WHERE Логин = ? AND Пароль = ?";

                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@p1", login);
                command.Parameters.AddWithValue("@p2", password);

                OleDbDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    string role = reader["Роль сотрудника"].ToString();
                    string fio = reader["ФИО"].ToString();

                    MainWindow window = new MainWindow(role, fio);
                    window.Show();

                    Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль");
                }
            }
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow("Гость", "Гость");
            window.Show();

            Close();
        }
    }
}
using System.Data.OleDb;
using System.Windows;
using ToyStore.Classes;

namespace ToyStore.Windows
{
    public partial class AddEditWindow : Window
    {
        private string editArticle = null;

        public AddEditWindow()
        {
            InitializeComponent();

            LoadData();

            Title = "Добавление товара";
        }

        public AddEditWindow(string article)
        {
            InitializeComponent();

            editArticle = article;

            LoadData();

            Title = "Редактирование товара";

            LoadProduct();
        }

        private void LoadData()
        {
            using (OleDbConnection connection =
                DatabaseService.GetConnection())
            {
                connection.Open();

                LoadCategories(connection);
                LoadSuppliers(connection);
                LoadManufacturers(connection);
            }
        }

        private void LoadCategories(OleDbConnection connection)
        {
            string query = "SELECT Наименование FROM Категория";

            OleDbCommand command =
                new OleDbCommand(query, connection);

            OleDbDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                CategoryComboBox.Items.Add(
                    reader["Наименование"].ToString());
            }

            reader.Close();
        }

        private void LoadSuppliers(OleDbConnection connection)
        {
            string query = "SELECT Наименование FROM Поставщик";

            OleDbCommand command =
                new OleDbCommand(query, connection);

            OleDbDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                SupplierComboBox.Items.Add(
                    reader["Наименование"].ToString());
            }

            reader.Close();
        }

        private void LoadManufacturers(OleDbConnection connection)
        {
            string query = "SELECT Наименование FROM Производитель";

            OleDbCommand command =
                new OleDbCommand(query, connection);

            OleDbDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                ManufacturerComboBox.Items.Add(
                    reader["Наименование"].ToString());
            }

            reader.Close();
        }

        private void LoadProduct()
        {
            using (OleDbConnection connection =
                DatabaseService.GetConnection())
            {
                connection.Open();

                string query =
                    "SELECT * FROM Товар WHERE Артикул = ?";

                OleDbCommand command =
                    new OleDbCommand(query, connection);

                command.Parameters.AddWithValue("@p1", editArticle);

                OleDbDataReader reader =
                    command.ExecuteReader();

                if (reader.Read())
                {
                    ArticleTextBox.Text =
                        reader["Артикул"].ToString();

                    NameTextBox.Text =
                        reader["Наименование товара"].ToString();

                    UnitTextBox.Text =
                        reader["Единица измерения"].ToString();

                    PriceTextBox.Text =
                        reader["Цена"].ToString();

                    DiscountTextBox.Text =
                        reader["Действующая скидка"].ToString();

                    CountTextBox.Text =
                        reader["Кол-во на складе"].ToString();

                    DescriptionTextBox.Text =
                        reader["Описание товара"].ToString();

                    PhotoTextBox.Text =
                        reader["Фото"].ToString();

                    CategoryComboBox.Text =
                        reader["Категория товара"].ToString();

                    SupplierComboBox.Text =
                        reader["Поставщик"].ToString();

                    ManufacturerComboBox.Text =
                        reader["Производитель"].ToString();
                }

                reader.Close();
            }

            ArticleTextBox.IsEnabled = false;
        }

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (editArticle == null)
            {
                AddProduct();
            }
            else
            {
                EditProduct();
            }
        }

        private void AddProduct()
        {
            using (OleDbConnection connection =
                DatabaseService.GetConnection())
            {
                connection.Open();

                string query =
                    @"INSERT INTO Товар
                    (
                        Артикул,
                        [Наименование товара],
                        [Единица измерения],
                        Цена,
                        Поставщик,
                        Производитель,
                        [Категория товара],
                        [Действующая скидка],
                        [Кол-во на складе],
                        [Описание товара],
                        Фото
                    )
                    VALUES
                    (
                        ?,?,?,?,?,?,?,?,?,?,?
                    )";

                OleDbCommand command =
                    new OleDbCommand(query, connection);

                command.Parameters.AddWithValue("@p1", ArticleTextBox.Text);
                command.Parameters.AddWithValue("@p2", NameTextBox.Text);
                command.Parameters.AddWithValue("@p3", UnitTextBox.Text);
                command.Parameters.AddWithValue("@p4", PriceTextBox.Text);
                command.Parameters.AddWithValue("@p5", SupplierComboBox.Text);
                command.Parameters.AddWithValue("@p6", ManufacturerComboBox.Text);
                command.Parameters.AddWithValue("@p7", CategoryComboBox.Text);
                command.Parameters.AddWithValue("@p8", DiscountTextBox.Text);
                command.Parameters.AddWithValue("@p9", CountTextBox.Text);
                command.Parameters.AddWithValue("@p10", DescriptionTextBox.Text);
                command.Parameters.AddWithValue("@p11", PhotoTextBox.Text);

                command.ExecuteNonQuery();
            }

            MessageBox.Show("Товар добавлен");

            Close();
        }

        private void EditProduct()
        {
            using (OleDbConnection connection =
                DatabaseService.GetConnection())
            {
                connection.Open();

                string query =
                    @"UPDATE Товар SET
                        [Наименование товара] = ?,
                        [Единица измерения] = ?,
                        Цена = ?,
                        Поставщик = ?,
                        Производитель = ?,
                        [Категория товара] = ?,
                        [Действующая скидка] = ?,
                        [Кол-во на складе] = ?,
                        [Описание товара] = ?,
                        Фото = ?
                      WHERE Артикул = ?";

                OleDbCommand command =
                    new OleDbCommand(query, connection);

                command.Parameters.AddWithValue("@p1", NameTextBox.Text);
                command.Parameters.AddWithValue("@p2", UnitTextBox.Text);
                command.Parameters.AddWithValue("@p3", PriceTextBox.Text);
                command.Parameters.AddWithValue("@p4", SupplierComboBox.Text);
                command.Parameters.AddWithValue("@p5", ManufacturerComboBox.Text);
                command.Parameters.AddWithValue("@p6", CategoryComboBox.Text);
                command.Parameters.AddWithValue("@p7", DiscountTextBox.Text);
                command.Parameters.AddWithValue("@p8", CountTextBox.Text);
                command.Parameters.AddWithValue("@p9", DescriptionTextBox.Text);
                command.Parameters.AddWithValue("@p10", PhotoTextBox.Text);
                command.Parameters.AddWithValue("@p11", editArticle);

                command.ExecuteNonQuery();
            }

            MessageBox.Show("Товар изменён");

            Close();
        }
    }
}
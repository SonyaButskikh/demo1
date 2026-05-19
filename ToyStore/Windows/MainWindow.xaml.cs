using System;
using System.Data;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ToyStore.Classes;

namespace ToyStore.Windows
{
    public partial class MainWindow : Window
    {
        private DataTable productsTable = new DataTable();
        private string currentRole;
        private string currentFio;

        public MainWindow(string role, string fio)
        {
            InitializeComponent();

            currentRole = role;
            currentFio = fio;

            UserTextBlock.Text = currentFio + " (" + currentRole + ")";

            LoadProducts();
            LoadFilters();
            LoadSorts();
            ApplyRoleRules();
        }

        private void LoadProducts()
        {
            using (OleDbConnection connection =
                DatabaseService.GetConnection())
            {
                connection.Open();

                string query = "SELECT * FROM Товар";

                OleDbDataAdapter adapter =
                    new OleDbDataAdapter(query, connection);

                productsTable.Clear();
                productsTable.Columns.Clear();

                adapter.Fill(productsTable);

                if (!productsTable.Columns.Contains("СтараяЦена"))
                {
                    productsTable.Columns.Add("СтараяЦена", typeof(string));
                }

                if (!productsTable.Columns.Contains("НоваяЦена"))
                {
                    productsTable.Columns.Add("НоваяЦена", typeof(string));
                }

                foreach (DataRow row in productsTable.Rows)
                {
                    string imageName = row["Фото"].ToString();

                    if (!string.IsNullOrWhiteSpace(imageName))
                    {
                        row["Фото"] =
                            new Uri(
                                "pack://application:,,,/Images/" + imageName,
                                UriKind.Absolute);
                    }
                    else
                    {
                        row["Фото"] =
                            new Uri(
                                "pack://application:,,,/Images/picture.png",
                                UriKind.Absolute);
                    }

                    decimal price = 0;
                    int discount = 0;

                    decimal.TryParse(row["Цена"].ToString(), out price);
                    int.TryParse(row["Действующая скидка"].ToString(), out discount);

                    if (discount > 0)
                    {
                        decimal newPrice =
                            price - price * discount / 100;

                        row["СтараяЦена"] =
                            price.ToString("0.00") + " ₽";

                        row["НоваяЦена"] =
                            newPrice.ToString("0.00") + " ₽";
                    }
                    else
                    {
                        row["СтараяЦена"] = "";
                        row["НоваяЦена"] =
                            price.ToString("0.00") + " ₽";
                    }
                }

                ProductsDataGrid.ItemsSource = null;
                ProductsDataGrid.ItemsSource = productsTable.DefaultView;

                ApplyFilters();
            }
        }

        private void LoadFilters()
        {
            FilterComboBox.Items.Clear();
            FilterComboBox.Items.Add("Все поставщики");

            using (OleDbConnection connection = DatabaseService.GetConnection())
            {
                connection.Open();

                string query = "SELECT Наименование FROM Поставщик";

                OleDbCommand command = new OleDbCommand(query, connection);
                OleDbDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    FilterComboBox.Items.Add(reader["Наименование"].ToString());
                }
            }

            FilterComboBox.SelectedIndex = 0;
        }

        private void LoadSorts()
        {
            SortComboBox.Items.Clear();

            SortComboBox.Items.Add("Без сортировки");
            SortComboBox.Items.Add("Цена по возрастанию");
            SortComboBox.Items.Add("Цена по убыванию");
            SortComboBox.Items.Add("Остаток по возрастанию");
            SortComboBox.Items.Add("Остаток по убыванию");

            SortComboBox.SelectedIndex = 0;
        }

        private void ApplyRoleRules()
        {
            AddButton.Visibility = Visibility.Collapsed;
            EditButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;

            SearchTextBlock.Visibility = Visibility.Collapsed;
            SearchTextBox.Visibility = Visibility.Collapsed;
            FilterTextBlock.Visibility = Visibility.Collapsed;
            FilterComboBox.Visibility = Visibility.Collapsed;
            SortTextBlock.Visibility = Visibility.Collapsed;
            SortComboBox.Visibility = Visibility.Collapsed;

            if (currentRole == "Администратор")
            {
                AddButton.Visibility = Visibility.Visible;
                EditButton.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;

                SearchTextBlock.Visibility = Visibility.Visible;
                SearchTextBox.Visibility = Visibility.Visible;
                FilterTextBlock.Visibility = Visibility.Visible;
                FilterComboBox.Visibility = Visibility.Visible;
                SortTextBlock.Visibility = Visibility.Visible;
                SortComboBox.Visibility = Visibility.Visible;
            }

            if (currentRole == "Менеджер")
            {
                SearchTextBlock.Visibility = Visibility.Visible;
                SearchTextBox.Visibility = Visibility.Visible;
                FilterTextBlock.Visibility = Visibility.Visible;
                FilterComboBox.Visibility = Visibility.Visible;
                SortTextBlock.Visibility = Visibility.Visible;
                SortComboBox.Visibility = Visibility.Visible;
            }
        }

        private void ProductsDataGrid_LoadingRow(
    object sender,
    DataGridRowEventArgs e)
        {
            DataRowView rowView = e.Row.Item as DataRowView;

            if (rowView == null)
            {
                return;
            }

            int discount = 0;
            int count = 0;

            int.TryParse(
                rowView["Действующая скидка"].ToString(),
                out discount);

            int.TryParse(
                rowView["Кол-во на складе"].ToString(),
                out count);

            if (count == 0)
            {
                e.Row.Background =
                    new SolidColorBrush(Color.FromRgb(173, 216, 230));
            }
            else if (discount > 17)
            {
                e.Row.Background =
                    new SolidColorBrush(Color.FromRgb(255, 222, 173));
            }
            else
            {
                e.Row.Background = Brushes.White;
            }
        }

        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void FilterComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (productsTable.Rows.Count == 0)
            {
                CountTextBlock.Text = "Количество товаров: 0";
                return;
            }

            string search = SearchTextBox.Text.Replace("'", "''");
            string filter = "";

            if (!string.IsNullOrWhiteSpace(search))
            {
                filter =
                    "Артикул LIKE '%" + search + "%'" +
                    " OR [Наименование товара] LIKE '%" + search + "%'" +
                    " OR [Описание товара] LIKE '%" + search + "%'" +
                    " OR [Категория товара] LIKE '%" + search + "%'" +
                    " OR Поставщик LIKE '%" + search + "%'" +
                    " OR Производитель LIKE '%" + search + "%'";
            }

            if (FilterComboBox.SelectedIndex > 0)
            {
                string supplier =
                    FilterComboBox.SelectedItem.ToString().Replace("'", "''");

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    filter = "(" + filter + ") AND Поставщик = '" + supplier + "'";
                }
                else
                {
                    filter = "Поставщик = '" + supplier + "'";
                }
            }

            productsTable.DefaultView.RowFilter = filter;

            if (SortComboBox.SelectedIndex == 0)
            {
                productsTable.DefaultView.Sort = "";
            }
            else if (SortComboBox.SelectedIndex == 1)
            {
                productsTable.DefaultView.Sort = "Цена ASC";
            }
            else if (SortComboBox.SelectedIndex == 2)
            {
                productsTable.DefaultView.Sort = "Цена DESC";
            }
            else if (SortComboBox.SelectedIndex == 3)
            {
                productsTable.DefaultView.Sort = "[Кол-во на складе] ASC";
            }
            else if (SortComboBox.SelectedIndex == 4)
            {
                productsTable.DefaultView.Sort = "[Кол-во на складе] DESC";
            }

            CountTextBlock.Text =
                "Количество товаров: " + productsTable.DefaultView.Count;
        }

        private void AddButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            AddEditWindow window = new AddEditWindow();

            window.ShowDialog();

            LoadProducts();
        }

        private void EditButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар для редактирования");
                return;
            }

            DataRowView selectedRow =
                ProductsDataGrid.SelectedItem as DataRowView;

            string article = selectedRow["Артикул"].ToString();

            AddEditWindow window = new AddEditWindow(article);

            window.ShowDialog();

            LoadProducts();
        }

        private void DeleteButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар для удаления");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Удалить выбранный товар?",
                "Удаление",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            DataRowView selectedRow =
                ProductsDataGrid.SelectedItem as DataRowView;

            string article = selectedRow["Артикул"].ToString();

            using (OleDbConnection connection = DatabaseService.GetConnection())
            {
                connection.Open();

                string query = "DELETE FROM Товар WHERE Артикул = ?";

                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@p1", article);

                command.ExecuteNonQuery();
            }

            MessageBox.Show("Товар удалён");

            LoadProducts();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow window = new LoginWindow();
            window.Show();

            Close();
        }
    }
}
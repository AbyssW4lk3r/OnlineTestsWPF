using System.Windows;
using System.Windows.Input;
using OnlineTests.Services;

namespace OnlineTests.Windows
{
    public partial class StudentWindow : Window
    {
        private int userId;
        private DatabaseService db = new DatabaseService();

        public StudentWindow(int userId, string fullName)
        {
            InitializeComponent();
            this.userId = userId;
            WelcomeText.Text = $"Привет, {fullName}!";
            LoadTests();
            LoadResults();
        }

        private void LoadTests()
        {
            TestsListView.ItemsSource = db.GetAvailableTests();
        }

        private void LoadResults()
        {
            ResultsListView.ItemsSource = db.GetStudentResults(userId);
        }

        private void TestsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = TestsListView.SelectedItem as Models.Test;
            if (selected != null)
            {
                new TestPassingWindow(userId, selected.Id, selected.Title).ShowDialog();
                LoadResults();
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
    }
}
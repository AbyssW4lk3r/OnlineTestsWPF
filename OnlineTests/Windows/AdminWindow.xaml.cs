using OnlineTests.Models;
using OnlineTests.Services;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OnlineTests.Windows
{
    public partial class AdminWindow : Window
    {
        private DatabaseService db = new DatabaseService();
        private bool isLoaded = false;

        public AdminWindow(string fullName)
        {
            InitializeComponent();
            WelcomeText.Text = $"Админ: {fullName}";
            this.Loaded += AdminWindow_Loaded;
        }

        private void AdminWindow_Loaded(object sender, RoutedEventArgs e)
        {
            isLoaded = true;
            LoadStudents();
        }

        private void LoadStudents()
        {
            if (!isLoaded) return;
            try { StudentsListView.ItemsSource = db.GetAllStudents(); }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
        }

        private void LoadTests()
        {
            if (!isLoaded) return;
            try { TestsListView.ItemsSource = db.GetAllTests(); }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
        }

        private void LoadResults(string sort = "date")
        {
            if (!isLoaded) return;
            try { ResultsListView.ItemsSource = db.GetAllResults(sort); }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isLoaded) return;
            if (StudentsTab.IsSelected) LoadStudents();
            else if (TestsTab.IsSelected) LoadTests();
            else if (ResultsTab.IsSelected) LoadResults("date");
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isLoaded) return;
            if (SortComboBox != null && SortComboBox.SelectedItem is ComboBoxItem item)
                LoadResults(item.Tag.ToString());
        }

        private void RefreshStudentsButton_Click(object sender, RoutedEventArgs e) => LoadStudents();

        private void DeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            int studentId = (int)btn.Tag;
            var student = (StudentsListView.ItemsSource as System.Collections.IList)
                .Cast<StudentView>().FirstOrDefault(s => s.Id == studentId);

            string name = student?.FullName ?? $"ID {studentId}";
            if (MessageBox.Show($"Удалить студента \"{name}\"?\nВсе его результаты тоже удалятся!",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                db.DeleteStudent(studentId);
                LoadStudents();
            }
        }

        private void RefreshTestsButton_Click(object sender, RoutedEventArgs e) => LoadTests();

        private void AddTestButton_Click(object sender, RoutedEventArgs e)
        {
            var w = new Window
            {
                Title = "Добавить тест",
                Width = 400,
                Height = 280,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x2F))
            };
            var sp = new StackPanel { Margin = new Thickness(20) };
            sp.Children.Add(new TextBlock { Text = "Название:", Foreground = Brushes.White });
            var tb = new TextBox { Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x44)), Foreground = Brushes.White, Height = 28, Margin = new Thickness(0, 5, 0, 15) };
            sp.Children.Add(tb);
            sp.Children.Add(new TextBlock { Text = "Описание:", Foreground = Brushes.White });
            var dbx = new TextBox { Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x44)), Foreground = Brushes.White, Height = 60, TextWrapping = TextWrapping.Wrap };
            sp.Children.Add(dbx);
            var btn = new Button { Content = "СОХРАНИТЬ", Height = 35, Background = new SolidColorBrush(Color.FromRgb(0x7B, 0x1F, 0xA2)), Foreground = Brushes.White, Margin = new Thickness(0, 20, 0, 0) };
            btn.Click += (s, ev) => { if (!string.IsNullOrWhiteSpace(tb.Text)) { db.AddTest(tb.Text, dbx.Text); LoadTests(); w.Close(); } };
            sp.Children.Add(btn);
            w.Content = sp;
            w.ShowDialog();
        }

        private void EditTest_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            int testId = (int)btn.Tag;
            var test = (TestsListView.ItemsSource as System.Collections.IList)
                .Cast<TestView>().FirstOrDefault(t => t.Id == testId);
            if (test != null)
            {
                new TestEditorWindow(test.Id, test.Title, test.Description).ShowDialog();
                LoadTests();
            }
        }

        private void DeleteTest_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            int testId = (int)btn.Tag;
            var test = (TestsListView.ItemsSource as System.Collections.IList)
                .Cast<TestView>().FirstOrDefault(t => t.Id == testId);
            string title = test?.Title ?? $"ID {testId}";
            if (MessageBox.Show($"Удалить тест \"{title}\"?\nВсе вопросы и результаты тоже удалятся!",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                db.DeleteTest(testId);
                LoadTests();
                LoadResults();
            }
        }

        private void RefreshResultsButton_Click(object sender, RoutedEventArgs e) => LoadResults("date");

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var results = db.GetAllResults();
                var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "CSV|*.csv", FileName = $"Результаты_{DateTime.Now:yyyyMMdd_HHmm}.csv" };
                if (dlg.ShowDialog() == true)
                {
                    using (var w = new StreamWriter(dlg.FileName, false, System.Text.Encoding.UTF8))
                    {
                        w.WriteLine("Студент;Тест;Дата;Баллы;Процент");
                        foreach (var r in results)
                            w.WriteLine($"{r.StudentName};{r.TestTitle};{r.AttemptDate};{r.Score};{r.PercentCorrect}");
                    }
                    MessageBox.Show("Экспортировано!");
                }
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
    }
}
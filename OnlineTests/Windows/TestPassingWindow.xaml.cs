using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OnlineTests.Models;
using OnlineTests.Services;

namespace OnlineTests.Windows
{
    public partial class TestPassingWindow : Window
    {
        private int userId;
        private int testId;
        private DatabaseService db = new DatabaseService();
        private List<QuestionControl> controls = new List<QuestionControl>();

        public TestPassingWindow(int userId, int testId, string title)
        {
            InitializeComponent();
            this.userId = userId;
            this.testId = testId;
            TestTitleText.Text = title;
            LoadQuestions();
        }

        private void LoadQuestions()
        {
            foreach (var q in db.GetQuestionsByTestId(testId))
            {
                var qc = new QuestionControl(db, q.Id, q.QuestionText, q.QuestionType);
                foreach (var a in db.GetAnswersByQuestionId(q.Id))
                    qc.AddAnswer(a.Id, a.AnswerText);
                controls.Add(qc);
                QuestionsPanel.Children.Add(qc);
            }
        }

        private void FinishTestButton_Click(object sender, RoutedEventArgs e)
        {
            int correct = 0;
            foreach (var c in controls)
                if (c.IsCorrect()) correct++;

            decimal percent = controls.Count > 0 ? (decimal)correct / controls.Count * 100 : 0;
            db.SaveResult(userId, testId, correct, percent);

            MessageBox.Show($"Тест завершён!\nПравильных: {correct} из {controls.Count}\nПроцент: {percent:F2}%", "Результат");
            this.Close();
        }
    }

    public class QuestionControl : StackPanel
    {
        private DatabaseService db;
        private int questionId;
        private List<RadioButton> radios = new List<RadioButton>();
        private List<int> answerIds = new List<int>();

        public QuestionControl(DatabaseService db, int id, string text, string type)
        {
            this.db = db;
            questionId = id;
            this.Margin = new Thickness(0, 0, 0, 20);
            this.Children.Add(new TextBlock
            {
                Text = $"[{type}] {text}",
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            });
        }

        public void AddAnswer(int answerId, string text)
        {
            var rb = new RadioButton
            {
                Content = text,
                Foreground = Brushes.LightGray,
                Margin = new Thickness(20, 2, 0, 2),
                GroupName = $"Q{questionId}"
            };
            radios.Add(rb);
            answerIds.Add(answerId);
            this.Children.Add(rb);
        }

        public bool IsCorrect()
        {
            for (int i = 0; i < radios.Count; i++)
                if (radios[i].IsChecked == true)
                    return db.IsAnswerCorrect(answerIds[i]);
            return false;
        }
    }
}
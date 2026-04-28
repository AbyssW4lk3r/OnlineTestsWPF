using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OnlineTests.Models;
using OnlineTests.Services;

namespace OnlineTests.Windows
{
    public partial class TestEditorWindow : Window
    {
        private int testId;
        private DatabaseService db = new DatabaseService();

        private static readonly SolidColorBrush BgDark = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x2F));
        private static readonly SolidColorBrush BgMedium = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x44));
        private static readonly SolidColorBrush AccentPurple = new SolidColorBrush(Color.FromRgb(0x7B, 0x1F, 0xA2));
        private static readonly SolidColorBrush AccentGreen = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
        private static readonly SolidColorBrush AccentRed = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));

        public TestEditorWindow(int testId, string title, string desc)
        {
            InitializeComponent();
            this.testId = testId;
            TestTitleText.Text = title;
            TestDescText.Text = desc;
            LoadQuestions();
        }

        private void LoadQuestions()
        {
            QuestionsPanel.Children.Clear();
            foreach (var q in db.GetQuestionsByTestId(testId))
            {
                var qc = new QuestionEditorControl(db, testId, q.Id, q.QuestionText, q.QuestionType);
                qc.OnDeleted += LoadQuestions;
                foreach (var a in db.GetAnswersByQuestionId(q.Id))
                    qc.AddAnswer(a.Id, a.AnswerText, a.IsCorrect);
                QuestionsPanel.Children.Add(qc);
            }
        }

        private void AddQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            var w = new Window
            {
                Title = "Новый вопрос",
                Width = 500,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = BgDark
            };
            var sp = new StackPanel { Margin = new Thickness(20) };
            var tb = new TextBox
            {
                Background = BgMedium,
                Foreground = Brushes.White,
                Height = 80,
                TextWrapping = TextWrapping.Wrap
            };
            var cb = new ComboBox
            {
                Background = BgMedium,
                Foreground = Brushes.White,
                Height = 25
            };
            cb.Items.Add("single");
            cb.Items.Add("multiple");
            cb.SelectedIndex = 0;

            var ap = new StackPanel();
            var inputs = new List<AnswerInput>();
            var addBtn = new Button
            {
                Content = "+ Ответ",
                Height = 25,
                Background = AccentGreen,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 10, 0, 0)
            };

            addBtn.Click += (s, ev) =>
            {
                var inp = new AnswerInput();
                inputs.Add(inp);
                ap.Children.Add(inp);
            };

            for (int i = 0; i < 2; i++)
            {
                var inp = new AnswerInput();
                inputs.Add(inp);
                ap.Children.Add(inp);
            }

            var saveBtn = new Button
            {
                Content = "СОХРАНИТЬ",
                Height = 35,
                Background = AccentPurple,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 15, 0, 0)
            };
            saveBtn.Click += (s, ev) =>
            {
                if (!string.IsNullOrWhiteSpace(tb.Text))
                {
                    int qid = db.AddQuestion(testId, tb.Text, cb.SelectedItem.ToString());
                    foreach (var inp in inputs)
                        if (!string.IsNullOrWhiteSpace(inp.Text))
                            db.AddAnswer(qid, inp.Text, inp.IsCorrect);
                    LoadQuestions();
                    w.Close();
                }
            };

            sp.Children.Add(new TextBlock { Text = "Текст вопроса:", Foreground = Brushes.White });
            sp.Children.Add(tb);
            sp.Children.Add(new TextBlock { Text = "Тип:", Foreground = Brushes.White, Margin = new Thickness(0, 10, 0, 0) });
            sp.Children.Add(cb);
            sp.Children.Add(new TextBlock { Text = "Ответы:", Foreground = Brushes.White, Margin = new Thickness(0, 10, 0, 0) });
            sp.Children.Add(ap);
            sp.Children.Add(addBtn);
            sp.Children.Add(saveBtn);
            w.Content = new ScrollViewer { Content = sp, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            w.ShowDialog();
        }
    }

    public class QuestionEditorControl : Border
    {
        private DatabaseService db;
        private StackPanel ap;
        private int qid;
        public event System.Action OnDeleted;

        private static readonly SolidColorBrush BgMedium = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x44));
        private static readonly SolidColorBrush AccentGreen = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
        private static readonly SolidColorBrush AccentRed = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));
        private static readonly SolidColorBrush AccentPurple = new SolidColorBrush(Color.FromRgb(0x7B, 0x1F, 0xA2));
        private static readonly SolidColorBrush BgDark = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x2F));

        public QuestionEditorControl(DatabaseService db, int tid, int qid, string text, string type)
        {
            this.db = db;
            this.qid = qid;
            Background = BgMedium;
            Margin = new Thickness(0, 0, 0, 10);
            Padding = new Thickness(10);
            CornerRadius = new CornerRadius(5);

            var sp = new StackPanel();
            var hg = new Grid();
            hg.ColumnDefinitions.Add(new ColumnDefinition());
            hg.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

            var qt = new TextBlock
            {
                Text = $"[{type}] {text}",
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetColumn(qt, 0);

            var delBtn = new Button
            {
                Content = "УДАЛИТЬ",
                Height = 25,
                Background = AccentRed,
                Foreground = Brushes.White,
                FontSize = 10
            };
            Grid.SetColumn(delBtn, 1);

            delBtn.Click += (s, e) =>
            {
                if (MessageBox.Show("Удалить вопрос и все ответы?", "?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    db.DeleteQuestion(qid);
                    OnDeleted?.Invoke();
                }
            };

            hg.Children.Add(qt);
            hg.Children.Add(delBtn);
            sp.Children.Add(hg);

            ap = new StackPanel { Margin = new Thickness(20, 0, 0, 0) };
            sp.Children.Add(ap);

            var addA = new Button
            {
                Content = "+ Ответ",
                Height = 25,
                Width = 120,
                Background = AccentGreen,
                Foreground = Brushes.White,
                FontSize = 10,
                Margin = new Thickness(20, 5, 0, 0)
            };
            addA.Click += (s, e) =>
            {
                var d = new Window
                {
                    Title = "Ответ",
                    Width = 350,
                    Height = 180,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Background = BgDark
                };
                var p = new StackPanel { Margin = new Thickness(20) };
                var tb = new TextBox { Background = BgMedium, Foreground = Brushes.White, Height = 25 };
                var ch = new CheckBox { Content = "Правильный", Foreground = Brushes.White, Margin = new Thickness(0, 10, 0, 0) };
                var sb = new Button
                {
                    Content = "СОХРАНИТЬ",
                    Height = 30,
                    Background = AccentPurple,
                    Foreground = Brushes.White,
                    Margin = new Thickness(0, 15, 0, 0)
                };
                sb.Click += (ss, ee) =>
                {
                    if (!string.IsNullOrWhiteSpace(tb.Text))
                    {
                        int aid = db.AddAnswer(qid, tb.Text, ch.IsChecked ?? false);
                        AddAnswer(aid, tb.Text, ch.IsChecked ?? false);
                        d.Close();
                    }
                };
                p.Children.Add(new TextBlock { Text = "Текст:", Foreground = Brushes.White });
                p.Children.Add(tb);
                p.Children.Add(ch);
                p.Children.Add(sb);
                d.Content = p;
                d.ShowDialog();
            };

            sp.Children.Add(addA);
            Child = sp;
        }

        public void AddAnswer(int aid, string text, bool correct)
        {
            var ac = new AnswerEditControl(db, qid, aid, text, correct);
            ac.OnDeleted += () => ap.Children.Remove(ac);
            ap.Children.Add(ac);
        }
    }

    public class AnswerEditControl : Grid
    {
        private DatabaseService db;
        private int aid;
        public event System.Action OnDeleted;

        private static readonly SolidColorBrush AccentRed = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));

        public AnswerEditControl(DatabaseService db, int qid, int aid, string text, bool correct)
        {
            this.db = db;
            this.aid = aid;
            Margin = new Thickness(0, 2, 0, 2);
            ColumnDefinitions.Add(new ColumnDefinition());
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });

            var tb = new TextBlock
            {
                Text = text,
                Foreground = correct ? Brushes.LightGreen : Brushes.LightGray,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(tb, 0);

            var cb = new CheckBox
            {
                Content = "Верный",
                Foreground = Brushes.White,
                IsChecked = correct,
                VerticalAlignment = VerticalAlignment.Center
            };
            cb.Checked += (s, e) => db.UpdateAnswerCorrect(aid, true);
            cb.Unchecked += (s, e) => db.UpdateAnswerCorrect(aid, false);
            Grid.SetColumn(cb, 1);

            var del = new Button
            {
                Content = "❌",
                Height = 20,
                Width = 40,
                Background = Brushes.Transparent,
                Foreground = AccentRed,
                FontSize = 10
            };
            del.Click += (s, e) =>
            {
                db.DeleteAnswer(aid);
                OnDeleted?.Invoke();
            };
            Grid.SetColumn(del, 2);

            Children.Add(tb);
            Children.Add(cb);
            Children.Add(del);
        }
    }

    public class AnswerInput : Grid
    {
        public string Text => tb.Text;
        public bool IsCorrect => cb.IsChecked ?? false;
        private TextBox tb;
        private CheckBox cb;

        private static readonly SolidColorBrush BgMedium = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x44));

        public AnswerInput()
        {
            Margin = new Thickness(0, 3, 0, 0);
            ColumnDefinitions.Add(new ColumnDefinition());
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            tb = new TextBox
            {
                Background = BgMedium,
                Foreground = Brushes.White,
                Height = 25,
                Margin = new Thickness(0, 0, 5, 0)
            };
            cb = new CheckBox
            {
                Content = "Верный",
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(tb, 0);
            Grid.SetColumn(cb, 1);
            Children.Add(tb);
            Children.Add(cb);
        }
    }
}
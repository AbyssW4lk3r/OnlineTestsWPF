using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineTests.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Login { get; set; }
        public string Role { get; set; }
    }

    public class Test
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class TestView
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string MaxScore { get; set; }
        public int QuestionCount { get; set; }
    }

    public class Question
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
    }

    public class Answer
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string AnswerText { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class StudentView
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Login { get; set; }
        public string Phone { get; set; }
        public string BirthDate { get; set; }
    }

    public class ResultView
    {
        public string TestTitle { get; set; }
        public string AttemptDate { get; set; }
        public string Score { get; set; }
        public string PercentCorrect { get; set; }
    }

    public class ResultAdminView
    {
        public string StudentName { get; set; }
        public string TestTitle { get; set; }
        public string AttemptDate { get; set; }
        public string Score { get; set; }
        public string PercentCorrect { get; set; }
    }
}
using OnlineTests.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace OnlineTests.Services
{
    public class DatabaseService
    {
        private readonly string connectionString = @"Server=.\SQLEXPRESS;Database=dota_test_db;Integrated Security=True;TrustServerCertificate=True;";

        public User AuthenticateUser(string login, string password)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, full_name, role FROM users WHERE login = @login AND password = @password";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", password);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new User { Id = reader.GetInt32(0), FullName = reader.GetString(1), Role = reader.GetString(2) };
                }
            }
            return null;
        }

        public List<StudentView> GetAllStudents()
        {
            List<StudentView> students = new List<StudentView>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, full_name, login, phone, birth_date FROM users WHERE role = 'student' ORDER BY full_name";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    students.Add(new StudentView
                    {
                        Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        FullName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        Login = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Phone = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        BirthDate = reader.IsDBNull(4) ? "" : reader.GetDateTime(4).ToString("dd.MM.yyyy")
                    });
                }
            }

            return students;
        }

        public void DeleteStudent(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM results WHERE user_id = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();

                query = "DELETE FROM users WHERE id = @id AND role = 'student'";
                cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Test> GetAvailableTests()
        {
            List<Test> tests = new List<Test>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, title, description FROM tests";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    tests.Add(new Test { Id = reader.GetInt32(0), Title = reader.GetString(1), Description = reader.IsDBNull(2) ? "" : reader.GetString(2) });
                }
            }
            return tests;
        }

        public List<TestView> GetAllTests()
        {
            List<TestView> tests = new List<TestView>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"SELECT t.id, t.title, t.description, t.max_score, 
                       (SELECT COUNT(*) FROM questions WHERE test_id = t.id) as qc 
                       FROM tests t";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tests.Add(new TestView
                    {
                        Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        Title = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        MaxScore = reader.IsDBNull(3) ? "0" : reader.GetInt32(3).ToString(),
                        QuestionCount = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                    });
                }
            }

            return tests;
        }

        public void AddTest(string title, string description)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO tests (title, description) VALUES (@t, @d)", conn);
                cmd.Parameters.AddWithValue("@t", title);
                cmd.Parameters.AddWithValue("@d", description ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteTest(int testId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM tests WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", testId);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Question> GetQuestionsByTestId(int testId)
        {
            List<Question> questions = new List<Question>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT id, test_id, question_text, question_type FROM questions WHERE test_id = @tid ORDER BY id", conn);
                cmd.Parameters.AddWithValue("@tid", testId);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    questions.Add(new Question { Id = reader.GetInt32(0), TestId = reader.GetInt32(1), QuestionText = reader.GetString(2), QuestionType = reader.GetString(3) });
                }
            }
            return questions;
        }

        public int AddQuestion(int testId, string questionText, string questionType)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO questions (test_id, question_text, question_type) VALUES (@tid, @t, @tp); SELECT SCOPE_IDENTITY();", conn);
                cmd.Parameters.AddWithValue("@tid", testId);
                cmd.Parameters.AddWithValue("@t", questionText);
                cmd.Parameters.AddWithValue("@tp", questionType);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void DeleteQuestion(int questionId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM questions WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", questionId);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Answer> GetAnswersByQuestionId(int questionId)
        {
            List<Answer> answers = new List<Answer>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT id, question_id, answer_text, is_correct FROM answers WHERE question_id = @qid ORDER BY id", conn);
                cmd.Parameters.AddWithValue("@qid", questionId);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    answers.Add(new Answer { Id = reader.GetInt32(0), QuestionId = reader.GetInt32(1), AnswerText = reader.GetString(2), IsCorrect = reader.GetBoolean(3) });
                }
            }
            return answers;
        }

        public int AddAnswer(int questionId, string answerText, bool isCorrect)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO answers (question_id, answer_text, is_correct) VALUES (@qid, @t, @c); SELECT SCOPE_IDENTITY();", conn);
                cmd.Parameters.AddWithValue("@qid", questionId);
                cmd.Parameters.AddWithValue("@t", answerText);
                cmd.Parameters.AddWithValue("@c", isCorrect);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void UpdateAnswerCorrect(int answerId, bool isCorrect)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("UPDATE answers SET is_correct = @c WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@c", isCorrect);
                cmd.Parameters.AddWithValue("@id", answerId);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteAnswer(int answerId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM answers WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", answerId);
                cmd.ExecuteNonQuery();
            }
        }

        public bool IsAnswerCorrect(int answerId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT is_correct FROM answers WHERE id = @aid", conn);
                cmd.Parameters.AddWithValue("@aid", answerId);
                return (bool)cmd.ExecuteScalar();
            }
        }

        public List<ResultView> GetStudentResults(int userId)
        {
            List<ResultView> results = new List<ResultView>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"SELECT t.title, r.attempt_date, r.score, r.percent_correct 
                               FROM results r JOIN tests t ON r.test_id = t.id 
                               WHERE r.user_id = @uid ORDER BY r.attempt_date DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@uid", userId);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new ResultView
                    {
                        TestTitle = reader.GetString(0),
                        AttemptDate = reader.GetDateTime(1).ToString("dd.MM.yyyy HH:mm"),
                        Score = reader.GetInt32(2).ToString(),
                        PercentCorrect = reader.GetDecimal(3).ToString("F2") + "%"
                    });
                }
            }
            return results;
        }

        public List<ResultAdminView> GetAllResults(string sortBy = "date")
        {
            List<ResultAdminView> results = new List<ResultAdminView>();

            string orderClause;
            switch (sortBy)
            {
                case "student":
                    orderClause = "ORDER BY u.full_name";
                    break;
                case "test":
                    orderClause = "ORDER BY t.title";
                    break;
                case "score":
                    orderClause = "ORDER BY r.score DESC";
                    break;
                default:
                    orderClause = "ORDER BY r.attempt_date DESC";
                    break;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = $@"SELECT u.full_name, t.title, r.attempt_date, r.score, r.percent_correct 
                        FROM results r 
                        JOIN users u ON r.user_id = u.id 
                        JOIN tests t ON r.test_id = t.id 
                        {orderClause}";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    results.Add(new ResultAdminView
                    {
                        StudentName = reader.IsDBNull(0) ? "" : reader.GetString(0),
                        TestTitle = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        AttemptDate = reader.IsDBNull(2) ? "" : reader.GetDateTime(2).ToString("dd.MM.yyyy HH:mm"),
                        Score = reader.IsDBNull(3) ? "0" : reader.GetInt32(3).ToString(),
                        PercentCorrect = reader.IsDBNull(4) ? "0%" : reader.GetDecimal(4).ToString("F2") + "%"
                    });
                }
            }

            return results;
        }

        public void SaveResult(int userId, int testId, int score, decimal percentCorrect)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO results (user_id, test_id, score, percent_correct) VALUES (@u, @t, @s, @p)", conn);
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@t", testId);
                cmd.Parameters.AddWithValue("@s", score);
                cmd.Parameters.AddWithValue("@p", percentCorrect);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteResult(int userId, int testId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM results WHERE user_id = @u AND test_id = @t", conn);
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@t", testId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
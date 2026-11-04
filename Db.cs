using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace activity3
{
    internal static class Db
    {
        private const string ConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=activity_DB;Integrated Security=True;TrustServerCertificate=True;";
        public static bool UsernameExists(string username)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM users WHERE username = @u", conn))
            {
                cmd.Parameters.AddWithValue("@u", username);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static void CreateUser(string username, string password)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(
                "INSERT INTO users (username, password_hash, role) VALUES (@u, @p, 'user')", conn))
            {
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", Hash(password));
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static bool ValidateUser(string username, string password)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT password_hash FROM users WHERE username = @u", conn))
            {
                cmd.Parameters.AddWithValue("@u", username);
                conn.Open();

                object result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    return false;
                }

                return string.Equals((string)result, Hash(password), StringComparison.OrdinalIgnoreCase);
            }
        }

        public static DataTable GetProducts()
        {
            const string sql = @"
                SELECT id AS No, code AS Code, product_name AS Product,
                       category AS Category, qty AS Qty, price AS Price
                FROM products
                ORDER BY id";

            var table = new DataTable();

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(sql, conn))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                adapter.Fill(table);
            }

            return table;
        }

        public static Dictionary<string, int> GetCategoryTotals()
        {
            var totals = new Dictionary<string, int>();

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT category, SUM(qty) FROM products GROUP BY category", conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        totals[reader.GetString(0)] = reader.GetInt32(1);
                    }
                }
            }

            return totals;
        }
        public static string Hash(string text)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
                var sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
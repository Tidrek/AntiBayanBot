using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace AntiBayanBot.Core.Dal
{
    public class StatisticsRepository
    {
        private static string ConnectionString => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;


        /// <summary>
        /// Get statistics with bayans count per user for chat.
        /// </summary>
        /// <param name="chatId">Chat ID.</param>
        /// <param name="limit">The amount of top bayaners of the chat (0 - all bayaners)</param>
        /// <returns>Dictionary, where key is the user ID and value is the amount of bayans posted to this chat.</returns>
        public Dictionary<long, int> GetChatStatistics(long chatId, int limit = 0)
        {
            var result = new Dictionary<long, int>();

            var query = new StringBuilder();
            query.Append("SELECT ");

            if (limit > 0)
            {
                query.Append("TOP " + limit + " ");
            }

            query.Append("UserId, Bayans FROM dbo.[Statistics] WHERE ChatId = @ChatId ORDER BY Bayans DESC");

            using (var connection = new SqlConnection(ConnectionString))
            {
                var command = new SqlCommand(query.ToString(), connection);
                command.Parameters.Add("@ChatId", SqlDbType.BigInt).Value = chatId;

                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        // Add user ID as key and his/her bayans count as value
                        result.Add(reader.GetInt64(0), reader.GetInt32(1));
                    }
                    reader.NextResult();
                }
            }

            return result;
        }


        /// <summary>
        /// Get bayans count of person in chat.
        /// </summary>
        /// <param name="chatId">Chat ID.</param>
        /// <param name="userId">User ID.</param>
        /// <returns>Bayans count.</returns>
        public int GetBayansCount(long chatId, long userId)
        {
            const string query = "SELECT TOP 1 Bayans FROM dbo.[Statistics] WHERE ChatId = @ChatId AND UserId = @UserId";
            object result;

            using (var connection = new SqlConnection(ConnectionString))
            {
                var command = new SqlCommand(query, connection);
                command.Parameters.Add("@ChatId", SqlDbType.BigInt).Value = chatId;
                command.Parameters.Add("@UserId", SqlDbType.BigInt).Value = userId;

                connection.Open();
                result = command.ExecuteScalar();
                connection.Close();
            }

            if (result == null)
                return 0;

            return (int) result;
        }

        /// <summary>
        /// Method for bayaners punishment - increments the bayans counter in statistics table.
        /// </summary>
        /// <param name="chatId">Chat ID.</param>
        /// <param name="userId">User ID.</param>
        /// <returns>New count of bayans.</returns>
        public int IncrementBayansCount(long chatId, long userId)
        {
            var bayans = GetBayansCount(chatId, userId);
            bayans++;

            var query =
                bayans == 1 ?
                "INSERT INTO dbo.[Statistics] (ChatId, UserId, Bayans) VALUES(@ChatId, @UserId, @Bayans)" :
                "UPDATE dbo.[Statistics] SET Bayans = @Bayans WHERE ChatId = @ChatId AND UserId = @UserId";

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@Bayans", SqlDbType.Int).Value = bayans;
                command.Parameters.Add("@ChatId", SqlDbType.BigInt).Value = chatId;
                command.Parameters.Add("@UserId", SqlDbType.BigInt).Value = userId;

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }

            return bayans;
        }
    }
}
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using AntiBayanBot.Core.Models;

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
        public List<Statistics> GetChatStatistics(long chatId, int limit = 0)
        {
            var result = limit > 0 ? new List<Statistics>(limit) : new List<Statistics>();

            var query = new StringBuilder();
            query.Append("SELECT ");

            if (limit > 0)
            {
                query.Append("TOP " + limit + " ");
            }

            query.Append("UserId, UserName, UserFullName, Bayans FROM dbo.[Statistics] WHERE ChatId = @ChatId ORDER BY Bayans DESC");

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
                        result.Add(new Statistics
                        {
                            UserId = reader.GetInt64(0),
                            UserName = reader.GetString(1),
                            UserFullName = reader.GetString(2),
                            Bayans = reader.GetInt32(3)
                        });
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
        /// <param name="messageData">Message data.</param>
        /// <returns>New count of bayans.</returns>
        public int IncrementBayansCount(MessageData messageData)
        {
            var bayans = GetBayansCount(messageData.ChatId, messageData.UserId);
            bayans++;

            var query =
                bayans == 1 ?
                "INSERT INTO dbo.[Statistics] (ChatId, UserId, UserName, UserFullName, Bayans) VALUES(@ChatId, @UserId, @UserName, @UserFullName, @Bayans)" :
                "UPDATE dbo.[Statistics] SET Bayans = @Bayans WHERE ChatId = @ChatId AND UserId = @UserId";

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@ChatId", SqlDbType.BigInt).Value = messageData.ChatId;
                command.Parameters.Add("@UserId", SqlDbType.BigInt).Value = messageData.UserId;
                command.Parameters.Add("@Bayans", SqlDbType.Int).Value = bayans;

                if (bayans == 1)
                {
                    command.Parameters.Add("@UserFullName", SqlDbType.NVarChar, 255).Value = messageData.UserFullName;
                    if (!string.IsNullOrEmpty(messageData.UserName))
                    {
                        command.Parameters.Add("@UserName", SqlDbType.NVarChar, 32).Value = messageData.UserName;
                    }
                }

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }

            return bayans;
        }
    }
}
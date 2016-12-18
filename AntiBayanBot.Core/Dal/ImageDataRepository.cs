using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using AntiBayanBot.Core.Models;

namespace AntiBayanBot.Core.Dal
{
    public class ImageDataRepository
    {
        private static string ConnectionString => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;


        /// <summary>
        /// Get all image data for selected chat from the DB.
        /// </summary>
        /// <returns>Image data list.</returns>
        public List<ImageData> GetForChat(int chatId)
        {
            var result = new List<ImageData>();

            using (var connection = new SqlConnection(ConnectionString))
            {
                var command = new SqlCommand("SELECT ChatId, UserId, Descriptors, DateTimeAdded FROM dbo.ImageData", connection);
                connection.Open();

                var reader = command.ExecuteReader();

                while (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        result.Add(new ImageData
                        {
                            ChatId = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            Descriptors = (byte[])reader.GetValue(2),
                            DateTimeAdded = reader.GetDateTime(3)
                        });
                    }

                    reader.NextResult();
                }
            }

            return result;
        }


        /// <summary>
        /// Saves image data to the DB.
        /// </summary>
        /// <param name="imageData">Image data.</param>
        public void Insert(ImageData imageData)
        {
            const string query =
                "INSERT INTO dbo.ImageData (ChatId, UserId, Descriptors, DateTimeAdded) VALUES (@ChatId, @UserId, @Descriptors, @DateTimeAdded)";

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@ChatId", SqlDbType.Int).Value = imageData.ChatId;
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = imageData.UserId;
                command.Parameters.Add("@Descriptors", SqlDbType.VarBinary).Value = imageData.Descriptors;
                command.Parameters.Add("@DateTimeAdded", SqlDbType.DateTime2).Value = imageData.DateTimeAdded;

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }
    }
}
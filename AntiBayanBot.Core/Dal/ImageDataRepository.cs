﻿using System;
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
        public List<ImageData> GetForChat(long chatId)
        {
            var result = new List<ImageData>();

            using (var connection = new SqlConnection(ConnectionString))
            {
                var command = new SqlCommand("SELECT MessageId, ChatId, UserId, Descriptors, DateTimeAdded, UserFullName, UserName FROM dbo.ImageData WHERE ChatId = @ChatId ORDER BY Id DESC", connection);
                command.CommandTimeout = 0;
                command.Parameters.Add("@ChatId", SqlDbType.BigInt).Value = chatId;

                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.HasRows && reader.Read())
                {
                    result.Add(new ImageData
                    {
                        MessageId = reader.GetInt32(0),
                        ChatId = reader.GetInt64(1),
                        UserId = reader.GetInt64(2),
                        Descriptors = (byte[])reader.GetValue(3),
                        DateTimeAdded = reader.GetDateTime(4),
                        UserFullName = reader.GetString(5),
                        UserName = reader.IsDBNull(6) ? null : reader.GetString(6)
                    });
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
            var query = string.IsNullOrEmpty(imageData.UserName) ?
                "INSERT INTO dbo.ImageData (MessageId, ChatId, UserId, Descriptors, DateTimeAdded, UserFullName) " +
                "VALUES (@MessageId, @ChatId, @UserId, @Descriptors, @DateTimeAdded, @UserFullName)" :
                "INSERT INTO dbo.ImageData (MessageId, ChatId, UserId, Descriptors, DateTimeAdded, UserFullName, UserName) " +
                "VALUES (@MessageId, @ChatId, @UserId, @Descriptors, @DateTimeAdded, @UserFullName, @UserName)";

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@MessageId", SqlDbType.Int).Value = imageData.MessageId;
                command.Parameters.Add("@ChatId", SqlDbType.BigInt).Value = imageData.ChatId;
                command.Parameters.Add("@UserId", SqlDbType.BigInt).Value = imageData.UserId;
                command.Parameters.Add("@Descriptors", SqlDbType.VarBinary).Value = imageData.Descriptors;
                command.Parameters.Add("@DateTimeAdded", SqlDbType.DateTime2).Value = imageData.DateTimeAdded;
                command.Parameters.Add("@UserFullName", SqlDbType.NVarChar, 255).Value = imageData.UserFullName;
                if (!string.IsNullOrEmpty(imageData.UserName))
                {
                    command.Parameters.Add("@UserName", SqlDbType.NVarChar, 32).Value = imageData.UserName;
                }
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public bool IsForwarded(DateTime dateForward, long userIdForward, long chatId)
        {
            var result = false;
            using (var connection = new SqlConnection(ConnectionString))
            {                
                var command = new SqlCommand(@"SELECT TOP 1 1 FROM ImageData
                                            WHERE DateTimeAdded=@dateForward
                                            AND UserId=@userIdForward
                                            AND ChatId=@chatId", connection);
                command.CommandTimeout = 0;

                command.Parameters.Add("@dateForward", SqlDbType.DateTime).Value = dateForward;
                command.Parameters.Add("@userIdForward", SqlDbType.BigInt).Value = userIdForward;
                command.Parameters.Add("@chatId", SqlDbType.BigInt).Value = chatId;
                connection.Open();
                var reader = command.ExecuteReader();
                result = reader.HasRows;
            }

            return result;
        }
    }
}
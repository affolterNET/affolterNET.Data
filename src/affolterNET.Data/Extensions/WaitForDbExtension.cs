using System;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Npgsql;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace affolterNET.Data.Extensions
{
    public static class WaitForDbExtension
    {
        public static void WaitForDbConnection(
            this string connstring,
            bool logFailures = true,
            TextWriter? outputWriter = null,
            int sleepTime = 500,
            int retries = 100,
            bool useNpgsql = false)
        {
            connstring.WaitForDbConnectionAsync(logFailures, outputWriter, sleepTime, retries, useNpgsql).GetAwaiter().GetResult();
        }

        public static async Task WaitForDbConnectionAsync(
            this string connstring,
            bool logFailures = true,
            TextWriter? outputWriter = null,
            int sleepTime = 500,
            int retries = 100,
            bool useNpgsql = false)
        {
            if (outputWriter == null)
            {
                outputWriter = Console.Out;
            }

            int counter = 0;
            DbConnection connection;
            string serverInfo;
            if (useNpgsql)
            {
                var npgBuilder = new NpgsqlConnectionStringBuilder(connstring);
                connection = new NpgsqlConnection(npgBuilder.ConnectionString);
                serverInfo = $"{npgBuilder.Host}:{npgBuilder.Port}/{npgBuilder.Database}";
            }
            else
            {
                var sqlBuilder = new SqlConnectionStringBuilder(connstring);
                connection = new SqlConnection(sqlBuilder.ConnectionString);
                serverInfo = $"{sqlBuilder.DataSource}/{sqlBuilder.InitialCatalog}";
            }

            using (connection)
            {
                while (true)
                {
                    try
                    {
                        await connection.OpenAsync();
                        await outputWriter.WriteLineAsync($@"Db-Connection established: {serverInfo}");
                        await connection.CloseAsync();
                        break;
                    }
                    catch
                    {
                        counter++;
                        if (counter > retries)
                        {
                            throw;
                        }

                        if (logFailures)
                        {
                            await outputWriter.WriteLineAsync(
                                $@"Retry Db-Connection {serverInfo} {counter}...");
                        }

                        Thread.Sleep(sleepTime);
                    }
                }
            }
        }
    }
}

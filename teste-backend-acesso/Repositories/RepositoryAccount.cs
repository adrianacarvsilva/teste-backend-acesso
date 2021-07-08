using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using teste_backend_acesso.Domain.Entities;
using teste_backend_acesso.Dto;

namespace teste_backend_acesso.Repositories
{
    public interface IRepositoryAccount
    {
        Task<Transfer> ReturnTransaction(string id);
        void InsertTransaction(Transfer transfer);
        void UpdateTransaction(Transfer transfer);
        void InsertLog(Log log);
    }

    public class RepositoryAccount : IRepositoryAccount
    {
        protected IConfiguration _configuration;
        public RepositoryAccount(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async Task<Transfer> ReturnTransaction(string id)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("teste")))
            {
                var sql =
                   $"SELECT " +
                       $"ID { nameof(Transfer.Id) }, " +
                       $"DATE { nameof(Transfer.Date) }, " +
                       $"STATUS { nameof(Transfer.Status) }, " +
                       $"MESSAGE { nameof(Transfer.Message) } " +
                   $"FROM TRANSACTIONS " +
                   $"WHERE ID = @id";

                var transfer = await connection.QueryFirstOrDefaultAsync<Transfer>(sql, new { id });

                return transfer;
            }
        }

        public async void InsertTransaction(Transfer transfer)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("teste")))
            {
                var sql =
                   $"INSERT INTO TRANSACTIONS " +
                   $"VALUES (@id, @date, @status, @message)";

                await connection.ExecuteScalarAsync<Transfer>(sql, new
                { id = transfer.Id, date = transfer.Date, status = transfer.Status, message = transfer.Message });
            }
        }

        public async void UpdateTransaction(Transfer transfer)
        {
            transfer.Date = DateTime.Now;

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("teste")))
            {
                var sql =
                   $"UPDATE TRANSACTIONS " +
                   $"SET DATE = @date, " +
                   $"STATUS = @status, " + 
                   $"MESSAGE = @message " +
                   $"WHERE ID = @id ";

                await connection.ExecuteScalarAsync<Transfer>(sql, new
                { id = transfer.Id, date = transfer.Date, status = transfer.Status, message = transfer.Message });
            }
        }

        public async void InsertLog(Log log)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("teste")))
            {
                var sql =
                   $"INSERT INTO LOG " +
                   $"VALUES (@id, @operation, @date, @message)";

                await connection.ExecuteScalarAsync<Transfer>(sql, new
                { id = log.Id, date = log.Date, operation = log.Operation, message = log.Message });
            }
        }
    }
}

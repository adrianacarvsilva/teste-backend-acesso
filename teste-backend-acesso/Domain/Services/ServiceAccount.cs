using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using teste_backend_acesso.Domain.Entities;
using teste_backend_acesso.Domain.Interfaces;
using teste_backend_acesso.Dto;
using teste_backend_acesso.Repositories;

namespace teste_backend_acesso.Domain.Services
{
    public class ServiceAccount : IServiceAccount
    {
        protected IConfiguration _configuration;
        protected IRepositoryAccount _repositoryAccount;
        protected string apiURL;

        public ServiceAccount(IConfiguration configuration, IRepositoryAccount repositoryAccount)
        {
            _configuration = configuration;
            _repositoryAccount = repositoryAccount;

            apiURL = _configuration.GetSection("APIURL").Get<string>();
        }

        public async Task<Response> Get()
        {
            try
            {
                using (var client = RetornarHttpClient())
                {
                    var response = await client.GetAsync($"{apiURL}/Account");

                    string result = response.Content.ReadAsStringAsync().Result;

                    var accounts = JsonConvert.DeserializeObject<List<Account>>(result);

                    return new Response()
                    {
                        Status = Status.Confirmed,
                        Data = accounts,
                    };
                }
            }
            catch (Exception e)
            {
                return new Response()
                {
                    Status = Status.Error,
                    Message = e.Message,
                };
            }
        }

        public async Task<Response> Get(string accountNumber)
        {
            try
            {
                using (var client = RetornarHttpClient())
                {
                    var response = await client.GetAsync($"{apiURL}/Account/{accountNumber}");

                    string result = response.Content.ReadAsStringAsync().Result;

                    var account = JsonConvert.DeserializeObject<Account>(result);

                    return new Response()
                    {
                        Status = Status.Confirmed,
                        Data = account,
                    };
                }
            }
            catch (Exception e)
            {
                return new Response()
                {
                    Status = Status.Error,
                    Message = e.Message,
                };
            }
        }

        private async Task<Account> GetAccount(string accountNumber)
        {
            var log = new Log();
            log.Id = Guid.NewGuid().ToString();
            log.Operation = LogOperation.Query;
            log.Date = DateTime.Now;
            Account account = null;
            
            using (var client = RetornarHttpClient())
            {
                var response = await client.GetAsync($"{apiURL}/Account/{accountNumber}");

                log.Message = $"{response.ReasonPhrase.ToString()} - {accountNumber} ";

                string result = response.Content.ReadAsStringAsync().Result;

                if(response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    account = JsonConvert.DeserializeObject<Account>(result);
                }

                _repositoryAccount.InsertLog(log);

                return account;
            }
        }

        private async Task<bool> AccountExists(string accountNumber)
        {
            var account = await GetAccount(accountNumber);

            return account != null;
        }

        private async Task<bool> BalanceAvailableForTransaction(string accountNumber, decimal value)
        {
            var log = new Log();
            log.Id = Guid.NewGuid().ToString();
            log.Operation = LogOperation.Query;
            log.Date = DateTime.Now;
            var status = false;

            var account = await GetAccount(accountNumber);

            if (account != null && account.Balance >= value)
            {
                log.Message = $"Sufficient balance - {accountNumber} ";
                status = true;
            }
            else
            {
                log.Message = $"Insufficient balance - {accountNumber} ";
            }

            _repositoryAccount.InsertLog(log);

            return status;
        }

        private async Task<(bool, string)> ValidateTransfer(Transfer transfer)
        {
            if (!await AccountExists(transfer.AccountOrigin))
            {
                return (false, "Origin account not found");
            }

            if (!await AccountExists(transfer.AccountDestination))
            {
                return (false, "Destination account not found");
            }

            if (!await BalanceAvailableForTransaction(transfer.AccountOrigin, transfer.Value))
            {
                return (false, "Insufficient balance");
            }

            return (true, string.Empty);
        }

        public async Task<Response> TransferFunds(Transfer transfer)
        {
            try
            {
                var transactionId = Guid.NewGuid().ToString();

                transfer.Id = transactionId;
                transfer.Date = DateTime.Now;
                var connection = GetConnectionFactory();

                var iconnection = CreateConnection(connection);
                var json = JsonConvert.SerializeObject(transfer);
                var body = Encoding.UTF8.GetBytes(json);

                CreateQueue($"TRANSACAO", iconnection, body: body);

                _repositoryAccount.InsertTransaction(transfer);

                return new Response()
                {
                    Status = Status.InQueue,
                    Data = transactionId,
                    Message = nameof(Status.InQueue),
                };
            }
            catch (Exception e)
            {
                return new Response()
                {
                    Status = Status.Error,
                    Message = e.Message,
                };
            }
        }

        public async Task<Response> TransferFunds(string transactionId)
        {
            var transaction = await _repositoryAccount.ReturnTransaction(transactionId);

            return new Response()
            {
                Status = transaction.Status,
                Data = transaction.Message,
                Date = transaction.Date,
            };
        }

        public async void ExecuteTransfer(Transfer transfer)
        {
            var logOrigin = new Log();
            var logDestiny = new Log();

            try
            {
                //using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                //{
                    (bool isValid, string message) = await ValidateTransfer(transfer);
                    var transactionId = Guid.NewGuid();

                    if (isValid)
                    {
                        using (var client = RetornarHttpClient())
                        {
                            var baseURL = $"{apiURL}/Account";

                            var transferOrigin = new Movement(transfer.AccountOrigin, TypeMovement.Debit, transfer.Value);

                            HttpResponseMessage responseOrigin = client.PostAsync(
                            baseURL, ReturnStringContent(transferOrigin)).Result;

                            logOrigin.Id = Guid.NewGuid().ToString();
                            logOrigin.Operation = LogOperation.Debit;
                            logOrigin.Date = DateTime.Now;
                            logOrigin.Message = $"{responseOrigin.ReasonPhrase} - {transfer.AccountOrigin} - Debit: { transfer.Value } ";

                            var transferDestiny = new Movement(transfer.AccountDestination, TypeMovement.Credit, transfer.Value);

                            HttpResponseMessage responseDestiny = client.PostAsync(
                            baseURL, ReturnStringContent(transferDestiny)).Result;


                            logDestiny.Id = Guid.NewGuid().ToString();
                            logDestiny.Operation = LogOperation.Credit;
                            logDestiny.Date = DateTime.Now;
                            logDestiny.Message = $"{responseDestiny.ReasonPhrase} - {transfer.AccountDestination} - Credit: { transfer.Value } ";

                            if (responseOrigin.StatusCode == System.Net.HttpStatusCode.OK &&
                                responseDestiny.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                transfer.Status = Status.Confirmed;
                                transfer.Message = "Success";
                            }
                            else
                            {
                                transfer.Status = Status.Error;
                                transfer.Message = responseOrigin.RequestMessage.ToString() + responseDestiny.RequestMessage.ToString();
                            }

                            _repositoryAccount.InsertLog(logOrigin);
                            _repositoryAccount.InsertLog(logDestiny);
                        }
                    }
                    else
                    {
                        transfer.Status = Status.Error;
                        transfer.Message = message;
                    }

                    //scope.Complete();
                //}
            }
            catch
            {
                transfer.Status = Status.Error;
                transfer.Message = "Request failed!";
            }
            finally
            {
                _repositoryAccount.UpdateTransaction(transfer);
               
            }
        }

        #region Queue

        public static ConnectionFactory GetConnectionFactory()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            return connectionFactory;
        }

        public static IConnection CreateConnection(ConnectionFactory connectionFactory)
        {
            return connectionFactory.CreateConnection();
        }

        public void CreateQueue(string queueName, IConnection connection, byte[] body)
        {
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                channel.BasicPublish(exchange: "", routingKey: queueName, false, basicProperties: null, body: body);
            }
        }

        #endregion

        private HttpClient RetornarHttpClient()
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        private StringContent ReturnStringContent(Movement movement)
        {
            var json = JsonConvert.SerializeObject(movement);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }


    }
}

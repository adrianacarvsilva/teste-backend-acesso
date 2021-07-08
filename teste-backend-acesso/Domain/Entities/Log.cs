using System;

namespace teste_backend_acesso.Domain.Entities
{
    public class Log
    { 
        public string Id { get; set; }
        public LogOperation Operation { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }
    }
}

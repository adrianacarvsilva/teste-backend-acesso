using System;

namespace teste_backend_acesso.Dto
{
    public class Response
    {
        public Status Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public DateTime Date { get; set; }
    }
}

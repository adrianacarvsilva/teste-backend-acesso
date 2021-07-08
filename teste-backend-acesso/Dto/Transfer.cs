using System;

namespace teste_backend_acesso.Dto
{
    public class Transfer
    {
        public string Id { get; set; }
        public string AccountOrigin { get; set; }
        public string AccountDestination { get; set; }
        public decimal Value { get; set; }
        public Status Status { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }
    }
}

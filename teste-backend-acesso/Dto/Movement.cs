using teste_backend_acesso.Domain;

namespace teste_backend_acesso.Dto
{
    public class Movement
    {
        public Movement(string accountNumber, TypeMovement type, decimal value)
        {
            this.AccountNumber = accountNumber;
            this.Type = type;
            this.Value = value;
        }

        public decimal Value { get; set; }
        public string AccountNumber { get; set; }
        public TypeMovement Type { get; set; }

    }
}

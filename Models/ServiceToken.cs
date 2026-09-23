namespace CustomHome.Models
{
    public class ServiceToken
    {
        public int Id { get; set; }

        public int TokenNumber { get; set; }

        public ServiceTokenStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
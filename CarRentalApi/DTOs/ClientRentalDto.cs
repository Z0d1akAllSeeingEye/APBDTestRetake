namespace WebApplication6.DTOs
{
    public class ClientRentalDto
    {
        public ClientDto Client { get; set; }
        public int CarId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}
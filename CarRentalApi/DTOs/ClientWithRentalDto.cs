namespace WebApplication6.DTOs
{
    public class ClientWithRentalsDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public List<RentalDto> Rentals { get; set; } = new();
    }
}
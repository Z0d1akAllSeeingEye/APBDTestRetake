using WebApplication6.DTOs;
using System.Data.SqlClient;

namespace WebApplication6.Services
{
    public class ClientService
    {
        private readonly string _connectionString;

        public ClientService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection") 
                                ?? "Server=localhost;Database=CarRental;Trusted_Connection=True;";
        }

        public async Task<ClientWithRentalsDto?> GetClientWithRentalsAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var clientCmd = new SqlCommand("SELECT ID, FirstName, LastName, Address FROM clients WHERE ID = @id", connection);
            clientCmd.Parameters.AddWithValue("@id", id);

            using var reader = await clientCmd.ExecuteReaderAsync();
            if (!reader.Read()) return null;

            var client = new ClientWithRentalsDto
            {
                Id = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Address = reader.GetString(3)
            };

            reader.Close();

            var rentalCmd = new SqlCommand(@"
                SELECT c.VIN, col.Name, m.Name, r.DateFrom, r.DateTo, r.TotalPrice
                FROM car_rentals r
                JOIN cars c ON r.CarID = c.ID
                JOIN colors col ON c.ColorID = col.ID
                JOIN models m ON c.ModelID = m.ID
                WHERE r.ClientID = @id", connection);

            rentalCmd.Parameters.AddWithValue("@id", id);

            using var rentalReader = await rentalCmd.ExecuteReaderAsync();
            while (await rentalReader.ReadAsync())
            {
                client.Rentals.Add(new RentalDto
                {
                    Vin = rentalReader.GetString(0),
                    Color = rentalReader.GetString(1),
                    Model = rentalReader.GetString(2),
                    DateFrom = rentalReader.GetDateTime(3),
                    DateTo = rentalReader.GetDateTime(4),
                    TotalPrice = rentalReader.GetInt32(5)
                });
            }

            return client;
        }

        public async Task<(bool Success, int ClientId, string? ErrorMessage)> AddClientWithRentalAsync(ClientRentalDto data)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var checkCarCmd = new SqlCommand("SELECT PricePerDay FROM cars WHERE ID = @carId", connection);
            checkCarCmd.Parameters.AddWithValue("@carId", data.CarId);
            var priceObj = await checkCarCmd.ExecuteScalarAsync();

            if (priceObj == null)
                return (false, 0, "Car does not exist");

            int pricePerDay = (int)priceObj;
            int totalPrice = (data.DateTo - data.DateFrom).Days * pricePerDay;

            var insertClientCmd = new SqlCommand(
                "INSERT INTO clients (FirstName, LastName, Address) OUTPUT INSERTED.ID VALUES (@fn, @ln, @addr)", connection);
            insertClientCmd.Parameters.AddWithValue("@fn", data.Client.FirstName);
            insertClientCmd.Parameters.AddWithValue("@ln", data.Client.LastName);
            insertClientCmd.Parameters.AddWithValue("@addr", data.Client.Address);

            int clientId = (int)await insertClientCmd.ExecuteScalarAsync();

            var insertRentalCmd = new SqlCommand(
                "INSERT INTO car_rentals (ClientID, CarID, DateFrom, DateTo, TotalPrice, Discount) VALUES (@cid, @carId, @from, @to, @price, 0)", connection);
            insertRentalCmd.Parameters.AddWithValue("@cid", clientId);
            insertRentalCmd.Parameters.AddWithValue("@carId", data.CarId);
            insertRentalCmd.Parameters.AddWithValue("@from", data.DateFrom);
            insertRentalCmd.Parameters.AddWithValue("@to", data.DateTo);
            insertRentalCmd.Parameters.AddWithValue("@price", totalPrice);

            await insertRentalCmd.ExecuteNonQueryAsync();

            return (true, clientId, null);
        }
    }
}

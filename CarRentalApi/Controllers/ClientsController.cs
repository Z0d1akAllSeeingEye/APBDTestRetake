
using WebApplication6.DTOs;
using WebApplication6.Services;
using Microsoft.AspNetCore.Mvc;


namespace CarRentalApi.Controllers
{
    [ApiController]
    [Route("api/clients")]
    public class ClientsController : ControllerBase
    {
        private readonly ClientService _service;

        public ClientsController(ClientService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient(int id)
        {
            var result = await _service.GetClientWithRentalsAsync(id);
            return result == null ? NotFound("Client not found") : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddClient([FromBody] ClientRentalDto data)
        {
            var result = await _service.AddClientWithRentalAsync(data);
            return result.Success
                ? Created($"/api/clients/{result.ClientId}", null)
                : BadRequest(result.ErrorMessage);
        }
    }
}
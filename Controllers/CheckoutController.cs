using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using appWeb2.Data;
using appWeb2.DTOs;
using appWeb2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace appWeb2.Controllers
{
  
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _client;
        private readonly AppDbContext _context; 

        public CheckoutController(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
            _client = new HttpClient();
            _client.BaseAddress = new Uri(_config["PayPal:BaseUrl"]);
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
        {
            var juego = await _context.VideoJuegos.FindAsync(request.VideoJuegoId);

            if (juego == null)
            {
                return NotFound("El videojuego no existe.");
            }

            var token = await GetPayPalAccessToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var orderData = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                new
                {
                    description = $"Compra de: {juego.titulo}",
                    amount = new
                    {
                        currency_code = "USD",
                        value = juego.precio.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
            }
            };

            var content = new StringContent(JsonSerializer.Serialize(orderData), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/v2/checkout/orders", content);

            return Content(await response.Content.ReadAsStringAsync(), "application/json");
        }


        [HttpPost("capture-order")]
        public async Task<IActionResult> CaptureOrder([FromBody] CaptureRequest request)
        {
            var token = await GetPayPalAccessToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var emptyContent = new StringContent("", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"/v2/checkout/orders/{request.OrderId}/capture", emptyContent);

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("El pago no pudo ser procesado por PayPal.");
            }

            var juego = await _context.VideoJuegos.FindAsync(request.VideoJuegoId);

            var nuevaCompra = new Compra
            {
                UsuarioId = request.UsuarioId,
                fechaCompra = DateTime.Now
            };

            _context.Compras.Add(nuevaCompra);
            await _context.SaveChangesAsync(); 

            var detalle = new DetalleCompra
            {
                idCompra = nuevaCompra.id,
                VideoJuegosId = juego.id,
                total = juego.precio,
                fechaHoraTransaccion = DateTime.Now,
                cantidad = 1,
                estadoCompra = "Aprobado",
                codigoTransaccion = request.OrderId
            };

            _context.DetallesCompra.Add(detalle);
            await _context.SaveChangesAsync(); 

            return Ok(new { mensaje = "Compra exitosa y guardada en BD" });
        }

        private async Task<string> GetPayPalAccessToken()
        {
            var clientId = _config["PayPal:ClientId"];
            var secret = _config["PayPal:Secret"];

            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{secret}"));

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var request = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await _client.PostAsync("/v1/oauth2/token", request);

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString();
        }
    }
}

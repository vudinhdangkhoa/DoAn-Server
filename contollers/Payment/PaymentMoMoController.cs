using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Models.Payment;
using server.Services;

namespace server.contollers.Payment
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentMoMoController : ControllerBase
    {
        private readonly MoMo_Services _momoService;

        public PaymentMoMoController(MoMo_Services momoService)
        {
            _momoService = momoService;
        }

        [HttpPost("CreatePayment")]
        public async Task<IActionResult> CreatePayment([FromBody] MoMoRequestModel model)
        {
            var result = await _momoService.CreateMoMoPaymentAsync(model);
            var json = JsonSerializer.Deserialize<object>(result);
            return Ok(json);
        }

        [HttpPost("Notify")]
        public IActionResult Notify([FromBody] object response)
        {
            Console.WriteLine("MoMo IPN: " + response);
            return Ok(new { message = "Received" });
        }

    }
}
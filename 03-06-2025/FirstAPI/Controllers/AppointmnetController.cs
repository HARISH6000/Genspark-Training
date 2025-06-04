
using System.Threading.Tasks;
using FirstAPI.Interfaces;
using FirstAPI.Models;
using FirstAPI.Models.DTOs;
using FirstAPI.Models.DTOs.DoctorSpecialities;
using FirstAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace FirstAPI.Controllers
{


    [ApiController]
    [Route("/api/[controller]")]
    public class AppointmnetController : ControllerBase
    {
        private readonly IAppointmnetService _appointmnetService;

        public AppointmnetController(IAppointmnetService appointmnetService)
        {
            _appointmnetService = appointmnetService;
        }


        [HttpPost]
        public async Task<ActionResult<Appointmnet>> PostAppointmnet([FromBody] AppointmnetDTO appointmnet)
        {
            try
            {
                var updatedAppointmnet = await _appointmnetService.AddAppointmnet(appointmnet);
                if (updatedAppointmnet != null)
                    return Created("", updatedAppointmnet);
                return BadRequest("Unable to process request at this moment");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("cancel")]
        [Authorize(Policy = "MinimumYearsOfExperience")]
        public async Task<ActionResult<Appointmnet>> CancelAppointmnet([FromBody] string appointmnetNumber)
        {
            try
            {
                var updatedAppointmnet = await _appointmnetService.CancelAppointmnet(appointmnetNumber);
                if (updatedAppointmnet != null)
                    return Ok(updatedAppointmnet);
                return BadRequest("Unable to process request at this moment");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}
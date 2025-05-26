using Microsoft.AspNetCore.Mvc;
using FirstAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace FirstAPI.Controllers
{
    [ApiController] 
    [Route("api/[controller]")] 
    public class PatientController : ControllerBase
    {
        
        private static List<Patient> _patients = new List<Patient>
        {
            new Patient { Id = 1, Name = "Alice", DOB = new DateTime(2003, 06, 19), Phone = "1234567891" },
            new Patient { Id = 2, Name = "Bob", DOB = new DateTime(2004, 01, 08), Phone = "1234567890" }
        };

        private static int _count = 3;
        
        [HttpGet] 
        public ActionResult<IEnumerable<Patient>> GetAllPatients()
        {
            return Ok(_patients); 
        }

        
        [HttpGet("{id}")]
        public ActionResult<Patient> GetPatientById(int id)
        {
            var patient = _patients.FirstOrDefault(p => p.Id == id);
            if (patient == null)
            {
                return NotFound($"Patient with ID {id} not found.");
            }
            return Ok(patient); 
        }

        
        [HttpPost] 
        public ActionResult<Patient> CreatePatient([FromBody] Patient patient)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }

            patient.Id = _count;
            _count += 1;

            _patients.Add(patient); 

            return CreatedAtAction(nameof(GetPatientById), new { id = patient.Id }, patient);
        }

        [HttpPut("{id}")] 
        public IActionResult UpdatePatient(int id, [FromBody] Patient patient)
        {
            if (id != patient.Id)
            {
                return BadRequest("Patient ID in URL does not match ID in body."); 
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingPatient = _patients.FirstOrDefault(p => p.Id == id);
            if (existingPatient == null)
            {
                return NotFound($"Patient with ID {id} not found.");
            }

            
            existingPatient.Name = patient.Name;
            existingPatient.DOB = patient.DOB;
            existingPatient.Phone = patient.Phone;

            return Ok(existingPatient); 
        }

        
        [HttpDelete("{id}")] 
        public IActionResult DeletePatient(int id)
        {
            var patientToRemove = _patients.FirstOrDefault(p => p.Id == id);
            if (patientToRemove == null)
            {
                return NotFound($"Patient with ID {id} not found.");
            }

            _patients.Remove(patientToRemove);

            return Ok("Successfully deleted patient");
        }
    }
}

using System.Security.Claims;
using FirstAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace FirstAPI.Authorization
{
    public class MinimumExperienceHandler : AuthorizationHandler<MinimumExperienceRequirement>
    {
        private readonly IDoctorService _doctorService; 
        private readonly ILogger<MinimumExperienceHandler> _logger;

        public MinimumExperienceHandler(IDoctorService doctorService, ILogger<MinimumExperienceHandler> logger)
        {
            _doctorService = doctorService;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumExperienceRequirement requirement)
        {
            
            if (!context.User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("MinimumExperienceHandler: User not authenticated.");
                context.Fail();
                return;
            }

            
            if (!context.User.IsInRole("Doctor"))
            {
                _logger.LogWarning("MinimumExperienceHandler: User is not a Doctor.");
                context.Fail();
                return;
            }

            
            var doctorIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

            if (doctorIdClaim == null || !int.TryParse(doctorIdClaim.Value, out int doctorId))
            {
                _logger.LogWarning("MinimumExperienceHandler: Doctor ID claim not found or invalid.");
                context.Fail();
                return;
            }

            try
            {
                
                var doctor = await _doctorService.GetDoctorById(doctorId); 

                if (doctor != null && doctor.YearsOfExperience >= requirement.MinimumYearsOfExperience)
                {
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogWarning($"MinimumExperienceHandler: Doctor {doctorId} does not meet experience requirement. Required: {requirement.MinimumYearsOfExperience}, Actual: {doctor?.YearsOfExperience}");
                    context.Fail();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"MinimumExperienceHandler: Error fetching doctor details for ID {doctorId}.");
                context.Fail();
            }
        }
    }
}
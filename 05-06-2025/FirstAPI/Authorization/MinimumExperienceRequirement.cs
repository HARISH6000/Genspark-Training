using Microsoft.AspNetCore.Authorization;

namespace FirstAPI.Authorization
{
    public class MinimumExperienceRequirement : IAuthorizationRequirement
    {
        public float MinimumYearsOfExperience { get; set; }

        public MinimumExperienceRequirement(float minimumYearsOfExperience)
        {
            MinimumYearsOfExperience = minimumYearsOfExperience;
        }
    }
}
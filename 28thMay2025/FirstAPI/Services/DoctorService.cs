using FirstAPI.Interfaces;
using FirstAPI.Models;
using FirstAPI.Models.DTOs.DoctorSpecialities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FirstAPI.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IRepository<int, Doctor> _doctorRepository;
        private readonly IRepository<int, Speciality> _specialityRepository;
        private readonly IRepository<int, DoctorSpeciality> _doctorSpecialityRepository;
        public DoctorService(IRepository<int, Doctor> doctorRepository,
                            IRepository<int, Speciality> specialityRepository,
                            IRepository<int, DoctorSpeciality> doctorSpecialityRepository)
        {
            _doctorRepository = doctorRepository;
            _specialityRepository = specialityRepository;
            _doctorSpecialityRepository = doctorSpecialityRepository;
        }

        public async Task<Doctor> AddDoctor(DoctorAddRequestDto doctorDto)
        {
            
            var doctor = new Doctor
            {
                Name = doctorDto.Name,
                YearsOfExperience = doctorDto.YearsOfExperience,
                Status = "Active"
            };

            
            var addedDoctor = await _doctorRepository.Add(doctor);

            
            if (doctorDto.Specialities != null && doctorDto.Specialities.Any())
            {
                foreach (var specialityDto in doctorDto.Specialities)
                {
                    
                    var speciality = (await _specialityRepository.GetAll())
                                     .FirstOrDefault(s => s.Name.ToLower() == specialityDto.Name.ToLower());

                    if (speciality == null)
                    {

                        speciality = new Speciality
                        {
                            Name = specialityDto.Name,
                            Status = "Active"
                        };
                        speciality = await _specialityRepository.Add(speciality);
                    }


                    var doctorSpeciality = new DoctorSpeciality
                    {
                        DoctorId = addedDoctor.Id,
                        SpecialityId = speciality.Id
                    };
                    await _doctorSpecialityRepository.Add(doctorSpeciality);
                }

            }
            return addedDoctor;
        }

        public async Task<Doctor> GetDoctByName(string name)
        {
            var doctors = await _doctorRepository.GetAll();
            var doctor = doctors.FirstOrDefault(d => d.Name.ToLower() == name.ToLower());
            return doctor ?? throw new Exception($"Doctor with name {name} not found.");
        }

        public async Task<ICollection<Doctor>> GetDoctorsBySpeciality(string specialityName)
        {
            var doctors = await _doctorRepository.GetAll();
            var specialities = await _specialityRepository.GetAll();
            var doctorSpecialities = await _doctorSpecialityRepository.GetAll();

            
            var targetSpeciality = specialities.FirstOrDefault(s => s.Name.ToLower() == specialityName.ToLower());

            if (targetSpeciality == null)
            {
                return new List<Doctor>();
            }

            
            var relevantDoctorSpecialities = doctorSpecialities
                                            .Where(ds => ds.SpecialityId == targetSpeciality.Id)
                                            .ToList();

            
            var doctorsWithSpeciality = doctors
                                        .Where(d => relevantDoctorSpecialities.Any(rds => rds.DoctorId == d.Id))
                                        .ToList();

            return doctorsWithSpeciality;
        }


    }
}
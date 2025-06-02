using System.Threading.Tasks;
using AutoMapper;
using FirstAPI.Interfaces;
using FirstAPI.Misc;
using FirstAPI.Models;
using FirstAPI.Models.DTOs.DoctorSpecialities;


namespace FirstAPI.Services
{
    public class PatientService : IPatientService
    {
        PatientMapper patientMapper ;
        private readonly IRepository<int, Patient> _patientRepository;
        private readonly IRepository<string, User> _userRepository;
        private readonly IOtherContextFunctionities _otherContextFunctionities;
        private readonly IEncryptionService _encryptionService;
        private readonly IMapper _mapper;

        public PatientService(IRepository<int, Patient> patientRepository,
                            IRepository<string,User> userRepository,
                            IOtherContextFunctionities otherContextFunctionities,
                            IEncryptionService encryptionService,
                            IMapper mapper)
        {
            patientMapper = new PatientMapper();
            _patientRepository = patientRepository;
            _userRepository = userRepository;
            _otherContextFunctionities = otherContextFunctionities;
            _encryptionService = encryptionService;
            _mapper = mapper;

        }

        public async Task<Patient> AddPatient(PatientAddRequestDto patient)
        {
            try
            {
                var user = _mapper.Map<PatientAddRequestDto, User>(patient);
                var encryptedData = await _encryptionService.EncryptData(new EncryptModel
                {
                    Data= patient.Password
                });
                user.Password = encryptedData.EncryptedData;
                user.HashKey = encryptedData.HashKey;
                user.Role = "Patient";
                user = await _userRepository.Add(user);
                var newPatient = patientMapper.MapPatientAddRequestPatient(patient);
                newPatient = await _patientRepository.Add(newPatient);
                if (newPatient == null)
                    throw new Exception("Could not add patient");

                return newPatient;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            
        }
    }
}
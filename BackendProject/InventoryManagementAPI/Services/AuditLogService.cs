using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using InventoryManagementAPI.Exceptions;
using InventoryManagementAPI.Mappers;
using InventoryManagementAPI.Utilities; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace InventoryManagementAPI.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IRepository<int, AuditLog> _auditLogRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuditLogService> _logger; 

        public AuditLogService(
            IRepository<int,AuditLog> auditLogRepository,
            IUserRepository userRepository, ILogger<AuditLogService> logger)
        {
            _auditLogRepository = auditLogRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<AuditLogResponseDto> LogActionAsync(AuditLogEntryDto auditLogEntry)
        {
            // Serialize OldValues and NewValues objects to JSON strings
            string? oldValuesJson = auditLogEntry.OldValues != null
                ? JsonSerializer.Serialize(auditLogEntry.OldValues, new JsonSerializerOptions { WriteIndented = false, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve  })
                : null;
            string? newValuesJson = auditLogEntry.NewValues != null
                ? JsonSerializer.Serialize(auditLogEntry.NewValues, new JsonSerializerOptions { WriteIndented = false, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve  })
                : null;

            Console.WriteLine($"user Id: {auditLogEntry.UserId}");
            _logger.LogInformation($"Logging action for UserId: {auditLogEntry.UserId}, TableName: {auditLogEntry.TableName}, RecordId: {auditLogEntry.RecordId}, ActionType: {auditLogEntry.ActionType}");
            var auditLog = new AuditLog
            {
                UserId = auditLogEntry.UserId,
                Timestamp = DateTime.UtcNow, 
                TableName = auditLogEntry.TableName,
                RecordId = auditLogEntry.RecordId,
                ActionType = auditLogEntry.ActionType,
                OldValues = oldValuesJson,
                NewValues = newValuesJson,
                Changes = auditLogEntry.Changes 
            };

            var addedAuditLog = await _auditLogRepository.Add(auditLog);
            
            // To return a DTO with username, we might need to fetch the added entry with its navigation property loaded
            // Or ensure the Add method in the repository returns the entity with relationships.
            // For simplicity, we'll fetch it again or manually populate Username if User was passed in entry.
            // A more efficient way would be to extend IRepository.Add to allow eager loading on return.
            // For now, let's just make sure the User nav property is available.
            if (addedAuditLog.User == null && addedAuditLog.UserId.HasValue)
            {
                addedAuditLog.User = await _userRepository.Get(addedAuditLog.UserId.Value);
            }

            return AuditLogMapper.ToAuditLogResponseDto(addedAuditLog);
        }

        public async Task<AuditLogResponseDto?> GetAuditLogByIdAsync(int auditLogId)
        {
            var auditLog = await _auditLogRepository.Get(auditLogId);
            if (auditLog == null) return null;
            return AuditLogMapper.ToAuditLogResponseDto(auditLog);
        }

        public async Task<IEnumerable<AuditLogResponseDto>> GetAuditLogsAsync(AuditLogFilterDto filter, string? sortBy = null)
        {
            var query = (await _auditLogRepository.GetAll()).AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.TableName))
            {
                query = query.Where(al => al.TableName.Equals(filter.TableName, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(filter.RecordId))
            {
                query = query.Where(al => al.RecordId.Equals(filter.RecordId, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(filter.ActionType))
            {
                query = query.Where(al => al.ActionType.Equals(filter.ActionType, StringComparison.OrdinalIgnoreCase));
            }
            if (filter.UserId.HasValue)
            {
                query = query.Where(al => al.UserId == filter.UserId.Value);
            }
            if (filter.StartDate.HasValue)
            {
                query = query.Where(al => al.Timestamp >= filter.StartDate.Value.ToUniversalTime());
            }
            if (filter.EndDate.HasValue)
            {
                // Add one day to EndDate to include all logs from that day
                query = query.Where(al => al.Timestamp <= filter.EndDate.Value.AddDays(1).ToUniversalTime());
            }

            var responseDtos = query.Select(al => AuditLogMapper.ToAuditLogResponseDto(al)).ToList(); // ToList() to execute query before sorting

            // Apply sorting using the common utility function
            if (!string.IsNullOrEmpty(sortBy))
            {
                responseDtos = SortHelper.ApplySorting(responseDtos, sortBy).ToList(); // .ToList() to make it concrete after sorting
            }
            
            return responseDtos;
        }
    }
}

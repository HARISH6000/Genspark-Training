using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using InventoryManagementAPI.Exceptions;
using InventoryManagementAPI.Mappers;
using Microsoft.EntityFrameworkCore; 
using System.Text.Json;

namespace InventoryManagementAPI.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRepository<int,Role> _roleRepository;
        private readonly IAuditLogService _auditLogService;

        public RoleService(IRepository<int,Role> roleRepository, IAuditLogService auditLogService)
        {
            _roleRepository = roleRepository;
            _auditLogService = auditLogService;
        }

        

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAll();
            return roles;
        }
    }
}

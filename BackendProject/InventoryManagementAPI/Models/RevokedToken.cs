using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.Models
{
    public class RevokedToken
    {
        [Key]
        [Required]
        public string Jti { get; set; } = string.Empty; 

        [Required]
        public DateTime ExpirationDate { get; set; }
    }
}
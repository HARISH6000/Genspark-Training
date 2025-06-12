using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementAPI.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        public string TokenHash { get; set; } = string.Empty; 

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public bool IsRevoked { get; set; } = false; 
        public string Jti { get; set; } = string.Empty; 

        // Navigation property
        public User? User { get; set; }
    }
}
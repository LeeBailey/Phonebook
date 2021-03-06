﻿using System.ComponentModel.DataAnnotations;

namespace Phonebook.Api.Models
{
    public class PostNewContactRequestData
    {
        [Required]
        [MaxLength(128)]
        public string? ContactFullName { get; set; }

        [Required]
        [MaxLength(32)]
        public string? ContactPhoneNumber { get; set; }
    }
}

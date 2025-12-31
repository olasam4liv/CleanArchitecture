using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.BaseEntities;

public record BaseEncryptedRequestDto
{
    //[StringLength(15, MinimumLength = 5, ErrorMessage = "The length for {0} is should be maximum of {1} characters long.")]
    [MinLength(32, ErrorMessage = "{0} must be more than {1} characters.")]
    public string? EncryptedData { get; set; }
    public bool IsValid(out string problemSource)
    {
        problemSource = string.Empty;

        if (string.IsNullOrEmpty(EncryptedData))
        {
            problemSource = "Encrypted Data";
            return false;
        }

        return true;
    }
}

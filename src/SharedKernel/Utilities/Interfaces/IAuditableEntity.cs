using System;
using System.Collections.Generic;
using System.Text;

namespace SharedKernel.Helper.Interfaces;

public interface IAuditableEntity
{
    string? CreatedBy { get; set; } 
    DateTime CreatedAt { get; set; }
    string? LastModifiedBy { get; set; }
    DateTime UpdatedAt { get; set; }
    string? RemoteIpAddress { get; set; }
}

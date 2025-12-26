using System;
using System.Collections.Generic;
using System.Text;

namespace SharedKernel.Helper.Interfaces;

public interface ISoftDelete
{
     DateTime? DeletedOn { get; set; }
     string? DeletedBy { get; set; }
}

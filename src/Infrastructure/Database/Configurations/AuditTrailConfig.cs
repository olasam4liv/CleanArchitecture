using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Infrastructure.AuditLog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Helper;

namespace Infrastructure.Database.Configurations;

public class AuditTrailConfig : IEntityTypeConfiguration<AuditTrailEntity>
{
    public void Configure(EntityTypeBuilder<AuditTrailEntity> builder) =>
        builder
            .ToTable("AuditTrails");
}

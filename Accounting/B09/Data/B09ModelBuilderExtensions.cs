using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Accounting.B09.Domain;

namespace NVOAMASIS.Accounting.B09.Data;

public static class B09ModelBuilderExtensions
{
    public static void ConfigureB09(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FinancialStatementNoteTemplate>(e =>
        {
            e.ToTable("FinancialStatementNoteTemplates");
            e.HasIndex(x => new { x.Code, x.Version }).IsUnique();
        });

        modelBuilder.Entity<FinancialStatementNoteSection>(e =>
        {
            e.ToTable("FinancialStatementNoteSections");
            e.HasIndex(x => new { x.TemplateId, x.Code }).IsUnique();
            e.HasOne(x => x.Template).WithMany(x => x.Sections).HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FinancialStatementNoteLine>(e =>
        {
            e.ToTable("FinancialStatementNoteLines");
            e.HasIndex(x => new { x.SectionId, x.Code }).IsUnique();
            e.HasOne(x => x.Section).WithMany(x => x.Lines).HasForeignKey(x => x.SectionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.ParentLine).WithMany(x => x.ChildLines).HasForeignKey(x => x.ParentLineId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FinancialStatementNoteMapping>(e =>
        {
            e.ToTable("FinancialStatementNoteMappings");
            e.Property(x => x.SignMultiplier).HasPrecision(18, 6);
            e.Property(x => x.DetailThresholdPercent).HasPrecision(9, 4);
            e.HasOne(x => x.NoteLine).WithMany(x => x.Mappings).HasForeignKey(x => x.NoteLineId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FinancialStatementNoteReport>(e =>
        {
            e.ToTable("FinancialStatementNoteReports");
            e.HasIndex(x => new { x.CompanyId, x.TemplateId, x.FiscalYear }).IsUnique();
            e.HasOne(x => x.Template).WithMany().HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FinancialStatementNoteValue>(e =>
        {
            e.ToTable("FinancialStatementNoteValues");
            e.HasIndex(x => new { x.ReportId, x.NoteLineId }).IsUnique();
            e.Property(x => x.CurrentSystemValue).HasPrecision(28, 4);
            e.Property(x => x.CurrentAdjustment).HasPrecision(28, 4);
            e.Property(x => x.PreviousSystemValue).HasPrecision(28, 4);
            e.Property(x => x.PreviousAdjustment).HasPrecision(28, 4);
            e.HasOne(x => x.Report).WithMany(x => x.Values).HasForeignKey(x => x.ReportId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.NoteLine).WithMany().HasForeignKey(x => x.NoteLineId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RelatedPartyProfile>(e =>
        {
            e.ToTable("RelatedPartyProfiles");
            e.HasIndex(x => new { x.CompanyId, x.ClientId, x.EffectiveFrom });
        });

        modelBuilder.Entity<CustodyAsset>(e =>
        {
            e.ToTable("CustodyAssets");
            e.Property(x => x.Quantity).HasPrecision(28, 4);
            e.Property(x => x.EstimatedValue).HasPrecision(28, 4);
            e.HasIndex(x => new { x.CompanyId, x.ClientId });
            e.HasIndex(x => new { x.CompanyId, x.ContainerId });
        });
    }
}

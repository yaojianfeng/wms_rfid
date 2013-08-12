using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using THOK.Common.Ef.MappingStrategy;

namespace THOK.Wms.DbModel.Mapping
{
    public class ContractDetailMap : EntityMappingBase<ContractDetail>
    {
        public ContractDetailMap() : base("Inter")
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.ID)
                .IsRequired();
            this.Property(t => t.ContractCode)
                .IsRequired()
                .HasMaxLength(50);
            this.Property(t => t.BrandCode)
                .IsRequired()
                .HasMaxLength(13);
            this.Property(t => t.Quantity)
                .IsRequired()
                .HasPrecision(18, 4);
            this.Property(t => t.Price)
                .HasPrecision(16, 2);
            this.Property(t => t.Amount)
                .HasPrecision(16, 2);
            this.Property(t => t.TaxAmount)
                .HasPrecision(16, 2);

            // Table & Column Mappings
            this.Property(t => t.ID).HasColumnName(ColumnMap.Value.To("ID"));
            this.Property(t => t.ContractCode).HasColumnName(ColumnMap.Value.To("ContractCode"));
            this.Property(t => t.BrandCode).HasColumnName(ColumnMap.Value.To("BrandCode"));
            this.Property(t => t.Quantity).HasColumnName(ColumnMap.Value.To("Quantity"));
            this.Property(t => t.Price).HasColumnName(ColumnMap.Value.To("Price"));
            this.Property(t => t.Amount).HasColumnName(ColumnMap.Value.To("Amount"));
            this.Property(t => t.TaxAmount).HasColumnName(ColumnMap.Value.To("TaxAmount"));

            // Relationships
            this.HasRequired(t => t.Contract)
                .WithMany(t => t.ContractDetails)
                .HasForeignKey(d => d.ContractCode)
                .WillCascadeOnDelete(false);
        }
    }
}

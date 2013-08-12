using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using THOK.Common.Ef.MappingStrategy;

namespace THOK.Wms.DbModel.Mapping
{
    public class PalletMap : EntityMappingBase<Pallet>
    {
        public PalletMap()
            : base("Inter")
        {
            // Primary Key
            this.HasKey(t => t.PalletID);

            // Properties
            this.Property(t => t.PalletID)
                .IsRequired();
            this.Property(t => t.WmsUUID)
                .HasMaxLength(64);
            this.Property(t => t.UUID)
                .HasMaxLength(64);
            this.Property(t => t.TicketNo)
                .HasMaxLength(100);
            this.Property(t => t.OperateType)
                .IsRequired()
                .HasMaxLength(50);
            this.Property(t => t.BarCodeType)
                .HasMaxLength(2);
            this.Property(t => t.RfidAntCode)
                .HasMaxLength(20);
            this.Property(t => t.PieceCigarCode)
                .HasMaxLength(13);
            this.Property(t => t.BoxCigarCode)
                .HasMaxLength(13);
            this.Property(t => t.CigaretteName)
                .HasMaxLength(50);
            this.Property(t => t.Quantity)
                .HasPrecision(18, 4);

            // Table & Column Mappings
            this.Property(t => t.PalletID).HasColumnName(ColumnMap.Value.To("PalletID"));
            this.Property(t => t.WmsUUID).HasColumnName(ColumnMap.Value.To("WmsUUID"));
            this.Property(t => t.UUID).HasColumnName(ColumnMap.Value.To("UUID"));
            this.Property(t => t.TicketNo).HasColumnName(ColumnMap.Value.To("TicketNo"));
            this.Property(t => t.OperateDate).HasColumnName(ColumnMap.Value.To("OperateDate"));
            this.Property(t => t.OperateType).HasColumnName(ColumnMap.Value.To("OperateType"));
            this.Property(t => t.BarCodeType).HasColumnName(ColumnMap.Value.To("BarCodeType"));
            this.Property(t => t.RfidAntCode).HasColumnName(ColumnMap.Value.To("RfidAntCode"));
            this.Property(t => t.PieceCigarCode).HasColumnName(ColumnMap.Value.To("PieceCigarCode"));
            this.Property(t => t.BoxCigarCode).HasColumnName(ColumnMap.Value.To("BoxCigarCode"));
            this.Property(t => t.CigaretteName).HasColumnName(ColumnMap.Value.To("CigaretteName"));
            this.Property(t => t.Quantity).HasColumnName(ColumnMap.Value.To("Quantity"));
            this.Property(t => t.ScanTime).HasColumnName(ColumnMap.Value.To("ScanTime"));
        }
    }
}

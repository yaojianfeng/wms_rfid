namespace THOK.Wms.Repository.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class modify_precision : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.wms_move_bill_detail", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_storage", "quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_storage", "in_frozen_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_storage", "out_frozen_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_check_bill_detail", "quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_check_bill_detail", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_in_bill_allot", "allot_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_in_bill_allot", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_in_bill_detail", "bill_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_in_bill_detail", "allot_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_in_bill_detail", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_out_bill_allot", "allot_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_out_bill_allot", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_out_bill_detail", "bill_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_out_bill_detail", "allot_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_out_bill_detail", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_profit_loss_bill_detail", "quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_sort_order_detail", "demand_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_sort_order_detail", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_sort_order_detail", "amount", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_sort_order_detail", "unit_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_sort_order", "quantity_sum", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_sort_order", "amount_sum", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_sorting_lowerlimit", "quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_unit_list", "quantity01", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_unit_list", "quantity02", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_unit_list", "quantity03", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_product_warning", "min_limited", c => c.Decimal(precision: 18, scale: 4));
            AlterColumn("dbo.wms_product_warning", "max_limited", c => c.Decimal(precision: 18, scale: 4));
            AlterColumn("dbo.wms_product_warning", "assembly_time", c => c.Decimal(precision: 18, scale: 4));
            AlterColumn("dbo.wms_tray_info", "Quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_in_bill_detail_history", "bill_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_in_bill_detail_history", "allot_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_in_bill_detail_history", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_in_bill_allot_history", "allot_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_in_bill_allot_history", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_out_bill_detail_history", "bill_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_out_bill_detail_history", "allot_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_out_bill_detail_history", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_out_bill_allot_history", "allot_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_out_bill_allot_history", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_move_bill_detail_history", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_check_bill_detail_history", "quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_check_bill_detail_history", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.wms_profit_loss_bill_detail_history", "quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.inter_contract_detail", "quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.inter_pallet", "quantity", c => c.Decimal(nullable: false, precision: 18, scale: 4));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.inter_pallet", "quantity", c => c.Decimal(nullable: false, precision: 16, scale: 4));
            AlterColumn("dbo.inter_contract_detail", "quantity", c => c.Decimal(nullable: false, precision: 16, scale: 4));
            AlterColumn("dbo.wms_profit_loss_bill_detail_history", "quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_check_bill_detail_history", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_check_bill_detail_history", "quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_move_bill_detail_history", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_out_bill_allot_history", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_out_bill_allot_history", "allot_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_out_bill_detail_history", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_out_bill_detail_history", "allot_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_out_bill_detail_history", "bill_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_in_bill_allot_history", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_in_bill_allot_history", "allot_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_in_bill_detail_history", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_in_bill_detail_history", "allot_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_in_bill_detail_history", "bill_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_tray_info", "Quantity", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.wms_product_warning", "assembly_time", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.wms_product_warning", "max_limited", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.wms_product_warning", "min_limited", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.wms_unit_list", "quantity03", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_unit_list", "quantity02", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_unit_list", "quantity01", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_sorting_lowerlimit", "quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_sort_order", "amount_sum", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_sort_order", "quantity_sum", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_sort_order_detail", "unit_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_sort_order_detail", "amount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_sort_order_detail", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_sort_order_detail", "demand_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_profit_loss_bill_detail", "quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_out_bill_detail", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_out_bill_detail", "allot_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_out_bill_detail", "bill_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_out_bill_allot", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_out_bill_allot", "allot_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_in_bill_detail", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_in_bill_detail", "allot_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_in_bill_detail", "bill_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_in_bill_allot", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_in_bill_allot", "allot_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_check_bill_detail", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_check_bill_detail", "quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.wms_storage", "out_frozen_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.wms_storage", "in_frozen_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.wms_storage", "quantity", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.wms_move_bill_detail", "real_quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}

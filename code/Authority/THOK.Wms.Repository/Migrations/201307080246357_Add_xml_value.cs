namespace THOK.Wms.Repository.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_xml_value : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.wms_xml_value",
                c => new
                    {
                        id = c.String(nullable: false, maxLength: 50),
                        date_time = c.DateTime(nullable: false),
                        xml_value_text = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.wms_xml_value");
        }
    }
}

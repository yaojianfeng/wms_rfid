using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using THOK.LedDisplay.Dal;

namespace THOK.LedDisplay
{
    public partial class LedDisplay : Form
    {

        private BillDal dal = new BillDal();

        private int x = 0;
        private int y = 0;
        private int width = 0;
        private int height = 0;
        private int rowNumber = 0;
        private int startCount = 0;
        private string type = "";

        public LedDisplay()
        {
            InitializeComponent();
        }

        private void LoadXML()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("DisplayConfig.xml");
            XmlNodeList locationList = xml.GetElementsByTagName("Location");
            foreach (XmlNode var in locationList[0].ChildNodes)
            {
                if (var.Attributes["Name"].InnerText == "x")
                {
                    x = Convert.ToInt32( var.Attributes["Value"].InnerText);
                }
                if (var.Attributes["Name"].InnerText == "y")
                {
                    y = Convert.ToInt32(var.Attributes["Value"].InnerText);
                }
            }

            XmlNodeList sizeList = xml.GetElementsByTagName("Size");
            foreach (XmlNode var in sizeList[0].ChildNodes)
            {
                if (var.Attributes["Name"].InnerText == "width")
                {
                    width = Convert.ToInt32(var.Attributes["Value"].InnerText);
                }
                if (var.Attributes["Name"].InnerText == "height")
                {
                    height = Convert.ToInt32(var.Attributes["Value"].InnerText);
                }
            }
            XmlNodeList dispalyTypeList = xml.GetElementsByTagName("DispalyType");
            this.type = dispalyTypeList[0].Attributes["Type"].InnerText;
            this.rowNumber = Convert.ToInt32(dispalyTypeList[0].Attributes["RowNumber"].InnerText);
        }

        private void LedDisplay_Load(object sender, EventArgs e)
        {
            this.LoadXML();
            this.Location = new Point(x, y);
            this.Size = new Size(width, height);
            this.Text = type;
            nIcon.Icon = new Icon("App.ico");
            ContextMenuStrip menu = new ContextMenuStrip();
            nIcon.ContextMenuStrip = menu;
            menu.Items.Add("入库", null, new EventHandler(OnSwitchClick));
            menu.Items.Add("出库", null, new EventHandler(OnSwitchClick));
            menu.Items.Add("移库", null, new EventHandler(OnSwitchClick));
            menu.Items.Add("盘点", null, new EventHandler(OnSwitchClick));
            menu.Items.Add("实时", null, new EventHandler(OnSwitchClick));
            menu.Items.Add("退出", null, new EventHandler(OnCloseClick));
            dgvInfo.EnableHeadersVisualStyles = false;
            dgvInfo.BackgroundColor = Color.Black;
  
            this.Dispaly();
        }



        private void Dispaly()
        {
            DataTable table = dal.GetBillList(type, rowNumber, startCount);
            this.Text = type;
            if(table.Rows.Count == 10)
                startCount = startCount + 10;
            else
                startCount = 0;
            this.dgvInfo.DataSource = table;
            
            this.dgvInfo.Enabled= false;
            if (this.dgvInfo.SelectedRows.Count > 0)
            {
                this.dgvInfo.SelectedRows[0].Selected = false;
            }
           
        }

        private void OnSwitchClick(object sender, EventArgs e)
        {
            type = sender.ToString();
            this.Dispaly();
        }


        private void OnCloseClick(object sender, EventArgs e)
        {
            nIcon.Visible = false;
            Application.Exit();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            this.Dispaly();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using THOK.WES.Interface;
using THOK.WES.Dal;
using THOK.WES.Interface.Model;

namespace THOK.WES.View
{
    public partial class CellQueryForm : THOK.AF.View.ToolbarForm
    {
        private Dictionary<int, DataRow[]> shelf = new Dictionary<int, DataRow[]>();

        private DataTable cellTable = null;
        private bool needDraw = false;
        private bool filtered = false;

        private int columns =38;
        private int rows = 3;
        private int cellWidth = 0;
        private int cellHeight = 0;
        private int currentPage = 1;
        private int[] top = new int[8];
        private int left = 5;
        private string url = @"http://59.61.87.212:8090/Task";
        private ConfigUtil configUtil = new ConfigUtil();
        ShelfInfo ShelfInfo = null;
        
        public CellQueryForm()
        {
            InitializeComponent();

            //设置双缓冲
            SetStyle(ControlStyles.DoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);

            THOKUtil.EnableFilter(dgvMain);            

            pnlData.Visible = true;
            pnlData.Dock = DockStyle.Fill;

            pnlChart.Visible = false;
            pnlChart.Dock = DockStyle.Fill;

            btnChart.Enabled = false;

            pnlChart.MouseWheel += new MouseEventHandler(pnlChart_MouseWheel);
        }
       
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                if (bsMain.Filter.Trim().Length != 0)
                {
                    DialogResult result = MessageBox.Show("重新读入数据请选择'是(Y)',清除过滤条件请选择'否(N)'", "询问", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    switch (result)
                    {
                        case DialogResult.No:
                            DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn.RemoveFilter(dgvMain);
                            return;
                        case DialogResult.Cancel:
                            return;
                    }
                }

                btnRefresh.Enabled = false;
                btnChart.Enabled = false;

                pnlProgress.Top = (pnlMain.Height - pnlProgress.Height) / 3;
                pnlProgress.Left = (pnlMain.Width - pnlProgress.Width) / 2;
                pnlProgress.Visible = true;
                Application.DoEvents();
                url = configUtil.GetConfig("URL")["URL"];
                Task task = new Task(url);
                try
                {
                    task.Getshelf();
                    task.GetShelf += new Task.GetShelfEventHandler(delegate(bool isSuccess, string msg, ShelfInfo[] shelfInfo)
                    {
                        if (shelfInfo != null)
                        {
                            //bsMain.DataSource = shelfInfo;
                            cellTable = new DataTable();
                            cellTable.Columns.Add("ShelfCode");
                            cellTable.Columns.Add("ShelfName");
                            cellTable.Columns.Add("CellCode");
                            cellTable.Columns.Add("CellName");
                            cellTable.Columns.Add("ProductCode");
                            cellTable.Columns.Add("ProductName");
                            cellTable.Columns.Add("QuantityTiao", typeof(decimal));
                            cellTable.Columns.Add("QuantityJian", typeof(decimal));
                            cellTable.Columns.Add("WareCode");
                            cellTable.Columns.Add("WareName");
                            cellTable.Columns.Add("IsActive");
                            cellTable.Columns.Add("RowNum");
                            cellTable.Columns.Add("ColNum");
                            cellTable.Columns.Add("Shelf");
                            foreach (ShelfInfo shelf in shelfInfo)
                            {
                                this.ShelfInfo = shelf;
                                DataRow dr = cellTable.NewRow();
                                dr["ShelfCode"] = shelf.ShelfCode;
                                dr["ShelfName"] = shelf.ShelfName;
                                dr["CellCode"] = shelf.CellCode;
                                dr["CellName"] = shelf.CellName;
                                dr["ProductCode"] = shelf.ProductCode;
                                dr["ProductName"] = shelf.ProductName;
                                dr["QuantityTiao"] = shelf.QuantityTiao;
                                dr["QuantityJian"] = shelf.QuantityJian;
                                dr["WareCode"] = shelf.WareCode;
                                dr["WareName"] = shelf.WareName;
                                dr["IsActive"] = shelf.IsActive;
                                dr["RowNum"] = shelf.RowNum;
                                dr["ColNum"] = shelf.ColNum;
                                dr["Shelf"] = shelf.Shelf;
                                cellTable.Rows.Add(dr);
                            }
                            if (cellTable.Rows.Count > 0)
                            {
                                btnChart.Enabled = true;
                                pnlProgress.Visible = false;
                                btnRefresh.Enabled = true;
                            }
                            bsMain.DataSource = cellTable;
                        }
                        else
                        {
                            bsMain.DataSource = null;
                        }
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("读取数据失败，原因：" + ex.Message, "消息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception exp)
            {
                THOKUtil.ShowInfo("读入数据失败，原因：" + exp.Message);
            }
        }

        private void btnChart_Click(object sender, EventArgs e)
        {
            if (cellTable != null && cellTable.Rows.Count != 0)
            {
                if (pnlData.Visible)
                {
                    filtered = bsMain.Filter != null;
                    needDraw = true;
                    btnRefresh.Enabled = false;
                    pnlData.Visible = false;
                    pnlChart.Visible = true;
                    btnChart.Text = "列表";
                    button1.Visible = true;
                    button2.Visible = true;
                    button3.Visible = false;
                    button4.Visible = true;
                    button5.Visible = true;
                   
                }
                else
                {
                    needDraw = false;
                    btnRefresh.Enabled = true;
                    pnlData.Visible = true;
                    pnlChart.Visible = false;
                    btnChart.Text = "图形";
                    button1.Visible = false;
                    button2.Visible = false;
                    button3.Visible = false;
                    button4.Visible = false;
                    button5.Visible = false;
                }
            }
        }

        private void pnlChart_Paint(object sender, PaintEventArgs e)
        {
            if (needDraw)
            {
                Font font = new Font("宋体", 9);
                SizeF size = e.Graphics.MeasureString("第1排", font);
                float adjustHeight = Math.Abs(size.Height - cellHeight) / 2;
                size = e.Graphics.MeasureString("13", font);
                float adjustWidth = (cellWidth - size.Width) / 2;

                for (int i = 0; i <= 7; i++)
                {
                    string keys = "";
                    int key = currentPage * 8 - (top.Length - (i + 1));
                    if (key < 10)
                    {
                        keys = "0" + key.ToString();
                    }
                    else
                    {
                        keys = key.ToString();
                    }
                    if (!shelf.ContainsKey(key))
                    {
                        DataRow[] rows = cellTable.Select(string.Format("Shelf= {0}", keys), "CellCode");
                        shelf.Add(key, rows);
                    }

                    DrawShelf(shelf[key], e.Graphics, top[i], font, adjustWidth, e);
                    int tmpLeft = left + columns * cellWidth + 5 + cellWidth;
                    for (int j = 0; j < rows; j++)
                    {
                        string s = string.Format("第{0}排第{1}层", shelf[key][i]["ShelfName"], Convert.ToString(j + 1).PadLeft(2, '0'));
                        e.Graphics.DrawString(s, font, Brushes.DarkCyan, tmpLeft, top[i] - 12 + (j + 1) * cellHeight + adjustHeight);//画右边的字体
                    }
                }

                if (filtered)
                {
                    int i = currentPage * top.Length;
                    foreach (DataGridViewRow gridRow in dgvMain.Rows)
                    {
                        DataRowView cellRow = (DataRowView)gridRow.DataBoundItem;
                        int shelf = Convert.ToInt32(cellRow["Shelf"]);
                        int column = Convert.ToInt32(cellRow["ColNum"]) - 1;
                        int row = Convert.ToInt32(cellRow["RowNum"]);
                        int quantity = Convert.ToInt32(cellRow["QuantityJian"]);
                        string storagename = cellRow["CellName"].ToString();
                       // DateTime outDate = Convert.ToDateTime(cellRow["INDATE"]);
                        int topa = 0;
                        if (shelf <= i)
                        {
                            if (currentPage == 1)
                            {
                                topa = top[shelf - 1];
                                FillCell(e.Graphics, topa, row, column, quantity, storagename, e);
                            }
                            else if (currentPage == 2)
                            {
                                if (shelf >= 9)
                                {
                                    topa = top[shelf - 9];
                                    FillCell(e.Graphics, topa, row, column, quantity, storagename, e);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DrawShelf(DataRow[] cellRows, Graphics g, int top, Font font, float adjustWidth, PaintEventArgs e)
        {
            int z = 0;
            for (int j = 0; j < columns; j++)
            {
                if (j + 1 == 19)
                {
                    z = cellWidth;//空出过道或者走廊
                }
                g.DrawString(Convert.ToString(j + 1), font, Brushes.DarkCyan, left + j * cellWidth + adjustWidth + z, top);//画上面的数字
            }
            foreach (DataRow cellRow in cellRows)
            {
                int column = Convert.ToInt32(cellRow["ColNum"]) - 1;
                int row = Convert.ToInt32(cellRow["RowNum"]);
                int quantity = Convert.ToInt32(cellRow["QuantityJian"]);
                string storagename = cellRow["CellName"].ToString();
                //DateTime outDate = Convert.ToDateTime(cellRow["INDATE"]);

                int x = left + column * cellWidth;
                int y = top + row * cellHeight-7;
                if (column >= 18)
                    x = x + cellWidth;//空出过道或者走廊
                g.DrawRectangle(Pens.Blue, new Rectangle(x, y , cellWidth, cellHeight));//画货位边框,y 这个可调整边框
                
                if (!filtered)
                    FillCell(g, top, row, column, quantity, storagename, e);
            }
        }

        private void FillCell(Graphics g, int top, int row, int column, int quantity, string storAgeName, PaintEventArgs e)
        {
            int x = left + column * cellWidth;
            int y = top + row * cellHeight-5;
           
            if (column >= 18)
                x = x + cellWidth;
            if (quantity >= 30)
                g.FillRectangle(Brushes.Gold, new Rectangle(x + 2, y, cellWidth - 3, cellHeight - 4));//画整托盘货位
            else if (quantity > 0)
                g.FillRectangle(Brushes.RoyalBlue, new Rectangle(x + 2, y, cellWidth - 3, cellHeight - 4));//画半托盘货位
            else if (quantity <= 0)
                g.FillRectangle(Brushes.White, new Rectangle(x + 2, y, cellWidth - 3, cellHeight - 4));//画空货位

            TimeSpan timeSpan = DateTime.Now - DateTime.Now;
            int day = timeSpan.Days;
            if (day > 180)
                g.FillRectangle(Brushes.Red, new Rectangle(x + 2, y, cellWidth - 3, cellHeight - 4));//画预警信息
        }

        private void pnlChart_Resize(object sender, EventArgs e)
        {
            cellWidth = (pnlContent.Width - 90 - sbShelf.Width - 20) / columns;
            cellHeight = ((pnlContent.Height / 8) / rows) - 7;
            
            top[0] = 0;
            for (int i = 1; i < top.Length; i++)
            {
                top[i] = pnlContent.Height / top.Length * i;
            }
        }

        private void pnlChart_MouseClick(object sender, MouseEventArgs e)
        {
            int i = 0;
            for (int j = 0; j < top.Length; j++)
            {
                if (e.Y > top[top.Length-1])
                {
                    i = 7; break;
                }
                if (e.Y < top[j + 1])
                {
                    i = j;
                    break;
                }
            }
            int shelf = 0;
            if (currentPage == 1)
                shelf = i + 1;
            else if (currentPage == 2)
                shelf = i + top.Length+1;
            int column = (e.X - left) / cellWidth + 1;
            if (column == 19)
                return;
            if (column >= 20)
                column = column - 1;
            int row = (e.Y - (top[i]) +7) / cellHeight;

            string keys = "";
            if (shelf < 10)
            {
                keys = "0" + shelf.ToString();
            }
            else
            {
                keys = shelf.ToString();
            }
            if (column <= columns+1 && row <= rows)
            {
                DataRow[] cellRows = cellTable.Select(string.Format("Shelf={0} AND ColNum='{1}' AND RowNum='{2}'", keys, column, row));
                if (cellRows.Length != 0)
                {
                    Dictionary<string, Dictionary<string, object>> properties = new Dictionary<string, Dictionary<string, object>>();
                    Dictionary<string, object> property = new Dictionary<string, object>();

                    property.Add("卷烟编码", cellRows[0]["ProductCode"]);
                    property.Add("卷烟名称", cellRows[0]["ProductName"]);
                    property.Add("卷烟件数", cellRows[0]["QuantityJian"] + "件烟");
                    property.Add("卷烟条数", cellRows[0]["QuantityTiao"] + "条烟");
                    //property.Add("入库时间", cellRows[0]["INDATE"]);                    
                    properties.Add("产品信息", property);

                    property = new Dictionary<string, object>();
                    property.Add("仓库编码", cellRows[0]["WareCode"]);
                    property.Add("仓库名称", cellRows[0]["WareName"]);
                    property.Add("货架名称", "第" + cellRows[0]["ShelfName"] + "排货架");
                    property.Add("货位编码", cellRows[0]["CellCode"]);
                    property.Add("货位名称", cellRows[0]["CellName"]);
                    property.Add("列", "第" + column + "列");
                    property.Add("层", "第" + row + "层");
                    property.Add("是否可用", cellRows[0]["IsActive"]);
                    properties.Add("仓库信息", property);

                    CellDialog cellDialog = new CellDialog(properties);
                    cellDialog.ShowDialog();
                }
            }
        }

        private void pnlChart_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0 && currentPage + 1 <= 3)
                sbShelf.Value = (currentPage) * 30;
            else if (e.Delta > 0 && currentPage - 1 >= 1)
                sbShelf.Value = (currentPage - 2) * 30;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Exit();
        }

        private void sbShelf_ValueChanged(object sender, EventArgs e)
        {
            int pos = sbShelf.Value / 30 + 1;
            if (pos != currentPage)
            {
                currentPage = pos;
                pnlChart.Invalidate();
            }
        }
    }
}
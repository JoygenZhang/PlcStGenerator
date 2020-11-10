using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlcStGenerator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            StGenerator.IsWorks3 = true;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
        }


        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void BtnCopyDeclare_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(txtDeclare.Text);
        }

        private void BtnCopyProg_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(txtProg.Text);
        }

        private void BtnPasteData_Click(object sender, EventArgs e)
        {
            Paste2(ref dataGridView1);
        }

        void Paste2(ref DataGridView myDataGridView)
        {
            DataObject o = (DataObject)Clipboard.GetDataObject();

            if (!o.GetDataPresent(DataFormats.Text))
            {
                return;
            }

            if (myDataGridView.RowCount > 0)
                myDataGridView.Rows.Clear();

            if (myDataGridView.ColumnCount > 0)
                myDataGridView.Columns.Clear();

            bool columnsAdded = false;
            string[] pastedRows = Regex.Split(o.GetData(DataFormats.Text).ToString().TrimEnd("\r\n".ToCharArray()), "\r\n");
            foreach (string pastedRow in pastedRows)
            {
                string[] pastedRowCells = pastedRow.Split(new char[] { '\t' });

                if (!columnsAdded)
                {
                    for (int i = 0; i < pastedRowCells.Length; i++)
                        myDataGridView.Columns.Add("col" + i, pastedRowCells[i]);

                    columnsAdded = true;
                    continue;
                }

                myDataGridView.Rows.Add();
                int myRowIndex = myDataGridView.Rows.Count - 1;

                using (DataGridViewRow myDataGridViewRow = myDataGridView.Rows[myRowIndex])
                {
                    for (int i = 0; i < pastedRowCells.Length; i++)
                        myDataGridViewRow.Cells[i].Value = pastedRowCells[i];
                }
            }
        }

        // PasteInData pastes clipboard data into the grid passed to it.
        void PasteInData(ref DataGridView dgv)
        {
            char[] rowSplitter = { '\n', '\r' };  // Cr and Lf.
            char columnSplitter = '\t';         // Tab.

            IDataObject dataInClipboard = Clipboard.GetDataObject();

            string stringInClipboard =
                dataInClipboard.GetData(DataFormats.Text).ToString();

            string[] rowsInClipboard = stringInClipboard.Split(rowSplitter,
                StringSplitOptions.RemoveEmptyEntries);

            int r = dgv.SelectedCells[0].RowIndex;
            int c = dgv.SelectedCells[0].ColumnIndex;

            if (dgv.Rows.Count < (r + rowsInClipboard.Length))
                dgv.Rows.Add(r + rowsInClipboard.Length + 1 - dgv.Rows.Count);

            // Loop through lines:

            int iRow = 0;
            while (iRow < rowsInClipboard.Length)
            {
                // Split up rows to get individual cells:

                string[] valuesInRow =
                    rowsInClipboard[iRow].Split(columnSplitter);

                // Cycle through cells.
                // Assign cell value only if within columns of grid:

                int jCol = 0;
                while (jCol < valuesInRow.Length)
                {
                    if ((dgv.ColumnCount - 1) >= (c + jCol))
                        dgv.Rows[r + iRow].Cells[c + jCol].Value =
                        valuesInRow[jCol];

                    jCol += 1;
                } // end while

                iRow += 1;
            } // end while
        } // PasteInData

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private List<DeclareData> _srcList;

        private void Button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count < 1)
                return;

            var declareList = new List<DeclareData>();
            for (var i = 0; i < dataGridView1.Rows.Count; i++)
            {
                var item = new DeclareData
                {
                    Name = (string) (dataGridView1.Rows[i].Cells[0].Value)
                };

                if (string.IsNullOrEmpty(item.Name))
                    continue;

                item.Type = (PlcVarType)Enum.Parse(typeof(PlcVarType), (string)(dataGridView1.Rows[i].Cells[1].Value));
                item.Group = (string)(dataGridView1.Rows[i].Cells[2].Value);

                declareList.Add(item);
            }

            _srcList = declareList;

            txtDeclare.Text = StGenerator.BuildDeclares(declareList).ToString();
            txtProg.Text = StGenerator.BuildProgs(declareList).ToString();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (_srcList == null)
                return;
            if (_srcList.Count < 1)
                return;

            var groups =
                from item in _srcList
                group item by item.Group into newGroup
                orderby newGroup.Key
                //select new { value = newGroup.Key, count = newGroup.Count() };
                select new { value = newGroup.Key };

            //var src = groups.Select(item => item.value).ToList();
            var src = new List<string>();
            foreach (var data in groups)
            {
                src.Add(data.value);
            }

            txtDeclare.Text = StGenerator.BuildGroupDeclares(src).ToString();

            txtProg.Text = StGenerator.BuildGroupProgs(_srcList).ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Paste2(ref dataGridView2);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var actList = new List<ActionList>();

            for (var i = 0; i < dataGridView2.Rows.Count; i++)
            {
                var item = new ActionList
                {
                    Name = (string) (dataGridView2.Rows[i].Cells[0].Value)
                };

                if (string.IsNullOrEmpty(item.Name))
                    continue;

                item.Act = (string)(dataGridView2.Rows[i].Cells[1].Value);
                item.Group = (string)(dataGridView2.Rows[i].Cells[2].Value);
                actList.Add(item);
            }

            txtProg.Text = StGenerator.BuildFlowProcess(actList).ToString();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var codes = new StringBuilder();

            codes.AppendLine("Name\tType\tGroup");

            codes.AppendLine("Down\tFbCylinder2x1y\tSt1");
            codes.AppendLine("Forward\tFbCylinder2x1y\tSt1");
            codes.AppendLine("Clamp\tFbCylinder0x1y\tSt1");

            codes.AppendLine("Down\tFbCylinder2x1y\tSt3");
            codes.AppendLine("Forward\tFbCylinder2x1y\tSt3");
            codes.AppendLine("Clamp\tFbCylinder0x1y\tSt3");

            Clipboard.SetText(codes.ToString());

            Paste2(ref dataGridView1);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var codes = new StringBuilder();

            codes.AppendLine("Name\tState\tGroup");

            codes.AppendLine("Down\ton\tSt1");
            codes.AppendLine("Clamp\ton\tSt1");
            codes.AppendLine("Down\toff\tSt1");
            codes.AppendLine("Forward\ton\tSt1");
            codes.AppendLine("Down\ton\tSt1");
            codes.AppendLine("Clamp\toff\tSt1");
            codes.AppendLine("Down\toff\tSt1");

            Clipboard.SetText(codes.ToString());

            Paste2(ref dataGridView2);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            var rb = sender as RadioButton;
            if (rb.Checked)
                StGenerator.IsWorks3 = false;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            var rb = sender as RadioButton;
            if (rb.Checked)
                StGenerator.IsWorks3 = true;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            var rb = sender as RadioButton;
            if (rb.Checked)
                StGenerator.BooleanAssignStyle = false;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            var rb = sender as RadioButton;
            StGenerator.BooleanAssignStyle = rb.Checked;
        }

        private void ckbStructureFirst_CheckedChanged(object sender, EventArgs e)
        {
            var chk = sender as CheckBox;
            StGenerator.StructureFirst = chk.Checked;
        }
    }
}

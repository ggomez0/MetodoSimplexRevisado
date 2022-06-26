using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ModifiedSimplexMethod
{
    public partial class MainForm : Form
    {
        int varCount, eqCount, varCountDefault = 4, eqCountDefault = 3;
        double cCoeff;
        ExMatrix C, b;
        ExMatrix[] Ps;
        ArrayList states;
        string[] varNames;
        public MainForm()
        {
            InitializeComponent();
            varCount = varCountDefault;
            eqCount = eqCountDefault;
            tbVars.Text = varCount.ToString();
            tbEqs.Text = eqCount.ToString();
            ApplyDimChange();
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void tbVars_tbEqs_Leave(object sender, EventArgs e)
        {
            int varCountNew = -1;
            int eqCountNew = -1;
            try
            {
                varCountNew = int.Parse(tbVars.Text);
                eqCountNew = int.Parse(tbEqs.Text);
                if (varCountNew <= eqCountNew)
                    throw new Exception();
            }
            catch
            {
                varCountNew = varCount;
                eqCountNew = eqCount;
                tbVars.Text = varCount.ToString();
                tbEqs.Text = eqCount.ToString();
            }
            if (varCountNew != varCount ||
                eqCountNew != eqCount)
            {
                varCount = varCountNew;
                eqCount = eqCountNew;
                try
                {
                    ApplyDimChange();
                }
                catch
                {
                    varCount = varCountDefault;
                    eqCount = eqCountDefault;
                    ApplyDimChange();
                }
            }
        }
        private void doCalculateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ReadData() == false)
                return;
            if (cbMax.Checked == false)
            {
                for (int i = 0; i < varCount; i++)
                    C.Elements[0, i] *= -1;
                cCoeff *= -1;
            }
            lbIters.Items.Clear();
            webBrowser1.DocumentText = "<P>Calculos interrumpidos</P>";
            MsMethod m = new MsMethod(Ps, b, C, cCoeff);
            SelectBasIndForm sForm = new SelectBasIndForm(varCount, eqCount, m);
            if (sForm.ShowDialog() == DialogResult.Cancel)
                return;
            int[] basInd = sForm.GetBasInd();
            m = new MsMethod(Ps, b, C, cCoeff);
            try
            {
                m.SetBasis(basInd);
                while (m.DoIteration()) ;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Calculos interrumpidos: " + ex.Message,
                    "Calculos interrumpidos", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            states = m.states;
            lbIters.SuspendLayout();
            for (int i = 0; i < states.Count; i++)
            {
                IterationState s = states[i] as IterationState;
                string str = i.ToString();
                if (s.isDecisionLegal)
                    str += ") Z = " + s.GetFuncValue().ToString();
                else
                    str += ") Z = " + s.GetFuncValue().ToString();
                lbIters.Items.Add(str);
                
            }
            lbIters.ResumeLayout();
            webBrowser1.DocumentText = (m.states[0] as IterationState).GetReport();
            tabControl1.SelectedIndex = 1;
            if (cbMax.Checked == false)
                MessageBox.Show("Porque se requiere minimizacion, " +
                    "la funcion objetiva se ha multiplicado por -1," +
                    "se ha maximizado",
                    "La funcion objetivo se multiplica por -1", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
        }
        private void ApplyDimChange()
        {
            tbVars.Text = varCount.ToString();
            tbEqs.Text = eqCount.ToString();
            cCoeff = 0;
            tbCoeffC.Text = "0";
            C = new ExMatrix(1, varCount);
            b = new ExMatrix(eqCount, 1);
            Ps = new ExMatrix[varCount];
            for (int i = 0; i < varCount; i++)
                Ps[i] = new ExMatrix(eqCount, 1);
            mdgvC.Matrix = C;
            mdgvB.Matrix = b;
            mdgvPs.Matrix = new ExMatrix(Ps);
            lbIters.Items.Clear();
            webBrowser1.DocumentText = "<P>Para empezar con el programa " +
                "Introduzca los datos originales</P>";
        }
        private bool ReadData()
        {
            try
            {
                cCoeff = double.Parse(tbCoeffC.Text);
                C = mdgvC.Matrix;
                b = mdgvB.Matrix;
                ExMatrix P = mdgvPs.Matrix;
                Ps = new ExMatrix[varCount];
                for (int j = 0; j < varCount; j++)
                {
                    Ps[j] = new ExMatrix(eqCount, 1);
                    for (int i = 0; i < eqCount; i++)
                        Ps[j].Elements[i, 0] = P.Elements[i, j];
                }
                return true;
            }
            catch
            {
                MessageBox.Show("Error de entrada de datos. Compruebe la disponibilidad " +
                    "Matriz de elementos vacios y valor de coeficiente " +
                    "Funcion objetivo con variable libre",
                    "Error de entrada", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        private void lbIters_SelectedValueChanged(object sender, EventArgs e)
        {
            int i = lbIters.SelectedIndex;
            if (states == null || i < 0 ||
                i >= states.Count || states.Count == 0)
                return;
            webBrowser1.DocumentText = (states[i] as IterationState).GetReport();
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }
        private void SaveFile(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create);
            StreamWriter w = new StreamWriter(fs, Encoding.Default);
            w.WriteLine("Variables " + varCount.ToString());
            w.WriteLine("Restricciones " + eqCount.ToString());
            w.WriteLine("Factor de funcion objetivo en variable libre");
            w.WriteLine(cCoeff.ToString());
            w.WriteLine("Мatriz С");
            C.StreamWrite(w);
            w.WriteLine("Matriz b");
            b.StreamWrite(w);
            for (int i = 0; i < Ps.Length; i++)
            {
                ExMatrix P = Ps[i] as ExMatrix;
                w.WriteLine("Matriz P" + i.ToString());
                P.StreamWrite(w);
            }            
            w.Close();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void OpenFile(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open);
            StreamReader r = new StreamReader(fs, Encoding.Default);
            char[] sep = new char[4] { ' ', '\t', '\r', '\n' };
            string[] arrStr = r.ReadLine().Split(sep);
            varCount = (int)int.Parse(arrStr[1]);
            arrStr = r.ReadLine().Split(sep);
            eqCount = (int)int.Parse(arrStr[1]);
            ApplyDimChange();
            r.ReadLine();
            string s = r.ReadLine();
            cCoeff = (double)double.Parse(s);
            r.ReadLine();
            C.StreamRead(r);
            r.ReadLine();
            b.StreamRead(r);
            Ps = new ExMatrix[varCount];
            for (int i = 0; i < Ps.Length; i++)
            {
                s = r.ReadLine();
                Ps[i] = new ExMatrix(r);                
            }
            mdgvC.Matrix = C;
            mdgvB.Matrix = b;
            mdgvPs.Matrix = new ExMatrix(Ps);
            tbCoeffC.Text = cCoeff.ToString();
            r.Close();
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (ReadData() == false)
                    return;
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    SaveFile(saveFileDialog1.FileName);
                }
            }
            catch
            {
                MessageBox.Show("Error al guardar el archivo",
                    "Error de guardado", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    OpenFile(openFileDialog1.FileName);
                }
            }
            catch
            {
                MessageBox.Show("Error al abrir el archivo",
                    "Error de apertura", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
       
    }
    public class MatrixDataGridView : DataGridView
    {
        public ExMatrix Matrix
        {
            set
            {
                if (value == null)
                    return;
                if (Rows.Count != value.M ||
                    Columns.Count != value.N)
                {
                    SuspendLayout();
                    Rows.Clear();
                    Columns.Clear();
                    for (int j = 0; j < value.N; j++)
                    {
                        DataGridViewColumn c =
                            new DataGridViewColumn(new DataGridViewTextBoxCell());
                        c.ValueType = typeof(double);
                        Columns.Add(c);
                    }
                    Rows.Add(value.M);
                    for (int i = 0; i < value.M; i++)
                        for (int j = 0; j < value.N; j++)
                            Rows[i].Cells[j].Value = value.Elements[i, j];
                    ResumeLayout();
                }
                else
                {
                    for (int i = 0; i < value.M; i++)
                        for (int j = 0; j < value.N; j++)
                            Rows[i].Cells[j].Value = value.Elements[i, j];
                }
            }
            get
            {
                if (Rows.Count == 0 || Columns.Count == 0)
                    return null;
                ExMatrix res = new ExMatrix(Rows.Count, Columns.Count);
                for (int i = 0; i < Rows.Count; i++)
                    for (int j = 0; j < Columns.Count; j++)
                        res.Elements[i, j] = (double)Rows[i].Cells[j].Value;
                return res;
            }
        }
        public MatrixDataGridView()
        {
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            this.ColumnHeadersVisible = false;
            this.RowHeadersVisible = false;
            this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.AutoSize = true;
            this.BackgroundColor = Color.LightGray;
        }
        protected override void OnDataError(bool displayErrorDialogIfNoHandler,
            DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show("Formato de datos Incorrecto. Para números fraccionarios, use una coma",
                "Error de entrada de datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            base.OnDataError(false, e);
        }
    }
}
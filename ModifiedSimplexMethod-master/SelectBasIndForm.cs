using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ModifiedSimplexMethod
{
    public partial class SelectBasIndForm : Form
    {
        int varCount, eqCount;
        int[] basInd;
        MsMethod method;
        public SelectBasIndForm(int varCount, int eqCount, MsMethod method)
        {
            InitializeComponent();
            this.varCount = varCount;
            this.eqCount = eqCount;
            this.method = method;
            for (int i = 0; i < varCount; i++)
            {
                bool chkState = false;
                if (i < eqCount)
                    chkState = true;
                clbBasInd.Items.Add(i, chkState);
            }
        }
        public int[] GetBasInd()
        {
            return basInd;
        }
        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                if (clbBasInd.CheckedIndices.Count != eqCount)
                    throw new Exception("Número incorrecto de variables base");
                basInd = new int[eqCount];
                for (int i = 0; i < eqCount; i++)
                    basInd[i] = clbBasInd.CheckedIndices[i];
                method.CheckUpData(basInd);
            }
            catch
            {
                MessageBox.Show("La base no se ha establecido correctamente", " Error en el establecimiento de la base",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
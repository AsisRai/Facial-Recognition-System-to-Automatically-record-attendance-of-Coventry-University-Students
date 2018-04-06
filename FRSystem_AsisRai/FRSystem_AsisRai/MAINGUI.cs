using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FRSystem_AsisRai
{
    public partial class MAINGUI : Form
    {
        public MAINGUI()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AddStudent add = new AddStudent();
            add.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DetectAndAttendance detect = new DetectAndAttendance();
            detect.Show();
            this.Hide();
        }

        private void MAINGUI_Load(object sender, EventArgs e)
        {

        }
    }
}

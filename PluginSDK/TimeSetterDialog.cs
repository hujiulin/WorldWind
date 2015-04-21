using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WorldWind
{
    public partial class TimeSetterDialog : Form
    {
        public DateTime DateTimeUtc
        {
            get 
            {
                if (checkBoxUTC.Checked)
                {
                    return dateTimePicker1.Value;
                }
                else
                {
                    return dateTimePicker1.Value.ToUniversalTime();
                }
            }
            set 
            {
                if (checkBoxUTC.Checked)
                {
                    dateTimePicker1.Value = value;
                }
                else
                {
                    dateTimePicker1.Value = value.ToLocalTime();
                }
            }
        }

        public TimeSetterDialog()
        {
            InitializeComponent();
        }

        private void checkBoxUTC_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxUTC.Checked)
            {
                dateTimePicker1.Value = dateTimePicker1.Value.ToUniversalTime();
            }
            else
            {
                dateTimePicker1.Value = dateTimePicker1.Value.ToLocalTime();
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (checkBoxUTC.Checked)
            {
                TimeKeeper.CurrentTimeUtc = dateTimePicker1.Value;
            }
            else
            {
                TimeKeeper.CurrentTimeUtc = dateTimePicker1.Value.ToUniversalTime();
            }
        }
    }
}
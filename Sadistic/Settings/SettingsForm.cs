using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sadistic.Settings;

namespace Sadistic.Settings
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = SadisticRoutine.WindowSettings;
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SadisticRoutine.WindowSettings.Save();
        }
    }
}
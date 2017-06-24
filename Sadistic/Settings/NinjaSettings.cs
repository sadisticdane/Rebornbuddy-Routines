using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using ff14bot.Helpers;

namespace Sadistic.Settings
{
    internal class NinjaSettings : SadisticSettings
    {
        public NinjaSettings(string filename = "Ninja-SadisticSettings") : base(filename)
        {
        }


        [Setting]
        [DefaultValue(false)]
        public bool UseAoe { get; set; }

    }



}
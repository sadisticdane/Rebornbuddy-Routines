using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ff14bot;
using ff14bot.Helpers;
using Newtonsoft.Json;

namespace Sadistic.Settings
{
    public class SadisticSettings : JsonSettings
    {
        [JsonIgnore] private static SadisticSettings _instance;

        public static SadisticSettings Instance
        {
            get { return _instance ?? (_instance = new SadisticSettings("SadisticSettings")); }
        }


        public SadisticSettings(string filename) : base(Path.Combine(CharacterSettingsDirectory, filename + ".json"))
        {
        }

        #region Misc
        
        [Setting]
        [DefaultValue(true)]
        public bool Debug { get; set; }

        [Setting]
        [DefaultValue(40f)]
        public float RestHealth { get; set; }

        [Setting]
        [DefaultValue(40f)]
        public float RestEnergy { get; set; }


        [Setting]
        [DefaultValue(false)]
        public bool SummonChocobo { get; set; }

        [Browsable(false)]
        public virtual float RestEnergyDone
        {
            get
            {
                if (RestEnergy*2 < 100)
                    return RestEnergy*2;
                return 100f;
            }
        }

        [Browsable(false)]
        public virtual float RestHealthDone
        {
            get
            {
                if (RestHealth*2 < 100)
                    return RestHealth*2;
                return 100f;
            }
        }
        #endregion

        #region Paladin

        [Setting]
        [DefaultValue(true)]
        public bool PaladinUseAOE { get; set; }

        #endregion
    }
}
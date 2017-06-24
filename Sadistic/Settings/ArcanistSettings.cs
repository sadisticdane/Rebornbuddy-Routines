using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using ff14bot.Helpers;

namespace Sadistic.Settings
{
    internal enum PetTypes
    {
        Caster,
        Tank,
        Attacker
    }


    internal class ArcanistSettings : SadisticSettings
    {
        public ArcanistSettings(string filename = "Arcanist-SadisticSettings"): base(filename)
        {
        }

        [Setting]
        [DefaultValue(PetTypes.Caster)]
        public PetTypes PetKind { get; set; }

        [Setting]
        [UpdateDefaultValue(60.0f,30.0f)]
        [DefaultValue(30.0f)]
        public float HealPet { get; set; }

        [Setting]
        [DefaultValue(60.0f)]
        public float SustainPet { get; set; }

    }

    internal class SummonerSettings : ArcanistSettings
    {
        public SummonerSettings(): base("Summoner-SadisticSettings")
        {
        }
    }

}
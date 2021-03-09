using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace AutomatedPopularPreLaunchExperiment
{
    public class AppleOptions : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "General Settings"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "APPLE"; } }
        public override string DisplaySection { get { return "APPLE"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return true; } }

        [GameParameters.CustomParameterUI("All On")]
        public bool allOn = true;

        [GameParameters.CustomParameterUI("Editor Filter By Size")]
        public bool sizeFilterOn = true;

        [GameParameters.CustomParameterUI("SAS on for Ships and Probes")]
        public bool sasOn = true;

        [GameParameters.CustomParameterUI("Display to Ap/Pe Info")]
        public bool manNodeModeOn = true;

        [GameParameters.CustomParameterUI("Set Brakes for Rovers and Planes")]
        public bool brakesOn = true;

        [GameParameters.CustomParameterUI("Vessel Lights use auto light sensors")]
        public bool shipLightsOn = true;

        [GameParameters.CustomParameterUI("Kerbal visors use auto light sensors")]
        public bool visorOn = true;

        [GameParameters.CustomParameterUI("Kerbal Lights use auto light sensors")]
        public bool kerbalLightsOn = true;

        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
        }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "EnabledForSave")
                return true;

            return true;
        }

    }
}

using ModuleWheels;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AutomatedPopularPreLaunchExperiment
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]

    public class AutomatedPopularPreLaunchExperiment : MonoBehaviour
    {
        // KSPFields set

        public VesselType currentVesselType;
        public AppleOptions appleOptions;
        public KerbalEVA.VisorStates visorStates;
        public bool sasDone = false;
        public bool sasSet;
        public bool dispDone = false;
        public bool dispSet;
        public bool brakesDone = false;
        public bool brakesSet;
        public bool shipLightsAreOn = false;
        public bool sLSet;
        public bool visorSet;
        public bool visorIsDown = false;
        public bool lightSet;
        public bool helmetLightOn = false;
        public bool autoSAS;
        public bool warpRateSet10;
        public bool gearSet250;
        public bool gearIsDeployed = false;
        public bool gearCanDeploy;
        
            

        public void Start()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                try 
                {
                    appleOptions = HighLogic.CurrentGame.Parameters.CustomParams<AppleOptions>();
                    currentVesselType = FlightGlobals.ActiveVessel.vesselType;
                    sasSet = appleOptions.sasOn;
                    dispSet = appleOptions.manNodeModeOn;
                    brakesSet = appleOptions.brakesOn;
                    sLSet = appleOptions.shipLightsOn;
                    visorSet = appleOptions.visorOn;
                    lightSet = appleOptions.kerbalLightsOn;
                    autoSAS = appleOptions.autoSetSAS;
                    warpRateSet10 = appleOptions.warp10;
                    gearSet250 = appleOptions.gear250;
                    
                    // set SAS on

                    if (sasSet && !sasDone)
                    {
                        if (currentVesselType == VesselType.Lander || currentVesselType == VesselType.Plane || 
                            currentVesselType == VesselType.Probe || currentVesselType == VesselType.Ship)
                        {
                            FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.SAS, true);
                        }

                        sasDone = true;                 
                    }

                    // change bottom/left panel to show Ap/Pe info

                    if (dispSet && !dispDone)
                    {
                        if (currentVesselType == VesselType.Lander || currentVesselType == VesselType.Plane ||
                            currentVesselType == VesselType.Probe || currentVesselType == VesselType.Ship)
                        {
                            FlightUIModeController.Instance.SetMode(FlightUIMode.MANEUVER_EDIT);
                        }

                        dispDone = true;
                    }

                    // set brakes for planes/rovers

                    if (brakesSet && !brakesDone)
                    {
                        if (currentVesselType == VesselType.Rover || currentVesselType == VesselType.Plane)
                        {
                            FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Brakes, true);
                        }
                        brakesDone = true;                  
                    }

                    // confirm visor state at start

                    if (FlightGlobals.ActiveVessel.isEVA)
                    {
                        visorStates = FlightGlobals.ActiveVessel.GetComponent<KerbalEVA>().VisorState;
                        if (visorStates.Equals(KerbalEVA.VisorStates.Raised) || visorStates.Equals(KerbalEVA.VisorStates.Raising))
                        {
                            visorIsDown = false;
                        }
                        else if (visorStates.Equals(KerbalEVA.VisorStates.Lowered) || visorStates.Equals(KerbalEVA.VisorStates.Lowering))
                        {
                            visorIsDown = true;
                        }
                    }

                    // set warp lead time

                    if (warpRateSet10)
                    {
                        GameSettings.WARP_TO_MANNODE_MARGIN = 10F;
                    }
                    else if (!warpRateSet10)
                    {
                        GameSettings.WARP_TO_MANNODE_MARGIN = 30F;
                    }

                    // check if any parts are landing gear/wheel related instead of having to check in fixedupdate

                    if (gearSet250)
                    {
                        gearCanDeploy = false;

                        foreach (var part in FlightGlobals.ActiveVessel.Parts)
                        {
                            if (part.HasModuleImplementing<ModuleWheelBase>())
                            {
                                gearCanDeploy = true;
                            }
                        }
                    }
                    
                    
                }
                catch
                {
                    // internal error
                }     
            }

        }


        public void Update()

        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                try
                {                   // Vessel lights
                    if (sLSet)
                    {

                        if (!FlightGlobals.ActiveVessel.directSunlight && !shipLightsAreOn)
                        {
                            foreach (var part in FlightGlobals.ActiveVessel.Parts)
                            {
                                if (part.HasModuleImplementing<ModuleLight>())
                                {
                                    part.GetComponent<ModuleLight>().LightsOn();
                                }
                                else if (part.HasModuleImplementing<ModuleColorChanger>())
                                {
                                    part.SendEvent("Lights On");
                                }
                            }

                            FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Light, true);
                            shipLightsAreOn = true;
                        }



                        // turn off lights if in sunlight

                        else if (FlightGlobals.ActiveVessel.directSunlight && shipLightsAreOn)
                        {
                            foreach (var part in FlightGlobals.ActiveVessel.Parts)
                            {
                                if (part.HasModuleImplementing<ModuleLight>())
                                {
                                    part.GetComponent<ModuleLight>().LightsOff();
                                }
                                else if (part.HasModuleImplementing<ModuleColorChanger>())
                                {
                                    part.SendEvent("Lights Off");
                                }
                            }
                            FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Light, false);
                            shipLightsAreOn = false;
                        }
                    }

                    // visor control

                    if (visorSet)
                    {
                        if (FlightGlobals.ActiveVessel.isEVA && FlightGlobals.ActiveVessel.directSunlight && !visorIsDown)
                        {
                            try
                            {
                                FlightGlobals.ActiveVessel.GetComponent<KerbalEVA>().LowerVisor();
                                visorIsDown = true;
                            }
                            catch
                            {
                                // no helmet
                            }
                        }

                        

                        else if (FlightGlobals.ActiveVessel.isEVA && !FlightGlobals.ActiveVessel.directSunlight && visorIsDown)
                        {
                            try
                            {
                                FlightGlobals.ActiveVessel.GetComponent<KerbalEVA>().RaiseVisor();
                                visorIsDown = false;
                            }

                            catch
                            { // no helmet 
                            }
                        }
                    }

                    // helmet light

                    if (lightSet)
                    {
                        if (FlightGlobals.ActiveVessel.isEVA && FlightGlobals.ActiveVessel.directSunlight && helmetLightOn)
                        {
                            try
                            {
                                FlightGlobals.ActiveVessel.evaController.headLamp.SetActive(false);
                                helmetLightOn = false;
                            }
                            catch
                            {
                                // no helmet
                            }
                        }

                        else if (FlightGlobals.ActiveVessel.isEVA && !FlightGlobals.ActiveVessel.directSunlight && !helmetLightOn)
                        {
                            try
                            {
                                FlightGlobals.ActiveVessel.evaController.headLamp.SetActive(true);
                                helmetLightOn = true;
                            }
                            catch
                            {
                                // no helmet
                            }
                        }
                    }

                    // auto manueuver node selector on SAS

                    if (autoSAS)
                    {
                        try
                        {
                            if (FlightGlobals.ActiveVessel.Autopilot.CanSetMode(VesselAutopilot.AutopilotMode.Maneuver))
                            {
                                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.SAS, true);
                                FlightGlobals.ActiveVessel.Autopilot.SetMode(VesselAutopilot.AutopilotMode.Maneuver);
                            }

                            else
                            {
                                return;
                            }
                        }
                        catch
                        {
                            return;
                        }

                    }
                }
                catch { // internal error
                }
            }
        }

        public void FixedUpdate()
        {

            // landing gear here due to terrain height check

            if (HighLogic.LoadedSceneIsFlight)
            {
                if (gearCanDeploy)
                {
                    if (FlightGlobals.ActiveVessel.vesselType == VesselType.Plane || FlightGlobals.ActiveVessel.vesselType ==
                        VesselType.Rover || FlightGlobals.ActiveVessel.vesselType == VesselType.Lander)
                    {

                        if (gearSet250 && !gearIsDeployed)
                        {
                            float vesHeight = FlightGlobals.ActiveVessel.heightFromTerrain;

                            if (vesHeight <= 1000F)
                            {
                                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Gear, true);
                                gearIsDeployed = true;
                            }

                        }
                        else if (gearSet250 && gearIsDeployed)
                        {
                            float vesHeight = FlightGlobals.ActiveVessel.heightFromTerrain;

                            if (vesHeight > 100F)
                            {
                                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Gear, false);
                                gearIsDeployed = false;
                            }

                        }
                    }
                }

            }
          
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



namespace AutomatedPopularPreLaunchExperiment
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]

    public class AutomatedPopularPreLaunchExperiment : MonoBehaviour
    {
        // KSPFields set

        public VesselType currentVesselType;
        public AppleOptions appleOptions;
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
        public KerbalEVA.VisorStates visorStates;
        public bool lightSet;
        public bool helmetLightOn = false;
        


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

                    if (sasSet && !sasDone)
                    {
                        if (currentVesselType == VesselType.Lander || currentVesselType == VesselType.Plane || 
                            currentVesselType == VesselType.Probe || currentVesselType == VesselType.Ship)
                        {
                            FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.SAS, true);
                        }

                        sasDone = true;                 
                    }

                    if (dispSet && !dispDone)
                    {
                        if (currentVesselType == VesselType.Lander || currentVesselType == VesselType.Plane ||
                            currentVesselType == VesselType.Probe || currentVesselType == VesselType.Ship)
                        {
                            FlightUIModeController.Instance.SetMode(FlightUIMode.MANEUVER_EDIT);
                        }

                        dispDone = true;
                    }

                    if (brakesSet && !brakesDone)
                    {
                        if (currentVesselType == VesselType.Rover || currentVesselType == VesselType.Plane)
                        {
                            FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Brakes, true);
                        }
                        brakesDone = true;                  
                    }

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
                {
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

                }
                catch { // internal error
                }
            }
        }

    }
}

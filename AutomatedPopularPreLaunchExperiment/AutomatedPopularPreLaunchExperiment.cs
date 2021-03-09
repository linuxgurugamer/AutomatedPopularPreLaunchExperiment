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
        private bool sasDone = false;
        private bool brakesDone = false;
        private bool lightsDone = false;
        private bool visorDone = false;
        
        private void Start()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                try 
                {
                    currentVesselType = FlightGlobals.ActiveVessel.vesselType;      // assign vessel type for launch
                }
                catch
                {
                    // not in flight
                }     
            }
        }


        private void Update()

        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                try
                {
                    currentVesselType = FlightGlobals.ActiveVessel.vesselType;      // calling again due to additonal implementations added

                    // set SAS
                    
                    if (!sasDone)
                    {
                        if (currentVesselType == VesselType.Lander || currentVesselType == VesselType.Plane || currentVesselType == VesselType.Probe
                            || currentVesselType == VesselType.Ship)
                        {
                            FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.SAS, true);
                            FlightUIModeController.Instance.SetMode(FlightUIMode.MANEUVER_EDIT);
                        }

                        sasDone = true;                 // only sets once on launch
                    }

                    // set brakes

                    if (!brakesDone)
                    {
                        if (currentVesselType == VesselType.Rover || currentVesselType == VesselType.Plane)
                        {
                            FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Brakes, true);
                        }
                        brakesDone = true;                  // only sets once on launch
                    }

                    // turn on lights if dark

                    if (!FlightGlobals.ActiveVessel.directSunlight && !lightsDone)
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
                        lightsDone = true;
                    }



                    // turn off lights if in sunlight

                    else if (FlightGlobals.ActiveVessel.directSunlight && lightsDone)
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
                        lightsDone = false;
                    }

                    // turn off lights and lower visor if eva in sunlight

                    if (FlightGlobals.ActiveVessel.isEVA & FlightGlobals.ActiveVessel.directSunlight && !visorDone)
                    {
                        FlightGlobals.ActiveVessel.evaController.headLamp.SetActive(false);

                        try
                        {
                            FlightGlobals.ActiveVessel.GetComponent<KerbalEVA>().LowerVisor();
                            visorDone = true;
                        }
                        catch
                        {
                            // no helmet
                        }
                    }

                    // turn on lights and raise visor if in the dark

                    else if (FlightGlobals.ActiveVessel.isEVA && !FlightGlobals.ActiveVessel.directSunlight && visorDone)
                    {
                        FlightGlobals.ActiveVessel.evaController.headLamp.SetActive(true);
                        
                        try
                        {
                            FlightGlobals.ActiveVessel.GetComponent<KerbalEVA>().RaiseVisor();
                            visorDone = false;
                        }

                        catch
                        { // no helmet 
                        }
                    }
                }
                catch { // not in flight scene
                }
            }
        }

    }
}

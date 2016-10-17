/* Copyright © 2016, Eliseo Martín <lttito@gmail.com>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using KSP.IO;
using KSP.UI.Screens.Flight;

namespace SpeedUnitChanger
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SpeedUnitChanger : MonoBehaviour
    {
        /// <summary>
        /// config file path
        /// </summary>
        private static readonly string CONFIG_FILE = KSPUtil.ApplicationRootPath + "GameData/SpeedUnitChanger/settings.dat";

        #region Constants
        /// <summary>
        /// Constant to indicate units are Meters per second
        /// </summary>
        private const int METERS_PER_SECOND = 0;

        /// <summary>
        /// Constant to indicate units are Kilometers per hour
        /// </summary>
        private const int KILOMETERS_PER_HOUR = 1;

        /// <summary>
        /// Constant to indicate units are Miles per hour
        /// </summary>
        private const int MILES_PER_HOUR = 2;

        /// <summary>
        /// Constant to indicate units are knots
        /// </summary>
        private const int KNOTS = 3;

        /// <summary>
        /// Constant to indicate units are feets per second
        /// </summary>
        private const int FEET_PER_SECOND = 4;

        /// <summary>
        /// Constant to indicate units are mach
        /// </summary>
        private const int MACH = 5;

        /// <summary>
        /// Constant to indicate altitude units are meters
        /// </summary>
        private const int METERS = 0;

        /// <summary>
        /// Constant to indicate altitude units are kilometers
        /// </summary>
        private const int KILOMETERS = 1;

        /// <summary>
        /// Constant to indicate altitude units are miles
        /// </summary>
        private const int MILES = 2;

        /// <summary>
        /// Constant to indicate altitude units are nautical miles
        /// </summary>
        private const int NAUTICAL_MILES = 3;

        /// <summary>
        /// Constant to indicate altitude units are feet
        /// </summary>
        private const int FEET = 4;

        #endregion Constants

        /// <summary>
        /// Flag for toolbar
        /// </summary>
        public static bool ToolBarEnabled = false;

        /// <summary>
        /// App variables.
        /// </summary>
        private string currentSpeed = "";
        private int winPosX;
        private int winPosY;
        private string currentUnit = "";
        private double altitude = 0.0;
        private string altitudeText;
        private int currentSpeedIndication = METERS_PER_SECOND;
        private int currentAltitudeIndication = METERS;
        private bool showAltitude = false;
        private ConfigNode config;
        private Rect AltitudeWindow;
        private Rect ConfigurationWindow;
        private string[] content;
        private string[] altitudeUnitNames;
        private SpeedDisplay display;
        private int timeToChangeApsis = 0;
        private bool showApoapsis = true;
        private int timeToChangeApsisThreshold = 500;

        /// <summary>
        /// Object contructor.
        /// </summary>
        public SpeedUnitChanger()
        {
            this.ConfigurationWindow = new Rect(50, 50, 185, 380);
            this.content = new string[6];
            this.altitudeUnitNames = new string[5];
            content[METERS_PER_SECOND] = "Meters per second (m/s)";
            content[KILOMETERS_PER_HOUR] = "Kilometers per hour (km/h)";
            content[MILES_PER_HOUR] = "Miles per hour (mph)";
            content[KNOTS] = "Knots (nmi/h)";
            content[FEET_PER_SECOND] = "Feet per second (ft/s)";
            content[MACH] = "Mach";
            altitudeUnitNames[METERS] = "Meters (m)";
            altitudeUnitNames[KILOMETERS] = "Kilometers (km)";
            altitudeUnitNames[MILES] = "Miles (mi)";
            altitudeUnitNames[NAUTICAL_MILES] = "Nautical miles (nmi)";
            altitudeUnitNames[FEET] = "Feet (ft)";
        }

        /// <summary>
        /// Called when destroyed
        /// </summary>
        void OnDestroy()
        {
            //Nothing to Destroy
            SaveSettings();
        }

        /// <summary>
        /// Prints a message in the debug console
        /// </summary>
        /// <param name="text">text to print</param>
        public static void DebugMessage(string text, string stackTrace = null)
        {
            print("Speed Unit Changer mod: " + text + stackTrace != null ? stackTrace : "");
        }

        private void loadConfig()
        {
            try
            {
                config = ConfigNode.Load(CONFIG_FILE);
                int val = Convert.ToInt32(config.GetValue("unit"));
                bool altWin = Convert.ToBoolean(config.GetValue("alt"));
                int altWinX = Convert.ToInt32(config.GetValue("x"));
                int altWinY = Convert.ToInt32(config.GetValue("y"));
                int altunit = Convert.ToInt32(config.GetValue("altunit"));
                int ttcAt;
                try
                {
                    ttcAt = Convert.ToInt32(config.GetValue("changeThreshold"));
                }
                catch
                {
                    ttcAt = 500;
                }

                config = null;
                currentSpeedIndication = val;
                showAltitude = altWin;
                winPosX = altWinX;
                winPosY = altWinY;
                timeToChangeApsisThreshold = ttcAt;
                currentAltitudeIndication = altunit;
            }
            catch (Exception)
            {
                currentSpeedIndication = METERS_PER_SECOND;
                showAltitude = false;
                winPosX = 50;
                winPosY = 50;
                currentAltitudeIndication = METERS;
                timeToChangeApsisThreshold = 500;
            }

            AltitudeWindow = new Rect(winPosX, winPosY, 100, 70);
        }

        private void SaveSettings()
        {
            ConfigNode savingNode = new ConfigNode();
            savingNode.AddValue("unit", currentSpeedIndication.ToString());
            savingNode.AddValue("alt", showAltitude.ToString());
            savingNode.AddValue("x", AltitudeWindow.x.ToString());
            savingNode.AddValue("y", AltitudeWindow.y.ToString());
            savingNode.AddValue("altunit", currentAltitudeIndication.ToString());
            savingNode.AddValue("changeThreshold", timeToChangeApsisThreshold.ToString());
            try
            {
                savingNode.Save(CONFIG_FILE);
            }
            catch (Exception ex)
            {
                SpeedUnitChanger.DebugMessage(ex.Message + "IN Saving configuration file");
            }
        }

        /// <summary>
        /// Called when plugin is loaded
        /// </summary>
        public void Start()
        {
            loadConfig();
        }

        /// <summary>
        /// Called when drawn
        /// </summary>
        public void OnGUI()
        {
            if (display == null)
            {
                display = GameObject.FindObjectOfType<SpeedDisplay>();
            }
            if (ToolBarEnabled)
            {
                ConfigurationWindow = GUI.Window(100, ConfigurationWindow, OnWindow, "Speed Unit Changer", HighLogic.Skin.window);
            }
        }

        /// <summary>
        /// Called when windowed
        /// </summary>
        /// <param name="windowId"></param>
        public void OnWindow(int windowId)
        {
            GUILayout.BeginVertical(GUILayout.Width(170f));
            showAltitude = GUILayout.Toggle(showAltitude, "Show Altitude");
            GUILayout.Label("Speed unit selection");
            currentSpeedIndication = GUILayout.SelectionGrid(currentSpeedIndication, content, 1);
            GUILayout.Label("Altitude unit selection");
            currentAltitudeIndication = GUILayout.SelectionGrid(currentAltitudeIndication, altitudeUnitNames, 1);
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        public void LateUpdate()
        {
            FlightGlobals.SpeedDisplayModes speedDisplayMode = FlightGlobals.speedDisplayMode;
            if (currentSpeedIndication != METERS_PER_SECOND)
            {
                UpdateSpeedValue(speedDisplayMode);
            }
            if (showAltitude)
            {
                UpdateAltitudeValue(speedDisplayMode);
            }
        }

        private void UpdateSpeedValue(FlightGlobals.SpeedDisplayModes speedDisplayMode)
        {
            switch (currentSpeedIndication)
            {
                case KILOMETERS_PER_HOUR:
                    currentUnit = "km/h";
                    if (speedDisplayMode == FlightGlobals.SpeedDisplayModes.Surface)
                    {
                        currentSpeed = (FlightGlobals.ActiveVessel.srfSpeed * 3.6f).ToString("0.0");
                    }
                    else if (speedDisplayMode == FlightGlobals.SpeedDisplayModes.Target)
                    {
                        currentSpeed = (FlightGlobals.ship_tgtSpeed * 3.6f).ToString("0.0");
                    }
                    else
                    {
                        currentSpeed = (FlightGlobals.ship_obtSpeed * 3.6f).ToString("0.0");
                    }
                    break;
                case MILES_PER_HOUR:
                    currentUnit = "mph";
                    if (speedDisplayMode == FlightGlobals.SpeedDisplayModes.Surface)
                    {
                        currentSpeed = (FlightGlobals.ActiveVessel.srfSpeed * 2.23693629f).ToString("0.0");
                    }
                    else if (speedDisplayMode == FlightGlobals.SpeedDisplayModes.Target)
                    {
                        currentSpeed = (FlightGlobals.ship_tgtSpeed * 2.23693629f).ToString("0.0");
                    }
                    else
                    {
                        currentSpeed = (FlightGlobals.ship_obtSpeed * 2.23693629f).ToString("0.0");
                    }
                    break;
                case KNOTS:
                    currentUnit = "knots";
                    if (speedDisplayMode == FlightGlobals.SpeedDisplayModes.Surface)
                    {
                        currentSpeed = (FlightGlobals.ActiveVessel.srfSpeed * 1.94384449f).ToString("0.0");
                    }
                    else if (speedDisplayMode == FlightGlobals.SpeedDisplayModes.Target)
                    {
                        currentSpeed = (FlightGlobals.ship_tgtSpeed * 1.94384449f).ToString("0.0");
                    }
                    else
                    {
                        currentSpeed = (FlightGlobals.ship_obtSpeed * 1.94384449f).ToString("0.0");
                    }
                    break;
                case FEET_PER_SECOND:
                    currentUnit = "ft/s";
                    if (speedDisplayMode == FlightGlobals.SpeedDisplayModes.Surface)
                    {
                        currentSpeed = (FlightGlobals.ActiveVessel.srfSpeed * 3.2808399f).ToString("0.0");
                    }
                    else if (speedDisplayMode == FlightGlobals.SpeedDisplayModes.Target)
                    {
                        currentSpeed = (FlightGlobals.ship_tgtSpeed * 3.2808399f).ToString("0.0");
                    }
                    else
                    {
                        currentSpeed = (FlightGlobals.ship_obtSpeed * 3.2808399f).ToString("0.0");
                    }
                    break;
                case MACH:
                    currentUnit = "Mach";
                    if (speedDisplayMode == FlightGlobals.SpeedDisplayModes.Surface)
                    {
                        currentSpeed = (FlightGlobals.ActiveVessel.mach).ToString("0.00");
                    }
                    else if (speedDisplayMode == FlightGlobals.SpeedDisplayModes.Target)
                    {
                        currentSpeed = FlightGlobals.ship_tgtSpeed.ToString("0.0");
                        currentUnit = "m/s";
                    }
                    else
                    {
                        currentSpeed = FlightGlobals.ship_obtSpeed.ToString("0.0");
                        currentUnit = "m/s";
                    }
                    break;
            }
            
            display.textSpeed.text = currentSpeed + " " + currentUnit;
        }

        private void UpdateAltitudeValue(FlightGlobals.SpeedDisplayModes speedDisplayMode)
        {
            switch(speedDisplayMode)
            {
                case FlightGlobals.SpeedDisplayModes.Surface:
                    switch (currentAltitudeIndication)
                    {
                        case METERS:
                            altitude = FlightGlobals.ActiveVessel.altitude;
                            if (altitude > 100000)
                            {
                                altitude /= 1000;
                                altitudeText = altitude.ToString("0.000") + " km";
                            }
                            else
                            {
                                altitudeText = altitude.ToString("0.000") + " m";
                            }
                            break;
                        case KILOMETERS:
                            altitude = FlightGlobals.ActiveVessel.altitude / 1000;
                            altitudeText = altitude.ToString("0.000") + " km";
                            break;
                        case MILES:
                            altitude = FlightGlobals.ActiveVessel.altitude / 1609.344;
                            altitudeText = altitude.ToString("0.000") + " mi";
                            break;
                        case NAUTICAL_MILES:
                            altitude = FlightGlobals.ActiveVessel.altitude / 1852;
                            altitudeText = altitude.ToString("0.000") + " nmi";
                            break;
                        case FEET:
                            altitude = FlightGlobals.ActiveVessel.altitude * 3.2808399;
                            if (altitude > 100000)
                            {
                                altitude /= 1000;
                                altitudeText = altitude.ToString("0.000") + " kft";
                            }
                            else
                            {
                                altitudeText = altitude.ToString("0.000") + " ft";
                            }
                            break;
                    }

                    string textTitle = display.textTitle.text;
                    display.textTitle.text = "ASL: " + altitudeText;
                    break;
                case FlightGlobals.SpeedDisplayModes.Orbit:
                    double apsis = FlightGlobals.ActiveVessel.GetCurrentOrbit().PeA;
                    string apsisLabel;
                    string apsisUnit = "m";
                    if (showApoapsis || apsis < 0)
                    {
                        //Apoapsis
                        apsis = FlightGlobals.ActiveVessel.GetCurrentOrbit().ApA;                        
                        apsisLabel = "Ap";
                        timeToChangeApsis++;
                    }
                    else
                    {
                        //Periapsis
                        apsis = FlightGlobals.ActiveVessel.GetCurrentOrbit().PeA;
                        apsisLabel = "Pe";
                        timeToChangeApsis++;
                    }
                    //First check to avoid overflow: m to km
                    if (apsis > 100000)
                    {
                        apsis = apsis / 1000;
                        apsisUnit = "km";
                    }
                    //Second check to avoid overflow: km to Mm
                    if (apsis > 10000)
                    {
                        apsis = apsis / 1000;
                        apsisUnit = "Mm";
                    }

                    display.textTitle.text = string.Format("{0}:{1}{2}", apsisLabel, apsis.ToString("0.000"), apsisUnit);
                    if (timeToChangeApsis >= timeToChangeApsisThreshold)
                    {
                        showApoapsis = !showApoapsis;
                        timeToChangeApsis = 0;
                    }
                    break;
                case FlightGlobals.SpeedDisplayModes.Target:
                    string targetText = string.Format("->{0}", FlightGlobals.ActiveVessel.targetObject.GetName());
                    if (targetText.Length > 25)
                    {
                        targetText = targetText.Substring(0, 22) + "...";
                    }
                    display.textTitle.fontSize = 5;
                    display.textTitle.text = targetText;
                    break;
            }
        }
    }
}

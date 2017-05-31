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

        /// <summary>
        /// Contant to automatically change unit to M to avoid wrapping / overflow.
        /// </summary>
        private const int THRESHOLD_TO_AUTO_CHANGE_M = 10000000;

        /// <summary>
        /// Contant to automatically change unit to K to avoid wrapping / overflow.
        /// </summary>
        private const int THRESHOLD_TO_AUTO_CHANGE_K = 100000;

        #endregion Constants

        /// <summary>
        /// Flag for toolbar
        /// </summary>
        public static bool ToolBarEnabled = false;

        /// <summary>
        /// App variables.
        /// </summary>
        private string currentSpeed = "";
        private string currentUnit = "";
        private double altitude = 0.0;
        private string altitudeText;
        private int currentSpeedIndication = METERS_PER_SECOND;
        private int currentAltitudeIndication = METERS;
        private bool showAltitude = false;
        private bool showSpeed = true;
        private ConfigNode config;
        private Rect ConfigurationWindow;
        private string[] content;
        private string[] altitudeUnitNames;
        private SpeedDisplay display;
        private float stockTitleFontSize;
        private float stockSpeedFontSize;
        /// <summary>
        /// Object contructor.
        /// </summary>
        public SpeedUnitChanger()
        {
            this.ConfigurationWindow = new Rect(50, 50, 280, 400);
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
            Destroy(this);
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
                int altunit = Convert.ToInt32(config.GetValue("altunit"));
                bool sSpeed = Convert.ToBoolean(config.GetValue("sSpeed"));

                config = null;
                currentSpeedIndication = val;
                showAltitude = altWin;
                currentAltitudeIndication = altunit;
                showSpeed = sSpeed;
            }
            catch (Exception)
            {
                currentSpeedIndication = METERS_PER_SECOND;
                showAltitude = false;
                currentAltitudeIndication = METERS;
                showSpeed = true;
            }
        }

        private void SaveSettings()
        {
            ConfigNode savingNode = new ConfigNode();
            savingNode.AddValue("unit", currentSpeedIndication.ToString());
            savingNode.AddValue("alt", showAltitude.ToString());
            savingNode.AddValue("altunit", currentAltitudeIndication.ToString());
            savingNode.AddValue("sSpeed", showSpeed.ToString());
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
                if (display != null)
                {
                    stockSpeedFontSize = display.textSpeed.fontSize;
                    stockTitleFontSize = display.textTitle.fontSize;
                }
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
            GUILayout.BeginVertical(GUILayout.Width(260f));
            showAltitude = GUILayout.Toggle(showAltitude, "Show AGL / Ap - Pe / Target Name");
            if(showAltitude)
            {
                showSpeed = GUILayout.Toggle(showSpeed, "Show speed on Orbit Mode");
            }
            if (!showAltitude || !showSpeed)
            {
                FlightGlobals.SetSpeedMode(FlightGlobals.speedDisplayMode);
                display.textSpeed.fontSize = stockSpeedFontSize;
                display.textTitle.fontSize = stockTitleFontSize;
            }
            GUILayout.Label("Speed unit selection");
            currentSpeedIndication = GUILayout.SelectionGrid(currentSpeedIndication, content, 1);
            GUILayout.Label("Altitude unit selection - AGL Mode only");
            currentAltitudeIndication = GUILayout.SelectionGrid(currentAltitudeIndication, altitudeUnitNames, 1);
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        public void LateUpdate()
        {
            if (display != null)
            {
                UpdateSpeedValue(FlightGlobals.speedDisplayMode);
                if (showAltitude)
                {
                    UpdateAltitudeValue(FlightGlobals.speedDisplayMode);
                }
            }
        }

        private void UpdateSpeedValue(FlightGlobals.SpeedDisplayModes speedDisplayMode, bool forceCalculate = false)
        {
            if (currentSpeedIndication != METERS_PER_SECOND && (forceCalculate|| showSpeed || speedDisplayMode != FlightGlobals.SpeedDisplayModes.Orbit || speedDisplayMode == FlightGlobals.SpeedDisplayModes.Orbit && !showAltitude))
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
        }

        private void UpdateAltitudeValue(FlightGlobals.SpeedDisplayModes speedDisplayMode)
        {
            double realAltitude = FlightGlobals.ActiveVessel.terrainAltitude > 0 ? FlightGlobals.ActiveVessel.altitude - FlightGlobals.ActiveVessel.terrainAltitude : FlightGlobals.ActiveVessel.altitude;
            display.textTitle.overflowMode = TMPro.TextOverflowModes.Overflow;
            switch (speedDisplayMode)
            {
                case FlightGlobals.SpeedDisplayModes.Surface:
                    display.textTitle.enableWordWrapping = false;
                    switch (currentAltitudeIndication)
                    {
                        case METERS:
                            altitude = realAltitude;
                            if (altitude > THRESHOLD_TO_AUTO_CHANGE_M)
                            {
                                altitude /= 1000000;
                                altitudeText = altitude.ToString("0.000") + " Mm";
                            }
                            else if (altitude > THRESHOLD_TO_AUTO_CHANGE_K)
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
                            altitude = realAltitude;
                            if (altitude > THRESHOLD_TO_AUTO_CHANGE_M)
                            {
                                altitude /= 1000000;
                                altitudeText = altitude.ToString("0.000") + " Mm";
                            }
                            else
                            {
                                altitude /= 1000;
                                altitudeText = altitude.ToString("0.000") + " km";
                            }
                            break;
                        case MILES:
                            altitude = realAltitude / 1609.344;
                            altitudeText = altitude.ToString("0.000") + " mi";
                            break;
                        case NAUTICAL_MILES:
                            altitude = realAltitude / 1852;
                            altitudeText = altitude.ToString("0.000") + " nmi";
                            break;
                        case FEET:
                            altitude = realAltitude * 3.2808399;
                            altitudeText = altitude.ToString("0.000") + " ft";
                            break;
                    }
                    display.textTitle.fontSize = stockTitleFontSize;
                    display.textSpeed.fontSize = stockSpeedFontSize;
                    display.textTitle.text = "AGL: " + altitudeText;
                    break;
                case FlightGlobals.SpeedDisplayModes.Orbit:
                    display.textTitle.enableWordWrapping = false;
                    double apoapsis = FlightGlobals.ActiveVessel.GetCurrentOrbit().ApA;
                    string apoapsisUnit = "m";
                    //Apoapsis: First check to avoid overflow: m to Mm
                    if (apoapsis > THRESHOLD_TO_AUTO_CHANGE_M)
                    {
                        apoapsis = apoapsis / 1000000;
                        apoapsisUnit = "Mm";
                    }
                    //Apoapsis: Second check to avoid overflow: m to km
                    else if (apoapsis > 100000)
                    {
                        apoapsis = apoapsis / 1000;
                        apoapsisUnit = "km";
                    }
                    
                    StringBuilder titleDisplayText = new StringBuilder();
                    titleDisplayText.Append(string.Format("Ap:{0}{1}", apoapsis.ToString("0.000"), apoapsisUnit));
                    if (showSpeed)
                    {
                        display.textSpeed.fontSize = stockSpeedFontSize;
                        display.textTitle.fontSize = stockTitleFontSize;
                    }

                    double periapsis = FlightGlobals.ActiveVessel.GetCurrentOrbit().PeA;
                    if (periapsis > 0)
                    {
                        string periapsisUnit = "m";
                        //Periapsis: First check to avoid overflow: m to km
                        if (periapsis > 100000)
                        {
                            periapsis = periapsis / 1000;
                            periapsisUnit = "km";
                        }
                        //Periapsis: Second check to avoid overflow: km to Mm
                        else if (periapsis > THRESHOLD_TO_AUTO_CHANGE_M)
                        {
                            periapsis = periapsis / 1000000;
                            periapsisUnit = "Mm";
                        }
                        if (showSpeed)
                        {
                            titleDisplayText.Append(Environment.NewLine);
                            titleDisplayText.Append(string.Format("Pe:{0}{1}", periapsis.ToString("0.000"), periapsisUnit));
                            display.textTitle.fontSize = 10;
                            display.textSpeed.fontSize = 11;
                        }
                        else
                        {
                            display.textTitle.text = titleDisplayText.ToString();
                            display.textSpeed.fontSize = display.textTitle.fontSize;
                            display.textSpeed.text = string.Format("Pe:{0}{1}", periapsis.ToString("0.000"), periapsisUnit);
                        }
                    }
                    else
                    {
                        UpdateSpeedValue(FlightGlobals.speedDisplayMode, true);
                    }

                    display.textTitle.text = titleDisplayText.ToString();
                    break;
                case FlightGlobals.SpeedDisplayModes.Target:
                    string displayText = string.Format("->{0}", FlightGlobals.ActiveVessel.targetObject.GetName());
                    if(displayText.Length <= 18)
                    {
                        display.textTitle.text = displayText;
                    }
                    else
                    {
                        display.textTitle.text = string.Format("{0}{1}", displayText.Substring(0, 15), "...");
                    }
                    display.textTitle.fontSize = stockTitleFontSize;
                    display.textSpeed.fontSize = stockSpeedFontSize;
                    break;
            }
        }
    }
}

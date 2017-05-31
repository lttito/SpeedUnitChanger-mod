/* Copyright © 2013-2016, Eliseo Martín <lttito@gmail.com>
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
using UnityEngine;
using KSP.UI.Screens;

namespace SpeedUnitChangerToolbar
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SpeedUnitChangerToolbar : MonoBehaviour
    {
        bool addedToGUI = false;
        ApplicationLauncherButton button;

        private void Start()
        {

            SpeedUnitChanger.SpeedUnitChanger.ToolBarEnabled = false;
            if (!addedToGUI)
            {
                OnGuiAppLauncherReady();
            }
        }

        private void OnGuiAppLauncherReady()
        {
            if (!this.addedToGUI)
            { 
                try
                {
                    this.button = ApplicationLauncher.Instance.AddModApplication(
                         () => SpeedUnitChanger.SpeedUnitChanger.ToolBarEnabled = true,
                         () => SpeedUnitChanger.SpeedUnitChanger.ToolBarEnabled = false,
                         null,
                         null,
                         null,
                         null,
                         ApplicationLauncher.AppScenes.FLIGHT,
                         GameDatabase.Instance.GetTexture("SpeedUnitChanger/Textures/iconToolbar", false)
                         );
                    this.addedToGUI = true;
                }
                catch (Exception ex)
                {
                    SpeedUnitChanger.SpeedUnitChanger.DebugMessage(ex.Message + "IN Catch BuildToolbar->OnGuiAppLauncherReady");
                }
            }
        }

        void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(this.OnGuiAppLauncherReady);

            if (this.addedToGUI && this.button != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(this.button);
                this.addedToGUI = false;
            }
            Destroy(this);
        }
    }
}

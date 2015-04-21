//----------------------------------------------------------------------------
// NAME: Joystick control
// VERSION: 1.0
// DESCRIPTION: Allow World Wind to be controlled by joystick/game controller
// DEVELOPER: Bjorn Reppen aka "Mashi"
// WEBSITE: http://www.mashiharu.com
// REFERENCES: Microsoft.DirectX.DirectInput
//----------------------------------------------------------------------------
//
// This file is in the Public Domain, and comes with no warranty. 
//
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using System.Windows.Forms;
using WorldWind.Renderable;
using WorldWind.Camera;
using WorldWind;
using System.IO;
using System;
using System.Threading;
using System.Reflection;

namespace Mashiharu.Sample
{
	/// <summary>
	/// The plugin (main class)
	/// </summary>
	public class Joystick : WorldWind.PluginEngine.Plugin 
	{
		DrawArgs drawArgs;
		Device joystick;
		Thread joyThread;
		const double RotationFactor = 5e-4f;
		const double ZoomFactor = 1.2;
		const int AxisRange = 100;

		/// <summary>
		/// Plugin entry point 
		/// </summary>
		public override void Load() 
		{
			drawArgs = ParentApplication.WorldWindow.DrawArgs;
			DeviceList dl = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly);
			dl.MoveNext();
			if(dl.Current==null)
			{
				throw new ApplicationException("No joystick detected.  Please check your connections and verify your device appears in Control panel -> Game Controllers.");
			}
			DeviceInstance di = (DeviceInstance) dl.Current;
			joystick = new Device( di.InstanceGuid );
			joystick.SetDataFormat(DeviceDataFormat.Joystick);
			joystick.SetCooperativeLevel(ParentApplication,  
				CooperativeLevelFlags.NonExclusive | CooperativeLevelFlags.Background);
			foreach(DeviceObjectInstance d in joystick.Objects) 
			{
				// For axes that are returned, set the DIPROP_RANGE property for the
				// enumerated axis in order to scale min/max values.
				if((d.ObjectId & (int)DeviceObjectTypeFlags.Axis)!=0) 
				{
					// Set the AxisRange for the axis.
					joystick.Properties.SetRange(ParameterHow.ById, d.ObjectId, new InputRange(-AxisRange, AxisRange));
					joystick.Properties.SetDeadZone(ParameterHow.ById, d.ObjectId, 1000); // 10%
				}
			}

			joystick.Acquire();

			// Start a new thread to poll the joystick
			// TODO: The Device supports events, use them
			joyThread = new Thread( new ThreadStart(JoystickLoop) );
			joyThread.IsBackground = true;
			joyThread.Start();
		}

		/// <summary>
		/// Unloads our plugin
		/// </summary>
		public override void Unload() 
		{
			if(joyThread != null && joyThread.IsAlive)
				joyThread.Abort();
			joyThread = null;
		}


		/// <summary>
		/// Background thread runs this function in a loop reading joystick state.
		/// </summary>
		void JoystickLoop()
		{
			while( true )
			{
				Thread.Sleep(20);
				try 
				{
					// Poll the device for info.
					joystick.Poll();
					HandleJoystick();
				}
				catch(InputException inputex) 
				{
					if((inputex is NotAcquiredException) || (inputex is InputLostException)) 
					{
						// Check to see if either the app
						// needs to acquire the device, or
						// if the app lost the device to another
						// process.
						try 
						{
							// Acquire the device.
							joystick.Acquire();
						}
						catch(InputException) 
						{
							// Failed to acquire the device.
							// This could be because the app
							// doesn't have focus.
							Thread.Sleep(1000);
						}
					}
				}
			}
		}

		/// <summary>
		/// Time to update things again.
		/// </summary>
		void HandleJoystick()
		{
			// Get the state of the device.
			JoystickState jss = joystick.CurrentJoystickState;
			byte[] button = jss.GetButtons();
			bool isButtonDown = false;
			if(button[0]!=0)
			{
				// Button 1 pressed
				isButtonDown = true;

				// Heading
				drawArgs.WorldCamera.RotationYawPitchRoll(
					Angle.Zero, 
					Angle.Zero,
					Angle.FromRadians( -jss.X*RotationFactor ) );
				
				// Zoom in/out
				double altitudeDelta = ZoomFactor * drawArgs.WorldCamera.Altitude * (float)jss.Y/AxisRange;
				drawArgs.WorldCamera.Altitude += altitudeDelta;
			}

			if(button[1]!=0)
			{
				// Button 2 pressed
				isButtonDown = true;
						
				// Bank
				drawArgs.WorldCamera.Bank += Angle.FromRadians( jss.X*RotationFactor );

				// Tilt
				drawArgs.WorldCamera.Tilt += Angle.FromRadians( jss.Y*RotationFactor );
			}


			if(!isButtonDown)
			{
				// Normal rotation
				double scaling = 0.2 * RotationFactor * drawArgs.WorldCamera.ViewRange.Radians;

				// Up/Down, Left/Right
				drawArgs.WorldCamera.RotationYawPitchRoll(
					Angle.FromRadians(jss.X*scaling), 
					Angle.FromRadians(-jss.Y*scaling),
					Angle.Zero );
			}
		}
	}
}

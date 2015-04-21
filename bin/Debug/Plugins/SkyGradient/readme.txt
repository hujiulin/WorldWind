Sky Gradient Plugins for WorldWind 1.4 or above (maybe...)
Version 0.5, november 2006
Patrick Murris, http://www.alpix.com/3d/worldwin/

What
----

The Sky Gradient plugin adds a global atmospheric effect to the planet. 
From a halo at hight altitude it fades into a full sky at ground level.

The plugin appears as a layer in the Layer Manager (key L).
Right click on the layer for settings.

Settings allow you to select different presets. 

All settings are saved for the current world.


Install
-------

1. You need WW 1.4 or above

2. Unzip in your WW Plugins directory. That should add a folder named 'SkyGradient'.
	Note: if you dont already have a 'Plugins' directory inside 
	Program Files/Nasa/WorldWind 1.3, then create one.

3. Start WW and the new plugin should be listed in Plug-ins->Load/Unload. 
	Select the plugin and click 'Load'. It should turn green.

4. Once loaded, it becomes a layer in the Layer Manager (key L)

5. Right Click on the layer for settings (properties)


Uninstall
---------

1. Delete the directory
	Program Files/NASA/World Wind 1.3/Plugins/SkyGradient 


Notes
-----

* Sky Gradient vs Sky and Atmosphere plug-ins

This plugin is a small footprint alternative to the bitmap based 
plug-ins Sky and Atmosphere i produced earlier.

The Sky Gradient plug-in paints an atmosphere by numbers only and 
does not use any bitmap pixel, saving texture memory and possibly 
rendering time (not so sure...)


* Adding presets

Presets are stored in individual text files in the plug-in directory. 
They have a .csv extention and can be edited with Notepad.
 
Presets for each worlds must have a name starting with the world name.

	Ex preset for Venus : Venus_Pink_100km.csv 

They are a ";" separated list of values defining the atmosphere : 

	- version of the plugin
	- atmosphere thickness in metres
	- sky color at zenith (decimal RGB values separated by blanks)
	- sky color at the horizon (same)

Ex: 0.1;60000;16 64 176;255 255 255

That is intended for version 0.1 
with a 60 kilometers thick atmosphere. 
Sky color goes from dark blue at zenith (16 64 176) 
to white (255 255 255) at the horizon.


* Adding sky gradient to new worlds

If you want to add the sky gradient effect to new worlds (or one for which 
there are no presets - or atmosphere...), you must create at least one preset 
file (see above) and also a .ini file for that world.

The .ini file must have the same name as the world.

	Ex : Moon.ini

The .ini file will allow the plug-in to load and must refer to the preset 
you just added. To do this, edit an existing .ini file, change the .csv 
file name and save it under the new world name.


* Open source, public domain

This plugin is not compiled and its source can be found in SkyGradient.cs.
Feel free to use, abuse, divert and copy any part of it. Thats how I got started.

Many thanks to the WorldWind community for their efforts and feedback, 
especialy Bjorn Reppen (http://www.mashiharu.com/) for his plugin architecture, 
usefull tips and inspiring examples.


Patrick Murris
Montreal - Nice








Planetary Rings Plugins for WorldWind 1.3.2 or above (maybe...)
Version 0.9, may 20, 2006
Patrick Murris - http://www.alpix.com/3d/worldwin/

What
----

The Planetary Rings plugin add a ring system for each known ringed world.

Settings allow to select different ring variations and to turn shadows on and off.


Install
-------

1. You need at least WW 1.3.2 or 1.3.5 (as of today). Wont work in 1.3.1.

2. Unzip in your WW Plugins directory. That should add a folder named 'PlanetaryRings'.
	Note: if you dont already have a 'Plugins' directory inside 
	Program Files/Nasa/WorldWind 1.3, then create one.

3. Start WW and the new plugin should be listed in Plug-ins->Load/Unload.

4. Once loaded, it becomes a layer in the Layer Manager (key L)

5. Right Click on the layer for settings (properties)


Uninstall
---------

1. Delete the Program Files/Nasa/WorldWind 1.3/Plugins/PlanetaryRings directory


Notes
-----

* Adding ring textures

You can add ring textures by dropping new .png files in the Planetary Rings plugin directory. 
All .png files for Saturn must be named Saturn_something.png. They will become available 
in the layer settings.

Ring textures should be 2 pixel high and several hundreds wide, the left end being close to 
the planet, the right being the outer part of the ring plane.

The alpha channel can often be made by simply using a black and white version of the ring 
texture itself - more or less dimed.

NOTE: the rings texture alpha channel should end with full transparency at both ends.
Otherwise, rings projected shadow on main body may be 'dirty' at the bottom edge...

If you want to add rings to other worlds, create a .ini file for that world from an edited 
copy of an existing one.


* Adjusting sun position for different shadows

The Sun has a default position that serves the shadows computation. You can change it 
by unlocking the sun position in the layer settings. 

When not locked, the sun will mimic the camera latitude, but keep its longitude, casting 
different shadows. To see the 'other side' of the shadows, change the 'Invert' setting.

Once you have a configuration that you like, just lock the sun in the settings. The sun 
orientation will be saved.


* Using night side shadow on world with no rings

It would be possible to use this plug-in just for the night side shadow on other worlds.
 
Do do so for Earth, you would just need a transparent texture named Earth_Void.png to be 
refered as the rings texture in a config file Earth.ini


* Ring textures credits

All textures are based on actual pictures taken by different missions. However, brightness 
and proportion may be a bit off sometimes.

Jupiter (PIA00657 ?), Saturn (PIA06175), Uranus (PIA01977), Neptune (c1141251) 
assembled by Pangloss

Most can be found at http://photojournal.jpl.nasa.gov.

	Ex: http://photojournal.jpl.nasa.gov/catalog/PIA00657


* WW Forum thread 

http://forum.worldwindcentral.com/showthread.php?t=6369


* Public domain & Open source

This plugin is not compiled and its source can be found in PlanetaryRings.cs.
Feel free to use, abuse, divert and copy any part of it. Thats how I got started.

Many thanks to the WorldWind community for their efforts and feedback, 
especialy pangloss for pushing this idea and processing so many ring textures. 
And Bjorn Reppen (http://www.mashiharu.com/) for his plugin architecture, 
usefull tips and inspiring examples.


Patrick Murris
Montreal - Nice








GlobalClouds Plugins for WorldWind 1.3.2 or above (maybe...)
Version 0.6, february 2006
Patrick Murris - http://www.alpix.com/3d/worldwin/

What
----

The GlobalClouds plugin downloads, process alpha channel (tranparency) 
and renders the latest available global cloud map over the globe.

It will update every three hours and keep an history of 10 days.

Settings allow to browse older cloud maps in the history.


Install
-------

1. You need WW 1.3.2 or 1.3.3.1 (as of today). Wont work in 1.3.1.

2. Unzip in your WW Plugins directory. That should add a folder named 'GlobalClouds'.
	Note: if you dont already have a 'Plugins' directory inside 
	Program Files/Nasa/WorldWind 1.3, then create one.

3. Start WW and the new plugin should be listed in Plug-ins->Load/Unload.

4. Once loaded, it becomes a layer in the Layer Manager (key L)

5. Right Click on a layer for settings (properties)

Uninstall
---------

1. Delete the Program Files/Nasa/WorldWind 1.3/Plugins/GlobalClouds directory

Notes
-----

* Keeping cloud maps past ten days

The plugin automaticaly cleans up the history by looking at files named "clouds*.png".
If you want to exclude cloud maps from this cleanup process so they dont go away, 
rename them "saved_clouds_date-time.png" or any name that does not start with 'clouds'.


* Cloud map source

A list of available servers is in GlobalCloudsServers.txt

The cloud map is produced by a script written by Hari Nair and mirrored on servers listed here :
http://xplanet.sourceforge.net/clouds.php

He says : "I create a global cloud map every three hours using GOES, METEOSAT, 
and GMS satellite imagery downloaded from the Geostationary Satellite Imagery page 
at Dundee University.". 

And "These images are not real snapshots nor are they particularly accurate; 
they are mosaics created from geostationary weather satellite images. The times of 
the source images may differ by several hours since they are only available at 
limited times during the day. The cloud maps are not calibrated or carefully geolocated. 
They are only meant to make the earth look pretty!" 

To avoid adding traffic to the Xplanet servers, TheBean (with Bull on his back) has setup 
a mirror for WW users of this plugin : 
http://www.twobeds.com/nasa/clouds/clouds_2048.jpg


* Adding or creating cloud maps

You can add bitmaps to your Plugins/GlobalClouds/ directory and they will 
become available in the layer settings. Supported format is .png.

For realistic effects, the bitmap should have a size ratio of 1:2. 
Example : 800px width x 400px height.

The bottom of the image is the south pole. The top the north pole.
It will be wrapped full circle around the inside of a sphere surrounding the world. 
The center of the image is at lat/lon zero.


More links at the Virtual Terrain Project
http://www.vterrain.org/Atmosphere/index.html


* Public domain & Open source

This plugin is not compiled and its source can be found in GlobalClouds.cs.
Feel free to use, abuse, divert and copy any part of them. Thats how I got started.

Many thanks to the WorldWind community for their efforts and feedback, 
especialy Bjorn Reppen (http://www.mashiharu.com/) for his plugin architecture, 
usefull tips and inspiring examples.


Patrick Murris
Montreal - Nice








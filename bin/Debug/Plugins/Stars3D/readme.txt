Stars3D Plugins for WorldWind 1.3.2 or above (maybe...)
Version 1.1, december 2005
Patrick Murris - http://www.alpix.com/3d/worldwin/

What
----

The Stars3D plugin renders a star background around the world  
and let you choose between different subsets of the Hipparcos 
Catalog. Number of stars range between 5000 and 31000.

The plugin appears as a layer in the Layer Manager (key L).
Right click on the layer to switch star catalogues.

All settings are saved.

Install
-------

1. You need WW 1.3.2 or 1.3.3.1 (as of today). Wont work in 1.3.1.
2. Unzip in your WW Plugins directory. That should add a folder named 'Stars3D'.
	Note: if you dont already have a 'Plugins' directory inside 
	Program Files/Nasa/WorldWind 1.3, then create one.
3. Start WW and the new plugin should be listed in Plug-ins->Load/Unload.
4. Once loaded, it becomes a layer in the Layer Manager (key L)
5. Right Click on the layer for settings (properties)

Uninstall
---------

1. Delete the Plugins/Stars3D directory

Notes
-----

Adding star catalogues

The actual catalogues are subsets of the Hipparcos catalog (ESA 1997) extracted 
with the VizieR Astronomical Server at http://cdsweb.u-strasbg.fr.

This plugin reads an ascii '.tsv' file with ';' separated values that 
looks like that:

recno;HIP;RAhms;DEdms;Vmag;B-V
;;;;mag;mag
--------;------;-----------;-----------;-----;------
     122;   122;00 01 35.85;-77 03 55.1; 4.78; 1.254
     154;   154;00 01 57.59;-06 00 50.3; 4.37; 1.631
     300;   301;00 03 44.37;-17 20 09.5; 4.55;-0.047

This version looks for the fields RAhms, DEdms, Vmag and B-V which can be 
in any order.

The vizieR interface allows to output such ascii files. Just make sure 
the list contains the above fields - which are not always available in catalogs 
other than Hipparcos and Tycho. 

When your list is ready, just drop the .tsv file in the Plugins/Stars3D/ folder. 
It will become available in the layer settings.


Open source

This plugin is not compiled and its source can be found in Stars3D.cs.
Feel free to use, abuse, divert and copy any part of it. Thats how I got started.

Many thanks to the WorldWind community for their efforts and feedback, 
especialy Bjorn Reppen (http://www.mashiharu.com/) for his plugin architecture, 
usefull tips and inspiring examples, and Bull (http://www.bullsworld.co.uk/) for 
pointing out the star backgrounds at NASA and pushing this starfield idea.

Patrick Murris
Montreal - Nice








Compass rose Plugins for WorldWind 1.3.2 or above (maybe...)
Version 0.5
Patrick Murris, december 2005 - http://www.alpix.com/3d/worldwin/

What
----

The Compass rose plugin displays a... compass rose pointing north. 

The plugin appears as a layer in the Layer Manager (key L).
Right click on the layer for settings.

Settings allow you to select various styles and placements for the rose. 

All settings are saved.

Install
-------

1. You need WW 1.3.2 or 1.3.3.1 (as of today). Wont work in 1.3.1.

2. Unzip in your WW Plugins directory. That should add a folder named 'Compass'.
	Note: if you dont already have a 'Plugins' directory inside 
	Program Files/Nasa/WorldWind 1.3, then create one.

3. Start WW and the new plugin should be listed in Plug-ins->Load/Unload. 
	Select the plugin and click 'Load'. It should turn green.

4. Once loaded, it becomes a layer in the Layer Manager (key L)

5. Right Click on the layer for settings (properties)

Uninstall
---------

1. Delete the directory
	Program Files/NASA/World Wind 1.3/Plugins/Compass 

Notes
-----

* Adding SVG compass shapes

You can drop .svg files in Program Files/NASA/World Wind 1.3/Plugins/Compass/ 
They will become available in the layer settings.

This version only suports very basic SVG shapes (line, rect, circle, polyline 
and polygon) and the 'stroke' attribute with named color.

Path, text, fill, stroke-width and hexadecimal color values are not supported...

Units are in screen pixels. The resulting graphic will be offset to match 
the selected placement.

Warning: the <!DOCTYPE ...> sends the plugin checking for the DTD at www.w3.org 
which adds a significant delay when loading the plugin or switching shapes. 
It is better to just comment out that line like that:
<!-- <!DOCTYPE svg PUBLIC "-//W3C//DTD SVG 1.1//EN" "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd"> -->

Some SVG links:

Inkscape - open source SVG editor (produces mainly <path> not supported yet...)
http://www.inkscape.org/

Adobe SVG viewer
http://www.adobe.com/svg/viewer/install/main.html


* Adding compass bitmaps

You can add new compass images (.png with alpha channel) in 
Program Files/NASA/World Wind 1.3/Plugins/Compass/ 
They will become available in the layer settings.

The images are not scaled down. The bigger they are, the bigger 
they will appear on screen... recommended size range between 64x64 
and 128x128 - except for a center-screen overlay...


* Image credits

I searched image.google.com for "Compass rose"... fascinating diversity!

Compass_Simple is widely used on the web and i assumed it is public domain.

Compass_Rose_Classic comes from Pacific Northwest National Laboratory site 
http://picturethis.pnl.gov/picturet.nsf/All/6.VR86?opendocument
not sure it is open...

Compass_Big comes from Joe 'WB2HOL' Leggio.
http://home.att.net/~jleggio/projects/rdf/rdf.htm

Alpha channels, Compass_Vector_Arrow, Compass_Cross_Minimal and 
Compass_Arrow_North are my humble contributions.

Compass_Simple_White is a variation sent by Hermetica (merci).
http://perso.wanadoo.fr/hermetica/index.htm


* Open source, public domain

This plugin is not compiled and its source can be found in Compass.cs.
Feel free to use, abuse, divert and copy any part of it. Thats how I got started.

Many thanks to the WorldWind community for their efforts and feedback, 
especialy Bjorn Reppen (http://www.mashiharu.com/) for his plugin architecture, 
usefull tips and inspiring examples.


Patrick Murris
Montreal - Nice








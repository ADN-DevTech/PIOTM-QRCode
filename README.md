# Plugin of the Month: QRCode

Plugin of the Month, December 2010     
Brought to you by the Autodesk Developer Network

Description
-----------
This plugin can be used embed QR Codes in AutoCAD drawings.

System Requirements
-------------------
This plugin has been tested with AutoCAD 2007 onwards.

A pre-built version of the plugin has been provided which should
work on 32- and 64-bit Windows systems.
The plugin has not been tested with all AutoCAD-based products,
but should work (see "Feedback", below, otherwise).

The source code has been provided as a Visual Studio 2008 project
containing C# code (not required to run the plugin).

Installation
------------
Copy the plugin module, "ADNPlugin-QRCodes.dll", to a location on
your local system (the best place is your AutoCAD-based application's
root program folder).

Inside your AutoCAD-based application, use the NETLOAD command to load
the plugin. As it loads the application will register itself to load
automatically in future sessions of the Autodesk product into which
it has been loaded.

Usage
-----
Once loaded, the QRGEN and -QRGEN commands can be used to create
QR Codes inside the active AutoCAD drawing. These commands provide
the possibility of creating various types of QR Code:

 - Calendar
 - Contact
 - Email
 - Geolocation
 - Phone
 - Text
 - Url
 
The required information can be entered via the command-line (in the
case of the -QRGEN command) or via a dialog GUI (in the case of
QRGEN). It is possible to embed the QR Code as a native AutoCAD hatch
or to use an external raster image.

Uninstallation
--------------
The REMOVEQR command can be used to "uninstall" the plugin, stopping
it from being loaded automatically in future editing sessions.

Known Issues
------------

Author
------
This plugin was written by Augusto Goncalves and Kean Walmsley.

Acknowledgements
----------------

Further Reading
---------------
For more information on developing with AutoCAD, please visit the
AutoCAD Developer Center at http://www.autodesk.com/developautocad

Feedback
--------
Email us at labs.plugins@autodesk.com with feedback or requests for
enhancements.

Release History
---------------

1.0    Original release

# Plugin of the Month: QRCode

Plugin of the Month, December 2010     
Brought to you by the Autodesk Developer Network

Description
-----------
This plugin can be used embed QR Codes in AutoCAD drawings.

System Requirements
-------------------
This plugin has been tested with AutoCAD 2007 onwards. The current 
version of the source code was migrated to AutoCAD 2017

The source code has been provided as a Visual Studio 2015 project
containing C# code

Installation
------------
Requires ObjectARX 2017 or AutoCAD 2017 installed on the machine to
compile this code. Requires AutoCAD 2017 to run this code.

Open the project (.csproj) on Visual Studio 2015. Make sure the reference
path are correct (predefined for default install folders). Compile the
code.

Copy the plugin module, "ADNPlugin-QRCodes.dll", to a location on
your local system (the best place is your AutoCAD-based application's
root program folder).

Inside your AutoCAD-based application, use the NETLOAD command to load
the plugin. 

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
Remove the .DLL

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
1.1    Migrated to AutoCAD 2017 (May 2016)

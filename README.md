# BMSY

I wanted a simple way to read the data of my JBD BMS but ended up also adding support to control and read data from my Growatt inverters.
Written in C# using .net core.

TODO: Add info on how to compile the project

In the current state the project supports a Growatt SPF5000ES and a JBD BMS. All data gathered is published on a MQTT server and can be queried via the REST API.
It should be straightforward to add a new inverter, just implement IInverter and return IInverterInfo, for a BMS it's IBMS and IBMSInfo.

The REST API allows for control of the inverters, you can switch charging sources, select modes, change chare current etc. 
I use this api to perform actions on the inverters from NodeRED. 

TODO: Add info on how to use the API

There is also a UI included that allows you to control the inverters using a browser, in BMSYUI.

In the current state it is stable for what I need it to do, I will be adding more stuff when I find the time.

All configuration is done via appsettings.json

I run it on a Raspberry PI 4 without any problems, as a service.


Licensed via GNU General Public License v3.0.


Thanks to EasyModbusTCP ( http://easymodbustcp.net/en/ ).

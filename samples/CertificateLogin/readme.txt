﻿For this sample, you'll need to have configured SSL in IIS Express for this project.
This can be done via F4 (developer server properties) on the project in the solution 
explorer and enabling SSL. 

Next, you'll need to configure your IIS Express server config to allow the <access>
element to be configured within an application's config file. This involved opening
C:\Users\<your username>\Documents\IISExpress\config\applicationhost.config and finding
the element (around line 81 in my config):

<section name="access" overrideModeDefault="Deny" />

and changing overrideModeDefault to "Allow".

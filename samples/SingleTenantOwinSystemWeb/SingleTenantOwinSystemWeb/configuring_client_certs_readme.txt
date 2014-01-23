If you want to support client certificates then you'll need to have configured SSL in IIS 
Express for this project. This can be done via F4 (developer server properties) on the project 
in the solution explorer and enabling SSL. 

Next, you'll need to configure your IIS Express server config to allow the <access>
element to be configured within an application's config file. This involved opening
C:\Users\<your username>\Documents\IISExpress\config\applicationhost.config and finding
the element (around line 81 in my config):

<section name="access" overrideModeDefault="Deny" />

and changing overrideModeDefault to "Allow".

Also, if you're using self-signed certs for testing you will also need to configure your 
web server as described in this article:

http://www.aspnetwiki.com/configuring-iis-7-with-self-signed-server-and-client-certifi


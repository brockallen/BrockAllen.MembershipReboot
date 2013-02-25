# What is MembershipReboot

The BrockAllen.MembershipReboot project is intended as an on-premise user account and identity management library. It has nothing to do with the ASP.NET Membership Provider (but was inspired by it). MembershipReboot is claims-aware and uses password stretching for proper password storage.

The most common use case will be to integrate this into an ASP.NET or ASP.NET MVC applcation, though the library can also be used over a network as a service.

# Getting Started with  MembershipReboot

There is a core project (BrockAllen.MembershipReboot) and a sample application (BrockAllen.MembershipReboot.Mvc) demonstrating proper use.

## WIF Configuration

The sample application's Web.config shows proper configuration of WIF which includes both of the following:

```xml
  <configSections>
   ...
    <section name="system.identityModel"
             type="System.IdentityModel.Configuration.SystemIdentityModelSection, System.IdentityModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089" />
    <section name="system.identityModel.services"
             type="System.IdentityModel.Services.Configuration.SystemIdentityModelServicesSection, System.IdentityModel.Services, Version=4.0.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089" />
  </configSections>
```
and

```xml
  <system.webServer>
   ...
    <modules>
      <add name="SessionAuthenticationModule"
           type="System.IdentityModel.Services.SessionAuthenticationModule, System.IdentityModel.Services, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
           preCondition="managedHandler" />
    </modules>
  </system.webServer>
```

## Username Configuration

Some applications will want to have a username that is distinct from an email address, but others will want to combine these. This can be configured by setting the _EmailIsUsername_ configuration value in Web.config. 
This value is later picked up within https://github.com/brockallen/BrockAllen.MembershipReboot/blob/master/BrockAllen.MembershipReboot/Services/Accounts/SecuritySettings.cs in the following code:

```C#
  EmailIsUsername = GetAppSettings("EmailIsUsername", false);
```

## Email Configuration

Here is an example configuration for email to use the www.sendgrid.com service:

```XML
  <system.net>
    <mailSettings>
      <smtp deliveryMethod="Network" from="foo+membershipreboot@gmail.com">
        <network 
          host="smtp.sendgrid.net" port="587" 
          userName="mySendgridUsername" password="mySendgridUsername"
          enableSsl="true"
        />
      </smtp>
    </mailSettings>
  </system.net>
```

> TIP: If you are using Google mail, it is very easy to create multiple email addresses for testing. If your real email address
>      is foo@gmail.com then you can also use foo+abc@gmail.com or foo+123@gmail.com and Google mail
>      sends them all to foo@gmail.com. 

## Database Configuration

The samples use Entity Framework. You don't need to.

You can also add claims directly to the database. For example:

```SQL
  INSERT INTO UserClaims (Type, Value, User_UserName)
  VALUES ('http://schemas.microsoft.com/ws/2008/06/identity/claims/role', 'Developer', 'billw')
```


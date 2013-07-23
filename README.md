# What is MembershipReboot

The MembershipReboot project is intended as a user account and identity management library. It has nothing to do with the ASP.NET Membership Provider (but was inspired by it). MembershipReboot was initiated due to [frustrations](http://brockallen.com/2012/06/04/membership-is-not-the-same-as-forms-authentication/) with the built-in ASP.NET Membership system. The goals are to improve upon and provide missing features from ASP.NET Membership.

Some of the features of MembershipReboot are:

* multi-tenant account management
* flexible account storage design (relational or object)
* user identities modeled with claims
* extensible email verification for accounts
* customizable username, password and email validation
* extensible notification system for account updates (e.g. for auditing)
* account linking with external identity providers (enterprise or social)
* proper password storage (via PBKDF2)
	* configurable iterations
	* defaults to OWASP recommendations for iterations (e.g. 64K in year 2012)

The most common use case will be to integrate this into an ASP.NET or ASP.NET MVC application, though the library can also be used over a network as a service.

# Getting Started with MembershipReboot

There is a core project (BrockAllen.MembershipReboot), a unit test project and several sample applications demonstrating various use-cases. The best way to see MembershipReboot in action is to start with the SingleTenantWebApp sample in BrockAllen.MembershipReboot\samples\CurrentSamples.

## MembershipReboot, Claims and Windows Identity Foundation (WIF)

MembershipReboot is intended to support modern identity management, thus it heavily uses claims and some related concepts based upon Windows Identity Foundation in .NET 4.5. If you need a primer before diving in, we suggest the following one-hour video by Dominick Baier: 

[Authentication & Authorization in .NET 4.5 - Claims & Tokens become the standard Model](http://vimeo.com/43549130)

## WIF Configuration

The sample application's web.config shows proper configuration of WIF which includes both of the following:

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

## MembershipReboot Configuration

MembershipReboot allows for some flexibility in how it manages user accounts via its SecuritySettings class. It has these properties:

* ConnectionStringName (string)
	* the connection string to use if using the default database
* MultiTenant (bool)
	* if the deployment is to support multi-tenant
* DefaultTenant (string)
	* the default tenant to use if one is not provided in the various APIs
* EmailIsUsername (bool)
	* is the identifier used for authentication intended to be an email address
* UsernamesUniqueAcrossTenants
	* even in a multi-tenant scenario, usernames must be unique
* RequireAccountVerification (bool)
	* requires a user's email address to be verified before then can login
* AllowLoginAfterAccountCreation (bool)
	* can a user login after account creation, or must they be approved first
* AccountLockoutFailedLoginAttempts (integer)
	* number of failed login attempts before the account is locked out
* AccountLockoutDuration (TimeSpan)
	* duration an account is locked out when the AccountLockoutFailedLoginAttempts is met
* AllowAccountDeletion (bool)
	* allow permanent account deletion in the database (or just use a "closed" flag)
* PasswordHashingIterationCount (integer)
	* number of iterations used in password storage
	* if not specified, then the OWASP recommondations are used (dynamically based upon the current year)
* PasswordResetFrequency (integer)
	* frequency (in number of days) a user must change their password

These settings are configurable in a .config file or in code. See the samples for examples.

## Email Configuration

The default configuration uses the .NET SMTP configuration to send emails. To run the samples you must configure your own SMTP settings. Here is an example configuration for email to use the www.sendgrid.com service:

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

The samples use Entiy Framework and SQL Server Compact. You don't need to and can configure your own repository (such as SQL Azure, or even a NoSql database). See the samples for an example.

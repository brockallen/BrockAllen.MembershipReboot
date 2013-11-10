xcopy ..\..\src\BrockAllen.MembershipReboot.WebHost\bin\release\BrockAllen.MembershipReboot.WebHost.dll lib\net45 /y
xcopy ..\..\src\BrockAllen.MembershipReboot.WebHost\bin\release\BrockAllen.MembershipReboot.WebHost.pdb lib\net45 /y
NuGet.exe pack BrockAllen.MembershipReboot.WebHost.nuspec -OutputDirectory ..\

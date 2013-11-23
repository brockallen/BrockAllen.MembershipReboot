xcopy ..\..\build\BrockAllen.MembershipReboot.Owin.dll lib\net45 /y
xcopy ..\..\build\BrockAllen.MembershipReboot.Owin.pdb lib\net45 /y
NuGet.exe pack BrockAllen.MembershipReboot.Owin.nuspec -OutputDirectory ..\

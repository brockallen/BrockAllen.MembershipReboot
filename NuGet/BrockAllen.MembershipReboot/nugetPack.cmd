xcopy ..\..\build\BrockAllen.MembershipReboot.dll lib\net45 /y
xcopy ..\..\build\BrockAllen.MembershipReboot.pdb lib\net45 /y
NuGet.exe pack BrockAllen.MembershipReboot.nuspec -OutputDirectory ..\

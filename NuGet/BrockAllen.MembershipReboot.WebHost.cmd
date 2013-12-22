mkdir BrockAllen.MembershipReboot.WebHost\lib\net45
xcopy ..\build\BrockAllen.MembershipReboot.WebHost.dll BrockAllen.MembershipReboot.WebHost\lib\net45 /y
xcopy ..\build\BrockAllen.MembershipReboot.WebHost.pdb BrockAllen.MembershipReboot.WebHost\lib\net45 /y
NuGet.exe pack BrockAllen.MembershipReboot.WebHost\BrockAllen.MembershipReboot.WebHost.nuspec -OutputDirectory .

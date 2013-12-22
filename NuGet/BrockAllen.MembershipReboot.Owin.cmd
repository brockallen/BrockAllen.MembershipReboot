mkdir BrockAllen.MembershipReboot.Owin\lib\net45
xcopy ..\build\BrockAllen.MembershipReboot.Owin.dll BrockAllen.MembershipReboot.Owin\lib\net45 /y
xcopy ..\build\BrockAllen.MembershipReboot.Owin.pdb BrockAllen.MembershipReboot.Owin\lib\net45 /y
NuGet.exe pack BrockAllen.MembershipReboot.Owin\BrockAllen.MembershipReboot.Owin.nuspec -OutputDirectory .

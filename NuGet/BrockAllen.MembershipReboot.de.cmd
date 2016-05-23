mkdir BrockAllen.MembershipReboot.de\lib\net45\de
xcopy ..\build\de\BrockAllen.MembershipReboot.resources.dll BrockAllen.MembershipReboot.de\lib\net45\de /y
NuGet.exe pack BrockAllen.MembershipReboot.de\BrockAllen.MembershipReboot.nuspec -OutputDirectory .

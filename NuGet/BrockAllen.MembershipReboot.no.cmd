mkdir BrockAllen.MembershipReboot.no\lib\net45\no
xcopy ..\build\no\BrockAllen.MembershipReboot.resources.dll BrockAllen.MembershipReboot.no\lib\net45\no /y
NuGet.exe pack BrockAllen.MembershipReboot.no\BrockAllen.MembershipReboot.nuspec -OutputDirectory .

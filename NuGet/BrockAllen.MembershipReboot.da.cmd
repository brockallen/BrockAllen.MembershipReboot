mkdir BrockAllen.MembershipReboot.da\lib\net45\da
xcopy ..\build\da\BrockAllen.MembershipReboot.resources.dll BrockAllen.MembershipReboot.da\lib\net45\da /y
NuGet.exe pack BrockAllen.MembershipReboot.da\BrockAllen.MembershipReboot.nuspec -OutputDirectory .

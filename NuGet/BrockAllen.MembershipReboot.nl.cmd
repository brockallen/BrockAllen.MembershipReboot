mkdir BrockAllen.MembershipReboot.nl\lib\net45\nl
xcopy ..\build\nl-nl\BrockAllen.MembershipReboot.resources.dll BrockAllen.MembershipReboot.nl\lib\net45\nl /y
NuGet.exe pack BrockAllen.MembershipReboot.nl\BrockAllen.MembershipReboot.nuspec -OutputDirectory .

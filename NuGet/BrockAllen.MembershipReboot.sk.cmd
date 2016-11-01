mkdir BrockAllen.MembershipReboot.sk\lib\net45\sk
xcopy ..\build\sk\BrockAllen.MembershipReboot.resources.dll BrockAllen.MembershipReboot.sk\lib\net45\sk /y
NuGet.exe pack BrockAllen.MembershipReboot.sk\BrockAllen.MembershipReboot.nuspec -OutputDirectory .

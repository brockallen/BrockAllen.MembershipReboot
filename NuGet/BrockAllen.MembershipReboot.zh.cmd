mkdir BrockAllen.MembershipReboot.zh\lib\net45\zh
xcopy ..\build\zh-cn\BrockAllen.MembershipReboot.resources.dll BrockAllen.MembershipReboot.zh\lib\net45\zh /y
NuGet.exe pack BrockAllen.MembershipReboot.zh\BrockAllen.MembershipReboot.nuspec -OutputDirectory .

mkdir BrockAllen.MembershipReboot.ru\lib\net45\ru
xcopy ..\build\ru\BrockAllen.MembershipReboot.resources.dll BrockAllen.MembershipReboot.ru\lib\net45\ru /y
NuGet.exe pack BrockAllen.MembershipReboot.ru\BrockAllen.MembershipReboot.nuspec -OutputDirectory .

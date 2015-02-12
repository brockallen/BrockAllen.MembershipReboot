mkdir BrockAllen.MembershipReboot.sv\lib\net45\sv
xcopy ..\build\sv\BrockAllen.MembershipReboot.resources.dll BrockAllen.MembershipReboot.sv\lib\net45\sv /y
NuGet.exe pack BrockAllen.MembershipReboot.sv\BrockAllen.MembershipReboot.nuspec -OutputDirectory .

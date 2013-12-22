mkdir BrockAllen.MembershipReboot.Ef\lib\net45
xcopy ..\build\BrockAllen.MembershipReboot.Ef.dll BrockAllen.MembershipReboot.Ef\lib\net45 /y
xcopy ..\build\BrockAllen.MembershipReboot.Ef.pdb BrockAllen.MembershipReboot.Ef\lib\net45 /y
NuGet.exe pack BrockAllen.MembershipReboot.Ef\BrockAllen.MembershipReboot.Ef.nuspec -OutputDirectory .

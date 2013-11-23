xcopy ..\..\build\BrockAllen.MembershipReboot.Ef.dll lib\net45 /y
xcopy ..\..\build\BrockAllen.MembershipReboot.Ef.pdb lib\net45 /y
NuGet.exe pack BrockAllen.MembershipReboot.Ef.nuspec -OutputDirectory ..\

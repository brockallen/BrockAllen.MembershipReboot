xcopy ..\..\src\BrockAllen.MembershipReboot.Ef\bin\release\BrockAllen.MembershipReboot.Ef.dll lib\net45 /y
xcopy ..\..\src\BrockAllen.MembershipReboot.Ef\bin\release\BrockAllen.MembershipReboot.Ef.pdb lib\net45 /y
NuGet.exe pack BrockAllen.MembershipReboot.Ef.nuspec -OutputDirectory ..\

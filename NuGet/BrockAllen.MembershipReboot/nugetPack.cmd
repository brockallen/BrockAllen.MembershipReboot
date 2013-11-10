xcopy ..\..\src\BrockAllen.MembershipReboot\bin\release\BrockAllen.MembershipReboot.dll lib\net45 /y
xcopy ..\..\src\BrockAllen.MembershipReboot\bin\release\BrockAllen.MembershipReboot.pdb lib\net45 /y
NuGet.exe pack BrockAllen.MembershipReboot.nuspec -OutputDirectory ..\

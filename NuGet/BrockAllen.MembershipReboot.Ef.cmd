mkdir BrockAllen.MembershipReboot.Ef\lib\net45
xcopy ..\build\BrockAllen.MembershipReboot.Ef.dll BrockAllen.MembershipReboot.Ef\lib\net45 /y
xcopy ..\build\BrockAllen.MembershipReboot.Ef.pdb BrockAllen.MembershipReboot.Ef\lib\net45 /y

xcopy ..\src\BrockAllen.MembershipReboot.Ef\Sql\v6_to_v7_migration.sql BrockAllen.MembershipReboot.Ef\sql /y
xcopy ..\src\BrockAllen.MembershipReboot.Ef\Sql\v7_schema.sql BrockAllen.MembershipReboot.Ef\sql /y

NuGet.exe pack BrockAllen.MembershipReboot.Ef\BrockAllen.MembershipReboot.Ef.nuspec -OutputDirectory .

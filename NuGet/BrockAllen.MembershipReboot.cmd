mkdir BrockAllen.MembershipReboot\lib\net45
xcopy ..\build\BrockAllen.MembershipReboot.dll BrockAllen.MembershipReboot\lib\net45 /y
xcopy ..\build\BrockAllen.MembershipReboot.pdb BrockAllen.MembershipReboot\lib\net45 /y

::DA
mkdir BrockAllen.MembershipReboot\lib\net45\da
xcopy ..\build\da\BrockAllen.MembershipReboot.resources.dll BrockAllen.MembershipReboot\lib\net45\da /y

::DE
mkdir BrockAllen.MembershipReboot\lib\net45\de
xcopy ..\build\de\BrockAllen.MembershipReboot.resources.dll BrockAllen.MembershipReboot\lib\net45\de /y

::FI
mkdir BrockAllen.MembershipReboot\lib\net45\fi
xcopy ..\build\fi\BrockAllen.MembershipReboot.resources.dll BrockAllen.MembershipReboot\lib\net45\fi /y

::FR
mkdir BrockAllen.MembershipReboot\lib\net45\fr
xcopy ..\build\fr\BrockAllen.MembershipReboot.resources.dll BrockAllen.MembershipReboot\lib\net45\fr /y

::NL
mkdir BrockAllen.MembershipReboot\lib\net45\nl-nl
xcopy ..\build\nl-nl\BrockAllen.MembershipReboot.resources.dll BrockAllen.MembershipReboot\lib\net45\nl-nl /y

::NO
mkdir BrockAllen.MembershipReboot\lib\net45\no
xcopy ..\build\no\BrockAllen.MembershipReboot.resources.dll BrockAllen.MembershipReboot\lib\net45\no /y

::PT-BR
mkdir BrockAllen.MembershipReboot\lib\net45\pt-br
xcopy ..\build\pt-br\BrockAllen.MembershipReboot.resources.dll BrockAllen.MembershipReboot\lib\net45\pt-br /y

::RU
mkdir BrockAllen.MembershipReboot\lib\net45\ru-ru
xcopy ..\build\ru-ru\BrockAllen.MembershipReboot.resources.dll BrockAllen.MembershipReboot\lib\net45\ru-ru /y

::SV
mkdir BrockAllen.MembershipReboot\lib\net45\sv
xcopy ..\build\sv\BrockAllen.MembershipReboot.resources.dll BrockAllen.MembershipReboot\lib\net45\sv /y

::ZH
mkdir BrockAllen.MembershipReboot\lib\net45\zh-cn
xcopy ..\build\zh-cn\BrockAllen.MembershipReboot.resources.dll BrockAllen.MembershipReboot\lib\net45\zh-cn /y

NuGet.exe pack BrockAllen.MembershipReboot\BrockAllen.MembershipReboot.nuspec -OutputDirectory .

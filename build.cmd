
cd %~dp0

SETLOCAL
SET NUGET_VERSION=latest
SET CACHED_NUGET=%LocalAppData%\NuGet\nuget.%NUGET_VERSION%.exe
SET BUILDCMD_KOREBUILD_VERSION=""
SET BUILDCMD_DNX_VERSION="1.0.0-beta7"

IF EXIST %CACHED_NUGET% goto copynuget
echo Downloading latest version of NuGet.exe...
IF NOT EXIST %LocalAppData%\NuGet md %LocalAppData%\NuGet
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://dist.nuget.org/win-x86-commandline/%NUGET_VERSION%/nuget.exe' -OutFile '%CACHED_NUGET%'"


:copynuget
echo Copy nuget
IF EXIST .nuget\nuget.exe goto restore
md .nuget
copy %CACHED_NUGET% .nuget\nuget.exe > nul


:restore
echo Restore....
IF EXIST packages\KoreBuild goto run
.nuget\NuGet.exe install KoreBuild -ExcludeVersion -o packages -nocache -pre
.nuget\NuGet.exe install Sake -version 0.2 -o packages -ExcludeVersion


:dnvm
echo DNVM....
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "&{$Branch='dev';iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}"
echo DNVM 1....
dnvm install 1.0.0-beta7
echo DNVM 2....
dnvm use 1.0.0-beta7
echo DNVM 3....
dnvm list
echo DNVM 4....

:run
echo Getting ready to run....
CALL packages\KoreBuild\build\dnvm use default -runtime CLR -arch x86
packages\Sake\tools\Sake.exe -I packages\KoreBuild\build -f makefile.shade %*


:test
cd test\SaanSoft.AspNet.Identity3.MongoDB.Tests
dnx test

echo END CMD
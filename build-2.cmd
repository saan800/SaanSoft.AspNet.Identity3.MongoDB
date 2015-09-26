cd %~dp0

:run
echo Getting ready to run....
CALL packages\KoreBuild\build\dnvm use default -runtime CLR -arch x86
packages\Sake\tools\Sake.exe -I packages\KoreBuild\build -f makefile.shade %*


:test
cd test\SaanSoft.AspNet.Identity3.MongoDB.Tests
dnx test

echo END CMD
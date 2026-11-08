@echo off
title Compilador Lumi
echo ==============================================
echo         Compilador do App Lumi (Windows)
echo ==============================================
echo.
echo Compilando codigo-fonte em C# para Windows...

set "CSC_PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC_PATH%" (
    set "CSC_PATH=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)

if not exist "%CSC_PATH%" (
    echo [ERRO] Compilador csc.exe nao encontrado no seu Windows.
    echo Certifique-se de que o .NET Framework 4.0 ou superior esteja instalado.
    pause
    exit /b 1
)

"%CSC_PATH%" /target:winexe /out:Lumi.exe /win32icon:icone.ico /codepage:65001 /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationFramework.dll","C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationCore.dll","C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\WindowsBase.dll","System.Xaml.dll","System.Web.Extensions.dll","System.Windows.Forms.dll","System.Drawing.dll" LumiApp.cs SplashWindow.cs

if %errorlevel% equ 0 (
    echo.
    echo ==============================================
    echo  [SUCESSO] Lumi.exe gerado com sucesso!
    echo ==============================================
    echo.
    echo Voce ja pode clicar duas vezes em Lumi.exe para iniciar!
) else (
    echo.
    echo [ERRO] Ocorreu uma falha durante a compilacao.
)

echo.
pause

@echo off

if [%1]==[] goto exec
echo Waiting for the bot to shut down ...
wait 5

:exec
del Nami.zip
del NamiResources.zip
echo Downloading ...
Powershell.exe -executionpolicy remotesigned -File dl.ps1
echo Extracting ...
"C:\Program Files\7-Zip\7z.exe" x Nami.zip -aoa
"C:\Program Files\7-Zip\7z.exe" x NamiResources.zip -aoa "-oResources"
echo Starting the bot ...
START dotnet TheGodfaher.dll

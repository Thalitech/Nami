@echo off
title Auto Git pusher
for /F "tokens=2" %%i in ('date /t') do set mydate=%%i
set mytime=%time%

git add *
git commit --all -m "Auto Push %mydate% %mytime%"
git push
echo closeing in 10s
sleep 10000
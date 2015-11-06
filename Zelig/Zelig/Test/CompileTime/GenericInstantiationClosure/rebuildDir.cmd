@echo off
setlocal

set DIR=%1

cd %DIR%
sd edit *
csc /out:%DIR%.exe /debug test.cs
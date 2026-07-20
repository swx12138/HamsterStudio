@echo off

@REM config
@REM cmake -B build -G "Visual Studio 17 2022" -A x64

@REM build
cmake --build build --config Release
@REM cmake --build build --config Debug

@REM install
cmake --install build -v --prefix "D:\Develop\Tools"
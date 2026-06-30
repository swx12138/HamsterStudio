@echo off

@REM config
cmake -B build -G "Visual Studio 17 2022" -A x64

@REM build
@REM cmake --build build --config Release
@REM cmake --build build --config Debug

@REM install
@REM cmake --install build -v
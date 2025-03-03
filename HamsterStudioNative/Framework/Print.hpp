#pragma once

#include <ostream>

/// <summary>
/// print to where
/// </summary>
namespace HamsterStudio::IO
{
    void Print(bool b)
    {
        printf_s(b ? "true" : "false");
        putchar('\n');
    }

    void Print(std::string const &str)
    {
        printf_s("%s\n", str.c_str());
    }

    void Print(int *arr, int len)
    {
        putchar('[');
        for (int i = 0; i < len; i++) {
            printf_s("%d, ", arr[i]);
        }
        printf_s("\b\b]\n");
    }

    void Print(int val)
    {
        printf_s("%d\n", val);
    }

    template <typename _Ty>
    void Print(_Ty const &)
    {

    }
}

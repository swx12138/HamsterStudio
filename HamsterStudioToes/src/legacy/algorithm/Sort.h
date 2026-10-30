#pragma once

namespace HamsterStudioToes
{
    template <typename _Ty>
    void Swap(_Ty &a, _Ty &b)
    {
        a = a ^ b;
        b = a ^ b;
        a = a ^ b;
    }
}

namespace HamsterStudioToes::Sort
{
    bool IsIncreasing(int* arr, int len);

    /**
     * 冒泡排序
     */
    void Buble(int* arr, int len);

}

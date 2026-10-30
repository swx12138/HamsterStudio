#pragma once

#include "Sort.h"

#include <cstdint>

namespace HamsterStudioToes::Search
{
    using Postion = size_t;
    const Postion NPos = UINT64_MAX;

    /// <summary>
    /// 直接查找
    /// </summary>
    class LinearSearch
    {
    public:
        Postion operator()(int target, int *arr, int len)
        {
            return this->operator()(target, arr, len, [](auto target, auto value)
                                    { return target == value; });
        }

        template <typename _Pred>
        Postion operator()(int target, int *arr, int len, _Pred pred)
        {
            for (int i = 0; i < len; i++)
            {
                if (pred(target, arr[i]))
                {
                    return i;
                }
            }
            return NPos;
        }

    };

    /// <summary>
    /// 二分查找
    /// </summary>
    class BinarySearch
    {
    public:
        Postion operator()(int target, int *arr, int len)
        {
            return this->operator()(target, arr, len, [](auto target, auto value)
                                    { return target - value; });
        }

        template <typename _Pred>
        Postion operator()(int target, int *arr, int len, _Pred pred)
        {
            if (!Sort::IsIncreasing(arr, len))
                return NPos;
            int start = 0, end = len - 1;
            while (start <= end)
            {
                int const mid = start + (end - start) / 2;
                int const pr = pred(target, arr[mid]);
                if (pr == 0)
                {
                    return mid;
                }
                else if (pr < 0)
                {
                    end = mid - 1;
                }
                else
                {
                    start = mid + 1;
                }
            }
            return NPos;
        }
    
    };

}
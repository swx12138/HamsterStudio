#pragma once

#include <cstdint>

namespace Algorithm::Search
{
    class ISearch
    {
    public:
        using Postion = size_t;
        const Postion NPos = UINT64_MAX;

    public:
        virtual Postion operator()(int target, int *arr, int len) = 0;
    };

    /// <summary>
    /// 直接查找
    /// </summary>
    class Linear : public ISearch
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
    class Binary : public ISearch
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
            while (start < end)
            {
                int mid = (end + start) / 2;
                int pr = pred(target, arr[mid]);
                if (pr == 0)
                {
                    return mid;
                }
                else if (pr < 0)
                {
                    end = mid;
                }
                else
                {
                    start = mid;
                }
            }
            return NPos;
        }
    };
}
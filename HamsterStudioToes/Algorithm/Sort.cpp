#include "Sort.h"

bool HamsterStudioToes::Sort::IsIncreasing(int* arr, int len)
{
    if (len < 2)
        return true;
    for (int i = 1; i < len; i++)
    {
        if (arr[i] < arr[i - 1])
            return false;
    }
    return true;
}

void HamsterStudioToes::Sort::Buble(int* arr, int len)
{
    bool swapped = false;
    for (int i = 0; i < len; i++)
    {
        swapped = false;
        for (int j = len - 1; j > i; j--)
        {
            if (arr[j - 1] > arr[j])
            {
                Swap(arr[j - 1], arr[j]);
                swapped = true;
            }
        }
        if (!swapped)
            break;
    }
}

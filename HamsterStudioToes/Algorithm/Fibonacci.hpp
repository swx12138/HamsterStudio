
#include <algorithm>
#include <string>
#include <vector>

template <typename _Ty>
class FibonacciPrototype
{
public:
    FibonacciPrototype(int n)
    {
        _Ty _Last_Last = 0;
        _Ty _Last = 1;
        int k = 1;
        while (++k <= n)
        {
            _Cur = _Last + _Last_Last;
            _Last_Last = _Last;
            _Last = _Cur;
        }
    }

    _Ty get() const
    {
        return _Cur;
    }

private:
    _Ty _Cur = 0;
};

template <typename _Ty>
class Fibonacci
{
    using DataArray = vector<_Ty>;
    using Postion = uint32_t;

public:
    Fibonacci(int cap = 100)
    {
        _Ans = DataArray(cap + 1);
        _Ans[0] = 0;
        _Ans[1] = 1;
        _Pos = 2;
    }

    auto operator[](Postion const index)
    {
        while (_Pos <= index)
        {
            _Ans[_Pos] = _Ans[_Pos - 1] + _Ans[_Pos - 2];
            _Pos++;
        }
        return _Ans[index];
    }

    bool compare(_Ty const &val, uint32_t index)
    {
        return _Ans[index] == val;
    }

private:
    DataArray _Ans;
    Postion _Pos;
};

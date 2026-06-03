#pragma once 

#include <string>
#include <functional>
#include <numeric>

namespace HamsterStudioToes::Math
{
    template <typename _Ty>
    _Ty Sum(std::vector<_Ty> const& arr)
    {
        _Ty ans{};
        for (auto const& e : arr)
        {
            ans += e;
        }
        return ans;
    }

    class Common {
    public:
        /// <summary>
        /// 辗转相除法求最大公因子
        /// </summary>
        static int gcd(int a, int b);
    };

    /// <summary>
    /// 转成指定进制
    /// </summary>
    class ScaleConversion
    {
    public:
        ScaleConversion(int scale);
        ~ScaleConversion();

        std::string operator()(int v) const;

    private:
        int m_scale;
    };

    /// <summary>
    /// 积分求解
    /// </summary>
    class IntegralResolver {
    public:
        IntegralResolver(uint8_t level = 3);

    public:
        double s(std::function<double(double)>&& func, double start, double end) const;

    private:
        uint8_t _level;
    };

    template<typename _Ty>
    inline double Normalize(std::vector<_Ty> const& rng, double maxv = 0)
    {
        if (abs(maxv - 0) <= DBL_EPSILON)
        {
            if constexpr (sizeof(_Ty) == 1) {
                maxv = 0xff;
            }
            else if constexpr (sizeof(_Ty) == 2) {
                maxv = 0xffff;
            }
            else if constexpr (sizeof(_Ty) == 4) {
                maxv = 0xffffffff;
            }
            else if constexpr (sizeof(_Ty) == 8) {
                maxv = 0xffffffffffffffff;
            }
        }
        auto sum = std::reduce(rng.begin(), rng.end(), 0.0, [](double val, uint8_t dat) { return val + dat / 255.0;});
        return  sum / rng.size();
    }

}
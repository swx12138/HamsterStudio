#pragma once 

#include <string>
#include <functional>

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

}
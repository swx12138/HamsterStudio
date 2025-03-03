#pragma once 

#include <string>
#include <functional>
#include <stack>
#include <exception>
#include <stdexcept>

namespace HamsterStudio
{
    class Common {
    public:
        /// <summary>
        /// 辗转相除法求最大公因子
        /// </summary>
        static int gcd(int a, int b)
        {
            int g = b;
            while (b > 0) {
                g = b;
                b = a % b;
                a = g;
            }
            return g;
        }
    };

    /// <summary>
    /// 转成指定进制
    /// </summary>
    class ScaleConversion
    {
    public:
        ScaleConversion(int scale) : m_scale(scale) {}
        ~ScaleConversion() {}

        virtual std::string operator()(int v) const
        {
            std::stack<int> sta;
            int ii = v;
            while (ii > 0) {
                sta.push(ii % m_scale);
                ii = ii / m_scale;
            }

            std::string str = "(";
            while (!sta.empty()) {
                // printf_s("%d", sta.Top());
                str = str + std::to_string(sta.top()) + " ";
            }
            str = str + "\b)" + std::to_string(m_scale);
            return str;
        }

    private:
        int m_scale;
    };

    /// <summary>
    /// 积分求解
    /// </summary>
    class IntegralResolver {
    public:
        IntegralResolver(uint8_t level = 3) {
            _level = level;
        }

    public:
        double s(std::function<double(double)> &&func, double start, double end) const {

            if (end <= start) {
                throw new std::runtime_error("argument check failed.");
            }

            double step = 1 / pow(10, _level);
            int nStep = (end - start) / step;

            double sum = 0.0;
            double offest = 0.0;
            for (int i = 0; i < nStep; i++) {
                offest += step;
                sum += func(start + offest) * step;
            }

            return round(sum);
        }

    private:
        uint8_t _level;
    };

}
#include "Math.h"

#include <stack>
#include <stdexcept>

int toes::Math::Common::gcd(int a, int b)
{
    int g = b;
    while (b > 0) {
        g = b;
        b = a % b;
        a = g;
    }
    return g;
}

toes::Math::ScaleConversion::ScaleConversion(int scale)
    : m_scale(scale)
{
}

toes::Math::ScaleConversion::~ScaleConversion()
{
}

std::string toes::Math::ScaleConversion::operator()(int v) const
{
    // m_scale == 0 会在取模时除零；m_scale == 1 会让下面的缩放循环永不收敛
    if (m_scale < 2) {
        return std::string();
    }

    std::stack<int> sta;
    int ii = v;
    while (ii > 0) {
        sta.push(ii % m_scale);
        ii = ii / m_scale;
    }

    std::string str = "(";
    while (!sta.empty()) {
        // 原来这里漏了 pop，栈永远非空，循环永远退不出去
        str = str + std::to_string(sta.top()) + " ";
        sta.pop();
    }
    str = str + "\b)" + std::to_string(m_scale);
    return str;
}

toes::Math::IntegralResolver::IntegralResolver(uint8_t level)
{
    _level = level;
}

double toes::Math::IntegralResolver::s(std::function<double(double)>&& func, double start, double end) const
{

    if (end <= start) {
        throw new std::runtime_error("argument check failed.");
    }

    double step = 1 / pow(10, _level);
    int nStep = static_cast<int>((end - start) / step);

    double sum = 0.0;
    double offest = 0.0;
    for (int i = 0; i < nStep; i++) {
        offest += step;
        sum += func(start + offest) * step;
    }

    return round(sum);
}

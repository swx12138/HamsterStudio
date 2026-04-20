#include "Math.h"

#include <stack>
#include <stdexcept>

int HamsterStudioToes::Math::Common::gcd(int a, int b)
{
    int g = b;
    while (b > 0) {
        g = b;
        b = a % b;
        a = g;
    }
    return g;
}

HamsterStudioToes::Math::ScaleConversion::ScaleConversion(int scale)
    : m_scale(scale)
{
}

HamsterStudioToes::Math::ScaleConversion::~ScaleConversion()
{
}

std::string HamsterStudioToes::Math::ScaleConversion::operator()(int v) const
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

HamsterStudioToes::Math::IntegralResolver::IntegralResolver(uint8_t level)
{
    _level = level;
}

double HamsterStudioToes::Math::IntegralResolver::s(std::function<double(double)>&& func, double start, double end) const
{

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

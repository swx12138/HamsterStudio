#pragma once

#include <ctime>

namespace HamsterStudioToes
{
    // 注意：本类目前没有任何地方使用。check() 用的是 CPU 时间（clock()）而不是挂钟
    // 时间，线程阻塞时不会计时；原有析构函数还会直接 exit(114514)，等于任何作用域
    // 退出都会杀掉宿主进程——该行为已删除。真正启用前需要重新设计。
    class WatchDog
    {
    public:
        explicit WatchDog(clock_t const to)
            : ct(0), timeout(to)
        {
            feed();
        }

        virtual ~WatchDog() = default;

        virtual void feed()
        {
            ct = clock();
        }

        virtual bool check() const
        {
            return (timeout > (clock() - ct));
        }

    private:
        clock_t ct;
        clock_t timeout;
    };

}

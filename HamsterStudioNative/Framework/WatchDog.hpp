#pragma once

#include <chrono>

class WatchDog
{
public:
    WatchDog(const clock_t to)
        : timeout(to)
    {
        feed();
    }
    ~WatchDog()
    {
        NotFeed();
    }
    virtual void feed()
    {
        ct = clock();
    }
    virtual bool check() const
    {
        return (timeout > (clock() - ct));
    }
    virtual void NotFeed() const
    {
        exit(114514);
    }

private:
    clock_t ct;
    clock_t timeout;
};
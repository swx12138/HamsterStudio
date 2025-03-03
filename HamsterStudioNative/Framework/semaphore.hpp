
#include <condition_variable>
#include <mutex>

class Semaphore
{
public:
    Semaphore(size_t nMaxThread)
        : nThreadMax(nMaxThread) {}

    void alloc()
    {
        std::unique_lock<std::mutex> ul(mtx);
        if (--nThreadMax < 0)
            cv.wait(ul);
    }

    void free()
    {
        std::unique_lock<std::mutex> ul(mtx);
        if (++nThreadMax <= 0)
            cv.notify_one();
    }

private:
    int nThreadMax;
    std::mutex mtx;
    std::condition_variable cv;
};
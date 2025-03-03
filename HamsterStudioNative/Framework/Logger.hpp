#pragma once

#include <functional>
#include <fstream>
#include <ostream>
#include <sstream>
#include <chrono>

namespace Core
{
    enum class LogLevel {
        INFO,
        WARN,
        ERROR,
        FATAL
    };

    std::ostream &operator<<(std::ostream &os, const LogLevel level) {
        switch (level) {
        case LogLevel::INFO:
            os << "[INFO]";
            break;
        case LogLevel::WARN:
            os << "[WARN]";
            break;
        case LogLevel::ERROR:
            os << "[ERROR]";
            break;
        case LogLevel::FATAL:
            os << "[FATAL]";
            break;
        }
        return os;
    }

    class Logger {
    public:
        void setLogFile(const std::string &filename) {
            m_logFile.open(filename, std::ios::out | std::ios::app);
            if (!m_logFile.is_open()) {
                throw std::runtime_error("Failed to open log file.");
            }
        }

        void registerCallback(std::function<void(const std::string &)> callback) {
            m_callbacks.push_back(callback);
        }

        void unregisterCallback(std::function<void(const std::string &)> callback) {
            m_callbacks.erase(std::remove(m_callbacks.begin(), m_callbacks.end(), callback), m_callbacks.end());
        }

        void log(LogLevel level, const std::string_view &msg) {
            auto now = std::chrono::system_clock::now();
            auto in_time_t = std::chrono::system_clock::to_time_t(now);
            std::stringstream ss;
            ss 
                //<< std::put_time(std::localtime(&in_time_t), "%Y-%m-%d %H:%M:%S")
                << " " << level << " "
                << msg << std::endl;

            m_logFile << ss.str(); // 输出到文件

            // 转发给所有已注册的回调
            for (const auto &callback : m_callbacks) {
                callback(ss.str());
            }
        }

        void info(const std::string_view &msg) {
            log(LogLevel::INFO, msg);
        }

        void warn(const std::string_view &msg) {
            log(LogLevel::WARN, msg);
        }

        void error(const std::string_view &msg) {
            log(LogLevel::ERROR, msg);
        }

        void fatal(const std::string_view &msg) {
            log(LogLevel::FATAL, msg);
            std::abort(); // 终止程序执行
        }

    private:
        std::ofstream m_logFile;
        std::vector<std::function<void(const std::string &)>> m_callbacks;
    };

}

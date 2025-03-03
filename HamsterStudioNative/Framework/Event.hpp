#pragma once

#include <mutex>
#include <functional>
#include <unordered_map>

namespace Core
{
    template <typename T>
    class PubSubEvent
    {
    public:
        using EventHandler = std::function<void(const T &)>;

        // �����¼�
        void Subscribe(const std::string &eventName, EventHandler handler)
        {
            std::lock_guard<std::mutex> lock(m_mutex);
            m_eventHandlers[eventName].push_back(std::move(handler));
        }

        // ȡ�������¼�
        void Unsubscribe(const std::string &eventName, EventHandler handler)
        {
            std::lock_guard<std::mutex> lock(m_mutex);
            auto it = m_eventHandlers.find(eventName);
            if (it != m_eventHandlers.end())
            {
                it->second.remove_if([=](const EventHandler &h)
                                     { return h.target_type() == handler.target_type(); });
            }
        }

        // �����¼�
        void Publish(const std::string &eventName, const T &eventData)
        {
            std::lock_guard<std::mutex> lock(m_mutex);
            auto it = m_eventHandlers.find(eventName);
            if (it != m_eventHandlers.end())
            {
                for (const auto &handler : it->second)
                {
                    handler(eventData);
                }
            }
        }

    private:
        std::unordered_map<std::string, std::vector<EventHandler>> m_eventHandlers;
        std::mutex m_mutex;
    };

    // �¼��ۺ�����
    template <typename T>
    class EventAggregator
    {
    public:
        // �����¼�
        void Subscribe(const std::string &eventName, EventHandler<T> handler)
        {
            std::lock_guard<std::mutex> lock(m_mutex);
            m_eventHandlers[eventName].insert(std::move(handler));
        }

        // ȡ�������¼�
        void Unsubscribe(const std::string &eventName, EventHandler<T> handler)
        {
            std::lock_guard<std::mutex> lock(m_mutex);
            auto it = m_eventHandlers.find(eventName);
            if (it != m_eventHandlers.end())
            {
                it->second.erase(it->second.find(handler));
            }
        }

        // �����¼�
        void Publish(const std::string &eventName, const T &eventData)
        {
            std::lock_guard<std::mutex> lock(m_mutex);
            auto it = m_eventHandlers.find(eventName);
            if (it != m_eventHandlers.end())
            {
                for (const auto &handler : it->second)
                {
                    handler(eventData);
                }
            }
        }

    private:
        std::unordered_multimap<std::string, EventHandler<T>> m_eventHandlers;
        std::mutex m_mutex;
    };

    // �¼��ۺ�����
    // class EventAggregator
    // {

    // private:
    //     std::unordered_map < int, std::vector
    // };
}
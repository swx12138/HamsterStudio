
#pragma once

#include "Drawing.hh"

namespace Control
{
    class Control
    {
    };

    class ButtonClickArg
    {
    };

    using Button_OnClick = void (*)(Control *, ButtonClickArg *);

    class Button : public Control
    {
    public:
        Button(Drawing::Rectangle rc, Button_OnClick listener) : m_Area(rc), m_Listener(listener) {}
        ~Button() {}

        /// @brief click test
        /// @return true - 结束 false - 往下传
        virtual bool HitTest(Drawing::Point const &pt) {
            if (m_Area.InRegion(pt)) {
                m_Listener(this, nullptr);
                return true;
            }
            return false;
        }

    private:
        Drawing::Rectangle m_Area;
        Button_OnClick m_Listener;
    };
}


#pragma once

namespace Drawing
{
    class Drawing
    {
    };

    class Point : virtual public Drawing
    {
    public:
        int x, y;
    };

    class Shape : virtual public Drawing
    {
    public:
        Shape() {}
        ~Shape() {}

        virtual bool InRegion(Point const &pt) const = 0;
    };

    class Rectangle : virtual public Shape
    {
    public:
        Rectangle(int x, int y, int w, int h) : x(x), y(y), w(w), h(h) {}
        ~Rectangle() {}

        virtual bool InRegion(Point const &pt) const {
            return ((pt.x >= x && pt.x <= x + w) && (pt.y <= y && pt.y >= y + h));
        }

    private:
        struct
        {
            int x, y, w, h;
        };
    };
}

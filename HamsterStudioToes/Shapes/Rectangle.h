#pragma once

#include "./ShapeIf.h"

namespace ShapeNamespace
{
	class Rectangle {
	public:
		long left, top, width, height;

	};

	// static_assert(IsShape<Rectangle>, "Rectangle is not a shape");
}
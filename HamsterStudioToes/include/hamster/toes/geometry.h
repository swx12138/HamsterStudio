#pragma once

#include <concepts>

#define ShapeNamespace HamsterStudioToes::Shapes

namespace ShapeNamespace
{
	template<typename _Ty>
	concept IsShape = true;

	class Rectangle {
	public:
		long left, top, width, height;

	};
}
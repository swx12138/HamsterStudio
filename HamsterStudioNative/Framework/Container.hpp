#pragma once

#include <unordered_map>
#include <type_traits>

namespace Core
{
	class Container {
	public:
		Container() = default;
		Container(Container const &) = delete;
		Container(Container &&) = delete;
		~Container() = default;
	public:

		template<typename _Interface, typename _Implantation>
		bool RegisterSingleton(_Implantation *impl = nullptr) {
			static_assert((!std::is_base_of_v<_Interface, _Implantation>()), "Can't create instance.");
			_Interface *interf = impl;

			int hashcode = _GetTypeHashCode<_Interface>();
			if (_Map.find(hashcode) != _Map.cend()) {
				return false;
			}
			if (interf == nullptr) {
				interf = new _Implantation();
			}
			_Map[hashcode] = interf;
		}

	private:
		template<typename _Ty>
		int _GetTypeHashCode() {
			return typeid(_Ty);
		}

	private:
		std::unordered_map<int, void *> _Map;
	};
}
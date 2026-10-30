#pragma once

// 符号可见性由宏选择，三者互斥：
//   PROJECTDLL_EXPORTS  由 DLL 工程自己定义 → 导出
//   TOES_IMPORT         由消费 DLL 的工程定义 → 导入
//   两者皆无            静态库构建 → 不加修饰，按普通符号链接
//
// 消费方若链接的是 DLL 的导入库，应当定义 TOES_IMPORT；否则编译器无法为
// 类成员生成导入跳转，只能碰巧通过链接器 thunk 工作，属于未定义边界。
#if defined(PROJECTDLL_EXPORTS)
#define TOES_API __declspec(dllexport)
#elif defined(TOES_IMPORT)
#define TOES_API __declspec(dllimport)
#else
#define TOES_API
#endif

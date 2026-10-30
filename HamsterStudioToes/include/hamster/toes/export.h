#pragma once

// 符号可见性由宏选择，三者互斥：
//   PROJECTDLL_EXPORTS  由 DLL 工程自己定义 → 导出
//   TOES_IMPORT         由纯 C++ 消费方定义 → 导入
//   两者皆无            静态库构建、或 C++/CLI 消费方 → 不加修饰
//
// 注意：C++/CLI（/clr）工程**不要**定义 TOES_IMPORT。在 /clr 编译单元里 dllimport 会让
// 编译器按托管约定（__clrcall）解析这些符号，生成带 $$F 的修饰名，与导入库里的原生
// 符号对不上，链接报 LNK2028。C++/CLI 消费方不定义该宏即可正常链接。
#if defined(PROJECTDLL_EXPORTS)
#define TOES_API __declspec(dllexport)
#elif defined(TOES_IMPORT)
#define TOES_API __declspec(dllimport)
#else
#define TOES_API
#endif

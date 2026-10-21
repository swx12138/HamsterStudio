#pragma once

// 构建 DLL 时（工程定义了 PROJECTDLL_EXPORTS）导出符号；
// 其余情况（静态库构建 / 消费方）为空，保证静态库与其他配置不受影响。
#ifdef PROJECTDLL_EXPORTS
#define TOES_API __declspec(dllexport)
#else
#define TOES_API
#endif

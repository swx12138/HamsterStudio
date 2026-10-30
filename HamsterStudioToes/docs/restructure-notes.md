# HamsterStudioToes 结构重组：实施记录

> 状态：**P0–P4 已执行完成**。基线、验证证据与偏差均记录在此。
> 方案原文见 [`restructure-plan.md`](restructure-plan.md)。

---

## 1. 结果

| 项 | 重组前 | 重组后 |
|---|---|---|
| 顶层目录 | 11 个功能/来源混杂目录 | `docs / include / src / Tests / x64` |
| 公开头 | 分散在 4 个路径 | `include/hamster/toes/` 下 7 个 |
| 活跃源码 | 34 个（vcxproj 手写清单） | 35 个，按 `core/media/imaging/stitch/color/win32` 划分 |
| 不可达代码 | 与活跃代码混在 11 个目录 | `src/legacy/` 42 个文件，不参与任何构建 |
| 零引用文件 | 8 个 | 已删除 |
| 源清单维护点 | 3 处手写 | 0 处（两套构建均用通配） |
| 库内 `main` | 4 个 | 1 个（`md5_test_main`、`stitch_cli_main` 已删；其余 2 个随 legacy 归档） |
| Win32 平台配置 | 4 个（从来编不过） | 已删除 |

最终目录：

```
HamsterStudioToes/
├─ include/hamster/toes/   color.h export.h geometry.h gradient.h image.h layout.h stitch.h
├─ src/
│   ├─ core/        diagnostics numbers stopwatch stream strings codec/{base64 hash_defines md5}
│   ├─ media/       baiting image io.hpp
│   ├─ imaging/     adjustments gradient popular_colors screenshot
│   ├─ stitch/      layout paste stitch
│   ├─ color/       hsv
│   ├─ win32/       os pe shell_api
│   ├─ dllmain.cpp
│   └─ legacy/      algorithm hash image media nstd tools win32（42 个文件，不编译）
├─ tests（= 既有 Tests/，见 §3.2）
├─ docs/
├─ Directory.Build.props
└─ HamsterStudioToes.vcxproj（通配式）
```

`src/` 与 `include/` 是两条 include 搜索根：对外 `#include <hamster/toes/x.h>`，对内 `#include "core/x.h"`。

---

## 2. 验收证据

| 检查 | 命令 | 结果 |
|---|---|---|
| 导出表基线 | `dumpbin /exports`（重组前） | 34 项，存于 `docs/abi-baseline.txt` |
| 导出表（P1 后） | 同上 | 34 项，`DIFF=EMPTY` |
| 导出表（P2 后） | 同上 | 34 项，`DIFF=EMPTY` |
| 导出表（P3 后） | 同上 | 34 项，`DIFF=EMPTY` |
| DLL 构建 | `msbuild … /p:Configuration=Release /p:Platform=x64` | 退出码 0 |
| 测试 | `Tests\bin\Release\ToesTests.exe` | `54 checks, 0 failure(s)` |
| CLI 构建 | `cmake --build build-cmkb --config Release`（VS 开发者环境） | 退出码 0，产出 5 个 exe + 静态库 |
| Bridge 构建 | `msbuild HamsterStudioBridge.vcxproj` | 退出码 0，产出 `HammerStudioBridge64.dll` |

**关键结论**：整个结构重组对 ABI 零影响。`add()` 导出项在本轮保留（未删），导出表前后完全一致。

---

## 3. 与方案的偏差（均已验证可行）

### 3.1 文件计数：legacy 是 42 个，不是 43

方案 §4 的合计算式把 `Shapes/Rectangle.h` + `ShapeIf.h` 合并成 1 个 `geometry.h` 后，两边计数方式不一致。实际构成：
6 个旧公开头 → 6 个公开头；Shapes 2 个文件 → 1 个合并头（共 7 个公开头）；活跃 35；legacy 42；删除 8；测试 1。合计 94 ✔

### 3.2 测试目录未改名

Windows 文件系统大小写不敏感，`tests/` 与既有 `Tests/` 指向同一目录。新建小写目录会导致在无提示的情况下覆盖既有目录，因此测试文件保留在 `Tests/ToesTests.cpp`，测试工程 `Tests/HamsterStudioToes.Tests.vcxproj` 原地更新。

### 3.3 测试未拆成 `tests/legacy/`，也未改为链接 DLL

- **不拆文件**：拆分需要改动断言代码，违背"纯搬移"。改为：测试工程额外编译 4 个 legacy 源（`Math.cpp`、`Sort.cpp`、`Wave.cpp`、`string.cpp`），legacy 代码只进入测试可执行体，**不进入 DLL**。54 条断言全部保留并通过。
- **不链接 DLL**：`TOES_API` 目前只有 `dllexport`、没有 `dllimport` 分支，消费方拿不到类符号，强行链接会失败。该项依赖下一轮的导出面改造。

### 3.4 legacy 内部引用已失效

legacy 文件被扁平化到 `src/legacy/<域>/`，原先的 `./PixelData/PixelData.h` 之类相对引用不再成立（例如 `ImageEditor.h`）。这些文件不参与构建，仅作归档；若将来要复活，需先修内部引用。

---

## 4. 下游改动

| 项目 | 文件 | 改动 |
|---|---|---|
| CLITools | `CMakeLists.txt` | 显式 28 条源清单 → `file(GLOB_RECURSE … CONFIGURE_DEPENDS)`；包含根改为 `include` + `src`；顺带补回原先漏掉的 HSV 源文件 |
| CLITools | `src/bait/main.cpp`、`src/stitch/main.cpp`、`src/tojpg/tojpeg.cpp`、`src/tovid/main.cpp` | 4 处 include 指向新路径 |
| Bridge | `ImageModuleWrapper.cpp` | 4 处 include 指向 `include/hamster/toes/`（保持原 UTF-16 编码） |
| Bridge | `HamsterStudioBridge.vcxproj` | 补 `..\HamsterStudioToes\include` 与 `..\HamsterStudioToes\src` 两条包含根；补 `/utf-8` 消除 C4819 |
| Toes | `Directory.Build.props` | 新增：`OpenCVRoot`、`stdcpplatest`、`/utf-8`、`/MP`、OpenMP 集中定义 |
| Toes | `HamsterStudioToes.vcxproj` | 源清单改为通配；删除 4 个 Win32 平台配置；OpenCV 路径改用 `$(OpenCVRoot)` |

---

## 5. 剩余事项（未在本轮做）

1. **`add()` 导出**：仍在导出表里，等确认后删除（删除后需同步更新基线）。
2. **`TOES_API` 的 `dllimport` 分支**：已在第二轮补上（见 §6）。
3. **OpenCV 链接集中化**：仍有 4 个源文件用 `#pragma comment(lib, "opencv_world4120[.d]")`，未迁到工程级 `AdditionalDependencies`。
4. **死脚本**：`makefile`（引用不存在的 `source/*.cc`）、`release.py`（只打包 `.cc/.hh`）、`gcc.md`、`note.md` 仍在根目录。
5. **legacy 内部引用**：见 §3.4。
6. **CI**：仓库仍无流水线；`.github/` 为空目录。
7. **逻辑缺陷**：已在第二轮修复（见 §6）。

---

## 6. 第二轮：导入分支与 P0 修复（2026-10-30）

### 6.1 改动

| 文件 | 改动 |
|---|---|
| `include/hamster/toes/export.h` | 增加 `TOES_IMPORT` → `dllimport` 分支；导出 / 导入 / 静态库三态互斥 |
| `src/core/strings.cpp` | `Hex` 改为查表零填充；`Dehex` 校验长度与字符合法性；`Split` 修复无分隔符越界并改为子串语义；`Join` 修复空容器解引用；`Replace` 修复空 needle 死循环与替换文本自包含；`to_lower`、`isValidNum` 修正 ctype 实参；`UnitedSize` 边界改 `>=` |
| `src/core/codec/base64.cpp` | `Decode` 按 `'='` 裁剪填充字节，长度非法抛 `invalid_argument`；`from_base64` 修正 ctype 实参 |
| `Tests/ToesTests.cpp` | 新增 `TestStringUtils`、`TestBase64` 两组共 22 条断言 |
| `tools/check-abi.ps1` | 新增：导出表与基线比对，可用于提交前检查或 CI；`-Update` 写回基线 |

### 6.2 验证

| 检查 | 结果 |
|---|---|
| DLL 构建 | 退出码 0 |
| 导出表 | 34 项，与基线一致（导入分支不改变导出） |
| 测试 | **76 checks, 0 failure(s)**（第一轮为 54 项，新增 22 项） |
| ABI 脚本 | `tools\check-abi.ps1` → 基线 34 / 当前 34，一致 |

### 6.3 两点说明

- 这些工具函数在库内**没有任何调用者**（`StringUtils` 与 `Base64` 的函数全库零引用）。上述缺陷属于**潜在陷阱**而非线上故障；修复的价值在于把行为固定成可测试的契约，避免后来者踩坑。
- `TOES_IMPORT` 目前没有消费方启用；且 **C++/CLI 消费方不应启用**（原因见 §7.4）。纯 C++ 消费方定义它可显式走 `dllimport`。

---

## 7. 第三轮：OpenCV 集中化、死脚本清理、根命名空间统一（2026-10-30）

### 7.1 变更

| 项 | 内容 |
|---|---|
| OpenCV 链接 | 删除 3 个源文件里的 `#pragma comment(lib, …)`；`Directory.Build.props` 新增 `$(OpenCVLibName)`（Debug→`opencv_world4120d.lib`，其余→`opencv_world4120.lib`）；两个工程在 `AdditionalDependencies` 引用它 |
| 死脚本 | 删除 `makefile`、`release.py`、`gcc.md`、`note.md` |
| 根命名空间 | `HammerStudioToes::` 全部改为 `toes::`；9 个 `#define XxxNamespace` 宏删除，改用真实命名空间 |
| 目录改名 | `src/win32/` → `src/platform/`（原因见 §7.3） |

命名空间映射：`Image`/`ImageTools` → `toes::image`、`ImageProcessor` → `toes::imaging`、`PixelData` → `toes::colors`、`Shapes` → `toes::shapes`、`Win32Utils` → `toes::win32`、`Diagnostics` → `toes::diagnostics`、`StringUtils` → `toes::strings`、`Base64` → `toes::base64`、`Hash` → `toes::hash`、`Tools` → `toes::tools`、`PortableExecutable` 等 → `toes::pe`、`BaitingMaster`/`StopWatch` → `toes` 根。legacy 一并改为 `toes::*`。

### 7.2 这是破坏性 ABI 变更

导出符号的修饰名含命名空间，因此**导出表整体变化**。`docs/abi-baseline.txt` 已用 `tools/check-abi.ps1 -Update` 重新生成（仍为 34 项，符号前缀变为 `toes::`）。任何针对旧 DLL 编译的二进制都必须重编。

### 7.3 发现的 .gitignore 陷阱

根 `.gitignore` 第 26 行 `[Ww][Ii][Nn]32/` 本意忽略 VS 的 `Win32/` 输出目录，但它会匹配**任意层级**下名为 win32 的目录。此前新建的 `src/win32/` 因此被静默忽略，5 个文件从未进入仓库，两次提交里的 "clean" 是假象。改名为 `src/platform/` 后恢复正常跟踪。

### 7.4 Bridge 链接问题（已定位并修复）

现象：Bridge 的 Release 构建报 LNK2028，符号修饰名带 `$$F`（`__clrcall`，托管调用约定）。

排查过程：

- 怀疑 LTCG（`/GL` 与 `/clr` 不兼容）→ 关闭全程序优化后仍然失败，排除；
- 怀疑原生头被当作托管代码 → `#pragma managed(push, off)` 触发 C3821，排除；
- 对照时间线：15:48 那次 Release 构建成功时，Bridge 尚未定义 `TOES_IMPORT`。

结论：**是 `TOES_IMPORT`（`dllimport`）导致的**。在 `/clr` 编译单元里，`__declspec(dllimport)` 会让编译器按托管约定解析外部符号，生成带 `$$F` 的修饰名，而导入库导出的是原生符号。去掉该宏、并确保链接的是当前导入库之后，Debug 与 Release 均链接成功。

结论与约束：

- `TOES_IMPORT` 分支对纯 C++ 消费方是正确的；
- **C++/CLI（`/clr`）消费方不能定义它**，那类工程依赖原有的 thunk 机制即可正常工作。这条限制已写入 `include/hamster/toes/export.h` 的注释。

**当前状态**：C# 端在 Debug 与 Release 下均可正常重建与运行（此前"Bridge 链接失败"的结论是错的，已更正）。

### 7.5 本轮操作失误（已恢复）

批量改写脚本把嵌套数组当作二维表传给函数，PowerShell 将其展平为字符串数组，`$pr[0]`/`$pr[1]` 退化成单个字符，于是对 6 个文件执行了单字符全局替换（如 `#include` → `#iaclude`）。已从 git 恢复这些文件并重做；恢复时发现 `pe.h`/`pe.cpp` 因 §7.3 的原因不在 git 中，改为从重构前的 `Windows/Platform.*` 历史版本重建。

---

## 8. 第四轮：去掉工程文件里的通配符（2026-10-30）

### 8.1 问题

构建时出现告警：

```
warning : VC 项目不支持项目项定义中使用的通配符和/或其他一些 MSBuild 构造
```

起因是第三轮为消除三套源清单的漂移，把两个工程改成了通配符（`src\**\*.cpp`、`..\src\core\**\*.cpp` 等）。MSBuild 能正常展开，但 **Visual C++ 的项目系统不支持项目项通配符**，它会告警并可能让 IDE 出现异常行为。

### 8.2 方案

保留"清单由文件系统生成"的做法，但**写回时展开成显式条目**。新增 `tools/sync-project.ps1`：

| 用法 | 作用 |
|---|---|
| `tools\sync-project.ps1` | 扫描 `src/`、`include/`、`tests/`，重写两个工程的文件清单 |
| `tools\sync-project.ps1 -Check` | 只比对不改写；失步时返回 1，可用于 CI 或提交前检查 |

规则：库工程收录 `src/` 下全部 `.cpp` 与 `.h/.hpp`（排除 `legacy/`），加上 `include/` 下的公开头；测试工程收录 `tests/` 与 `src/` 下的全部 `.cpp`（排除 `legacy/` 与 `dllmain.cpp`），外加 4 个它要测的 legacy 源。

注意测试工程的条目要带 `..\` 前缀（相对工程所在目录），库工程则相对工作区根。

### 8.3 结果

| 工程 | 条目 | 通配符 |
|---|---|---|
| `HamsterStudioToes.vcxproj` | 18 个源 + 24 个头 | 0 |
| `HamsterStudioToes.Tests.vcxproj` | 23 个源 | 0 |

两个工程重建均无告警；测试 76 项全部通过；`-Check` 报 `in sync`。

### 8.4 使用约定

新增、移动或删除源文件后跑一次 `tools\sync-project.ps1`。忘记跑时，`-Check` 会报告失步并按非零码退出，可以挂在 CI 或提交前检查里。

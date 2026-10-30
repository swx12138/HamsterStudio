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
- `TOES_IMPORT` 目前没有消费方启用：Bridge 与 CLITools 都未定义该宏，行为与之前一致。启用它可以消除"靠链接器 thunk 偶然工作"的未定义边界，但需要消费方工程同步定义宏。

# HamsterStudioToes 结构重组清单（v2）

> 状态：**已执行**（2026-10-30）。实施记录、验收证据与偏差说明见 [`restructure-notes.md`](restructure-notes.md)。
> 本文档保留为方案原文，便于与实际改动逐条对照。

---

## 1. 约束（决定了方案形状）

### 1.1 唯一的跨模块公开契约

真实调用链：

```
C# (HamsterStudio.ImageTool)
  → HamsterStudioBridge64.dll        C++/CLI, net10.0, CLRSupport=NetCore
  → HamsterStudioToes64.dll          经由 import lib 链接
```

`HammerStudioBridge` 的 vcxproj 第 130/150 行：
`AdditionalDependencies = $(TargetDir)HammerStudioToes$(PlatformArchitecture).lib`

`HammerStudioBridge/ImageModuleWrapper.cpp:4-7` 只 include 四个头：

| 头文件 | 其中被使用的符号 |
|---|---|
| `Image/ImageStitcher.h` | `ImageShape`、`StretchMode`、`stitch(std::vector<void*>…)` |
| `Image/Tools/GradientFiller.h` | `FillBilinear` |
| `Image/Tools/LayoutHelper.h` | `ImageLayout`、`LayoutHelper::CalculateGridLayout` |
| `ColourSpaces/HueSaturationValue.h` | `HueSaturationValue::CreateHistogram` |

其余 90 个文件都是内部实现，可自由搬动。

### 1.2 另外两组已知 include 依赖

- **CLITools**（角度包含，根 = Toes 项目目录）：
  - `src/bait/main.cpp` → `<Image/Image.h>`、`<Tools/BaitingMaster.h>`
  - `src/stitch/main.cpp` → `<Image/ImageStitcher.h>`
  - `src/tojpg/tojpeg.cpp`、`src/tovid/main.cpp` → `<Image/Tools/ImageUtils.hpp>`
  - `src/cat/cat.cpp` → 不依赖 Toes
- **测试工程** `Tests/ToesTests.cpp:22-33` → 11 个项目头（见 §4.5）

### 1.3 写入边界

DSH 文件沙箱当前只允许写 `HamsterStudioToes\`。CLITools 与 Bridge 的修改需逐次 `danger-full-access` 授权。

### 1.4 本轮不做的事

不改命名空间、不改类名/函数签名、不修逻辑缺陷、不动 `TOES_API` 语义。
移动文件不改变 MSVC 修饰名，因此**导出表必须前后一致**——这是 P2 的验收标准。

---

## 2. 规模基线

| 项 | 现状 |
|---|---|
| 源文件（`.cpp/.h/.hpp`，不含构建产物） | 94 |
| 总行数 | 7,863 |
| 顶层目录 | 11（Algorithm / ColourSpaces / Hash / Image / Media / nstd / Shapes / Tests / Tools / Windows / 根） |
| 构建系统 | 3 套并行：`.vcxproj`（手写 34 条）、CLITools `CMakeLists.txt`（手写 28 条）、测试工程（通配 `..\*.cpp`） |

三套清单已经互相失步：`Hash/SecureHashAlgorithm.cpp` 只被测试工程编译；`ColourSpaces/HueSaturationValue.cpp` 只被 vcxproj 编译（CMake 清单漏了它）。

目标：**7 个公开头 + 7 个 `src/` 子目录**，两套构建共用一份 glob。

---

## 3. 目标结构

```
HamsterStudioToes/
├─ include/hamster/toes/          公开面（7 个头）
│    export.h  geometry.h  image.h  stitch.h  layout.h  gradient.h  color.h
├─ src/
│    dllmain.cpp
│    core/      diagnostics  strings  numbers  stream  stopwatch  codec/{base64 md5 hash_defines}
│    media/     image.cpp  io.hpp  baiting
│    imaging/   adjustments  gradient  screenshot  popular_colors.hpp
│    stitch/    stitch.cpp  layout  paste
│    color/     hsv
│    win32/     pe  os  shell_api
│    legacy/    43 个文件，不参与任何构建
├─ tests/       toes_tests.cpp  legacy/legacy_tests.cpp
├─ docs/restructure-plan.md       ← 本文档
├─ CMakeLists.txt                 ← 新增，与 vcxproj 等价
├─ Directory.Build.props          ← 新增，集中编译选项
└─ HamsterStudioToes.vcxproj      ← 改为 glob，去掉 Win32 僵尸配置
```

include 搜索根统一为两条：`include/`（对外 `#include <hamster/toes/x.h>`）与 `src/`（对内 `#include "core/diagnostics.h"`）。以后再搬文件不必改 include。

---

## 4. 完整去向表（94 个文件）

图例：**公开** = 进入 `include/`；**活跃** = 进入 `src/` 并参与构建；
**legacy** = 移入 `src/legacy/`，不参与构建；**删除** = 从仓库移除（均验证过零引用）。
「引用」= 项目内 include 该头的文件数。

### 4.1 根目录（2）

| 现路径 | 去向 | 处置 | 引用 | 备注 |
|---|---|---|---|---|
| `Export.h` | `include/hamster/toes/export.h` | 公开 | 5 | 补 `dllimport` 分支留待下一轮 |
| `dllmain.cpp` | `src/dllmain.cpp` | 活跃 | – | `add()` 导出见 §7.1 |

### 4.2 Image/（24）

| 现路径 | 去向 | 处置 | 引用 | 备注 |
|---|---|---|---|---|
| `Image/Image.h` | `include/hamster/toes/image.h` | 公开 | 3 | `ImageShape` 在此定义，被 stitch 依赖 |
| `Image/Image.cpp` | `src/media/image.cpp` | 活跃 | – | |
| `Image/ImageStitcher.h` | `include/hamster/toes/stitch.h` | 公开 | 3 | `StretchMode` 定义处 |
| `Image/ImageStitcher.cpp` | `src/stitch/stitch.cpp` | 活跃 | – | **删除 302–472 行 CLI**（§7.2） |
| `Image/ImageProcessor.h` | `src/imaging/adjustments.h` | 活跃 | 3 | |
| `Image/ImageProcessor.cpp` | `src/imaging/adjustments.cpp` | 活跃 | – | 全库质量最高，原样搬 |
| `Image/Tools/LayoutHelper.h` | `include/hamster/toes/layout.h` | 公开 | 4 | `ImageLayout` 定义处 |
| `Image/Tools/LayoutHelper.cpp` | `src/stitch/layout.cpp` | 活跃 | – | |
| `Image/Tools/GradientFiller.h` | `include/hamster/toes/gradient.h` | 公开 | 4 | |
| `Image/Tools/GradientFiller.cpp` | `src/imaging/gradient.cpp` | 活跃 | – | |
| `Image/Tools/PasteImageUtils.h` | `src/stitch/paste.h` | 活跃 | 3 | |
| `Image/Tools/PasteImageUtils.cpp` | `src/stitch/paste.cpp` | 活跃 | – | |
| `Image/Tools/ImageUtils.hpp` | `src/media/io.hpp` | 活跃 | 4 | 补 `#pragma once`（搬移必需，非逻辑修复） |
| `Image/ImageEditor.h` | `src/legacy/image/ImageEditor.h` | legacy | 1 | |
| `Image/ImageEditor.cpp` | `src/legacy/image/ImageEditor.cpp` | legacy | – | |
| `Image/ImageSlider.h` | `src/legacy/image/ImageSlider.h` | legacy | 1 | |
| `Image/ImageSlider.cpp` | `src/legacy/image/ImageSlider.cpp` | legacy | – | |
| `Image/PixelData/PixelData.h` | `src/legacy/image/PixelData.h` | legacy | 2 | 仅 ImageEditor 使用 |
| `Image/PixelData/PixelData.cpp` | `src/legacy/image/PixelData.cpp` | legacy | – | |
| `Image/PixelData/PixelDataProcessor.h` | `src/legacy/image/PixelDataProcessor.h` | legacy | 2 | |
| `Image/PixelData/PixelDataProcessor.cpp` | `src/legacy/image/PixelDataProcessor.cpp` | legacy | – | |
| `Image/PixelData/PopularColors.hpp` | `src/imaging/popular_colors.hpp` | **活跃** | 3 | 被 `stitch` 引用，见 §4.6 |
| `Image/Effects/BlackSoftEffect.hpp` | `src/legacy/image/BlackSoftEffect.hpp` | legacy | 0 | |
| `Image/Effects/Effects.hpp` | `src/legacy/image/Effects.hpp` | legacy | 1 | |

### 4.3 Tools/（24）

| 现路径 | 去向 | 处置 | 引用 | 备注 |
|---|---|---|---|---|
| `Tools/Diagnostics.h` | `src/core/diagnostics.h` | 活跃 | 8 | 全库基础 |
| `Tools/Diagnostics.cpp` | `src/core/diagnostics.cpp` | 活跃 | – | |
| `Tools/StringUtils.h` | `src/core/strings.h` | 活跃 | 1 | 缺陷原样搬（§7.3） |
| `Tools/StringUtils.cpp` | `src/core/strings.cpp` | 活跃 | – | |
| `Tools/NumberUtils.h` | `src/core/numbers.h` | 活跃 | 2 | |
| `Tools/NumberUtils.cpp` | `src/core/numbers.cpp` | 活跃 | – | |
| `Tools/StreamReader.h` | `src/core/stream.h` | 活跃 | 2 | |
| `Tools/StreamReader.cpp` | `src/core/stream.cpp` | 活跃 | – | |
| `Tools/StopWatch.hpp` | `src/core/stopwatch.hpp` | 活跃 | 1 | |
| `Tools/BaitingMaster.h` | `src/media/baiting.h` | 活跃 | 1 | 被 `hamster-bait` 使用 |
| `Tools/BaitingMaster.cpp` | `src/media/baiting.cpp` | 活跃 | – | |
| `Tools/Screenshot.h` | `src/imaging/screenshot.h` | 活跃 | 1 | 已编译、无调用者，暂留活跃 |
| `Tools/Screenshot.cpp` | `src/imaging/screenshot.cpp` | 活跃 | – | |
| `Tools/Keygen.h` | `src/legacy/tools/Keygen.h` | legacy | 1 | MT19937 生成密码 |
| `Tools/Keygen.cpp` | `src/legacy/tools/Keygen.cpp` | legacy | – | |
| `Tools/BigInteger.h` | `src/legacy/tools/BigInteger.h` | legacy | 1 | |
| `Tools/BigInteger.cpp` | `src/legacy/tools/BigInteger.cpp` | legacy | – | |
| `Tools/CmdLineArgsParser.h` | `src/legacy/tools/CmdLineArgsParser.h` | legacy | 1 | 依赖 MSVC `__argc` |
| `Tools/CmdLineArgsParser.cpp` | `src/legacy/tools/CmdLineArgsParser.cpp` | legacy | – | |
| `Tools/utilities.hpp` | — | **删除** | 0 | 非 inline 定义在头里，一 include 就重定义 |
| `Tools/Event.hpp` | — | **删除** | 0 | `EventHandler<T>` 未定义，实例化即编译失败 |
| `Tools/WatchDog.hpp` | — | **删除** | 0 | 自注「目前没有任何地方使用」 |
| `Tools/Semaphore.hpp` | — | **删除** | 0 | 缺 `#pragma once`，且 `wait` 无谓词 |
| `Tools/HtmlDocument.hpp` | — | **删除** | 0 | |

### 4.4 Windows/（17）

| 现路径 | 去向 | 处置 | 引用 | 备注 |
|---|---|---|---|---|
| `Windows/Platform.h` | `src/win32/pe.h` | 活跃 | 1 | |
| `Windows/Platform.cpp` | `src/win32/pe.cpp` | 活跃 | – | 全库质量最高，原样搬 |
| `Windows/Win32Utils.h` | `src/win32/os.h` | 活跃 | 6 | |
| `Windows/Win32Utils.cpp` | `src/win32/os.cpp` | 活跃 | – | 泄漏部分留待下一轮 |
| `Windows/ShellApi.h` | `src/win32/shell_api.h` | 活跃 | 1 | 被 `os.h` include，不能进 legacy |
| `Windows/Application.h` | `src/legacy/win32/Application.h` | legacy | 2 | |
| `Windows/Application.cpp` | `src/legacy/win32/Application.cpp` | legacy | – | |
| `Windows/Window.h` | `src/legacy/win32/Window.h` | legacy | 1 | |
| `Windows/Window.cpp` | `src/legacy/win32/Window.cpp` | legacy | – | |
| `Windows/Control.h` | `src/legacy/win32/Control.h` | legacy | 1 | |
| `Windows/Drawing.h` | `src/legacy/win32/Drawing.h` | legacy | 1 | `InRegion` 的 y 轴比较反了 |
| `Windows/Tools/GdiplusEnv.h` | `src/legacy/win32/GdiplusEnv.h` | legacy | 1 | |
| `Windows/Tools/GdiplusEnv.cpp` | `src/legacy/win32/GdiplusEnv.cpp` | legacy | – | |
| `Windows/Tools/TaskbarCtrl.h` | `src/legacy/win32/TaskbarCtrl.h` | legacy | 1 | |
| `Windows/Tools/TaskbarCtrl.cpp` | `src/legacy/win32/TaskbarCtrl.cpp` | legacy | – | |
| `Windows/Tools/WindowComposition.h` | `src/legacy/win32/WindowComposition.h` | legacy | 2 | |
| `Windows/Tools/WindowComposition.cpp` | `src/legacy/win32/WindowComposition.cpp` | legacy | – | |

### 4.5 Hash / Algorithm / Media / ColourSpaces / Shapes / nstd / Tests（27）

| 现路径 | 去向 | 处置 | 引用 | 备注 |
|---|---|---|---|---|
| `Hash/Base64.h` | `src/core/codec/base64.h` | 活跃 | 2 | |
| `Hash/Base64.cpp` | `src/core/codec/base64.cpp` | 活跃 | – | 填充解码缺陷原样搬 |
| `Hash/MessageDigest.h` | `src/core/codec/md5.h` | 活跃 | 2 | |
| `Hash/MessageDigest.cpp` | `src/core/codec/md5.cpp` | 活跃 | – | 删 `md5_test_main`（§7.2） |
| `Hash/defines.h` | `src/core/codec/hash_defines.h` | 活跃 | 2 | MD5 与 legacy 的 SHA 共用 |
| `Hash/SecureHashAlgorithm.h` | `src/legacy/hash/SecureHashAlgorithm.h` | legacy | 1 | 只被测试工程通配编译过 |
| `Hash/SecureHashAlgorithm.cpp` | `src/legacy/hash/SecureHashAlgorithm.cpp` | legacy | – | |
| `Hash/hash.hpp` | — | **删除** | 0 | 非 inline `Hash()` 定义在头里 |
| `ColourSpaces/HueSaturationValue.h` | `include/hamster/toes/color.h` | 公开 | 1 | |
| `ColourSpaces/HueSaturationValue.cpp` | `src/color/hsv.cpp` | 活跃 | – | |
| `Shapes/Rectangle.h` | `include/hamster/toes/geometry.h` | 公开 | 2 | 与 `ShapeIf.h` 合并为一个头 |
| `Shapes/ShapeIf.h` | ↑ 合并 | — | 0 | 空 concept，内容并入 `geometry.h` |
| `Algorithm/Math.h` | `src/legacy/algorithm/Math.h` | legacy | 1 | 被测试引用 |
| `Algorithm/Math.cpp` | `src/legacy/algorithm/Math.cpp` | legacy | – | `throw new` 留待下一轮 |
| `Algorithm/Sort.h` | `src/legacy/algorithm/Sort.h` | legacy | 2 | |
| `Algorithm/Sort.cpp` | `src/legacy/algorithm/Sort.cpp` | legacy | – | |
| `Algorithm/Search.hpp` | `src/legacy/algorithm/Search.hpp` | legacy | 1 | |
| `Algorithm/Array.hpp` | — | **删除** | 0 | `Push` 的数组 placement-new 堆溢出 |
| `Algorithm/Fibonacci.hpp` | — | **删除** | 0 | |
| `Media/MPEGLayer3.h` | `src/legacy/media/MPEGLayer3.h` | legacy | 1 | |
| `Media/MPEGLayer3.cpp` | `src/legacy/media/MPEGLayer3.cpp` | legacy | – | 调试 `printf_s` 留待下一轮 |
| `Media/Wave.h` | `src/legacy/media/Wave.h` | legacy | 2 | |
| `Media/Wave.cpp` | `src/legacy/media/Wave.cpp` | legacy | – | |
| `nstd/containers.h` | `src/legacy/nstd/containers.h` | legacy | 0 | |
| `nstd/containers/string.h` | `src/legacy/nstd/string.h` | legacy | 2 | 被测试引用 |
| `nstd/containers/string.cpp` | `src/legacy/nstd/string.cpp` | legacy | – | |
| `Tests/ToesTests.cpp` | `tests/toes_tests.cpp` | 活跃 | – | 拆分见 §4.6 |

**合计**：公开 7 + 活跃 34 + legacy 43 + 删除 8 + 合并吸收 1 + 测试 1 = 94 ✔

### 4.6 两处需要你知道的细节

1. **`PopularColors.hpp` 被活跃代码引用**：`ImageStitcher.cpp:14` 用它取潘通年度色。纯搬移不能把它挪进 legacy，否则 `stitch` 编不过。故上调为 `src/imaging/popular_colors.hpp`（活跃），表中已按此列。
2. **测试拆分**：`ToesTests.cpp` 里有 5 组用例针对 legacy 模块（`Sort::Buble`、`Search::BinarySearch`、`Math::ScaleConversion`、`WaveFormat`、`nstd::String`）。
   - 活跃用例 → `tests/toes_tests.cpp`，改为**链接 DLL**。
   - legacy 用例 → `tests/legacy/legacy_tests.cpp`，单独编译 `src/legacy/` 下相关源。
   - **54 个断言一个不丢。**

---

## 5. 跨项目修改（需授权，逐次弹窗）

### 5.1 `HammerStudioCLITools/CMakeLists.txt`

```diff
-set(HAMSTER_TOES_SOURCES
-    ... 28 条手工路径 ...
-)
+file(GLOB_RECURSE HAMSTER_TOES_SOURCES CONFIGURE_DEPENDS
+    ${HAMSTER_TOES_DIR}/src/core/*.cpp
+    ${HAMSTER_TOES_DIR}/src/media/*.cpp
+    ${HAMSTER_TOES_DIR}/src/imaging/*.cpp
+    ${HAMSTER_TOES_DIR}/src/stitch/*.cpp
+    ${HAMSTER_TOES_DIR}/src/color/*.cpp
+    ${HAMSTER_TOES_DIR}/src/win32/*.cpp
+)
+# legacy 不参与：src/legacy 不在上面的列表里

 set(HAMSTER_TOES_INCLUDE_DIRS
-    ${HAMSTER_TOES_DIR}
+    ${HAMSTER_TOES_DIR}/include
+    ${HAMSTER_TOES_DIR}/src
     ${OpenCV_INCLUDE_DIRS}
 )
```

顺带修掉两个既有问题：**漏掉的 `HueSaturationValue.cpp`**、以及 include 根从项目根改为 `include/` + `src/`。

### 5.2 CLITools 的 4 个 include

```diff
- #include <Image/Image.h>                 →  #include <hamster/toes/image.h>
- #include <Tools/BaitingMaster.h>         →  #include "media/baiting.h"
- #include <Image/ImageStitcher.h>         →  #include <hamster/toes/stitch.h>
- #include <Image/Tools/ImageUtils.hpp>    →  #include "media/io.hpp"
```

涉及 `src/bait/main.cpp`、`src/stitch/main.cpp`、`src/tojpg/tojpeg.cpp`、`src/tovid/main.cpp`。

### 5.3 `HammerStudioBridge/ImageModuleWrapper.cpp:4-7`

```diff
- #include "../HammerStudioToes/Image/ImageStitcher.h"
- #include "../HammerStudioToes/Image/Tools/GradientFiller.h"
- #include "../HammerStudioToes/Image/Tools/LayoutHelper.h"
- #include "../HammerStudioToes/ColourSpaces/HueSaturationValue.h"
+ #include "../HamsterStudioToes/include/hamster/toes/stitch.h"
+ #include "../HamsterStudioToes/include/hamster/toes/gradient.h"
+ #include "../HamsterStudioToes/include/hamster/toes/layout.h"
+ #include "../HamsterStudioToes/include/hamster/toes/color.h"
```

命名空间与符号名一律不动，因此 Bridge 里 `ImageNamespace::` / `ImageUtilsNamespace::` / `toes::colors::` 的调用代码不受影响。

---

## 6. 执行顺序

| 步 | 内容 | 验证 | 提交信息 |
|---|---|---|---|
| **P0** | `dumpbin /exports` 存档为 ABI 基线；跑通现有构建 | 快照文件 + 绿色基线 | `chore: 记录重构前 ABI 基线` |
| **P1** | 建 `src/legacy/`，迁入 43 个文件；两套构建剔除；8 个文件删除 | 双构建绿；导出表与基线一致 | `refactor: 隔离不可达模块到 legacy` |
| **P2** | 34 个活跃文件搬入新结构 + 7 个公开头 + 全库 include 改写（约 60 处） | **导出表与基线逐项一致** | `refactor: 按功能重组 src 与 include` |
| **P3** | `Directory.Build.props` + 两处 glob + OpenCV 集中到工程级 + 删 Win32 僵尸配置 | 双构建绿，CMake 不再漏文件 | `build: 统一三套源清单与编译选项` |
| **P4** | CLITools/Bridge 修改（§5）+ 测试拆分与接入 | Bridge 编译通过 + C# 冒烟 | `build: 同步下游项目到新结构` |

### 验证命令

```powershell
# ABI 基线对比（P2 后必须为空 diff）
dumpbin /exports x64\Release\HammerStudioToes64.dll > exports-after.txt
Compare-Object (Get-Content exports-baseline.txt) (Get-Content exports-after.txt)

# 双构建
cmake --build HammerStudioCLITools/build-cmkb --config Release
msbuild HamsterStudioToes\HamsterStudioToes.vcxproj /p:Configuration=Release /p:Platform=x64

# 测试
msbuild HamsterStudioToes\Tests\HamsterStudioToes.Tests.vcxproj /p:Configuration=Release /p:Platform=x64 /t:Rebuild
.\Tests\bin\Release\ToesTests.exe
```

---

## 7. 需要你拍板的三件事

### 7.1 `add()` 导出

`dllmain.cpp:38-43` 有一个 Win32 DLL 模板残留的 `extern "C" add(int,int)`，无任何头文件声明、无调用者，但**确实在导出表里**。

- 选项 A：一并删除 → 导出表少 1 项（预期内，P0 基线里标注为豁免）
- 选项 B：保留 → 导出表 diff 严格为空

### 7.2 库内 4 个入口

| 位置 | 内容 | 处置 |
|---|---|---|
| `ImageStitcher.cpp:302-472` | `stitch_cli_main`（170 行，含参数解析 / `std::cerr` / 目录扫描） | 删除 |
| `ImageEditor.cpp:84-103` | `image_editor_test_main` | 随 legacy 走，保留 |
| `MessageDigest.cpp:175-182` | `md5_test_main` | 删除 |
| `Window.cpp:127-133` | `CreateWindowTestMain` | 随 legacy 走 |

CLITools 已有 `hamster-stitch`，另有 `HammerStudioNativeCLI/main.cpp` 一份——三份重复收敛为 CLITools 一份。
**需你确认 CLITools 那份覆盖你现在的用法**（尤其 `portrait|landscape|all` 三种模式与默认目录行为）。

### 7.3 搬移时"必须顺带"的最小改动

为让新结构能编译，以下不属于逻辑修复，属搬移必需：

| 改动 | 原因 |
|---|---|
| `io.hpp` 补 `#pragma once` | 原文件没有，两处 include 已属侥幸 |
| `geometry.h` 合并 `Rectangle.h` + `ShapeIf.h` | `ShapeIf.h` 是空 concept，独立成文件无意义 |
| 全库 include 路径改写 | 机械替换，约 60 处 |

除此之外**不碰任何逻辑**：`StringUtils::Hex` 的 `%2X`、`Split` 的越界、`Replace` 死循环、`Base64::Decode` 的多余字节、`Math::IntegralResolver` 的 `throw new`、`Drawing::Rectangle::InRegion` 的反向比较——全部原样带到新位置，下一轮专项修。

---

## 8. 回滚

| 步 | 回滚方式 |
|---|---|
| P1 / P2 | `git revert <commit>`；纯文件移动、无内容变更，冲突面极小 |
| P3 | 单独 revert；`.props` 与 glob 只影响构建，不碰源码 |
| P4 | 下游项目改动与库解耦，可独立回滚 |

`git log --follow` 对 P2 的移动有效，历史不会断。

---

## 9. 预期结果

| 指标 | 前 | 后 |
|---|---|---|
| 顶层目录 | 11 | 6（`include/ src/ tests/ docs/` + 构建脚本） |
| 参与编译的源文件 | 34（vcxproj）/ 28（CMake） | 34（两者一致，由 glob 保证） |
| 源清单维护点 | 3 处手写 | 0 处手写 |
| 公开头 | 隐含在 4 个分散路径 | 7 个，集中在 `include/hamster/toes/` |
| 库内 `main` | 4 | 0 |
| ABI | 基线 | 与基线一致（除 §7.1 待定项） |

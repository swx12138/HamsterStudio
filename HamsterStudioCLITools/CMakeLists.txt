cmake_minimum_required(VERSION 3.20)
project(HamsterStudioCLITools VERSION 1.0.0 LANGUAGES CXX)

# 默认安装路径
set(CMAKE_INSTALL_PREFIX "D:/Develop/Tools" CACHE PATH "Install path" FORCE)

# ============================================================
# C++ Standard
# ============================================================
set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# ============================================================
# OpenMP (用于 BaitingMaster 的并行加速)
# ============================================================
find_package(OpenMP REQUIRED)

# ============================================================
# OpenCV
# ============================================================
# 优先查找 CUDA 版本
find_package(OpenCV QUIET
    PATHS "D:/Develop/Libraries/opencv/build_cuda/install"
    NO_DEFAULT_PATH
)
if(OpenCV_FOUND)
    # 检查是否真的有 CUDA 模块
    list(FIND OpenCV_LIBS "opencv_cudawarping" _cuda_idx)
    if(_cuda_idx GREATER -1)
        add_compile_definitions(_CUDA_OPENCV)
        message(STATUS "Found CUDA-enabled OpenCV: ${OpenCV_DIR}")
    else()
        message(STATUS "Found OpenCV (no CUDA): ${OpenCV_DIR}")
    endif()
else()
    find_package(OpenCV REQUIRED
        PATHS "D:/Develop/Libraries/opencv/build"
        NO_DEFAULT_PATH
    )
    message(STATUS "Found CPU OpenCV: ${OpenCV_DIR}")
endif()

# ============================================================
# HamsterStudioToes 静态库
# ============================================================
# HamsterStudioToes 源码（相对于项目根目录 ../HamsterStudioToes）
set(HAMSTER_TOES_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../HamsterStudioToes")

set(HAMSTER_TOES_SOURCES
    # Hash
    ${HAMSTER_TOES_DIR}/Hash/Base64.cpp
    ${HAMSTER_TOES_DIR}/Hash/MessageDigest.cpp

    # Algorithm
    ${HAMSTER_TOES_DIR}/Algorithm/Math.cpp
    ${HAMSTER_TOES_DIR}/Algorithm/Sort.cpp

    # Image
    ${HAMSTER_TOES_DIR}/Image/Image.cpp
    ${HAMSTER_TOES_DIR}/Image/ImageEditor.cpp
    ${HAMSTER_TOES_DIR}/Image/ImageProcessor.cpp
    ${HAMSTER_TOES_DIR}/Image/ImageSlider.cpp
    ${HAMSTER_TOES_DIR}/Image/ImageStitcher.cpp

    # Image/Tools
    ${HAMSTER_TOES_DIR}/Image/Tools/GradientFiller.cpp
    ${HAMSTER_TOES_DIR}/Image/Tools/LayoutHelper.cpp
    ${HAMSTER_TOES_DIR}/Image/Tools/PasteImageUtils.cpp

    # Image/PixelData
    ${HAMSTER_TOES_DIR}/Image/PixelData/PixelData.cpp
    ${HAMSTER_TOES_DIR}/Image/PixelData/PixelDataProcessor.cpp

    # Tools
    ${HAMSTER_TOES_DIR}/Tools/BaitingMaster.cpp
    ${HAMSTER_TOES_DIR}/Tools/BigInteger.cpp
    ${HAMSTER_TOES_DIR}/Tools/CmdLineArgsParser.cpp
    ${HAMSTER_TOES_DIR}/Tools/Diagnostics.cpp
    ${HAMSTER_TOES_DIR}/Tools/Keygen.cpp
    ${HAMSTER_TOES_DIR}/Tools/NumberUtils.cpp
    ${HAMSTER_TOES_DIR}/Tools/Screenshot.cpp
    ${HAMSTER_TOES_DIR}/Tools/StreamReader.cpp
    ${HAMSTER_TOES_DIR}/Tools/StringUtils.cpp

    # Media
    ${HAMSTER_TOES_DIR}/Media/MPEGLayer3.cpp
    ${HAMSTER_TOES_DIR}/Media/Wave.cpp

    # Windows
    ${HAMSTER_TOES_DIR}/Windows/Application.cpp
    ${HAMSTER_TOES_DIR}/Windows/Platform.cpp
    ${HAMSTER_TOES_DIR}/Windows/Win32Utils.cpp
    ${HAMSTER_TOES_DIR}/Windows/Window.cpp

    # Windows/Tools
    ${HAMSTER_TOES_DIR}/Windows/Tools/GdiplusEnv.cpp
    ${HAMSTER_TOES_DIR}/Windows/Tools/TaskbarCtrl.cpp
    ${HAMSTER_TOES_DIR}/Windows/Tools/WindowComposition.cpp
)

# 收集所有头文件路径
set(HAMSTER_TOES_INCLUDE_DIRS
    ${HAMSTER_TOES_DIR}
    ${OpenCV_INCLUDE_DIRS}
)

# 收集链接库
set(HAMSTER_TOES_LIBS
    ${OpenCV_LIBS}
    ${CMAKE_DL_LIBS}
)

# Windows 特有链接库
if(WIN32)
    list(APPEND HAMSTER_TOES_LIBS
        gdiplus
        shlwapi
        comctl32
        comdlg32
        ole32
        oleaut32
        shell32
        user32
        gdi32
    )
endif()

# 创建 HamsterStudioToes 静态库
add_library(HamsterStudioToes STATIC ${HAMSTER_TOES_SOURCES})
target_include_directories(HamsterStudioToes PUBLIC ${HAMSTER_TOES_INCLUDE_DIRS})
target_link_libraries(HamsterStudioToes PUBLIC ${HAMSTER_TOES_LIBS} OpenMP::OpenMP_CXX)

target_compile_options(HamsterStudioToes PRIVATE /utf-8)
target_compile_definitions(HamsterStudioToes PRIVATE
    _LIB
    $<$<CONFIG:Debug>:_DEBUG>
    $<$<NOT:$<CONFIG:Debug>>:NDEBUG>
    UNICODE
    _UNICODE
    _CRT_SECURE_NO_WARNINGS
    _SILENCE_ALL_CXX17_DEPRECATION_WARNINGS
)

# ============================================================
# 可执行文件 1: hamster-stitch (与 HamsterStudioNativeCLI 相同)
# ============================================================
add_executable(hamster-stitch
    src/stitch/main.cpp
)
target_link_libraries(hamster-stitch PRIVATE HamsterStudioToes)
target_compile_options(hamster-stitch PRIVATE /utf-8)

# ============================================================
# 可执行文件 2: hamster-bait (调用 BaitingMaster)
# ============================================================
add_executable(hamster-bait
    src/bait/main.cpp
)
target_link_libraries(hamster-bait PRIVATE HamsterStudioToes)
target_compile_options(hamster-bait PRIVATE /utf-8)

# ============================================================
# 安装规则
# ============================================================
install(TARGETS hamster-stitch hamster-bait
    RUNTIME DESTINATION hamster-cli
)

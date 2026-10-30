cmake_minimum_required(VERSION 3.20)
project(HamsterStudioCLITools VERSION 1.0.0 LANGUAGES CXX)

# 榛樿瀹夎璺緞
set(CMAKE_INSTALL_PREFIX "D:/Develop/Tools" CACHE PATH "Install path" FORCE)

# ============================================================
# C++ Standard
# ============================================================
set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)


# ============================================================
# OpenMP (鐢ㄤ簬 BaitingMaster 鐨勫苟琛屽姞閫?
# ============================================================
find_package(OpenMP REQUIRED)

# ============================================================
# OpenCV
# ============================================================
# 浼樺厛鏌ユ壘 CUDA 鐗堟湰
# 娉ㄦ剰锛欳UDA 鐗堟湰鐨?OpenCVConfig.cmake 鍦?Release/Debug 瀛愮洰褰曚笅锛堣€岄潪椤跺眰锛夛紝
#       闇€瑕佹寚鍚戝叿浣撳瓙鐩綍銆俈isual Studio 澶氶厤缃敓鎴愬櫒涓嬬粺涓€鐢?Release銆?
# 娑堥櫎 OpenCV 鏃х増 FindCUDA 妯″潡鐨?CMP0146 璀﹀憡
if(CMAKE_VERSION VERSION_GREATER_EQUAL "3.27")
    cmake_policy(SET CMP0146 OLD)
endif()

find_package(OpenCV REQUIRED)
if(OpenCV_FOUND)
    # 妫€鏌ユ槸鍚︾湡鐨勬湁 CUDA 妯″潡
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
# HamsterStudioToes 闈欐€佸簱
# ============================================================
# HamsterStudioToes 婧愮爜锛堢浉瀵逛簬椤圭洰鏍圭洰褰?../HamsterStudioToes锛?
set(HAMSTER_TOES_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../HamsterStudioToes")

file(GLOB_RECURSE HAMSTER_TOES_SOURCES CONFIGURE_DEPENDS
    ${HAMSTER_TOES_DIR}/src/core/*.cpp
    ${HAMSTER_TOES_DIR}/src/media/*.cpp
    ${HAMSTER_TOES_DIR}/src/imaging/*.cpp
    ${HAMSTER_TOES_DIR}/src/stitch/*.cpp
    ${HAMSTER_TOES_DIR}/src/color/*.cpp
    ${HAMSTER_TOES_DIR}/src/win32/*.cpp
)

# 鏀堕泦鎵€鏈夊ご鏂囦欢璺緞
set(HAMSTER_TOES_INCLUDE_DIRS
    ${HAMSTER_TOES_DIR}/include
    ${HAMSTER_TOES_DIR}/src
    ${OpenCV_INCLUDE_DIRS}
)

# 鏀堕泦閾炬帴搴?
set(HAMSTER_TOES_LIBS
    ${OpenCV_LIBS}
    ${CMAKE_DL_LIBS}
)

# Windows 鐗规湁閾炬帴搴?
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

# 鍒涘缓 HamsterStudioToes 闈欐€佸簱
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
# 鍙墽琛屾枃浠?1: hamster-stitch (涓?HamsterStudioNativeCLI 鐩稿悓)
# ============================================================
add_executable(hamster-stitch
    src/stitch/main.cpp
)
target_link_libraries(hamster-stitch PRIVATE HamsterStudioToes)
target_compile_options(hamster-stitch PRIVATE /utf-8)

# ============================================================
# 鍙墽琛屾枃浠?2: hamster-bait (璋冪敤 BaitingMaster)
# ============================================================
add_executable(hamster-bait
    src/bait/main.cpp
)
target_link_libraries(hamster-bait PRIVATE HamsterStudioToes)
target_compile_options(hamster-bait PRIVATE /utf-8)

# ============================================================
# 鍙墽琛屾枃浠?3: hamster-cat (Windows 鐗?cat 鍛戒护)
# ============================================================
add_executable(hamster-cat
    src/cat/cat.cpp
)
target_link_libraries(hamster-cat PRIVATE HamsterStudioToes)
target_compile_options(hamster-cat PRIVATE /utf-8)

# ============================================================
# 鍙墽琛屾枃浠?4: hamster-tojpg (鍥剧墖杞?JPEG 鍛戒护)
# ============================================================
add_executable(hamster-tojpg
    src/tojpg/tojpeg.cpp
)
target_link_libraries(hamster-tojpg PRIVATE HamsterStudioToes)
target_compile_options(hamster-tojpg PRIVATE /utf-8)

# ============================================================
# 鍙墽琛屾枃浠?5: hamster-tovid (鍥剧墖鍚堟垚瑙嗛鍛戒护)
# ============================================================
add_executable(hamster-tovid
    src/tovid/main.cpp
)
target_link_libraries(hamster-tovid PRIVATE HamsterStudioToes)
target_compile_options(hamster-tovid PRIVATE /utf-8)

# ============================================================
# 瀹夎瑙勫垯
# ============================================================
install(TARGETS hamster-stitch hamster-bait hamster-cat hamster-tojpg hamster-tovid
    RUNTIME DESTINATION ../Tools/hamster-cli
)

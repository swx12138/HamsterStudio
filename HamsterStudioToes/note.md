
# 进程

##  WinMain

### 原型

int WINAPI WinMain(HINSTANCE hInst, HINSTANCE hPrevInst, LPSTR lpCmdLine, int nCmdShow);

### 参数

1. hInst 实际值是一个内存基地址，系统将exe的映像加载到这个位置；

> HMODULE和HINSTANCE是一个东西，但在16bit系统中是不同类型的数据

2. hPrevInst NULL

> 兼容16bit系统的保留参数

3. lpCmdLine 命令行参数
4. nCmdShow 窗口显示状态

## 环境变量

### GetEnvirmentStrings()
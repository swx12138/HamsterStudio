#include "core/diagnostics.h"

#include <algorithm>
#include <system_error>

toes::diagnostics::AssertException::AssertException(const std::string &message)
	: std::runtime_error(message.data())
{ }

void toes::diagnostics::Assert(bool condition, std::string const &message)
{
	if (!condition) {
		throw AssertException(message.data());
	}
}

#include <Windows.h>

void toes::diagnostics::ThrowLastError(std::vector<int> const &toleratedCodes)
{
	DWORD const code = ::GetLastError();

	// 0 表示"没有错误"，没什么可报的
	if (code == 0) {
		return;
	}

	if (std::find(toleratedCodes.begin(), toleratedCodes.end(), static_cast<int>(code)) != toleratedCodes.end()) {
		return;
	}

	// 原来这里要手工 FormatMessage、切控制台代码页、失败时弹 MessageBox 再
	// ExitProcess。std::system_error 会用系统错误表翻译错误码，既不需要那些
	// 附带动作，也不会替调用方把进程杀掉。
	throw std::system_error(static_cast<int>(code), std::system_category());
}

void toes::diagnostics::Trace::TraceInfo(std::string const &message)
{
	OutputToDebugConsole(std::string("Info: ") + message);
}

void toes::diagnostics::Trace::TraceInfo(std::wstring const &message)
{
	OutputToDebugConsole(std::wstring(L"Info: ") + message);
}

void toes::diagnostics::Trace::TraceWarning(std::string const &message)
{
	OutputToDebugConsole(std::string("Warning: ") + message);
}

void toes::diagnostics::Trace::TraceWarning(std::wstring const &message)
{
	OutputToDebugConsole(std::wstring(L"Warning: ") + message);
}

void toes::diagnostics::Trace::TraceWarning(std::exception const &ex)
{
	TraceWarning(std::string("Exception: ") + ex.what());
}

void toes::diagnostics::Trace::TraceError(std::string const &message)
{
	OutputToDebugConsole(std::string("Error: ") + message);
}

void toes::diagnostics::Trace::TraceError(std::wstring const &message)
{
	OutputToDebugConsole(std::wstring(L"Error: ") + message);
}

void toes::diagnostics::Trace::TraceError(std::exception const &ex)
{
	TraceError(std::string("Exception: ") + ex.what());
}

#include <mutex>

std::mutex OutputDebugStringMtx;

void toes::diagnostics::Trace::OutputToDebugConsole(std::string const& message)
{
	OutputToDebugConsole(std::wstring(message.begin(), message.end()));
}

void toes::diagnostics::Trace::OutputToDebugConsole(std::wstring const& message)
{
	std::lock_guard g { OutputDebugStringMtx };

	static ::HANDLE _stdout = ::GetStdHandle(STD_OUTPUT_HANDLE);
	::DWORD written;
	::WriteConsoleW(_stdout, message.c_str(), static_cast<DWORD>(message.length()), &written, NULL);

	::OutputDebugStringW(message.c_str());
	if (!message.ends_with(L'\n')) {
		::OutputDebugStringW(L"\n");
		::WriteConsoleW(_stdout, L"\n", 1, &written, NULL);
	};
}

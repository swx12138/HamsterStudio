#include "Debug.h"
#include "Debug.h"

HamsterStudioToes::Debug::AssertException::AssertException(const std::string& message)
	: std::runtime_error(message.data())
{
}

void HamsterStudioToes::Debug::Assert(bool condition, std::string const& message)
{
	if (!condition) {
		throw AssertException(message.data());
	}
}

#include <Windows.h>

void HamsterStudioToes::Debug::ErrorExit(std::vector<int> safeCodes)
{
	// Ensure console input code page is set to UTF-8
	if (CP_UTF8 != GetConsoleCP()) {
		if (FALSE == SetConsoleOutputCP(CP_UTF8)) {
			MessageBox(NULL, TEXT("Failed to set console code page to UTF-8"), TEXT("Error"), MB_OK);
			ExitProcess(1);
		}
	}

	LPVOID lpMsgBuf;
	DWORD dw = GetLastError();

	if (FormatMessage(
		FORMAT_MESSAGE_ALLOCATE_BUFFER |
		FORMAT_MESSAGE_FROM_SYSTEM |
		FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL,
		dw,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR)&lpMsgBuf,
		0, NULL) == 0) {
		MessageBox(NULL, TEXT("FormatMessage failed"), TEXT("Error"), MB_OK);
		ExitProcess(dw);
	}

	//MessageBox(NULL, (LPCTSTR)lpMsgBuf, TEXT("Error"), MB_OK);
	wprintf_s(L"Error: %ws\n", (LPCTSTR)lpMsgBuf);

	LocalFree(lpMsgBuf);

	if (dw != 0 && (safeCodes.empty() || std::find(safeCodes.begin(), safeCodes.end(), dw) == safeCodes.end())) {
		ExitProcess(dw);
	}
}

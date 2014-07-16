#include "timer_t.h"
#include <stdexcept>
#include <Windows.h>

struct __declspec(novtable) scheduler_interface
{
	virtual void schedule(TaskProc_t, _In_ void*) = 0;
};


class windows_scheduler : public scheduler_interface
{
public:
	virtual void schedule(TaskProc_t proc, _In_ void* param);
};


class windows_timer : public timer_impl::_Timer_interface
{
public:
	windows_timer(TaskProc_t userFunc, void * context)
		: m_userFunc(userFunc), m_userContext(context)
	{
	}

	virtual ~windows_timer()
	{
	}

	virtual void start(unsigned int ms, bool repeat)
	{
		if (!CreateTimerQueueTimer(&m_hTimer, NULL, _TimerCallback, this, ms, repeat ? ms : 0, WT_EXECUTEDEFAULT))
		{
			throw std::bad_alloc();
		}
	}

	virtual void stop(bool waitForCallbacks)
	{
		while (!DeleteTimerQueueTimer(NULL, m_hTimer, waitForCallbacks ? INVALID_HANDLE_VALUE : NULL))
		{
			if (GetLastError() == ERROR_IO_PENDING)
				break;
		}

		delete this;
	}

private:

	static void CALLBACK _TimerCallback(PVOID context, BOOLEAN)
	{
		auto timer = static_cast<windows_timer *>(context);
		timer->m_userFunc(timer->m_userContext);
	}

	HANDLE m_hTimer;
	TaskProc_t m_userFunc;
	void * m_userContext;
};

void timer_impl::start(unsigned int ms, bool repeat, TaskProc_t userFunc, _In_ void * context)
{
	/*_ASSERTE(m_timerImpl == nullptr);*/
	m_timerImpl = new windows_timer(userFunc, context);
	m_timerImpl->start(ms, repeat);
}

void timer_impl::stop(bool waitForCallbacks)
{
	if (m_timerImpl != nullptr)
	{
		m_timerImpl->stop(waitForCallbacks);
		m_timerImpl = nullptr;
	}
}
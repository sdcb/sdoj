#pragma once

typedef void(__cdecl * TaskProc_t)(void *);

class timer_impl
{
public:

	timer_impl()
		: m_timerImpl(nullptr)
	{
	}

	void start(unsigned int ms, bool repeat, TaskProc_t userFunc, void * context);
	void stop(bool waitForCallbacks);

	class _Timer_interface
	{
	public:
		virtual ~_Timer_interface()
		{
		}

		virtual void start(unsigned int ms, bool repeat) = 0;
		virtual void stop(bool waitForCallbacks) = 0;
	};

private:
	_Timer_interface * m_timerImpl;
};

/// <summary>
/// Timer
/// </summary>
typedef timer_impl timer_t;
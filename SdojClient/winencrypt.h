#pragma once

#include "handle.h"
#include <bcrypt.h>
#pragma comment(lib, "bcrypt")

namespace wincrypt
{
	using namespace KennyKerr;

	struct provider_traits
	{
		using pointer = BCRYPT_ALG_HANDLE;

		static auto invalid() throw() -> pointer
		{
			return nullptr;
		}

		static auto close(pointer value) throw() -> void
		{
			VERIFY_(ERROR_SUCCESS, BCryptCloseAlgorithmProvider(value, 0));
		}
	};

	using provider = unique_handle<provider_traits>;

	struct status_exception
	{
		NTSTATUS code;

		status_exception(NTSTATUS result) :
			code{ result }
		{
		}
	};

	auto check(NTSTATUS const status) -> void;

	auto open_provider(wchar_t const * algorithm)->provider;

	auto random(provider const & p,
				void * buffer,
				unsigned size) -> void;

	template <typename T, unsigned Count>
	auto random(provider const & p,
				T(&buffer)[Count]) -> void
	{
		random(p,
			   buffer,
			   sizeof(T)* Count);
	}

	template <typename T>
	auto random(provider const & p,
				T & buffer) -> void
	{
		random(p,
			   &buffer,
			   sizeof(T));
	}








	struct hash_traits
	{
		using pointer = BCRYPT_ALG_HANDLE;

		static auto invalid() throw() -> pointer
		{
			return nullptr;
		}

		static auto close(pointer value) throw() -> void
		{
			VERIFY_(ERROR_SUCCESS, BCryptDestroyHash(value));
		}
	};

	using hash = unique_handle<hash_traits>;

	auto create_hash(provider const & p)->hash;

	auto combine(hash const & h,
				 void const * buffer,
				 unsigned size) -> void;

	template <typename T>
	auto get_property(BCRYPT_HANDLE handle,
					  wchar_t const * name,
					  T & value) -> void
	{
		auto bytesCopied = ULONG{};

		check(BCryptGetProperty(handle,
			name,
			reinterpret_cast<PUCHAR>(&value),
			sizeof(T),
			&bytesCopied,
			0));
	}

	auto get_value(hash const & h,
				   void * buffer,
				   unsigned size) -> void;







	struct key_traits
	{
		using pointer = BCRYPT_KEY_HANDLE;

		static auto invalid() throw() -> pointer
		{
			return nullptr;
		}

		static auto close(pointer value) throw() -> void
		{
			VERIFY_(ERROR_SUCCESS, BCryptDestroyKey(value));
		}
	};

	using key = unique_handle<key_traits>;


	auto create_key(provider const & p,
					void const * secret,
					unsigned size)->key;

	auto create_asymmetric_key(provider const & p,
							   unsigned keysize = 0)->key;

	auto export_key(key const & fk,
					wchar_t const * blobtype)->std::vector<UCHAR>;

	auto import_key(provider const & p,
					wchar_t const * blobtype,
					void const * blob,
					unsigned blobsize)->key;

	auto get_agreement(key const & fk,
					   key const & pk)->std::vector<UCHAR>;

	auto encrypt(key const & k,
				 void const * plaintext,
				 unsigned plaintext_size,
				 void * ciphertext,
				 unsigned ciphertext_size,
				 unsigned flags) -> unsigned;

	auto decrypt(key const & k,
				 void const * ciphertext,
				 unsigned ciphertext_size,
				 void * plaintext,
				 unsigned plaintext_size,
				 unsigned flags) -> unsigned;

	auto create_shared_secret(std::string const & secret)->std::vector<BYTE>;

	auto encrypt_message(wchar_t const * algorithm,
						 std::vector<BYTE> const & shared,
						 std::string const & plaintext)->std::vector<BYTE>;


	auto decrypt_message(wchar_t const * algorithm,
						 std::vector<BYTE> const & shared,
						 std::vector<BYTE> const & ciphertext)->std::string;
}



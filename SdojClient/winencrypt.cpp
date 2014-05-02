#include "stdafx.h"
#include "winencrypt.h"

namespace wincrypt
{
	auto random(provider const & p,
				void * buffer,
				unsigned size) -> void
	{
		check(BCryptGenRandom(p.get(),
			static_cast<PUCHAR>(buffer),
			size,
			0));
	}

	auto create_hash(provider const & p) -> hash
	{
		auto h = hash{};

		check(BCryptCreateHash(p.get(),
			h.get_address_of(),
			nullptr,
			0,
			nullptr,
			0,
			0));

		return h;
	}

	auto combine(hash const & h,
				 void const * buffer,
				 unsigned size) -> void
	{
		check(BCryptHashData(h.get(),
			static_cast<PUCHAR>(const_cast<void *>(buffer)),
			size,
			0));
	}

	auto get_value(hash const & h,
				   void * buffer,
				   unsigned size) -> void
	{
		check(BCryptFinishHash(h.get(),
			static_cast<PUCHAR>(buffer),
			size,
			0));
	}

	auto create_key(provider const & p,
					void const * secret,
					unsigned size) -> key
	{
		auto k = key{};

		check(BCryptGenerateSymmetricKey(
			p.get(),
			k.get_address_of(),
			nullptr,
			0,
			static_cast<PUCHAR>(const_cast<void *>(secret)),
			size,
			0));

		return k;
	}

	auto create_asymmetric_key(provider const & p,
							   unsigned keysize) -> key
	{
		auto fk = key{};
		check(BCryptGenerateKeyPair(p.get(),
			fk.get_address_of(),
			keysize,
			0));
		check(BCryptFinalizeKeyPair(fk.get(), 0));
		return fk;
	}

	auto export_key(key const & fk,
					wchar_t const * blobtype) -> std::vector<UCHAR>
	{
		ULONG publickey_size;
		check(BCryptExportKey(
			fk.get(),
			nullptr,
			blobtype,
			nullptr,
			0,
			&publickey_size,
			0));
		auto value = std::vector<UCHAR>(publickey_size);
		check(BCryptExportKey(
			fk.get(),
			nullptr,
			blobtype,
			&value[0],
			publickey_size,
			&publickey_size,
			0));

		return value;
	}

	auto import_key(provider const & p,
					wchar_t const * blobtype,
					void const * blob,
					unsigned blobsize) -> key
	{
		key k;
		check(BCryptImportKeyPair(
			p.get(),
			nullptr,
			blobtype,
			k.get_address_of(),
			static_cast<PUCHAR>(const_cast<void *>(blob)),
			blobsize,
			0));
		return k;
	}

	auto get_agreement(key const & fk,
					   key const & pk, 
					   wchar_t const * hash_name) -> std::vector<UCHAR>
	{
		BCRYPT_SECRET_HANDLE handle;
		check(BCryptSecretAgreement(
			fk.get(),
			pk.get(),
			&handle,
			0));

		wstring hash_name_string = hash_name;

		BCryptBufferDesc params;
		params.ulVersion = BCRYPTBUFFER_VERSION;
		params.cBuffers = 1;
		params.pBuffers = new BCryptBuffer[params.cBuffers];
		params.pBuffers[0].cbBuffer = (unsigned)hash_name_string.size()*2+2;
		params.pBuffers[0].BufferType = KDF_HASH_ALGORITHM;
		params.pBuffers[0].pvBuffer = &hash_name_string[0];

		ULONG derivedkey_size;
		check(BCryptDeriveKey(
			handle,
			BCRYPT_KDF_HASH,
			&params,
			nullptr,
			0,
			&derivedkey_size,
			0));

		auto value = std::vector<UCHAR>(derivedkey_size);
		check(BCryptDeriveKey(
			handle,
			BCRYPT_KDF_HASH,
			&params,
			&value[0],
			derivedkey_size,
			&derivedkey_size,
			0));

		check(BCryptDestroySecret(handle));
		return value;
	}

	auto encrypt(key const & k,
				 void const * plaintext,
				 unsigned plaintext_size,
				 void * ciphertext,
				 unsigned ciphertext_size,
				 unsigned flags) -> unsigned
	{
		auto bytesCopied = ULONG{};

		check(BCryptEncrypt(
			k.get(),
			static_cast<PUCHAR>(const_cast<void *>(plaintext)),
			plaintext_size,
			nullptr,
			nullptr,
			0,
			static_cast<PUCHAR>(ciphertext),
			ciphertext_size,
			&bytesCopied,
			flags));

		return bytesCopied;
	}

	auto decrypt(key const & k,
				 void const * ciphertext,
				 unsigned ciphertext_size,
				 void * plaintext,
				 unsigned plaintext_size,
				 unsigned flags) -> unsigned
	{
		auto bytesCopied = ULONG{};

		check(BCryptDecrypt(
			k.get(),
			static_cast<PUCHAR>(const_cast<void *>(ciphertext)),
			ciphertext_size,
			nullptr,
			nullptr,
			0,
			static_cast<PUCHAR>(plaintext),
			plaintext_size,
			&bytesCopied,
			flags));

		return bytesCopied;
	}

	auto decrypt(key const & k,
				 vector<byte> const & ciphertext,
				 vector<byte> & iv,
				 unsigned flags)->vector<byte>
	{
		auto bytesCopied = ULONG{};		

		check(BCryptDecrypt(
			k.get(),
			static_cast<PUCHAR>(const_cast<PUCHAR>(&ciphertext[0])),
			static_cast<unsigned>(ciphertext.size()),
			nullptr,
			static_cast<PUCHAR>(const_cast<PUCHAR>(&iv[0])),
			static_cast<unsigned>(iv.size()),
			nullptr, 
			0, 
			&bytesCopied,
			flags));

		auto plaintext = vector<byte>(bytesCopied);

		check(BCryptDecrypt(
			k.get(),
			static_cast<PUCHAR>(const_cast<PUCHAR>(&ciphertext[0])),
			static_cast<unsigned>(ciphertext.size()),
			nullptr,
			static_cast<PUCHAR>(const_cast<PUCHAR>(&iv[0])),
			static_cast<unsigned>(iv.size()),
			static_cast<PUCHAR>(const_cast<PUCHAR>(&plaintext[0])),
			static_cast<unsigned>(plaintext.size()),
			&bytesCopied,
			flags));

		return plaintext;
	}

	auto create_shared_secret(std::string const & secret) -> std::vector<BYTE>
	{
		auto p = open_provider(BCRYPT_SHA256_ALGORITHM);

		auto h = create_hash(p);

		combine(h, secret.c_str(), secret.size());

		auto size = unsigned{};

		get_property(h.get(), BCRYPT_HASH_LENGTH, size);

		auto value = std::vector<BYTE>(size);

		get_value(h, &value[0], size);

		return value;
	}

	auto encrypt_message(wchar_t const * algorithm,
						 std::vector<BYTE> const & shared,
						 std::string const & plaintext) -> std::vector<BYTE>
	{
		auto p = open_provider(algorithm);

		auto k = create_key(p,
							&shared[0],
							shared.size());

		auto size = encrypt(k,
							plaintext.c_str(),
							plaintext.size(),
							nullptr,
							0,
							BCRYPT_BLOCK_PADDING);

		std::vector<BYTE> ciphertext(size);

		encrypt(k,
				plaintext.c_str(),
				plaintext.size(),
				&ciphertext[0],
				size,
				BCRYPT_BLOCK_PADDING);

		return ciphertext;
	}


	auto decrypt_message(wchar_t const * algorithm,
						 std::vector<BYTE> const & shared,
						 std::vector<BYTE> const & ciphertext) -> std::string
	{
		auto p = open_provider(algorithm);

		auto k = create_key(p,
							&shared[0],
							shared.size());

		auto size = decrypt(k,
							&ciphertext[0],
							ciphertext.size(),
							nullptr,
							0,
							BCRYPT_BLOCK_PADDING);

		auto plaintext = std::string(size, '\0');

		decrypt(k,
				&ciphertext[0],
				ciphertext.size(),
				&plaintext[0],
				size,
				BCRYPT_BLOCK_PADDING);

		plaintext.resize(strlen(plaintext.c_str()));

		return plaintext;
	}

	auto check(NTSTATUS const status) -> void
	{
		if (status != ERROR_SUCCESS)
		{
			throw status_exception{ status };
		}
	}

	auto open_provider(wchar_t const * algorithm) -> provider
	{
		auto p = provider{};
		check(BCryptOpenAlgorithmProvider(
			p.get_address_of(),
			algorithm,
			nullptr,
			0));
		return p;
	}
}
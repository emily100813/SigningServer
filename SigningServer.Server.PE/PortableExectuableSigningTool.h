#pragma once

#include <Windows.h>
#include "ILogger.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Security::Cryptography::X509Certificates;
using namespace SigningServer::Contracts;

namespace SigningServer { namespace Server { namespace PE {

	public ref class PortableExectuableSigningTool
		: public ISigningTool
	{
	public:
		PortableExectuableSigningTool(ILogger^ log);
		static PortableExectuableSigningTool();
		virtual ~PortableExectuableSigningTool();

		virtual bool IsFileSupported(String^ fileName);
		property array<String^>^ SupportedFileExtensions { virtual array<String^>^ get(); }
		property array<String^>^ SupportedHashAlgorithms { virtual array<String^>^ get(); }
		
		virtual void SignFile(String^ inputFileName, X509Certificate2^ certificate, String^ timestampUrl, SignFileRequest^ signFileRequest, SignFileResponse^ signFileResponse);
		virtual bool IsFileSigned(String^ fileName);
		virtual void UnsignFile(String^ fileName);

	private:
		ILogger^ m_log;

		static initonly HashSet<String^>^ PESupportedExtensions;
		static initonly Dictionary<String^, unsigned int>^ PESupportedHashAlgorithms;
		static initonly bool CanSign;
	};

}
}
}

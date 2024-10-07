---
page_type: sample
languages:
- csharp
- java
- powershell
- python
- vb
products:
- azure
description: "Sample code of Microsoft HPC Pack 2019"
urlFragment: hpcpack-samples
---

# Microsoft HPC Pack 2019 Sample Code
Sample code of Microsoft HPC Pack 2019

Please choose SDK version (_in VisualStudio or by altering .csproj) according to your need:
- Version 6.3.8187-beta: preview SDK with Linux support
- Version 6.2.7756: for __Microsoft HPC Pack 2019 Update 2__
- Version 6.1.7531: for __Microsoft HPC Pack 2019 Update 1__
- Version 6.0.7121 or 6.0.7205: for __Microsoft HPC Pack 2019 RTM__

## .NET Standard 2.0 and Linux support

### Registry
The Linux SDK supports specifying configuration originally taken care of by Windows registry keys by environment variables prefixed with `CCP_CONFIG_`. For example, the `CertificateValidationType` registry key under `HKLM\SOFTWARE\Microsoft\HPC` can be specified on Linux by passing in the environment variable `CCP_CONFIG_CertificateValidationType`.

In addition, configuration can be specified via the `/etc/hpcpack/config.json` configuration file. For example, to configure `CertificateValidationType` to `1` (Skip CN check), use the following JSON config:
```json
{
	"CertificateValidationType": 1
}
```

### Certificates
- On Linux, users need to manually add their certificate into the appropriate [X.509 certificate store](https://learn.microsoft.com/en-us/dotnet/standard/security/cross-platform-cryptography#x509store) corresponding to `CurrentUser\My` and `LocalMachine\Root` for Linux. For `CurrentUser\My`, user would need to import their certificate using code similar to the following:
```
using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadWrite))
{
    store.Add(new X509Certificate2(
        "./thePathToTheCert.pfx", "passwordOfTheCert", 
        X509KeyStorageFlags.PersistKeySet));
}
```
For `LocalMachine\Root`, user would need to import the certificate into the default OpenSSL CA bundle using the appropriate command for your Linux distribution. See [here](https://ubuntu.com/server/docs/install-a-root-ca-certificate-in-the-trust-store) and [here](https://www.redhat.com/sysadmin/configure-ca-trust-list) for examples of how to do it on Ubuntu and RHEL.

### Logging
Logging can be configured via `appsettings.json`. See [here](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line#configure-logging-without-code) for more information.

## Known issues with .NET Standard 2.0 SDK and Linux support
- When getting certificates from certificate stores on Windows and Linux, there is no UI pop-up when more than one certificate is available, resulting in no certificate being chosen and failure downstream.
- Connecting to cluster via .NET Remoting is not supported
- Entering credentials interactively is not supported. Pass username and password explicitly or use `CCP_USERNAME` and `CCP_PASSWORD` environment variables instead.
- Excel isn't supported

## Contributing
This project welcomes contributions and suggestions. Most contributions require you to
agree to a Contributor License Agreement (CLA) declaring that you have the right to,
and actually do, grant us the rights to use your contribution. For details, visit
https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need
to provide a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the
instructions provided by the bot. You will only need to do this once across all repositories using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)
or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

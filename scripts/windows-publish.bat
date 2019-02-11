if defined CIRRUS_RELEASE (
    hub release edit -m "" --attach .\bin\release\netcoreapp2.1\win-x64\native\BDInfo.exe#bdinfox-windows.exe %CIRRUS_RELEASE%
) else (
    echo Not a release. No need to deploy!
)
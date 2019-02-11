if defined CIRRUS_RELEASE (
    echo Uploading asset...
    curl -X POST --data-binary @bin\release\netcoreapp2.1\win-x64\native\BDInfo.exe --header "Authorization: token %GITHUB_TOKEN%" --header "Content-Type: application/octet-stream" "https://uploads.github.com/repos/%CIRRUS_REPO_FULL_NAME%/releases/%CIRRUS_RELEASE%/assets?name=bdinfox-windows.exe"
) else (
    echo Not a release. No need to deploy!
)
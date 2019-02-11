# escape=`

FROM microsoft/dotnet-framework:4.7.2-sdk

# Install VCTools Workload
# https://download.visualstudio.microsoft.com/download/pr/02dffa51-7828-4905-9dd0-c012c0eca0f3/58cdff8a6914bf2df48cd598ce1c8dad/vs_buildtools.exe
RUN Invoke-WebRequest -UseBasicParsing https://download.visualstudio.microsoft.com/download/pr/12210059/e64d79b40219aea618ce2fe10ebd5f0d/vs_BuildTools.exe -OutFile vs_BuildTools.exe; `
    Start-Process vs_BuildTools.exe -ArgumentList 'modify', '--add', 'Microsoft.VisualStudio.Workload.VCTools', '--includeRecommended', '--quiet', '--norestart', '--nocache' ,'--installPath', \"`\"${Env:ProgramFiles(x86)}\Microsoft Visual Studio\2017\BuildTools`\"\" -NoNewWindow -Wait; `
    Remove-Item -Force vs_buildtools.exe; `
    Remove-Item -Force -Recurse \"${Env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\"; `
    Remove-Item -Force -Recurse ${Env:TEMP}\*; `
    iex (new-object net.webclient).downloadstring('https://get.scoop.sh'); `
    scoop install curl; `
    scoop cache rm *;
   
# Restore the default Windows shell for correct batch processing below.
SHELL ["cmd", "/S", "/C"]

# Start developer command prompt with any other commands specified.
ENTRYPOINT ["\"C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\BuildTools\\Common7\\Tools\\VsDevCmd.bat\"", "-arch=amd64", "-host_arch=amd64", "&&"]

# Default to PowerShell if no other command specified.
CMD ["powershell.exe", "-NoLogo", "-ExecutionPolicy", "Bypass"]
# escape=`

FROM microsoft/dotnet-framework:4.7.2-sdk

# Install AzureBuildTools Workload 
RUN Invoke-WebRequest -UseBasicParsing https://download.visualstudio.microsoft.com/download/pr/12210059/e64d79b40219aea618ce2fe10ebd5f0d/vs_BuildTools.exe -OutFile vs_BuildTools.exe; `
    Start-Process vs_BuildTools.exe -ArgumentList 'modify', '--add', 'Microsoft.VisualStudio.Workload.VCTools', '--includeRecommended', '--quiet', '--norestart', '--nocache' ,'--installPath', \"`\"${Env:ProgramFiles(x86)}\Microsoft Visual Studio\2017\BuildTools`\"\" -NoNewWindow -Wait; `
    Remove-Item -Force vs_buildtools.exe; `
    Remove-Item -Force -Recurse \"${Env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\"; `
    Remove-Item -Force -Recurse ${Env:TEMP}\*
   
SHELL ["cmd", "/S", "/C"]
ENTRYPOINT ["\"C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\BuildTools\\Common7\\Tools\\VsDevCmd.bat\"", "-arch=amd64", "-host_arch=amd64", "&&"]
CMD ["powershell.exe", "-NoLogo", "-ExecutionPolicy", "Bypass"]
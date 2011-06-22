
. .\utilities.ps1

.\cleanup.ps1

VerifyIISWebAdministrationIsInstalled


$applicationPoolName = "delete_me_later_jeterror"


$null = CreateIISAppPool $applicationPoolName;

CreateIISSite -appName "deleteme-web1" -hostname "127.0.0.21" -port 8080 -appPoolName $applicationPoolName -localPath (gi ".\Web1").fullname

CreateIISSite -appName "deleteme-web2" -hostname "127.0.0.22" -port 8080 -appPoolName $applicationPoolName -localPath (gi ".\Web2").fullname

"Sites created...  replicating soon"
[System.Threading.Thread]::Sleep(5000);

$v4_net_version = (ls "$env:windir\Microsoft.NET\Framework\v4.0*").Name
&"C:\Windows\Microsoft.NET\Framework\$v4_net_version\MSBuild.exe" ".\RavenConsole\RavenConsole.sln"

#.\RavenConsole\RavenConsole\bin\debug\RavenConsole.exe begin-replicate -w http://127.0.0.21:8080/ http://127.0.0.22:8080/
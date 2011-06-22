
. .\utilities.ps1


VerifyIISWebAdministrationIsInstalled


$applicationPoolName = "delete_me_later_jeterror"


$null = CreateIISAppPool $applicationPoolName;

CreateIISSite -appName "deleteme-web1" -hostname "127.0.0.21" -port 8080 -appPoolName $applicationPoolName -localPath "Web1"

CreateIISSite -appName "deleteme-web2" -hostname "127.0.0.22" -port 8080 -appPoolName $applicationPoolName -localPath "Web2"



function VerifyIISWebAdministrationIsInstalled
{
	# WebAdministration module requires we run in x64
	if ($env:ProgramFiles.Contains("x86")) {
		throw "IIS module WebAdministration requires powershell be running in 64bit."
	}

	try
	{
		import-module WebAdministration
	}
	catch
	{
		"Installing IIS WebAdministration..." | write-host -fore yellow

		import-module WebAdministration
	}
}


function RemoveIISMembersOf($appPoolName) {

    $expectedAppPoolPath = "IIS:\AppPools\$appPoolName";

    if (test-path $expectedAppPoolPath) {
        $appPool = gi $expectedAppPoolPath;

        ls "IIS:\sites" | ? { $_.applicationPool -eq $appPool.name } | remove-item -recurse

        $appPool | remove-item -recurse
    }
}


function CreateIISAppPool($appPoolName) {

    $expectedAppPoolPath = "IIS:\AppPools\$appPoolName";

    if (-not (test-path $expectedAppPoolPath)) {
		$appPool = new-item $expectedAppPoolPath
		$appPool.processModel.pingingEnabled = "False"
		$appPool.managedPipelineMode = "Integrated"
		$appPool.managedRuntimeVersion = "v4.0"
		$appPool | set-item
	}
	
	gi $expectedAppPoolPath
}


function CreateIISSite($appName, $address = "*", $hostname, $port, $appPoolName, $localPath, $protocol = "http") {

    $appPath = "iis:\sites\$appName";
        
    $site = new-item $appPath -bindings @{protocol=$protocol;bindingInformation=$address + ":" + $port + ":" + $hostname} -physicalPath $localPath 

    Set-ItemProperty $site.PSPath -name applicationPool -value $appPoolName
}


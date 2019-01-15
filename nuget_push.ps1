
############################################################################
###                                                                      ###
###                    NUGET  PACKAGE and PUBLISH                        ###
###                                                                      ###
############################################################################



param (
  [string]$version = "4.7.0",
  [string]$apiKey = "yx1234",
  [string]$source = $PSScriptRoot,
  [string]$destination = $PSScriptRoot,
  [string]$pushSource = "http://tfs:8088/nuget",
  [string]$nuget = "nuget.exe",
  [bool]$clean = $false,
  [bool]$pack = $false,
  [bool]$ignoreError = $true
)

$self = $MyInvocation.MyCommand.Name

function DisplayCommandLineArgs()
{
	
    "Options provided:"
    "    => version: $version"
    "    => source: $source"
    "    => destination: $destination"
    "    => nuget: $nuget"
    "    => api key: $apiKey"
    "    => clean: $clean"
	"    => pack: $pack"
	"    => ignoreError: $ignoreError"

    ""
    "eg. $self -version 0.1-alpha"
    "eg. $self -version 0.1-alpha -destination C:\temp\TempNuGetPackages"
    "eg. $self -version 0.1-alpha -source ../nugetspecs/ -destination C:\temp\TempNuGetPackages"
    "eg. $self -version 0.1-alpha -nuget c:\temp\nuget.exe"
    "eg. $self -version 0.1-alpha -nuget c:\temp\nuget.exe -apiKey ABCD-EFG..."
    ""

    if (-Not $version)
    {
        ""
        "**** The version of this NuGet package is required."
        "**** Eg. ./NuGetPackageAndPublish.ps1 -version 0.1-alpha"
        ""
        ""
        throw;
    }

    if ($source -eq "")
    {
        ""
        "**** A source parameter provided cannot be an empty string."
        ""
        ""
        throw;
    }

    if ($destination -eq "")
    {
        ""
        "**** A destination parameter provided cannot be an empty string."
        ""
        ""
        throw;
    }

    if ($pushSource -eq "")
    {
        ""
        "**** The NuGet push source parameter provided cannot be an empty string."
        ""
        ""
        throw;
    }

    # Setup the nuget path.
    if (-Not $nuget -eq "")
    {
        $global:nugetExe = $nuget
    }
    else
    {
        # Assumption, nuget.exe is the current folder where this file is.
        $global:nugetExe = Join-Path $source "nuget" 
    }

    $global:nugetExe

    if (!(Test-Path $global:nugetExe -PathType leaf))
    {
        ""
        "**** Nuget file was not found. Please provide the -nuget parameter with the nuget.exe path -or- copy the nuget.exe to the current folder, side-by-side to this powershell file."
        ""
        ""
        throw;
    }
}


function CleanUp()
{
    if ($clean -eq $false)
    {
        return;
    }

    $nupkgFiles = @(Get-ChildItem $destination -Filter *.nupkg)

    if ($nupkgFiles.Count -gt 0)
    {
        "Found " + $nupkgFiles.Count + " *.nupkg files. Deleting..."

        foreach($nupkgFile in $nupkgFiles)
        {
            $combined = Join-Path $destination $nupkgFile
            "... Removing $combined."
            Remove-Item $combined
        }
        
        "... Done!"
    }
}


function PackageTheSpecifications()
{
	if ($pack -eq $false)
	{
		return;
	}
    ""
    "Getting all *.nuspec files to package in directory: $source"

    $files = Get-ChildItem $source -Filter *.nuspec

    if ($files.Count -eq 0)
    {
        ""
        "**** No nuspec files found in the directory: $source"
        "Terminating process."
        throw;
    }

    "Found: " + $files.Count + " files :)"

    foreach($file in $files)
    {
        &$nugetExe pack $file -Version $version -OutputDirectory $destination

        ""
    }
}

# 检查包是否需要上传：版本未更新则不上传
function ValidateVersion($nupkg)
{
	$pattern="^(?<name>.*)\.(?<version>([1-9]\d|[1-9])(\.([1-9]\d|\d)){2,3}).nupkg$"
	if ($nupkg -match $pattern)
	{
		$nupkgName = $matches["name"]
		$nupkgVersion = $matches["version"]		
		Write-Host  "Current package is $nupkgName with version $nupkgVersion"
		$serverReply = &$nugetExe list -Source $pushSource $nupkgName
		Write-Host "Server replies:  $serverReply"
		if ($serverReply.StartsWith($nupkgName))
		{
			$serverVersion = $serverReply.Split(' ')[1];
			Write-Host "Server version is $serverVersion"
			$c = CompareVersion -a $nupkgVersion -b $serverVersion 
			if ($c -eq 1)
			{
				return $true
			}
			return $false
		}		
		return $true
	}
	return $false
}

# 比较版本号大小
function CompareVersion($a, $b)
{
	$va = $a.Split('.')
	$vb = $b.Split('.')
	$length = $va.Length
	if ($vb.Length -gt $length)
	{
		$length = $b.Length
	}
	for ($i=0; $i -lt $length; $i++)
	{
		if ($i -ge $va.Length -and $i -lt $vb.Length)
		{
			# a < b
			return -1 
		}
		
		if ($i -ge $vb.Length -and $i -lt $va.Length)
		{
			# a > b
			return 1
		}
		$ai = [int]$va[$i]
		$bi = [int]$vb[$i]
		if ($ai -lt $bi)
		{
			return -1
		}
		
		if ($ai -gt $bi)
		{
			return 1
		}
    }
	# a = b
	return 0
}


function PushThePackagesToNuGet()
{
    if ($apiKey -eq "")
    {
        "@@ No NuGet server api key provided - so not pushing anything up."
        return;
    }


    ""
    "Getting all *.nupkg's files to push to : $pushSource"

    $files = Get-ChildItem $destination -Filter *.nupkg

    "Found: " + $files.Count + " files :)"

    foreach($file in $files)
    {
		$validation = ValidateVersion -nupkg $file.Name 
		if($validation -eq $true)
		{
			&$nugetExe push ($file.FullName) -Source $pushSource -apiKey $apiKey 
		}
		else
		{
			$file.Name + " is up to date"
		}
        ""
    }
}

##############################################################################
##############################################################################

$ErrorActionPreference = "Stop"
$global:nugetExe = "nuget.exe"

""
" ---------------------- start script ----------------------"
""
""
"  Starting NuGet packing/publishing script -  (╯°□°）╯︵ ┻━┻"
""
"  This script will look for -all- *.nuspec files in a source directory,"
"  then paackage them up to *.nupack files. Finally, it can publish"
"  them to a NuGet server, if an api key was provided."
""

DisplayCommandLineArgs

PackageTheSpecifications

PushThePackagesToNuGet

CleanUp

""
""
" ---------------------- end of script ----------------------"
""
""

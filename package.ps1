if($args.Count -lt 1){
  throw 'Need at least one argument!'
}

[System.Environment]::CurrentDirectory = $pwd

function use-var([IDisposable]$instance, [ScriptBlock]$script){
  try{
    &$script
  }finally{
    $instance.Dispose();
  }
}

$nuspec = [string]$args[0]
if([System.IO.Path]::GetExtension($nuspec).ToUpperInvariant() -ne '.NUSPEC'){
  $nuspec += '.nuspec'
}

if(!(Test-Path $nuspec)){
  throw "File ""$nuspec"" is not found!"
}

'Updating: ' + $nuspec

if($args.Count -lt 2){
  $list = @($args[0])
}else{
  $list = [Array]$args[1..($args.Count-1)]
}

$ver = $null
$last = 0,0,0,0

$exts = '.DLL', '.EXE'
:ver foreach($item in $list){
  $src = [string]$item
  $ext = [System.IO.Path]::GetExtension($nuspec).ToUpperInvariant()
  if(!$exts.Contains($ext)){
    $src += '.dll'
  }

  if(!(Test-Path $src)){
    "File ""$src"" is not found!"
    continue
  }

  $file = gi $src
  $vi = [System.Diagnostics.FileVersionInfo]$file.VersionInfo
  $curr = $vi.FileMajorPart, $vi.FileMinorPart, $vi.FileBuildPart, $vi.FilePrivatePart

  "- $src :: $curr → $($vi.ProductVersion)"

  for($i = 0; $i -lt 4; $i++){
    if($curr[$i] -gt $last[$i]) { break }
    if($curr[$i] -lt $last[$i]) { continue ver }
  }

  $last = $curr
  $ver = $vi
}

$iver = $ver.ProductVersion
"Final Version: $nuspec → $iver"

$xs = New-Object System.Xml.XmlWriterSettings
$xs.Indent = $true
$xs.IndentChars = "  "
$xml = [xml](Get-Content $nuspec)
$meta = $xml.package.metadata
$nupkg = "$($meta.id).$iver.nupkg"
$meta.version = $iver

use-var($xw = [System.Xml.XmlWriter]::Create($nuspec, $xs)){
  $xml.Save($xw)
}

'Updating Acomplished.'

&nuget pack "$nuspec" -Verbosity detailed

if(Test-Path Env:LocalNupkg){
  if(-not (Test-Path $Env:LocalNupkg)){
    md $Env:LocalNupkg
  }
  copy $nupkg $Env:LocalNupkg
}

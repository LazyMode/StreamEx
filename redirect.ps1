$dest = $args[0] + '.nuspec';
$list = [Array]$args[1..($args.Count-1)];

$xml = (gc $dest)
$x = [xml]$xml

foreach($dep in $x.package.metadata.dependencies.dependency){
  $list | where { $_ -eq $dep.id } | % { 
    $dep.version = '[' + ([xml](gc "$_.nuspec")).package.metadata.version + ']'
  }
}

$xs = New-Object System.Xml.XmlWriterSettings
$xs.Indent = $true
$xs.IndentChars = '  '
$xw = [System.Xml.XmlWriter]::Create($dest, $xs)
$x.Save($xw)

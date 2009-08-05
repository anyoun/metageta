$ErrorActionPreference = "Stop"

function create-directory($dir) {
	if(-not (test-path $dir)) {
		(mkdir $dir).FullName + '\'
	} else {
		join-path (resolve-path $dir) '\'
	}
}


function recreate-directory($dir) {
	if(test-path $dir) {
		rmdir -Recurse $dir
	}
	create-directory $dir
}

if(test-path hkcu:\Software\7-Zip) {
	$7zipPath = join-path (gi hkcu:\Software\7-Zip).GetValue("Path") "7z.exe"
} else {
	throw "No 7zip!"
}

$extrafiles = @( "log4net.xml", "TvdbLib.xml", "System.Data.SQLite.xml" )

$timestamp = (get-date).ToString("yyyyMMdd")

pushd ..

create-directory "output"
$x86Dir = recreate-directory "output\MetaGeta.$timestamp.x86"
$x64Dir = recreate-directory "output\MetaGeta.$timestamp.x64"

msbuild MetaGeta\MetaGeta.sln /v:q /t:Rebuild "/p:Configuration=Release;Platform=x86;OutDir=$x86Dir;OutputPath=$x86Dir"
msbuild MetaGeta\MetaGeta.sln /v:q /t:Rebuild "/p:Configuration=Release;Platform=x64;OutDir=$x64Dir;OutputPath=$x64Dir"

pushd $x86Dir
	$extrafiles | rm
popd
pushd $x64Dir
	$extrafiles | rm
popd


& $7zipPath a -mx9 -mmt "output\MetaGeta.$timestamp.x86.7z" "$x86Dir"
& $7zipPath a -mx9 -mmt "output\MetaGeta.$timestamp.x64.7z" "$x64Dir"

popd